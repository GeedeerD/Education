using DataBase.Models;

namespace fiwe.Models
{
    public class ChatViewModel
    {
        public string ChatId { get; set; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }
        public DateTime LastActiveDateTime { get; set; }
        public string LastMessagePreview { get; set; }
        public string RecipientId { get; set; }

        public static ChatViewModel WrapModel(ChatModelDto modelDto, string userId) {
            return new ChatViewModel
            {
                ChatId = modelDto.ObjectId.ToString(),
                Name = modelDto.Name,
                LastActiveDateTime = modelDto.ActiveDate,
                RecipientId = modelDto.UserObjectIds.Where(u => u.ToString() != userId).FirstOrDefault().ToString()
            };
        }
    }
}