namespace DataBase.Models
{
    public class AccessInfo
    {
        public AccessType Type { get; set; }
        public bool IsDeny => Type == 0;
        public IEnumerable<string> Recipients { get; set; } = [];
    }

    public enum AccessType : byte
    {
        None = 0,
        General = 1,
        Creator = 2,
        Moderator = 4,
        Admin = 8,
    }
}