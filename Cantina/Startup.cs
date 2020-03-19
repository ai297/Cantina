using System;
using System.Text;
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
using Microsoft.AspNetCore.SignalR;

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
            services.AddCors(options => {
                options.AddDefaultPolicy(builder => {
                    builder.WithOrigins(Configuration.GetSection("CorsOrigins").Get<string[]>())
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });                                       // Крос-доменные http запросы.
            services.AddDbContext<DataContext>();                                   // Контекст базы данных.
            services.AddTransient<HashService>();                                   // Сервис для хэширования (например пароля)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)      // Сервис авторизации юзеров с использованием токенов.
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;   //TODO: заменить false на true
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,                                                  // Проверять ли издателя токена.
                        ValidIssuer = AuthOptions.Issuer,                                       // Валидный издатель
                        ValidateAudience = false,                                               // Проверять ли аудиторию.
                        ValidateLifetime = true,                                                // Проверка времени жизни токена.
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SECURITY_KEY"])),  // Ключ безопасности.
                        ValidateIssuerSigningKey = true                                         // Проверка ключа безопасности.
                    };

                    // Если обращаемся к хабу, то токен надо получать из строки запроса, иначе из заголовка.
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(MainHub.PATH))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddAuthorization(options =>
            {
                // политика доступа только для админов
                options.AddPolicy(AuthOptions.AuthPolicy.RequireAdminRole, policy =>
                {
                    policy.RequireClaim(AuthOptions.Claims.Role, UserRoles.Admin.ToString());
                });
            });                               // Сервис позволяет настраивать политику авторизации с различными правами юзеров.
            services.AddTransient<TokenGenerator>();                                // Сервис генерирует токены авторизации.
            //services.AddMemoryCache();                                              // Сервис для работы с кешем.
            services.AddTransient<UsersHistoryService>();                           // Сервис для работы с историй действий юзеров.
            services.AddTransient<UserService>();                                   // Сервис для работы с юзерами
            services.AddSingleton<OnlineService>();                                 // Cервис хранит список посетителей онлайн.
            services.AddSingleton<MessagesService>();                               // Сервис обрабатывает сообщения в чате и сохраняет их в архив.
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();         // провайдер User Id для Хаба signalR
            services.AddSignalR(hubOptions =>                                       // SignalR (для реал-тайм обмена сообщениями через WebSockets)
            {
                hubOptions.EnableDetailedErrors = true;                         // TODO: заменить на false;
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(10);    // Если в течении 10 минут нет сообщений от клиента - закрыть соединение.
                hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(30);         // Время ожидания подтверждения о подключении от юзера, сек.
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(5);         // Частота отправки Ping-сообщений.
            });
            services.AddControllers();                                              // Используем контроллеры из архитектуры MVC (без View).
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

            // крос-доменные запросы.
            // TODO: Безопасно настроить политику крос-доменных запросов
            app.UseCors();

            app.UseAuthentication();                        // Используем аутентификацию
            app.UseAuthorization();                         // и авторизацию.

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();                 // Маршрутизация на контроллеры на основе атрибутов.
                endpoints.MapHub<MainHub>(MainHub.PATH);    // Хаб, принимает и пересылает сообщения.
            });

        }

    }
}
