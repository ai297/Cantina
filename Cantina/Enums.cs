﻿namespace Cantina
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
        User = 10,
        Admin = 50,
        Bot = 100,
    }

    /// <summary>
    /// Тип действия юзера, которое сохраняется в историю действий.
    /// </summary>
    public enum ActivityTypes : byte
    {
        Register,           // Регистрация аккаунта.
        ChangeEmail,        // Смена e-mail'a.
        ChangeName,         // Смена никнейма.
        Activation,         // Активация аккаунта.
        Visit = 100,        // Посещение чата.
    }


    /// <summary>
    /// Статус пользователя в онлайне.
    /// </summary>
    public enum UserOnlineStatus : byte
    {
        Hidden,             // Невидим.
        Online,             // Юзер в онлайне.
        Absentee,           // Отошедший
    }

    /// <summary>
    /// Типы сообщений
    /// </summary>
    public enum MessageTypes: byte
    {
        System,
        Base,
        Privat,
        ThirdPerson,
    }
}