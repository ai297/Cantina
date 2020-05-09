using Cantina.Controllers;
using Cantina.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис хранит список юзеров онлайн.
    /// </summary>
    public class OnlineUsersService
    {
        // сервисы
        private readonly IServiceProvider _services;
        private readonly ILogger<OnlineUsersService> _logger;
        private readonly IHubContext<MainHub, IChatClient> _chatHub;
        private readonly bool _isDevelopMode;
        private readonly MessageService _messageService;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheOptions; // время хранения данных в кеше

        private readonly TimeSpan _offlineInterval;
        private readonly TimeSpan _inactivityTime;


        // список юзеров в онлайне
        private Dictionary<int, OnlineSession> OnlineUsers = new Dictionary<int, OnlineSession>(30);
        // блокировщик для изменений списка юзеров
        private object _locker = new object();

        public OnlineUsersService(IServiceProvider serviceProvider, ILogger<OnlineUsersService> logger,
            MessageService messageService, IHubContext<MainHub, IChatClient> hub, IWebHostEnvironment env, IMemoryCache cache, IOptions<IntevalsOptions> options)
        {
            _services = serviceProvider;
            _logger = logger;
            _chatHub = hub;
            _messageService = messageService;
            _isDevelopMode = env.IsDevelopment();
            _memoryCache = cache;
            _cacheOptions = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(options.Value.UserCacheTime) };
            _offlineInterval = TimeSpan.FromMinutes(options.Value.OnlineUsersCheck);
            _inactivityTime = TimeSpan.FromMinutes(options.Value.InactivityTime);
        }

        /// <summary>
        /// Подключение нового клиента
        /// </summary>
        public async Task AddUser(int userId, string connectionId)
        {
            // Если юзера нет в списке онлайн - добавляем
            if (!OnlineUsers.ContainsKey(userId))
            {
                using var scope = _services.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var userProfile = userService.GetUserProfile(userId);
                lock (_locker)
                {
                    OnlineUsers.Add(userId, new OnlineSession(connectionId, userProfile));
                }

                if (_isDevelopMode) _logger.LogInformation("User '{0}' connected to chat", userProfile.Name);

                // рассылаем сообщение о входе
                var enterMessage = new ChatMessage
                {
                    AuthorId = 0,
                    AuthorName = userProfile.Name,
                    DateTime = DateTime.UtcNow,
                    Type = MessageTypes.System,
                    Text = "В Кантину заходит <author />."
                };
                _messageService.AddMessage(enterMessage);
                await _chatHub.Clients.AllExcept(connectionId).ReceiveMessage(enterMessage);
                await _chatHub.Clients.AllExcept(connectionId).AddUserToOnlineList(OnlineUsers[userId]);

            }
            // если юзер в списке уже есть - добавляем соединение
            else
            {
                lock (_locker)
                {
                    OnlineUsers[userId].AddConnection(connectionId);
                    if (OnlineUsers[userId].Connections == 1 && OnlineUsers[userId].Status == UserOnlineStatus.Online) OnlineUsers[userId].EnterTime = DateTime.UtcNow;
                }
                await _chatHub.Clients.All.AddUserToOnlineList(OnlineUsers[userId]);

                if (_isDevelopMode) _logger.LogInformation("User '{0}' connected.", OnlineUsers[userId].Name);
            }
        }

        /// <summary>
        /// Отключение клиента / юзера
        /// </summary>
        public async Task RemoveConnection(int userId, string connectionId)
        {
            if (OnlineUsers.ContainsKey(userId))
            {
                lock (_locker)
                {
                    OnlineUsers[userId].RemoveConnection(connectionId);
                }
                if (OnlineUsers[userId].Connections == 0)
                {
                    var onlineTime = 0;
                    if (OnlineUsers[userId].EnterTime != DateTime.MinValue) onlineTime = Convert.ToInt32((DateTime.UtcNow - OnlineUsers[userId].EnterTime).TotalMinutes);
                    lock (_locker)
                    {
                        OnlineUsers[userId].OnlineTime += onlineTime;
                        OnlineUsers[userId].EnterTime = DateTime.MinValue;
                    }
                    await _chatHub.Clients.All.AddUserToOnlineList(OnlineUsers[userId]);
                    if (_isDevelopMode) _logger.LogInformation("User '{0}' disconnected.", OnlineUsers[userId].Name);
                }
            }
        }

        /// <summary>
        /// Удаляет юзера из списка онлайна в случае, если у него не более одного активного соединения
        /// сохраняес в профиле время онлайна и записывает визит в историю дейстивий
        /// </summary>
        public async Task RemoveUser(int userId)
        {
            if (OnlineUsers.ContainsKey(userId) && OnlineUsers[userId].Connections <= 1)
            {

                if(OnlineUsers[userId].EnterTime != DateTime.MinValue) OnlineUsers[userId].OnlineTime += Convert.ToInt32((DateTime.UtcNow - OnlineUsers[userId].EnterTime).TotalMinutes); // время юзера в онлайне
                var profile = OnlineUsers[userId].GetProfile();
                profile.OnlineTime += OnlineUsers[userId].OnlineTime;
                using var scope = _services.CreateScope();
                var dataBase = scope.ServiceProvider.GetRequiredService<DataContext>();
                dataBase.UserProfiles.Update(profile);

                // запись в истории об изменении имени
                if (!OnlineUsers[userId].OriginalName.Equals(profile.Name)) dataBase.History.Add(new UserHistory
                {
                    Date = DateTime.UtcNow,
                    Type = ActivityTypes.ChangeName,
                    UserID = profile.UserId,
                    Description = $"{OnlineUsers[userId].OriginalName}>{profile.Name}"
                });

                if (OnlineUsers[userId].OnlineTime > 3) dataBase.History.Add(new UserHistory            // "Визит" сохраняем в иторию только если онлайн больше 3х минут
                {
                    Date = DateTime.UtcNow,
                    Type = ActivityTypes.Visit,
                    UserID = profile.UserId,
                    Description = OnlineUsers[userId].OnlineTime.ToString()
                });
                try
                {
                    await dataBase.SaveChangesAsync();
                    User user = null;
                    if(_memoryCache.TryGetValue<User>(profile.UserId, out user))
                    {
                        user.Profile = profile;
                        _memoryCache.Set<User>(user.Id, user, _cacheOptions);
                    }
                    if (_isDevelopMode) _logger.LogInformation("{0}'s profile has been successfully updated.", profile.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing to database while saving profile");
                }

                // рассылаем уведомление о выходе из чата
                if(OnlineUsers[userId].Status != UserOnlineStatus.Hidden)
                {
                    var exitMessage = new ChatMessage
                    {
                        AuthorId = 0,
                        AuthorName = OnlineUsers[userId].Name,
                        DateTime = DateTime.UtcNow,
                        Type = MessageTypes.System,
                        Text = "<author /> покидает Кантину.",
                    };
                    _messageService.AddMessage(exitMessage);
                    await _chatHub.Clients.All.ReceiveMessage(exitMessage);
                }
                await _chatHub.Clients.All.RemoveUserFromOnlineList(userId);
                if (_isDevelopMode) _logger.LogInformation("User '{0}' disconnected after {1} min.", OnlineUsers[userId].Name, OnlineUsers[userId].OnlineTime);
                
                // удаляем юзера из памяти
                lock (_locker)
                {
                    OnlineUsers.Remove(userId);
                }

            }

        }

        /// <summary>
        /// Метод проверяет состояние списка юзеров онлайн
        /// помечает неактивных юзеров
        /// удаляет из списка всех юзеров со статусом offline
        /// и сохраняет обновлённое время онлайна в профиле
        /// а так же добавляет запись о визите в таблицу действий
        /// </summary>
        public async Task CheckUsersStatus()
        {
            var nowTime = DateTime.UtcNow;
            List<UserProfile> updatedProfiles = new List<UserProfile>();
            List<UserHistory> historyData = new List<UserHistory>();

            foreach (var keyValues in OnlineUsers)
            {
                var userSession = keyValues.Value;
                var profile = userSession.GetProfile();
                int onlineTime = 0; 
                if(userSession.EnterTime != DateTime.MinValue && userSession.LastActivityTime > userSession.EnterTime)
                {
                    onlineTime = Convert.ToInt32((userSession.LastActivityTime - userSession.EnterTime).TotalMinutes);
                }

                // если юзер неактивен дольше определенного времени - помечаем как неактивного
                if (userSession.Connections > 0 && (nowTime - userSession.LastActivityTime) > _inactivityTime && userSession.Status != UserOnlineStatus.NotActive)
                {
                    lock (_locker)
                    {
                        userSession.Status = UserOnlineStatus.NotActive;
                        userSession.EnterTime = DateTime.MinValue;
                        userSession.OnlineTime += onlineTime;
                    }
                    await _chatHub.Clients.All.AddUserToOnlineList(userSession); // рассылаем клиентам новые данные сессии юзера
                    
                    if (_isDevelopMode) _logger.LogInformation("Set 'Not Active' status for user '{0}' ", profile.Name);

                }
                // если юзер отмечен как оффлайн - удаляем из памяти
                else if (userSession.Connections == 0 && (nowTime - userSession.LastActivityTime) > _offlineInterval)
                {
                    onlineTime += userSession.OnlineTime;
                    lock (_locker)
                    {
                        profile.OnlineTime += onlineTime;
                        OnlineUsers.Remove(userSession.UserId);
                    }
                    updatedProfiles.Add(profile);
                    // запись в истории об изменении имени
                    if(!userSession.OriginalName.Equals(profile.Name)) historyData.Add(new UserHistory
                    {
                        Date = nowTime,
                        Type = ActivityTypes.ChangeName,
                        UserID = userSession.UserId,
                        Description = $"{userSession.OriginalName}>{profile.Name}"
                    });

                    if(onlineTime > 3) historyData.Add(new UserHistory
                    {
                        Date = nowTime,
                        Type = ActivityTypes.Visit,
                        UserID = userSession.UserId,
                        Description = onlineTime.ToString()
                    });
                    var message = new ChatMessage
                    {
                        AuthorId = 0,
                        AuthorName = profile.Name,
                        DateTime = nowTime,
                        Type = MessageTypes.System,
                        Text = "<author /> куда-то пропадает..."
                    };
                    _messageService.AddMessage(message);
                    await _chatHub.Clients.All.ReceiveMessage(message);
                    await _chatHub.Clients.All.RemoveUserFromOnlineList(profile.UserId);
                    if (_isDevelopMode) _logger.LogInformation("User '{0}' romoved from online users (online - {1} min.).", profile.Name, onlineTime);
                }

                // обновляем кеш
                User user = null;
                if (_memoryCache.TryGetValue<User>(profile.UserId, out user))
                {
                    user.Profile = profile;
                    _memoryCache.Set<User>(user.Id, user, _cacheOptions);
                }
            }

            // сохраняем изменения в бд
            if (updatedProfiles.Count == 0 && historyData.Count == 0) return;
            using var scope = _services.CreateScope();
            var dataBase = scope.ServiceProvider.GetRequiredService<DataContext>();

            dataBase.UpdateRange(updatedProfiles);
            dataBase.AddRange(historyData);
            try
            {
                var changes = await dataBase.SaveChangesAsync();
                if (_isDevelopMode) _logger.LogInformation($"Updates {changes} fields in DataBase. Update profiles - {updatedProfiles.Count}, added visits to history - {historyData.Count}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to update database");
            }
        }


        /// <summary>
        /// Сессия конкретного юзера в онлайне или null, если юзера нет.
        /// </summary>
        public OnlineSession GetSessionInfo(int userId)
        {
            if (OnlineUsers.ContainsKey(userId)) return OnlineUsers[userId];
            else return null;
        }


        /// <summary>
        /// Смена онлайн-статуса юзера
        /// </summary>
        public async Task ChangeOnlineStatus(int userId, UserOnlineStatus status)
        {
            if(OnlineUsers.ContainsKey(userId))
            {
                var now = DateTime.UtcNow;
                var message = new ChatMessage
                {
                    AuthorId = 0,
                    AuthorName = OnlineUsers[userId].Name,
                    DateTime = now,
                    Type = MessageTypes.System,
                    Text = null
                };

                switch (status)
                {
                    case UserOnlineStatus.NotAvailable:
                        var onlineTime = 0;
                        if(OnlineUsers[userId].EnterTime != DateTime.MinValue) onlineTime = Convert.ToInt32((now - OnlineUsers[userId].EnterTime).TotalMinutes);
                        lock (_locker)
                        {
                            OnlineUsers[userId].Status = status;
                            OnlineUsers[userId].LastActivityTime = now;
                            OnlineUsers[userId].OnlineTime += onlineTime;
                            OnlineUsers[userId].EnterTime = DateTime.MinValue;
                        }
                        message.Text = "<author /> отходит на пару минут...";
                        break;
                    case UserOnlineStatus.Online:
                        if(OnlineUsers[userId].Status == UserOnlineStatus.NotAvailable) message.Text = "<author /> возвращается в чат.";
                        else if(OnlineUsers[userId].Status == UserOnlineStatus.NotActive) message.Text = "<author /> вылезает из-под стола.";
                        lock (_locker)
                        {
                            OnlineUsers[userId].Status = status;
                            if (OnlineUsers[userId].EnterTime == DateTime.MinValue) OnlineUsers[userId].EnterTime = now;
                            OnlineUsers[userId].LastActivityTime = now;
                        }
                        break;
                }

                await _chatHub.Clients.All.AddUserToOnlineList(OnlineUsers[userId]);

                if (_isDevelopMode) _logger.LogInformation("User '{0}' set status is {1}", OnlineUsers[userId].Name, status.ToString());

                if (!String.IsNullOrEmpty(message.Text))
                {
                    _messageService.AddMessage(message);
                    await _chatHub.Clients.All.ReceiveMessage(message);
                }
            }
        }

        /// <summary>
        /// Возвращаем коллекцию юзеров онлайн
        /// </summary>
        public IEnumerable<OnlineSession> GetOnlineUsers(int userId, bool isAdmin = false)
        {
            var users = OnlineUsers.Select(keyVal => keyVal.Value).Where(session => {
                if (session.UserId == userId || session.Status != UserOnlineStatus.Hidden || isAdmin) return true;
                else return false;
            });
            return users;
        }


        public bool UpdateUserProfile(UserProfile profile)
        {
            if (OnlineUsers.ContainsKey(profile.UserId))
            {
                lock (_locker)
                {
                    OnlineUsers[profile.UserId].Profile = profile;
                }
                return true;
            }
            return false;
        }

    }
}