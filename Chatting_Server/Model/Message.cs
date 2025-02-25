using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatting_Server.Model
{
    class Message
    {
        public byte MsgId { get; set; }
        public byte RoomId { get; set; }
        public string UserId { get; set; }
        public string[] MemberId { get; set; }
        public string Chat { get; set; }
    }
}
