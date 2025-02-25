using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chatting.Model
{
    class User
    {
        public ObservableCollection<string> UserList{ get; set; }
    }
}
