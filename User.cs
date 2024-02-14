namespace Hv.Sos100.SingleSignOn;

public class User
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Token { get; set; }
    public DateTime? LastActivity { get; set; }
}