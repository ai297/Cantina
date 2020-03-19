using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cantina.Models.Response;

namespace Cantina.Controllers
{
    public interface IChatClient
    {
        Task ReceiveMessage(ChatMessage message);
    }
}
