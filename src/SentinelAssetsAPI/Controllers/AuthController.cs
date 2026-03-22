using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// ========================================
// CONTROLLER AUTH - Responsável por gerar tokens JWT de acesso
// Endpoint público (sem [Authorize])
// ========================================
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  // ========================================
  // POST /api/auth/login - GERA TOKEN JWT
  // Mock auth simples: user admin/password (futuro: DB users)
  // Token contém claims, assinado com chave secreta, válido 1h
  // ========================================
  [HttpPost("login")]
  public IActionResult Login([FromBody] LoginModel model)
  {
    // VALIDAÇÃO MOCK - Substitua por service real com hash bcrypt etc.
    if (model.Username != "admin" || model.Password != "password")
      return Unauthorized("Credenciais inválidas");

    // CRIA CLAIM - Identidade do user (claims como "permissões")
    var claims = new[]
    {
            new Claim(ClaimTypes.Name, model.Username)
            // Adicione ClaimTypes.Role = "Admin" para RBAC futuro
        };

    // CHAVE SECRETA - De appsettings.json (mude para produção!)
    var key = Encoding.ASCII.GetBytes("super-secret-key-min-32-chars-long!!!");
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Expires = DateTime.UtcNow.AddHours(1), // Expira em 1h
      SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature),
      Issuer = "SentinelAPI", // Quem emitiu token
      Audience = "SentinelAPI" // Para quem (API)
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Ok(new { Token = tokenString });
  }
}

// DTO - Data Transfer Object para request login (boa prática)
public class LoginModel
{
  public string Username { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
}
