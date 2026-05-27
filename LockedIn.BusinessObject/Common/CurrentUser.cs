namespace LockedIn.BusinessObject.Common;

public class CurrentUser
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
}
