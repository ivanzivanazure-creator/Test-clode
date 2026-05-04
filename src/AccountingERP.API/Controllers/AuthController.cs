using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AccountingERP.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IConfiguration configuration) : ControllerBase
{
    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token, string Username, string Role, int TenantId, DateTime ExpiresAt);

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Demo authentication — replace with real DB lookup + BCrypt verify
        if (request.Username == "admin" && request.Password == "Admin123!")
        {
            var token = GenerateToken("admin", "Admin", tenantId: 1);
            return Ok(token);
        }
        return Unauthorized(new { message = "Pogrešno korisničko ime ili lozinka" });
    }

    private LoginResponse GenerateToken(string username, string role, int tenantId)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:   configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims:   claims,
            expires:  expires,
            signingCredentials: creds);

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            username, role, tenantId, expires);
    }
}
