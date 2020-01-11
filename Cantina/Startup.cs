using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Cantina.Controllers;
using Cantina.Services;
using Cantina.Models;

namespace Cantina
{
    public class Startup
    {
        IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            // Контекст базы данных.
            services.AddDbContext<DataContext>();
            
            // Сервис возвращает хэш-сумму для пароля. Просто для удобства. используется при регистрации и авторизации.
            services.AddTransient<IHashService, HashPasswordService>();
            
            // Сервис авторизации юзеров с использованием токенов.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;   //TODO: заменить false на true
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Проверять ли издателя токена.
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.Issuer,
                    // Проверять ли аудиторию.
                    ValidateAudience = false,
                    // Проверка времени жизни токена.
                    ValidateLifetime = true,
                    // Ключ безопасности.
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(Configuration),
                    // Проверка ключа безопасности.
                    ValidateIssuerSigningKey = true
                };
            });

            // Сервис позволяет настраивать политику авторизации с различными правами юзеров.
            services.AddAuthorization(options =>
            {
                // Политика для авторизации по рефреш-токену. Используется в контроллере авторизации для обновления access-токена.
                options.AddPolicy(AuthOptions.ClaimUA, policy => policy.RequireClaim(AuthOptions.ClaimUA));
            });

            // Сервис кеширования.
            services.AddMemoryCache();
            // Сервис для работы с юзерами с использованием кеша.
            services.AddTransient<ICacheService<User>, UserCacheService>();

            // Используем контроллеры из архитектуры MVC (без View).
            services.AddControllers();

            // SignalR (для реал-тайм обмена сообщениями через WebSockets)
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
                /// Настраиваем Middleware ///
            
            if (env.IsDevelopment())
            {
                // Вывод сообщений об ошибках, если приложение на стадии разработки.
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Переадресация на https.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseRouting();                               // Подключаем маршрутизацию.
            app.UseAuthentication();                        // Используем аутентификацию
            app.UseAuthorization();                         // и авторизацию.
            app.UseFileServer();                            // Используем статические файлы (вэб-клиент).

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();                 // Маршрутизация на контроллеры на основе атрибутов.
                endpoints.MapHub<MainHub>("/hub/main");    // Хаб, принимает и пересылает сообщения.
            });

        }

    }
}
