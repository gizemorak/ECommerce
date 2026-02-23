using Microsoft.IdentityModel.Tokens;
using Order.Api.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Order.Api.Services;

public interface ITokenService
{
 string GenerateToken(string userId, string email, string[] roles);
}

public class TokenService : ITokenService
{
 private readonly JwtOptions _jwtOptions;

 public TokenService(JwtOptions jwtOptions)
 {
 _jwtOptions = jwtOptions;
 }

 public string GenerateToken(string userId, string email, string[] roles)
 {
 var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);
 var securityKey = new SymmetricSecurityKey(key);
 var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

 var claims = new List<Claim>
 {
 new Claim(ClaimTypes.NameIdentifier, userId),
 new Claim(ClaimTypes.Email, email)
 };

 foreach (var role in roles)
 {
 claims.Add(new Claim(ClaimTypes.Role, role));
 }

 var token = new JwtSecurityToken(
 issuer: _jwtOptions.Issuer,
 audience: _jwtOptions.Audience,
 claims: claims,
 expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes),
 signingCredentials: credentials
 );

 return new JwtSecurityTokenHandler().WriteToken(token);
 }
}
