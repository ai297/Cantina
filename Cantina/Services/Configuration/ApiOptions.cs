using System.Collections.Generic;

namespace Cantina.Services
{
    public class ApiOptions
    {
        public int MessagesBufferSize { get; set; } = 20;
        public List<string> AllowedTags { get; set; }

        public ApiOptions()
        {
            AllowedTags = new List<string>();
        }
    }
}
