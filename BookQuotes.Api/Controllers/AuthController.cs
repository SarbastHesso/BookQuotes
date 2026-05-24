using BookQuotes.Api.Data;
using BookQuotes.Api.DTOs;
using BookQuotes.Api.Models;
using BookQuotes.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace BookQuotes.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    internal const string AuthCookieName = "bookquotes_auth";
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    // POST: api/auth/register
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var normalizedUserName = NormalizeInput(dto.UserName);

        // 1. Check if the username already exists in the database
        if (await _context.Users.AnyAsync(u => u.UserName.ToLower() == normalizedUserName.ToLower()))
            return BadRequest(new { message = "User already exists" });

        // 2. Generate password hash + salt
        CreatePasswordHash(dto.Password, out byte[] hash, out byte[] salt);

        // 3. Create a new User entity
        var user = new User
        {
            UserName = normalizedUserName,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        // 4. Save the new user to the database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User registered successfully" });
    }

    // POST: api/auth/login
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var normalizedUserName = NormalizeInput(dto.UserName);

        // 1. Try to find the user by username
        var user = await _context.Users
        .FirstOrDefaultAsync(u => u.UserName.ToLower() == normalizedUserName.ToLower());
        if (user == null)
            return Unauthorized(new { message = "Invalid username or password" });

        // 2. Verify the password using stored hash + salt
        if (!VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized(new { message = "Invalid username or password" });

        // 3. Generate JWT token
        var token = _tokenService.CreateToken(user);

        Response.Cookies.Append(AuthCookieName, token, BuildAuthCookieOptions());

        // 4. Return user context without exposing the token to browser storage
        return Ok(new
        {
            token,
            userId = user.Id,
            userName = user.UserName
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = User.Identity?.Name;

        if (!int.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(userName))
        {
            return Unauthorized(new { message = "Authentication token is missing or invalid." });
        }

        return Ok(new
        {
            userId,
            userName
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieName, BuildAuthCookieOptions());
        return NoContent();
    }

    // ---------------------------------------------------------
    // Helper methods for password hashing and verification
    // ---------------------------------------------------------

    private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
    {
        // HMACSHA512 automatically generates a secure random key (salt)
        using var hmac = new HMACSHA512();

        salt = hmac.Key; // This is the salt
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Hash(password + salt)
    }

    private bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
    {
        // Recreate the HMAC object using the stored salt
        using var hmac = new HMACSHA512(storedSalt);

        // Compute a new hash from the provided password
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        // Compare the newly computed hash with the stored hash
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }

    private static string NormalizeInput(string value)
    {
        return value.Trim();
    }

    private CookieOptions BuildAuthCookieOptions()
    {
        // Browsers require `SameSite=None` cookies to also be `Secure`.
        // For local (non-HTTPS) development fall back to `Lax` to avoid browsers rejecting the cookie.
        var isHttps = Request.IsHttps;
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/",
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_tokenService.GetExpiryInMinutes())
        };
    }
}
