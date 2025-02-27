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
        private static Receive_Message _ui = new();
        public static Receive_Message UI => _ui; // 읽기 전용
        public static string UserId { get; set; }
    }

}
