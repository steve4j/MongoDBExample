namespace MongoDBLib
{
    public class Result
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FullText { get; set; }
        public string LastWriteTime { get; set; }
    }

    public class SearchResult
    {
        public long RequestDuration { get; set; }
        public List<Result> Results { get; set; }
    }
}
