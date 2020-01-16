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
        user,
        admin,
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
    /// Варианты размера шрифта в стиле сообщения.
    /// </summary>
    public enum FontSizes : byte
    {
        Small,              // Я-всех-стесняюсь мелкий шрфит.
        Medium,             // Стандартный размер шрифта.
        Large,              // Хочу-быть-заметным крупный шрифт.
    }

    /// <summary>
    /// Статус пользователя в онлайне.
    /// </summary>
    public enum UserOnlineStatus : byte
    {
        Online,             // Юзер в онлайне.
    }
}