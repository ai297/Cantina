using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
            services.AddCors();
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
                
                // Если обращаемся к хабу, то токен надо получать из строки запроса, иначе из заголовка.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(MainHub.path))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
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

            // Этот сервис хранит список посетителей онлайн.
            services.AddSingleton<UsersOnlineService>();

            // SignalR (для реал-тайм обмена сообщениями через WebSockets)
            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true; // TODO: заменить на false;
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(5); // Если в течении 5 минут нет сообщений от клиента - закрыть соединение.
                hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(30);     // Время ожидания подтверждения о подключении от юзера.
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(2);     // Частота отправки Ping-сообщений.
            });

            // Используем контроллеры из архитектуры MVC (без View).
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
                /// Настраиваем Middleware ///
            
            if (env.IsDevelopment())
            {
                // Вывод сообщений об ошибках, если приложение на стадии разработки.
                app.UseDeveloperExceptionPage();
                // крос-доменные запросы.
                // TODO: Безопасно настроить политику крос-доменных запросов
                app.UseCors(builder => 
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    );
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
                endpoints.MapHub<MainHub>(MainHub.path);    // Хаб, принимает и пересылает сообщения.
            });

        }

    }
}
