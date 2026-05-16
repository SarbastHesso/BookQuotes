namespace BookQuotes.Api.Models;

public class User
{
    public int Id { get; set; }

    // Unique username for login
    public string UserName { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    // Navigation: one user → many quotes
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}
