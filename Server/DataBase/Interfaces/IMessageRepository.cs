using DataBase.Models;
using Models.Messages;

namespace DataBase.Interfaces
{
    public interface IMessageRepository
    {
        IEnumerable<MessageModel> GetAllMessages(IndividualMessageFilter filter);
        void AddMessage(MessageModel message);
        int GetId();
    }
}