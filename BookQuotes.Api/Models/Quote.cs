namespace BookQuotes.Api.Models;

public class Quote
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;

    // Timestamp when the quote was created
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
