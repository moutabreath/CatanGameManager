namespace CommonLib.Config
{
    public class MongoConfig
    {
        public string MongoConnectionString { get; set; }
        public string MongoDatabaseName { get; set; }
        public string MongoGameDocumentName{ get;  set; }
        public string MongoPlayerDocumentName{ get;  set; }
    }
}
