using Cantina.Controllers;
using Cantina.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;

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
            // настройка конфигурации
            services.Configure<ApiOptions>(Configuration.GetSection("ApiOptions"));
            services.Configure<IntevalsOptions>(Configuration.GetSection("IntevalsOptions"));
            services.Configure<AuthOptions>(Configuration.GetSection("AuthOptions"));
            services.Configure<EmailOptions>(Configuration.GetSection("EmailOptions"));

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.SetIsOriginAllowed(isOriginAllowed: _ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });                                       // Крос-доменные http запросы.
            services.AddDbContext<DataContext>(options =>
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });                     // Контекст базы данных.
            services.AddTransient<HashService>();                                   // Сервис для хэширования (например пароля)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)      // Сервис авторизации юзеров с использованием токенов.
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;                                       //TODO: заменить false на true?
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,                                                  // Проверять ли издателя токена.
                        ValidIssuer = Configuration.GetSection("AuthOptions").GetValue<string>("TokenIssuer"), // Валидный издатель
                        ValidateAudience = false,                                               // Проверять ли аудиторию.
                        ValidateLifetime = true,                                                // Проверка времени жизни токена.
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("AuthOptions").GetValue<string>("SecurityKey"))),  // Ключ безопасности.
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
                })
                .AddJwtBearer(ChatConstants.AuthPolicy.ConfirmAccaunt, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,                                                 // Проверять ли издателя токена.
                        ValidateAudience = false,                                               // Проверять ли аудиторию.
                        ValidateLifetime = true,                                                // Проверка времени жизни токена.
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("AuthOptions").GetValue<string>("ConfirmKey"))),  // Ключ безопасности.
                        ValidateIssuerSigningKey = true                                         // Проверка ключа безопасности.
                    };
                });
            services.AddAuthorization(options =>
            {
                // политика доступа только для админов
                options.AddPolicy(ChatConstants.AuthPolicy.RequireDeveloperRole, policy =>
                {
                    policy.RequireClaim(ChatConstants.Claims.Role, UserRoles.Developer.ToString());
                });
                // политика для доступа по токену активации аккаунта
                options.AddPolicy(ChatConstants.AuthPolicy.ConfirmAccaunt, policy =>
                {
                    policy.AddAuthenticationSchemes(ChatConstants.AuthPolicy.ConfirmAccaunt);
                    policy.RequireClaim(ChatConstants.Claims.Email);
                });
            });                               // Сервис позволяет настраивать политику авторизации с различными правами юзеров.
            services.AddTransient<TokenGenerator>();                                // Сервис генерирует токены авторизации.
            services.AddMemoryCache();                                              // Сервис для работы с кешем.
            services.AddScoped<UserService>();                                      // Сервис для работы с юзерами
            services.AddSingleton<OnlineUsersService>();                            // Cервис хранит список посетителей онлайн.
            services.AddSingleton<MessageService>();                                // Сервис отвечает за список сообщений в вчате и сохранение архива.
            services.AddHostedService<ChatTimerService>();                          // Сервис запускает фоновые задачи в чате по таймеру.
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();         // провайдер User Id для Хаба signalR
            services.AddSignalR(hubOptions =>                                       // SignalR (для реал-тайм обмена сообщениями через WebSockets)
            {
                hubOptions.EnableDetailedErrors = true;                             // TODO: заменить на false;
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(2);         // Если в течении 2 минут нет сообщений от клиента - закрыть соединение.
                //hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(30);             // Время ожидания подтверждения о подключении от юзера, сек.
                hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(30);            // Частота отправки Ping-сообщений.
            });
            services.AddMvc();
            services.AddTransient<EmailSender>();                                   // сервис для отправки email
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {

            /// Настраиваем Middleware ///
            if (env.IsDevelopment())
            {
                // Вывод сообщений об ошибках, если приложение на стадии разработки.
                app.UseDeveloperExceptionPage();
                logger.LogWarning(new EventId(0, "StartUp"), "\n\n Cantina Server started in DEVELOPMENT mode. \n\n");
            }
            else
            {
                var version = Configuration.GetValue<string>("ServerVersion");
                logger.LogInformation($"\n\n Cantina Server started. \n {version}\n\n");
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
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
