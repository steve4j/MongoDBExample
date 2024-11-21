using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using MongoDBLib;
using Microsoft.Data.SqlClient;

namespace MongoDBWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchController : Controller
    {
        [HttpGet("SearchMongoDb")]
        public SearchResult SearchMongo(string fullText, bool includeFulltext)
        {
            var sw = Stopwatch.StartNew();

            var res = new List<Result>();
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
                var ftRes = new Result();
                var eleFt = doc["fullText"];
                var eleFn = doc["fileName"];
                var lastWriteTime = doc["lastWriteTime"];

                if (includeFulltext)
                    ftRes.FullText = (eleFt != null ? eleFt.ToString() : null);

                ftRes.FileName = (eleFn != null ? eleFn.ToString() : null);

                //ftRes.FileName = doc.Elements["fileName"];

                res.Add(ftRes);
            }

            var searchResult = new SearchResult { RequestDuration = sw.ElapsedMilliseconds, Results = res };

            return searchResult;
        }

        [HttpGet("SearchSqlDb")]
        public SearchResult SearchSql(string fullText, bool includeFulltext)
        {
            var sw = Stopwatch.StartNew();

            var res = new List<Result>();
            string query = "SELECT * FROM [dbo].[File]";

            if (!string.IsNullOrEmpty(fullText))
            {
                query += $" where Text like '%{fullText}%'";
            }

            using (SqlConnection connection = new SqlConnection(SQLHelper.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var fileResult = new Result
                        {
                            Id = (int)reader["Id"],
                            FileName = reader["Name"]?.ToString()
                        };
                        if (includeFulltext)
                            fileResult.FullText = reader["Text"].ToString();
                        res.Add(fileResult);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            var searchResult = new SearchResult { RequestDuration = sw.ElapsedMilliseconds, Results = res };

            return searchResult;
        }
    }
}
