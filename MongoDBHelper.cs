using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MongoDBIndexer
{
    #region MongoDBHelper class

    public static class MongoDBHelper
    {

        public static string[] GetDbNames(MongoClient dbClient)
        {
            List<string> names = new List<string>();
            var dbList = dbClient.ListDatabases().ToList();

            foreach (var db in dbList)
            {
                var name = db["name"];

                if (name is BsonValue)
                {
                    names.Add((string)name);
                }
            }

            return names.ToArray();
        }

        public static string GetConnString(string serverName = null)
        {
            if (serverName == null)
                serverName = "localhost";

            return "mongodb://" + serverName + ":27017";
        }

        public static FilterDefinition<BsonDocument> PrepareRegexFilter(string regex)
        {
            var regexFilter = Builders<BsonDocument>.Filter.Regex("fullText", 
                new BsonRegularExpression("/" + Regex.Escape(regex) + "/is"));

            return regexFilter;
        }

        public static bool IsInstalled()
        {
            string serviceName = "MongoDB";

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                string dn = sc.DisplayName;
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }

    #endregion
}
