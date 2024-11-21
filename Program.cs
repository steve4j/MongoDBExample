// See https://aka.ms/new-console-template for more information
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDBIndexer;
using System;

if (MongoDBHelper.IsInstalled())
{
    Console.WriteLine("MongoDB server has been identified");
}

string atlasString = MongoDBHelper.GetConnString();

MongoClient dbClient = new MongoClient(atlasString);

var dbNames = MongoDBHelper.GetDbNames(dbClient);
var database = dbClient.GetDatabase("dbmigration");
var collection = database.GetCollection<BsonDocument>("tables");
var specifications = database.GetCollection<BsonDocument>("specifications");

void indexDocs()
{
    string cd = Environment.CurrentDirectory;
    string path = Path.Combine(cd, "datenblätter");

    foreach(string file in Directory.GetFiles(path, "*.pdf"))
    {
        var pdfEx = new PdfExtractor(file);
        var txt = pdfEx.ExtractText();
        var fn = Path.GetFileName(file);
        var filter = Builders<BsonDocument>.Filter.Eq("fileName", fn);
        string objectId = (Guid.NewGuid().ToString()).Replace("(", "").Replace(")", "").Replace("-", "");

        var document = new BsonDocument
        {
            { "_id", new ObjectId(objectId) },
            { "fileName", fn },
            { "fullText", txt }
        };

        var doc = specifications.Find(filter).FirstOrDefault();

        if (doc != null)
        {
            //collection.UpdateOne(filter, document);
            specifications.DeleteOne(filter);
            specifications.InsertOne(document);
            //collection.UpdateOne(filter, document);
        }
        else
        {
            specifications.InsertOne(document);

        }

        Console.WriteLine(txt);
    }
}

void collectDocs()
{
    Console.WriteLine("Indexing CSV files ...");

    string currentPath = Path.Combine(Environment.CurrentDirectory, "csvFiles");
    string[] files = Directory.GetFiles(currentPath, "*.csv");

    foreach(string csvFile in files)
    {
        Console.WriteLine("Indexing CSV file " + csvFile);

        MongoCSVLoader loader = new MongoCSVLoader(database, collection, csvFile);

        loader.Read();
    }
}

/* from original sample project. Archive an email
void insertDoc(ELORepositoryNode repNodeOri = null)
{
    if (repNodeOri == null)
        return;

    var tmp = new ELORepositoryNodeCollection();
    ELORepositoryNode rn = null;

    if (repNodeOri != null)
    {
        rn = repNodeOri;
        tmp.Add(rn);
    }
    else
        tmp.Add(rn = new ELORepositoryNode() { ShortName = "test" });

    foreach (ELORepositoryNode repNode in tmp)
    {
        string guid = repNodeOri.GUID;
        string objectId = guid.Replace("(", "").Replace(")", "").Replace("-", "");


        var bsonDocument = new BsonDocument();


        foreach (ELOObjectKey key in repNode.Keys)
        {
            if (key.ValueObject != null)
            {
                bsonDocument.Add(new BsonElement(key.Name, key.Value));
            }
        }

        var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(objectId));

        var document = new BsonDocument
        {
            { "_id", new ObjectId(objectId) },
            { "object_id", repNode.ID },
            { "object_type", repNode.Type },
            { "object_guid", repNode.GUID },
            { "subject", repNode.ShortName },
            { "parentid", repNode.ParentID },
            { "class_id", 480},
            { "keys", bsonDocument }
        };

        var doc = collection.Find(filter).FirstOrDefault();

        if (doc != null)
        {
            //collection.UpdateOne(filter, document);
            collection.DeleteOne(filter);
            collection.InsertOne(document);
            //collection.UpdateOne(filter, document);
        }
        else
        {
            collection.InsertOne(document);

        }
    }
}
*/

void filterDocs()
{

    var builderDetail = Builders<BsonDocument>.Filter;

    // subjekt filter
    var gmailFilter = Builders<BsonDocument>.Filter.Regex("fullText", new BsonRegularExpression("/smtext\\.Dokument/is"));

    // filter the detail
    var filterDetail = builderDetail.Eq("tableName", "tp_reportsystem");
    filterDetail &= gmailFilter;

    bool allDocuments = true;

    if (allDocuments)
    {
        var ff = collection.Find(filterDetail).ToListAsync();
        var totalCount = 0;

        foreach(var doc in ff.Result)
        {
            Console.WriteLine("FD: " + doc["fullText"]);
            totalCount++;
        }

        Console.WriteLine("Total-Count: " + totalCount);
    }
    else
    {
        var firstDoc = collection.Find(filterDetail).FirstOrDefault();

        Console.WriteLine("FD: " + firstDoc["fullText"]);
    }
}

indexDocs();

collectDocs();

//insertDoc();

filterDocs();