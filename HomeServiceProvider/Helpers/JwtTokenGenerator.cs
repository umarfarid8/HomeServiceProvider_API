using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HomeServiceProvider.DataAccess.Entities;
using Microsoft.IdentityModel.Tokens;

namespace HomeServiceProvider.Helpers;

public class JwtTokenGenerator
{
    private readonly IConfiguration _config;

    public JwtTokenGenerator(IConfiguration config) => _config = config;

    public (string token, DateTime expiry) GenerateToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddDays(
            int.Parse(jwtSettings["ExpiryDays"]!));

        var claims = new[]
        {
            // NameIdentifier carries the User.Id — extracted in controllers via extension method
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("isEmailVerified", user.IsEmailVerified.ToString().ToLower())
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(tokenDescriptor), expiry);
    }
}