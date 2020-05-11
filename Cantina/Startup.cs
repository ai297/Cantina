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
            // ��������� ������������
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
            });                                       // ����-�������� http �������.
            services.AddDbContext<DataContext>(options =>
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });                     // �������� ���� ������.
            services.AddTransient<HashService>();                                   // ������ ��� ����������� (�������� ������)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)      // ������ ����������� ������ � �������������� �������.
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;                                       //TODO: �������� false �� true?
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,                                                  // ��������� �� �������� ������.
                        ValidIssuer = Configuration.GetSection("AuthOptions").GetValue<string>("TokenIssuer"), // �������� ��������
                        ValidateAudience = false,                                               // ��������� �� ���������.
                        ValidateLifetime = true,                                                // �������� ������� ����� ������.
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("AuthOptions").GetValue<string>("SecurityKey"))),  // ���� ������������.
                        ValidateIssuerSigningKey = true                                         // �������� ����� ������������.
                    };

                    // ���� ���������� � ����, �� ����� ���� �������� �� ������ �������, ����� �� ���������.
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
                        ValidateIssuer = false,                                                 // ��������� �� �������� ������.
                        ValidateAudience = false,                                               // ��������� �� ���������.
                        ValidateLifetime = true,                                                // �������� ������� ����� ������.
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("AuthOptions").GetValue<string>("ConfirmKey"))),  // ���� ������������.
                        ValidateIssuerSigningKey = true                                         // �������� ����� ������������.
                    };
                });
            services.AddAuthorization(options =>
            {
                // �������� ������� ������ ��� �������
                options.AddPolicy(ChatConstants.AuthPolicy.RequireDeveloperRole, policy =>
                {
                    policy.RequireClaim(ChatConstants.Claims.Role, UserRoles.Developer.ToString());
                });
                // �������� ��� ������� �� ������ ��������� ��������
                options.AddPolicy(ChatConstants.AuthPolicy.ConfirmAccaunt, policy =>
                {
                    policy.AddAuthenticationSchemes(ChatConstants.AuthPolicy.ConfirmAccaunt);
                    policy.RequireClaim(ChatConstants.Claims.Email);
                });
            });                               // ������ ��������� ����������� �������� ����������� � ���������� ������� ������.
            services.AddTransient<TokenGenerator>();                                // ������ ���������� ������ �����������.
            services.AddMemoryCache();                                              // ������ ��� ������ � �����.
            services.AddScoped<UserService>();                                      // ������ ��� ������ � �������
            services.AddSingleton<OnlineUsersService>();                            // C����� ������ ������ ����������� ������.
            services.AddSingleton<MessageService>();                                // ������ �������� �� ������ ��������� � ����� � ���������� ������.
            services.AddHostedService<ChatTimerService>();                          // ������ ��������� ������� ������ � ���� �� �������.
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();         // ��������� User Id ��� ���� signalR
            services.AddSignalR(hubOptions =>                                       // SignalR (��� ����-���� ������ ����������� ����� WebSockets)
            {
                hubOptions.EnableDetailedErrors = true;                             // TODO: �������� �� false;
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(2);         // ���� � ������� 2 ����� ��� ��������� �� ������� - ������� ����������.
                //hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(30);             // ����� �������� ������������� � ����������� �� �����, ���.
                hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(30);            // ������� �������� Ping-���������.
            });
            services.AddMvc();
            services.AddTransient<EmailSender>();                                   // ������ ��� �������� email
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {

            /// ����������� Middleware ///
            if (env.IsDevelopment())
            {
                // ����� ��������� �� �������, ���� ���������� �� ������ ����������.
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

            app.UseRouting();                               // ���������� �������������.

            // ����-�������� �������.
            // TODO: ��������� ��������� �������� ����-�������� ��������
            app.UseCors();

            app.UseAuthentication();                        // ���������� ��������������
            app.UseAuthorization();                         // � �����������.

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();                 // ������������� �� ����������� �� ������ ���������.
                endpoints.MapHub<MainHub>(MainHub.PATH);    // ���, ��������� � ���������� ���������.
            });

        }

    }
}
