using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDBLib;
using System.Text;
using System.Text.RegularExpressions;

namespace MongoDBWebApp.Controllers
{
    public enum SearchMethod
    {
        NoSQL = 1,
        SQL = 2
    }

    #region FulltextController class

    [ApiController]
    [Route("[controller]")]
    public class FulltextController : Controller
    {
        [HttpGet(Name = "Search")]
        public IEnumerable<FulltextResult> Search(string fullText, bool includeFulltext, SearchMethod searchMethod = SearchMethod.NoSQL)
        {
            List<FulltextResult> res = new List<FulltextResult>();

            if (searchMethod == SearchMethod.SQL)
            {
                res = SQLHelper.SelectDataWithLikeFilter(fullText, includeFulltext);
            }
            else
            {
                MongoClient dbClient = new MongoClient("mongodb://localhost:27017");

                var rex = Regex.Escape(fullText);
                var ftFilter = Builders<BsonDocument>.Filter.Regex("fullText", new BsonRegularExpression("/" + rex + "/i"));

                var database = dbClient.GetDatabase("dbmigration");
                var specifications = database.GetCollection<BsonDocument>("specifications");
                var json = "{ fullText: { $regex: \"*" + fullText + "*\" } }";
                var bdoc = BsonDocument.Parse(json);
                var fo = new FindOptions() { };
                var docs = specifications.Find(ftFilter).ToList();

                foreach (var doc in docs)
                {
                    FulltextResult ftRes = new FulltextResult();
                    var eleFt = doc["fullText"];
                    var eleFn = doc["fileName"];
                    var lastWriteTime = doc["lastWriteTime"];

                    if (includeFulltext)
                        ftRes.FullText = (eleFt != null ? eleFt.ToString() : null);

                    ftRes.FileName = (eleFn != null ? eleFn.ToString() : null);

                    //ftRes.FileName = doc.Elements["fileName"];

                    res.Add(ftRes);
                }
            }

            return res;
        }
    }

    #endregion
}
