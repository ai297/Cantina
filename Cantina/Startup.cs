using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Cantina.Models;
using Cantina.Controllers;
using Cantina.Services;
using System.Text;

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
            // �������� ���� ������
            services.AddDbContext<DataContext>();
            
            // ������ ���������� ���-����� ��� ������. ������ ��� ��������. ������������ ��� ����������� � �����������
            services.AddTransient<IHashService, HashPasswordService>();
            
            // ������ ����������� ������ � �������������� �������
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;   //TODO: �������� false �� true
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // ��������� �� �������� ������
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.Issuer,
                    // ��������� �� ���������
                    ValidateAudience = false,
                    // �������� ������� ����� ������
                    ValidateLifetime = true,
                    // ���� ������������
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(Configuration),
                    // �������� ����� ������������
                    ValidateIssuerSigningKey = true
                };
            });

            // ������ ��������� ����������� �������� ����������� � ���������� ������� ������
            services.AddAuthorization(options =>
            {
                // �������� ��� ����������� �� ������-������. ������������ � ����������� ����������� ��� ���������� access-������
                options.AddPolicy(AuthOptions.ClaimUA, policy => policy.RequireClaim(AuthOptions.ClaimUA));
            });

            // ���������� ����������� �� ����������� MVC (��� View)
            services.AddControllers();

            // SignalR (��� ����-���� ������ ����������� ����� WebSockets)
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
                /// ����������� Middleware ///
            
            if (env.IsDevelopment())
            {
                // ����� ��������� �� �������, ���� ���������� �� ������ ����������
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // ����� ������������� �� https
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseRouting();                               // ���������� �������������
            app.UseAuthentication();                        // ���������� ��������������
            app.UseAuthorization();                         // � �����������
            app.UseFileServer();                            // ���������� ����������� ����� (���-������)

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();                 // ������������� �� ����������� �� ������ ���������
                endpoints.MapHub<MainHub>("/hub/main");    // ���, ��������� � ���������� ���������
            });

        }

    }
}
