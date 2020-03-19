using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

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
