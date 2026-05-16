using BookQuotes.Api.Data;
using BookQuotes.Api.DTOs;
using BookQuotes.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookQuotes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuotesController : BaseController
{
    private readonly AppDbContext _context;

    public QuotesController(AppDbContext context)
    {
        _context = context;
    }

    // ---------------------------------------------------------
    // GET ALL QUOTES (Public)
    // ---------------------------------------------------------
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllQuotes()
    {
        var quotes = await _context.Quotes
            .Include(q => q.User)
            .Select(q => new
            {
                q.Id,
                q.Text,
                q.CreatedAt,
                userName = q.User.UserName
            })
            .ToListAsync();

        return Ok(quotes);
    }

    // ---------------------------------------------------------
    // GET MY QUOTES (Protected)
    // ---------------------------------------------------------
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyQuotes()
    {
        var userId = GetUserId();

        var quotes = await _context.Quotes
            .Where(q => q.UserId == userId)
            .ToListAsync();

        return Ok(quotes);
    }

    // ---------------------------------------------------------
    // GET QUOTE BY ID (Public)
    // ---------------------------------------------------------
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetQuoteById(int id)
    {
        var quote = await _context.Quotes
            .Include(q => q.User)
            .Select(q => new
            {
                q.Id,
                q.Text,
                q.CreatedAt,
                userName = q.User.UserName
            })
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
            return NotFound(new { message = "Quote not found" });

        return Ok(quote);
    }

    // ---------------------------------------------------------
    // CREATE QUOTE (Protected)
    // ---------------------------------------------------------
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateQuote(CreateQuoteDto dto)
    {
        var userId = GetUserId();
        var normalizedText = dto.Text.Trim();

        var quoteCount = await _context.Quotes.CountAsync(q => q.UserId == userId);
        if (quoteCount >= 5)
            return BadRequest(new { message = "You can only save up to 5 quotes." });

        // Duplicate check (case-insensitive)
        var duplicate = await _context.Quotes
            .AnyAsync(q => q.UserId == userId &&
                           q.Text.ToLower() == normalizedText.ToLower());

        if (duplicate)
            return BadRequest(new { message = "You already added this quote." });

        var quote = new Quote
        {
            Text = normalizedText,
            UserId = userId
        };

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuoteById), new { id = quote.Id }, quote);
    }

    // ---------------------------------------------------------
    // UPDATE QUOTE (Protected)
    // ---------------------------------------------------------
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateQuote(int id, UpdateQuoteDto dto)
    {
        var userId = GetUserId();
        var normalizedText = dto.Text.Trim();

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

        if (quote == null)
            return NotFound(new { message = "Quote not found" });

        // Duplicate check (case-insensitive)
        var duplicate = await _context.Quotes
            .AnyAsync(q => q.Id != id &&
                           q.UserId == userId &&
                           q.Text.ToLower() == normalizedText.ToLower());

        if (duplicate)
            return BadRequest(new { message = "You already have a quote with this text." });

        quote.Text = normalizedText;

        await _context.SaveChangesAsync();

        return Ok(quote);
    }

    // ---------------------------------------------------------
    // DELETE QUOTE (Protected)
    // ---------------------------------------------------------
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteQuote(int id)
    {
        var userId = GetUserId();

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

        if (quote == null)
            return NotFound(new { message = "Quote not found" });

        _context.Quotes.Remove(quote);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Quote deleted" });
    }
}
