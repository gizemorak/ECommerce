using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Order.Api.Options;
using System.Text;

namespace Order.Api.Extensions;

public static class AuthenticationExtensions
{
 public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
 {
 var jwtOptions = new JwtOptions();
 configuration.GetSection(nameof(JwtOptions)).Bind(jwtOptions);

 services.AddSingleton(jwtOptions);

 var key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

 services
 .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
 options.TokenValidationParameters = new TokenValidationParameters
 {
 ValidateIssuer = true,
 ValidateAudience = true,
 ValidateLifetime = true,
 ValidateIssuerSigningKey = true,
 ValidIssuer = jwtOptions.Issuer,
 ValidAudience = jwtOptions.Audience,
 IssuerSigningKey = new SymmetricSecurityKey(key),
 ClockSkew = TimeSpan.Zero
 };
 });

 return services;
 }
}
