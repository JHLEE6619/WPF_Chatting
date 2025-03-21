﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatting.Model
{
    public class Receive_Message
    {
        public byte MsgId { get; set; }
        public string UserId { get; set; }
        public List<string> ConnectedUser { get; set; } = [];
        public byte RoomId { get; set; }
        public string Chat { get; set; }
        public string Time { get; set; }
        public List<(string, string, string)> ChatRecord { get; set; } = [];
        
        

    }

}
