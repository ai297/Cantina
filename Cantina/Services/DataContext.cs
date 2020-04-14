using Cantina.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace Cantina.Services
{
    /// <summary>
    /// Контекст данных для работы с БД чере Entity Framework Core
    /// </summary>
    public class DataContext : DbContext
    {
        private readonly IConfiguration _conf;
        private readonly bool _isDevelopment;

        private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });


        /// <summary>
        /// Аккаунты пользователей
        /// </summary>
        public DbSet<User> Users { get; set; }
        /// <summary>
        /// Профили юзеров
        /// </summary>
        public DbSet<UserProfile> UserProfiles { get; set; }
        /// <summary>
        /// История активности
        /// </summary>
        public DbSet<UserHistory> History { get; set; }
        /// <summary>
        /// Список занятых имён
        /// </summary>
        public DbSet<ForbiddenNames> ForbiddenNames { get; set; }
        /// <summary>
        /// Архив сообщений
        /// </summary>
        public DbSet<ChatMessage> Archive { get; set; }

        public DataContext(DbContextOptions<DataContext> options, IConfiguration configuration, IWebHostEnvironment environment) : base(options)
        {
            _conf = configuration;
            _isDevelopment = environment.IsDevelopment();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_conf.GetConnectionString("Default"));
            if (_isDevelopment) optionsBuilder.UseLoggerFactory(_loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // сохраняем в базе так же приватные поля
            modelBuilder.Entity<User>().Property("password").HasColumnName("Password").IsRequired();
            modelBuilder.Entity<UserProfile>().Property("settings").HasColumnName("Settings");
            // ключевые поля и индексы
            modelBuilder.Entity<User>().HasAlternateKey(user => user.Email);
            modelBuilder.Entity<UserProfile>().HasKey(up => up.UserId);
            modelBuilder.Entity<ForbiddenNames>().HasIndex(fn => fn.Name).IsUnique();
            modelBuilder.Entity<UserProfile>().HasIndex(up => up.Name).IsUnique();
            modelBuilder.Entity<ChatMessage>().HasIndex(archive => archive.DateTime);
        }
    }
}