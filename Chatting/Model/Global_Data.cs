using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatting.Model
{
    public class Global_Data
    {
        public static ObservableCollection<User> UserList { get; set; } = [];
        public static ObservableCollection<ChatRoom> ChatRoomList { get; set; } = [];
        public static ObservableCollection<Chat> ChatRecord { get; set; } = [];
        public static string UserId { get; set; }
    }

}
