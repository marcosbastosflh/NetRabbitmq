namespace NetRabbitmq.Shared
{
    public class MessageModel
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public DateTime SendTime { get; private set; } = DateTime.Now;
        public string From { get; private set; } = Environment.MachineName;
        public string? Body { get; set; }
        public MessageModel(string body)
        {
            Body = body;
        }
    }
}