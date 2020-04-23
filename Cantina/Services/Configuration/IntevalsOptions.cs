namespace Cantina.Services
{
    public class IntevalsOptions
    {
        /// <summary>
        /// Период сохранения сообщений в архив, минуты
        /// </summary>
        public int ArchiveSaving { get; set; } = 5;

        /// <summary>
        /// Частота проверки состояния юзеров в онлайне, минуты
        /// </summary>
        public int OnlineUsersCheck { get; set; } = 4;

        /// <summary>
        /// Время неактивности, превысив которое юзер переводится в статус "не активен".
        /// Должен быть больше OnlineUsersCheck, т.к. проверяется с этим интервалом
        /// </summary>
        public int InactivityTime { get; set; } = 15;

        /// <summary>
        /// "Скользящее" время жизни кеша юзеров
        /// </summary>
        public int UserCacheTime { get; set; } = 30;

        public IntevalsOptions() { }
    }
}
