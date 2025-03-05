using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chatting.Model
{
    public class User : ICloneable
    {
        public string UserId { get; set; }
        public bool IsChecked { get; set; } = false;

        public object Clone()
        {
            User deepCopy = new() {UserId = this.UserId, IsChecked = this.IsChecked };
            return deepCopy;
        }
    }
}
