namespace Cantina.Models
{
    /// <summary>
    /// Таблица с никнеймами, запрещёнными к использованию (включая уже занятые кем-то никнеймы)
    /// </summary>
    public class ForbiddenNames
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
