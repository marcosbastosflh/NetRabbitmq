namespace NetRabbitmq.Worker.Models
{
    public class MessageDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
}
