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
        public string Msg { get; set; }
        public DateTime CurrentTime { get; set; }
    }
}
