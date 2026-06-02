namespace LockedIn.BusinessObject.DTOs.Uploads;

public class FileUploadResponse
{
    public string PublicId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string SecureUrl { get; set; } = string.Empty;
    public long Bytes { get; set; }
    public string Format { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
}
