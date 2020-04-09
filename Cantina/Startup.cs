using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
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
            });                                       // ����-�������� http �������.
            services.AddDbContext<DataContext>(options => {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });                     // �������� ���� ������.
            services.AddTransient<HashService>();                                   // ������ ��� ����������� (�������� ������)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)      // ������ ����������� ������ � �������������� �������.
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;   //TODO: �������� false �� true
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,                                                  // ��������� �� �������� ������.
                        ValidIssuer = AuthOptions.Issuer,                                       // �������� ��������
                        ValidateAudience = false,                                               // ��������� �� ���������.
                        ValidateLifetime = true,                                                // �������� ������� ����� ������.
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SECURITY_KEY"])),  // ���� ������������.
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
            //services.AddMemoryCache();                                              // ������ ��� ������ � �����.
            services.AddScoped<UsersHistoryService>();                           // ������ ��� ������ � ������� �������� ������.
            services.AddScoped<UserService>();                                   // ������ ��� ������ � �������
            services.AddSingleton<OnlineService>();                                 // C����� ������ ������ ����������� ������.
            services.AddHostedService<OnlineUsersMonitor>();                        // ������ ������ �� ��������� ������ � ����.
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();         // ��������� User Id ��� ���� signalR
            services.AddSignalR(hubOptions =>                                       // SignalR (��� ����-���� ������ ����������� ����� WebSockets)
            {
                hubOptions.EnableDetailedErrors = true;                         // TODO: �������� �� false;
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(1);    // ���� � ������� 10 ����� ��� ��������� �� ������� - ������� ����������.
                //hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(30);         // ����� �������� ������������� � ����������� �� �����, ���.
                hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(30);         // ������� �������� Ping-���������.
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
                logger.LogWarning(new EventId(0, "StartUp"), "Cantina Server started in development mode.");
            }
            else
            {
                // ������������� �� https.
                app.UseHsts();
                app.UseHttpsRedirection();
                logger.LogInformation("Cantina Server is starting.");
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
