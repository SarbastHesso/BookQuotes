using BookQuotes.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookQuotes.Api.Services;

public class TokenService
{
    private const string DevelopmentFallbackJwtKey = "BookQuotes_Dev_Test_Key_ChangeMe";
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(User user)
    {
        var keyString = ResolveJwtKey();
        var issuer = _config["Jwt:Issuer"] ?? "BookQuotesApi";
        var audience = _config["Jwt:Audience"] ?? "BookQuotesClient";

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
            return 60;

        return Convert.ToDouble(expiresIn);
    }

    private string ResolveJwtKey()
    {
        return _config["Jwt:Key"]
            ?? _config["STAGING_JWT_KEY"]
            ?? Environment.GetEnvironmentVariable("STAGING_JWT_KEY")
            ?? DevelopmentFallbackJwtKey;
    }
}
