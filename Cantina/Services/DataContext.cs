using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Cantina.Models;

namespace Cantina.Services
{
    /// <summary>
    /// Контекст данных для работы с БД чере Entity Framework Core
    /// </summary>
    public class DataContext : DbContext
    {
        private IConfiguration conf;
        
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

        public DataContext(DbContextOptions<DataContext> options, IConfiguration configuration) : base(options)
        {
            conf = configuration;
            //Database.EnsureCreated();   // Создаёт базу данных, если её нет
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(conf.GetConnectionString("Default"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // сохраняем в базе так же приватные поля
            modelBuilder.Entity<User>().Property("password").HasColumnName("Password").IsRequired();
            modelBuilder.Entity<UserProfile>().Property("settings").HasColumnName("Settings");
            // ключевые поля
            modelBuilder.Entity<User>().HasAlternateKey(user => user.Email);                // email юзера - дополнительный ключ (уникальное поле)
            modelBuilder.Entity<UserProfile>().HasKey(up => up.UserId);                     // UserId - ключ в таблице профиля
            modelBuilder.Entity<ForbiddenNames>().HasIndex(fn => fn.Name).IsUnique();
            modelBuilder.Entity<UserProfile>().HasIndex(up => up.Name).IsUnique();          // Никнейм в профиле - дополнительный ключ (уникальное значение)
        }
    }
}