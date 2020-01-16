using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Models.Messages
{
    public abstract class BaseMassege : IMessage
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int SenderId { get; set; }
        public List<int> ReceiversId { get; set; }
        public string Text { get; set; }
        public List<string> Variables { get; set; }
        public MessageStyle Style { get; set; }
    }
}
