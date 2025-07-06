namespace DataBase.Models
{
    public class IndividualMessageFilter
    {
        public int? MessageId { get; set; }
        public Guid ChatId { get; set; }
        public string FromName { get; set; }
        public string ToName { get; set; }
        public DateTime? FromDate { get; set; }
    }
}