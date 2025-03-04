using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatting.Model
{
    public class Send_Message
    {
        public byte MsgId { get; set; }
        public byte RoomId { get; set; }
        public string UserId { get; set; }
        public List<string> MemberId { get; set; } = [];
        public string Chat { get; set; }
        public DateTime Time { get; set; }
    }
}
