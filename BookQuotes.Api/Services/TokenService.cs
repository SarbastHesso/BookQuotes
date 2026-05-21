using BookQuotes.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookQuotes.Api.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(User user)
    {
        // Validate config values
        var keyString = _config["Jwt:Key"];
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var expiresIn = _config["Jwt:ExpiresInMinutes"];

        if (string.IsNullOrWhiteSpace(keyString))
            throw new Exception("JWT Key is missing in configuration.");
        if (string.IsNullOrWhiteSpace(issuer))
            throw new Exception("JWT Issuer is missing in configuration.");
        if (string.IsNullOrWhiteSpace(audience))
            throw new Exception("JWT Audience is missing in configuration.");
        var expiresInMinutes = GetExpiryInMinutes();

        // 1. Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        // 2. Key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

        // 3. Credentials
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 4. Token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        // 5. Create token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public double GetExpiryInMinutes()
    {
        var expiresIn = _config["Jwt:ExpiresInMinutes"];

        if (string.IsNullOrWhiteSpace(expiresIn))
            throw new Exception("JWT expiration time is missing in configuration.");

        return Convert.ToDouble(expiresIn);
    }
}
