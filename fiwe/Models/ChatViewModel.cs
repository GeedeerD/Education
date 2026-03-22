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

        public static ChatViewModel WrapModel(ChatModelDto modelDto) {
            return new ChatViewModel
            {
                ChatId = modelDto.ObjectId.ToString(),
                Name = modelDto.Name,
                LastActiveDateTime = modelDto.ActiveDate,
            };
        }
    }
}