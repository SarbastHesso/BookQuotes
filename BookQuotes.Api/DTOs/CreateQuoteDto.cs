using System.ComponentModel.DataAnnotations;

namespace BookQuotes.Api.DTOs;

public class CreateQuoteDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Quote text cannot be empty or whitespace.")]
    public string Text { get; set; } = string.Empty;
}
