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
                        ValidIssuer = AuthOptions.Issuer,                                       // �������� ��������
                        ValidateAudience = false,                                               // ��������� �� ���������.
                        ValidateLifetime = true,                                                // �������� ������� ����� ������.
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("SECURITY_KEY"))),  // ���� ������������.
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
                });
            services.AddAuthorization(options =>
            {
                // �������� ������� ������ ��� �������
                options.AddPolicy(AuthOptions.AuthPolicy.RequireAdminRole, policy =>
                {
                    policy.RequireClaim(AuthOptions.Claims.Role, UserRoles.Admin.ToString());
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
