using MongoDB.Bson;

namespace NetRabbitmq.Shared
{
    public class MessageModel
    {
        public ObjectId Id { get; init; }
        public DateTime SendTime { get; init; } = DateTime.Now;
        public string From { get; init; } = Environment.MachineName;
        public string? Body { get; init; }
        public MessageModel(string body)
        {
            Body = body;
        }
    }
}