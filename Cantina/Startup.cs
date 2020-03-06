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
            services.AddCors();                                                     // ����-�������� http �������.
            services.AddDbContext<DataContext>();                                   // �������� ���� ������.
            services.AddTransient<IHashService, HashPasswordService>();             // ������ ���������� ���-����� ��� ������.
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
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(Configuration),  // ���� ������������.
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
            services.AddAuthorization(options =>                                    // ������ ��������� ����������� �������� ����������� � ���������� ������� ������.
            {
                // �������� ��� ����������� �� ������-������. ������������ � ����������� ����������� ��� ���������� access-������.
                options.AddPolicy(AuthOptions.ClaimUA, policy => policy.RequireClaim(AuthOptions.ClaimUA));
            });
            services.AddTransient<TokenGenerator>();                                // ������ ���������� ������ �����������.
            services.AddMemoryCache();                                              // ������ ��� ������ � �����.
            services.AddTransient<UsersHistoryService>();                           // ������ ��� ������ � ������� �������� ������.
            services.AddTransient<UserService>();                                   // ������ ��� ������ � �������
            services.AddSingleton<UsersOnlineService>();                            // ���� ������ ������ ������ ����������� ������.
            services.AddScoped<ConnectionService>();                                // ������ ������������ ����������� � ���������� ������.
            services.AddSingleton<MessagesService>();                               // ������ ������������ ��������� � ���� � ��������� �� � �����.
            services.AddSignalR(hubOptions =>                                       // SignalR (��� ����-���� ������ ����������� ����� WebSockets)
            {
                hubOptions.EnableDetailedErrors = true;                         // TODO: �������� �� false;
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(10);    // ���� � ������� 10 ����� ��� ��������� �� ������� - ������� ����������.
                hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(30);         // ����� �������� ������������� � ����������� �� �����, ���.
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(5);         // ������� �������� Ping-���������.
            });
            services.AddControllers();                                              // ���������� ����������� �� ����������� MVC (��� View).
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
                /// ����������� Middleware ///
            
            if (env.IsDevelopment())
            {
                // ����� ��������� �� �������, ���� ���������� �� ������ ����������.
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // ������������� �� https.
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseRouting();                               // ���������� �������������.

            // ����-�������� �������.
            // TODO: ��������� ��������� �������� ����-�������� ��������
            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:8080")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                );

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
