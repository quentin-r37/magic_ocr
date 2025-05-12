namespace OcrApp.Server.Db;

public class ApiKey
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Key { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}