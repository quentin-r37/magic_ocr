namespace OcrApp.Server.Models
{
    public class ApiKeyCreateModel
    {
        public required string Name { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
