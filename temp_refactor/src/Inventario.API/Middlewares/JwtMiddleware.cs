using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Inventario.Core.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Inventario.API.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtSettings _jwtSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtSettings)
        {
            _next = next;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task Invoke(HttpContext context, ICurrentUserService currentUserService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await AttachUserToContext(context, currentUserService, token);

            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, ICurrentUserService currentUserService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // Set clockskew to zero so tokens expire exactly at token expiration time
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "id").Value;
                var userRoles = jwtToken.Claims
                    .Where(x => x.Type == ClaimTypes.Role)
                    .Select(x => x.Value)
                    .ToList();

                // Attach user to context on successful JWT validation
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, jwtToken.Claims.First(x => x.Type == "username").Value),
                    new Claim(ClaimTypes.Email, jwtToken.Claims.First(x => x.Type == "email").Value)
                };

                // Add roles to the claims
                claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

                var identity = new ClaimsIdentity(claims);
                var principal = new ClaimsPrincipal(identity);

                context.User = principal;
            }
            catch
            {
                // Token validation failed, do not attach user to context
                // The user is not attached to the context, so the request will not have access to secure routes
            }
        }
    }

    public static class JwtMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtMiddleware>();
        }
    }

    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpireMinutes { get; set; } = 60;
        public int RefreshTokenExpireDays { get; set; } = 7;
    }
}
