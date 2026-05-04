using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccountingERP.Domain.Interfaces;

namespace AccountingERP.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(
    IConfiguration  configuration,
    IUnitOfWork     unitOfWork,
    IPasswordHasher passwordHasher) : ControllerBase
{
    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token, string Username, string Role, int TenantId, DateTime ExpiresAt);

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody]        LoginRequest request,
        CancellationToken             ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return Unauthorized(new { message = "Korisničko ime i lozinka su obavezni." });

        // Tenant 1 is the default tenant for demo; in multi-tenant setups this
        // would be resolved from the request hostname or a claim.
        const int tenantId = 1;

        var user = await unitOfWork.Users.GetByUsernameAsync(tenantId, request.Username, ct);

        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "Pogrešno korisničko ime ili lozinka" });

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Pogrešno korisničko ime ili lozinka" });

        var token = GenerateToken(user.Username, user.Role.ToString(), tenantId);
        return Ok(token);
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
            issuer:             configuration["Jwt:Issuer"],
            audience:           configuration["Jwt:Audience"],
            claims:             claims,
            expires:            expires,
            signingCredentials: creds);

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            username, role, tenantId, expires);
    }
}
