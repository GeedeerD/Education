namespace Models.Messages
{
    public class MessageModel
    {
        public int Id { get; set; }
        public Guid? ChatId { get; set; }
        public string FromName { get; set; }
        public string ToName { get; set; }
        public string MessageBody { get; set; }
        public bool IsViewed { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsModified { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime ViewedAt { get; set; }
        public DateTime DeletedAt { get; set; }
    }
    public class MessageViewModel
    {
        public string ToName { get; set; }
        public string MessageBody { get; set; }
    }
}
