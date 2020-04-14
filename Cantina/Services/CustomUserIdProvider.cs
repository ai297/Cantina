using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Cantina.Services
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirstValue(AuthOptions.Claims.ID);
        }
    }
}
