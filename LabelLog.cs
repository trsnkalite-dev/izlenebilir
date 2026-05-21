namespace Kalite.API.Entitity
{
    public class LabelLog
    {
        public int Id { get; set; }
        public int LabelId { get; set; }

        public string? Action { get; set; } // CREATE, TRANSFER, EXIT
        public string? Description { get; set; }

        public DateTime? LogDate { get; set; }
        public string? User { get; set; }
    }
}
