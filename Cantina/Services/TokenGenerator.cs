using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Cantina.Models;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис генерирует токены авторизации
    /// </summary>
    public class TokenGenerator
    {
        IHashService hashService;
        IConfiguration configuration;

        public TokenGenerator(IHashService hashService, IConfiguration configuration)
        {
            this.hashService = hashService;
            this.configuration = configuration;
        }

        /// <summary>
        /// Метод генерирует токены авторизации.
        /// </summary>
        public TokenResponse GetTokenResponse(User user, string agent = null)
        {
            // генерируем access-токен
            var expires = DateTime.UtcNow.AddMinutes(AuthOptions.TokenLifetime);    // срок действия токена

            var token = new JwtSecurityToken(
                issuer: AuthOptions.Issuer,
                claims: new Claim[]
                {
                    new Claim(AuthOptions.ClaimID, user.Id.ToString()),                                 // токен хранит Id юзера,
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),                          // имя юзера,
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.ToString())                // роль (админ / юзер / бот)
                },
                expires: expires,
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(configuration), AuthOptions.SecurityAlgorithm)
            );
            var accessJWT = new JwtSecurityTokenHandler().WriteToken(token);

            // генерируем refresh-токен
            var refreshExpires = DateTime.UtcNow.AddHours(AuthOptions.RefreshLifetime);
            var ClaimUAValue = hashService.SimpleHash(agent);
            token = new JwtSecurityToken(
                issuer: AuthOptions.Issuer,
                claims: new Claim[]
                {
                    new Claim(AuthOptions.ClaimID, user.Id.ToString()),                 // записываем в рефреш-токен Id юзера
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),         // email в качестве username
                    new Claim(AuthOptions.ClaimUA, ClaimUAValue)                        // хэш заголовка юзер-агента (данный клэйм отличает обычный токен от рефреш-токена
                },
                expires: refreshExpires,
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(configuration), AuthOptions.SecurityAlgorithm)
            );
            var refreshJWT = new JwtSecurityTokenHandler().WriteToken(token);

            // Формируем ответ
            return new TokenResponse
            {
                UserId = user.Id,
                AccessToken = accessJWT,
                AccessExpires = expires,
                RefreshToken = refreshJWT,
                RefreshExpires = refreshExpires
            };
        }
    }
}