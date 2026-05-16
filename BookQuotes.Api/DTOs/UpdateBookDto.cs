using System.ComponentModel.DataAnnotations;

namespace BookQuotes.Api.DTOs;

public class UpdateBookDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Title cannot be empty or whitespace.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Author cannot be empty or whitespace.")]
    public string Author { get; set; } = string.Empty;

    [Required]
    [Range(1450, 2100)]
    public int PublishedYear { get; set; }
}
