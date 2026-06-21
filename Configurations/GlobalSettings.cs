namespace Configurations
{
    public class GlobalSettings
    {

    }

    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }

    public class MongoDbSettings
    {
        public const string SectionName = "MongoDbSettings";
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
