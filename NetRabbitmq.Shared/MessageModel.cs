using MongoDB.Bson;
using System.Net;

namespace NetRabbitmq.Shared
{
    public class MessageModel
    {
        public ObjectId Id { get; init; }
        public DateTime SendTime { get; init; } = DateTime.Now;
        public string From { get; init; } = Environment.MachineName;
        public string Ip { get; init; } = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
        public string? Body { get; init; }
        public MessageModel(string body)
        {
            Body = body;
        }
    }
}