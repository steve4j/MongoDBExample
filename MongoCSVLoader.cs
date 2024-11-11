using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Security.Cryptography;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MongoDBIndexer
{
    #region MongoCSVLoader class

    public class MongoCSVLoader
    {
        private IMongoDatabase mongoDatabase;
        private IMongoCollection<BsonDocument> mongoCollection;
        private string fileName;

        public MongoCSVLoader(IMongoDatabase db, IMongoCollection<BsonDocument> mongoCollection, string fileName)
        {
            this.mongoDatabase = db;
            this.mongoCollection = mongoCollection;
            this.fileName = fileName;
        }

        private string buildFulltext(IDictionary<string,object> dic)
        {
            List<string> ret = new List<string>();

            foreach(var entry in dic)
            {
                ret.Add(entry.Key + ": " + entry.Value);
            }

            return string.Join(", ", ret.ToArray());
        }

        public void Read()
        {
            using (var tr = new StreamReader(this.fileName, Encoding.UTF8))
            using (var md5 = MD5.Create())
            {

                var tableName = Path.GetFileNameWithoutExtension(this.fileName);
                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.CurrentCulture);

                config.Delimiter = ";";

                CsvHelper.CsvReader reader = new CsvHelper.CsvReader(tr, config);
                CsvHelper.CsvDataReader dataReader = new CsvHelper.CsvDataReader(reader);

                while (reader.Read())
                {
                    string[] headerRecord = reader.HeaderRecord;
                    Dictionary<string, object> dic = new Dictionary<string, object>();

                    for (int i = 0; i < reader.ColumnCount; i++)
                    {
                        string fieldValue = reader.GetField(i);                        

                        if (fieldValue == "NULL")
                            fieldValue = "";

                        dic[headerRecord[i]] = fieldValue;
                    }

                    dic["tableName"] = tableName;
                    dic["fullText"] = buildFulltext(dic);

                    byte[] buffer = Encoding.UTF8.GetBytes((string)dic["fullText"]);
                    string tn = BitConverter.ToString(Encoding.UTF8.GetBytes(tableName)).Replace("-", "");
                    string md5Str = BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", "");
                    ObjectId objId = new ObjectId();

                    dic["_id"] = objId;
                    dic["md5"] = md5Str;

                    var filter = Builders<BsonDocument>.Filter.Eq("md5", md5Str);
                    var bdoc = mongoCollection.Find(filter).FirstOrDefault();

                    if (bdoc == null)
                    {
                        bdoc = new BsonDocument(dic);
                        mongoCollection.InsertOne(bdoc);
                    }
                    else
                    {
                        mongoCollection.DeleteOne(filter);
                        mongoCollection.InsertOne(new BsonDocument(dic));
                    }
                }
            }
        }
    }

    #endregion
}
