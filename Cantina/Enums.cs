namespace Cantina
{
    /// <summary>
    /// Пол юзера
    /// </summary>
    public enum Gender : byte
    {
        Uncertain,          // не определился
        Male,               // Мужской
        Female              // Женский
    }

    /// <summary>
    /// Роли юзеров
    /// </summary>
    public enum UserRoles : byte
    {
        user,
        admin,
    }

    /// <summary>
    /// Тип действия юзера, которое сохраняется в историю действий
    /// </summary>
    public enum ActivityTypes : byte
    {
        Register,           // Регистрация аккаунта
        Visit,              // Посещение чата
        ChangeName,         // Смена никнейма
        Ban,                // Бан пользователя
    }

    /// <summary>
    /// Типы сообщений
    /// </summary>
    public enum MessageTypes : byte
    {
        SystemMessage,              // Системное сообщение (например уведомление, что кто-то вошёл/вышел
        BaseMessage,                // Обычное сообщение
        PrivatMessage,              // Приватное сообщение
    }

}