﻿using Microsoft.Extensions.Configuration;
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
        IConfiguration configuration;

        public TokenGenerator(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Метод генерирует токен авторизации.
        /// </summary>
        public string GetToken(int id, string email, UserRoles role, string agent = null)
        {
            var expires = DateTime.UtcNow.AddHours(AuthOptions.TokenLifetime);                                  // срок действия токена
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["SECURITY_KEY"]));
            // информация о юзере, которая будет содержаться в токене
            var claims = new List<Claim> {
                new Claim(AuthOptions.Claims.ID, id.ToString()),                                                // Id юзера,
                new Claim(AuthOptions.Claims.Email, email),                                                     // email юзера,
                new Claim(AuthOptions.Claims.UserAgent, agent)                                                  // заголовок юзер-агент
            };

            if (role != UserRoles.User) claims.Add(new Claim(AuthOptions.Claims.Role, role.ToString()));        // роль (админ / юзер / бот))

            var accessJWT = new JwtSecurityToken(
                issuer: AuthOptions.Issuer,
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(securityKey, AuthOptions.SecurityAlgorithm)
            );
            return new JwtSecurityTokenHandler().WriteToken(accessJWT);
        }
    }
}