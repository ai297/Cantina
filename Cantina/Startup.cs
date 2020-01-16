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
            // �������� ���� ������.
            services.AddDbContext<DataContext>();
            
            // ������ ���������� ���-����� ��� ������. ������ ��� ��������. ������������ ��� ����������� � �����������.
            services.AddTransient<IHashService, HashPasswordService>();
            
            // ������ ����������� ������ � �������������� �������.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;   //TODO: �������� false �� true
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // ��������� �� �������� ������.
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.Issuer,
                    // ��������� �� ���������.
                    ValidateAudience = false,
                    // �������� ������� ����� ������.
                    ValidateLifetime = true,
                    // ���� ������������.
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(Configuration),
                    // �������� ����� ������������.
                    ValidateIssuerSigningKey = true
                };
                
                // ���� ���������� � ����, �� ����� ���� �������� �� ������ �������, ����� �� ���������.
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

            // ������ ��������� ����������� �������� ����������� � ���������� ������� ������.
            services.AddAuthorization(options =>
            {
                // �������� ��� ����������� �� ������-������. ������������ � ����������� ����������� ��� ���������� access-������.
                options.AddPolicy(AuthOptions.ClaimUA, policy => policy.RequireClaim(AuthOptions.ClaimUA));
            });

            // ������ �����������.
            services.AddMemoryCache();
            // ������ ��� ������ � ������� � �������������� ����.
            services.AddTransient<ICacheService<User>, UserCacheService>();

            // ���� ������ ������ ������ ����������� ������.
            services.AddSingleton<UsersOnlineService>();

            // SignalR (��� ����-���� ������ ����������� ����� WebSockets)
            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true; // TODO: �������� �� false;
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(5); // ���� � ������� 5 ����� ��� ��������� �� ������� - ������� ����������.
                hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(30);     // ����� �������� ������������� � ����������� �� �����.
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(2);     // ������� �������� Ping-���������.
            });

            // ���������� ����������� �� ����������� MVC (��� View).
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
                /// ����������� Middleware ///
            
            if (env.IsDevelopment())
            {
                // ����� ��������� �� �������, ���� ���������� �� ������ ����������.
                app.UseDeveloperExceptionPage();
                // ����-�������� �������.
                // TODO: ��������� ��������� �������� ����-�������� ��������
                app.UseCors(builder => 
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    );
            }
            else
            {
                // ������������� �� https.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseRouting();                               // ���������� �������������.
            app.UseAuthentication();                        // ���������� ��������������
            app.UseAuthorization();                         // � �����������.
            app.UseFileServer();                            // ���������� ����������� ����� (���-������).

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();                 // ������������� �� ����������� �� ������ ���������.
                endpoints.MapHub<MainHub>(MainHub.path);    // ���, ��������� � ���������� ���������.
            });

        }

    }
}
