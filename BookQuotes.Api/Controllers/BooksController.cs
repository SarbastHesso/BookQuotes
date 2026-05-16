using BookQuotes.Api.Data;
using BookQuotes.Api.DTOs;
using BookQuotes.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookQuotes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : BaseController
{
    private readonly AppDbContext _context;

    public BooksController(AppDbContext context)
    {
        _context = context;
    }

    // ---------------------------------------------------------
    // GET ALL BOOKS (Public)
    // ---------------------------------------------------------
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllBooks()
    {
        var books = await _context.Books.ToListAsync();
        return Ok(books);
    }

    // ---------------------------------------------------------
    // GET BOOK BY ID (Public)
    // ---------------------------------------------------------
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBookById(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
            return NotFound(new { message = "Book not found" });

        return Ok(book);
    }

    // ---------------------------------------------------------
    // CREATE BOOK (Protected)
    // ---------------------------------------------------------
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateBook(CreateBookDto dto)
    {
        var normalizedTitle = dto.Title.Trim();
        var normalizedAuthor = dto.Author.Trim();

        // Duplicate check
        var exists = await _context.Books
        .AnyAsync(b => b.Title.ToLower() == normalizedTitle.ToLower() &&
                   b.Author.ToLower() == normalizedAuthor.ToLower());

        if (exists)
            return BadRequest(new { message = "A book with the same title and author already exists." });

        var book = new Book
        {
            Title = normalizedTitle,
            Author = normalizedAuthor,
            PublishedYear = dto.PublishedYear
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
    }

    // ---------------------------------------------------------
    // UPDATE BOOK (Protected)
    // ---------------------------------------------------------
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateBook(int id, UpdateBookDto dto)
    {
        var normalizedTitle = dto.Title.Trim();
        var normalizedAuthor = dto.Author.Trim();

        var book = await _context.Books.FindAsync(id);

        if (book == null)
            return NotFound(new { message = "Book not found" });


        // Duplicate check (exclude current book)
        var exists = await _context.Books
        .AnyAsync(b => b.Id != id &&
                       b.Title.ToLower() == normalizedTitle.ToLower() &&
                       b.Author.ToLower() == normalizedAuthor.ToLower());

        if (exists)
            return BadRequest(new { message = "Another book with the same title and author already exists." });

        book.Title = normalizedTitle;
        book.Author = normalizedAuthor;
        book.PublishedYear = dto.PublishedYear;

        await _context.SaveChangesAsync();

        return Ok(book);
    }

    // ---------------------------------------------------------
    // DELETE BOOK (Protected)
    // ---------------------------------------------------------
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
            return NotFound(new { message = "Book not found" });

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Book deleted" });
    }
}
