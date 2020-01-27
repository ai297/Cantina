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
        /// История активности
        /// </summary>
        public DbSet<UserHistory> History { get; set; }

        public DataContext(DbContextOptions<DataContext> options, IConfiguration configuration) : base(options)
        {
            conf = configuration;
            Database.EnsureCreated();   // Создаёт базу данных, если её нет
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(conf.GetConnectionString("Default"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // зависимая сущность - профиль юзера
            modelBuilder.Entity<User>().OwnsOne(user => user.Profile, profile =>
            {
                profile.Property("messageStyle").HasColumnName("Profile_MessageStyle");
            });
            // сохраняем в базе так же приватные поля
            modelBuilder.Entity<User>().Property("passwordHash").HasColumnName("PasswordHash");
            modelBuilder.Entity<User>().Property("salt").HasColumnName("salt");
            modelBuilder.Entity<User>().Property("name").HasColumnName("Name").HasMaxLength(20).IsRequired();

            modelBuilder.Entity<User>().HasAlternateKey(user => user.Email);        // email юзера - дополнительный ключ (уникальное поле)
            modelBuilder.Entity<User>().HasIndex("name").IsUnique();                // никнейм должен быть уникальным
        }
    }
}