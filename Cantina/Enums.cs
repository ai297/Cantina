namespace Cantina
{
    /// <summary>
    /// Пол юзера.
    /// </summary>
    public enum Gender : byte
    {
        Uncertain,          // Не определился.
        Male,               // Мужской.
        Female              // Женский.
    }

    /// <summary>
    /// Роли юзеров.
    /// </summary>
    public enum UserRoles : byte
    {
        guest,
        user,
        admin,
        bot,
    }

    /// <summary>
    /// Тип действия юзера, которое сохраняется в историю действий.
    /// </summary>
    public enum ActivityTypes : byte
    {
        Register,           // Регистрация аккаунта.
        Visit,              // Посещение чата.
        ChangeName,         // Смена никнейма.
    }


    /// <summary>
    /// Статус пользователя в онлайне.
    /// </summary>
    public enum UserOnlineStatus : byte
    {
        Hidden,             // Невидим.
        Offline,            // Юзер не в сети.
        Online,             // Юзер в онлайне.
    }

    /// <summary>
    /// Типы сообщений.
    /// </summary>
    public enum MessageTypes : byte
    {
        System,         // Системное уведомление.
        Base,           // Обычное сообщение, которое видят все.
        Privat,         // Личное сообщение, которое видят только отправитель и получатель.
    }
}