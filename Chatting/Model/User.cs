using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chatting.Model
{
    public class User
    {
        public string UserId { get; set; }
        public bool IsChecked { get; set; } = false;
    }
}
