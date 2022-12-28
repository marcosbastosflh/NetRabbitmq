namespace NetRabbitmq.Shared
{
    public class MessageModel
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime SendTime { get; init; } = DateTime.Now;
        public string From { get; init; } = Environment.MachineName;
        public string? Body { get; init; }
        public MessageModel(string body)
        {
            Body = body;
        }
    }
}