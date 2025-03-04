using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatting.Model
{
    public class Chat
    {
        public string UserId { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }
    }
}
