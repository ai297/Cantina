using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис генерирует токен авторизации
    /// </summary>
    public class TokenGenerator
    {
        private readonly IOptions<AuthOptions> _options;

        public TokenGenerator(IOptions<AuthOptions> options)
        {
            _options = options;
        }

        /// <summary>
        /// Метод генерирует токен авторизации.
        /// </summary>
        public string GetAuthToken(int id, string email, UserRoles role, string agent = null)
        {
            var expires = DateTime.UtcNow.AddHours(_options.Value.AuthTokenLifetime);   // срок действия токена
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SecurityKey));
            // информация о юзере, которая будет содержаться в токене
            var claims = new List<Claim> {
                new Claim(ChatConstants.Claims.ID, id.ToString()),                                                // Id юзера,
                new Claim(ChatConstants.Claims.Email, email),                                                     // email юзера,
                new Claim(ChatConstants.Claims.UserAgent, agent)                                                  // заголовок юзер-агент
            };

            if (role != UserRoles.User) claims.Add(new Claim(ChatConstants.Claims.Role, role.ToString()));        // роль (админ / юзер / бот))

            var accessJWT = new JwtSecurityToken(
                issuer: _options.Value.TokenIssuer,
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(securityKey, ChatConstants.SecurityAlgorithm)
            );
            return new JwtSecurityTokenHandler().WriteToken(accessJWT);
        }


        /// <summary>
        /// Токен для активации аккаунта
        /// </summary>
        public string GetActivationToken(string email)
        {
            var expires = DateTime.UtcNow.AddDays(_options.Value.ActivationTokenLifetime);                        // срок действия токена, дней
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.ConfirmKey));
            // информация о юзере, которая будет содержаться в токене
            var claims = new List<Claim> {
                new Claim(ChatConstants.Claims.Email, email),                                                     // email юзера,
            };

            var accessJWT = new JwtSecurityToken(
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(securityKey, ChatConstants.SecurityAlgorithm)
            );
            return new JwtSecurityTokenHandler().WriteToken(accessJWT);
        }
    }
}