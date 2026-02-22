using DataBase;
using DataBase.Models;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace Tests
{
    public class MessageRepositoryTests : IDisposable
    {
        private MongoDbRunner _runner;
        private MessageRepository _repo;

        [SetUp]
        public void SetUp()
        {
            _runner = MongoDbRunner.Start();
            var client = new MongoClient(_runner.ConnectionString);
            var database = client.GetDatabase("test-db");

            _repo = new MessageRepository(database);
        }

        [Test]
        public async Task CreateChatIfNotExists_ByNewChat_ShouldReturnChatId()
        {
            var userId = ObjectId.GenerateNewId();
            var chat = new ChatModelDto() { UserObjectIds = [userId] };

            await _repo.CreateChatAsync(userId, chat);

            Assert.That(ObjectId.Empty, Is.Not.EqualTo(chat.ObjectId));
        }


        [Test]
        public async Task SendMessage_WithChatIdAndFromUserObjectId_ShouldSendMessage()
        {
            var message = new MessageModelDto() { FromUserObjectId = ObjectId.GenerateNewId(), ChatId = ObjectId.GenerateNewId(), MessageBody = "Message" };

            await _repo.SendMessageAsync(message);

            Assert.That(ObjectId.Empty, Is.Not.EqualTo(message.ObjectId));
        }


        [Test]
        public async Task SendMessage_WithoutChatIdAndFromUserObjectId_ShouldException()
        {
            var message = new MessageModelDto() { FromUserObjectId = ObjectId.GenerateNewId(), MessageBody = "Message" };

            Assert.ThrowsAsync<ArgumentException>(async () => await _repo.SendMessageAsync(message));
        }


        [Test]
        public async Task ReadMessage_ByObjectId_ShouldReturnMessage()
        {
            var fromUserId = ObjectId.GenerateNewId();
            var toUserId = ObjectId.GenerateNewId();
            var chat = new ChatModelDto() { UserObjectIds = [fromUserId, toUserId] };

            await _repo.CreateChatAsync(fromUserId, chat);

            var message = new MessageModelDto() { FromUserObjectId = fromUserId, ChatId = chat.ObjectId , MessageBody = "Message" };
            const string messageBody = "Test";
            message.MessageBody = messageBody;

            await _repo.SendMessageAsync(message);

            Assert.That(ObjectId.Empty, Is.Not.EqualTo(message.ObjectId));

            // Action
            var result = await _repo.ReadMessageAsync(toUserId, message.ObjectId);

            // Assert
            Assert.That(messageBody, Is.EqualTo(message.MessageBody));
        }


        [Test]
        public async Task ReadMessages_ByChatId_ShouldReturnMessages()
        {
            ObjectId fromUserId = ObjectId.GenerateNewId();
            ObjectId toUserId = ObjectId.GenerateNewId();

            var chat = new ChatModelDto() { UserObjectIds = [fromUserId, toUserId] };
            await _repo.CreateChatAsync(fromUserId, chat);
            ObjectId chatId = chat.ObjectId;

            var message1 = new MessageModelDto() { FromUserObjectId = fromUserId, ChatId = chatId, MessageBody = "Body 1"};
            var message2 = new MessageModelDto() { FromUserObjectId = toUserId, ChatId = chatId, MessageBody = "Body 2"};
            var message3 = new MessageModelDto() { FromUserObjectId = fromUserId, ChatId = chatId, MessageBody = "Body 3"};

            await _repo.SendMessageAsync(message1);
            await _repo.SendMessageAsync(message2);
            await _repo.SendMessageAsync(message3);

            // Action
            var result = await _repo.GetMessagesAsync(fromUserId, chatId);

            // Assert
            Assert.That(message1.MessageBody, Is.EqualTo(result.First(x=>x.ObjectId == message1.ObjectId).MessageBody));
            Assert.That(message2.MessageBody, Is.EqualTo(result.First(x=>x.ObjectId == message2.ObjectId).MessageBody));
            Assert.That(message3.MessageBody, Is.EqualTo(result.First(x=>x.ObjectId == message3.ObjectId).MessageBody));
        }

        [Test]
        public async Task GetChat_ByParticipants_ShouldReturnChat()
        {
            ObjectId fromUserId = ObjectId.GenerateNewId();
            ObjectId toUserId = ObjectId.GenerateNewId();

            var chat = new ChatModelDto() { UserObjectIds = [fromUserId, toUserId] };
            await _repo.CreateChatAsync(fromUserId, chat);

            // Action
            var result = await _repo.GetChatByUserOjectIdsAsync(fromUserId, toUserId);

            // Assert
            Assert.That(chat.ObjectId, Is.EqualTo(result.ObjectId));
        }

        [Test]
        public async Task GetChat_ById_ShouldReturnChat()
        {
            ObjectId ownerUserId = ObjectId.GenerateNewId();

            var chat = new ChatModelDto() { UserObjectIds = [ownerUserId] };
            await _repo.CreateChatAsync(ownerUserId, chat);

            // Action
            var result = await _repo.GetChatAsync(chat.ObjectId);

            // Assert
            Assert.That(chat.ObjectId, Is.EqualTo(result.ObjectId));
        }

        [Test]
        public async Task GetChats_ByUserId_ShouldReturnChats()
        {
            ObjectId ownerUserId = ObjectId.GenerateNewId();

            var chat1 = new ChatModelDto() { UserObjectIds = [ownerUserId, ObjectId.GenerateNewId()] };
            var chat2 = new ChatModelDto() { UserObjectIds = [ownerUserId, ObjectId.GenerateNewId()] };
            var chat3 = new ChatModelDto() { UserObjectIds = [ownerUserId, ObjectId.GenerateNewId()] };
            await _repo.CreateChatAsync(ownerUserId, chat1);
            await _repo.CreateChatAsync(ownerUserId, chat2);
            await _repo.CreateChatAsync(ownerUserId, chat3);

            // Action
            var result = await _repo.GetChatsAsync(ownerUserId);

            // Assert
            Assert.That(chat1.ObjectId, Is.AnyOf(result.Select(x => x.ObjectId).ToArray()));
            Assert.That(chat2.ObjectId, Is.AnyOf(result.Select(x => x.ObjectId).ToArray()));
            Assert.That(chat3.ObjectId, Is.AnyOf(result.Select(x => x.ObjectId).ToArray()));
        }

        public void Dispose()
        {
            _runner.Dispose();
        }
    }
}
