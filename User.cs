namespace Hv.Sos100.SingleSignOn;

internal class User
{
    internal int Id { get; set; }
    internal string? Username { get; set; }
    internal string? Password { get; set; }
    internal string? Token { get; set; }
    internal DateTime? LastActivity { get; set; }
}