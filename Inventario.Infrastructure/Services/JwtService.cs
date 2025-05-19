using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Inventario.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _key;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            var jwtKey = _configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException("JwtSettings:Secret no está configurado");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }

        public string GenerateJwtToken(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(GetTokenExpirationMinutes());

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("El token no puede estar vacío", nameof(token));
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateLifetime = false // Importante: no validar la expiración aquí
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Validar el token
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            // Verificar que el token sea un JWT y use el algoritmo correcto
            if (securityToken is JwtSecurityToken jwtSecurityToken)
            {
                if (!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Algoritmo de token inválido");
                }
            }
            else
            {
                throw new SecurityTokenException("Token JWT inválido");
            }

            return principal;
        }

        public int GetTokenExpirationMinutes()
        {
            if (int.TryParse(_configuration["JwtSettings:TokenLifetimeMinutes"], out var minutes))
            {
                return minutes > 0 ? minutes : 1440; // Valor por defecto: 1440 minutos (24 horas)
            }
            return 1440; // Valor por defecto si no está configurado
        }
    }
}
