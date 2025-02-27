using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatting.Model
{
    public class Receive_Message : INotifyPropertyChanged
    {
        public byte MsgId { get; set; }

        private List<string> _connectedUser = [];
        public List<string> ConnectedUser
        {
            get { return _connectedUser; }
            set
            {
                _connectedUser = value;
                OnPropertyChanged(nameof(ConnectedUser));
            }
        }

        private Dictionary<byte, string> _chatRoomList = [];
        public Dictionary<byte, string> ChatRoomList
        {
            get { return _chatRoomList; }
            set
            {
                _chatRoomList = value;
                OnPropertyChanged(nameof(ChatRoomList));
            }
        }

        private List<(string, string, DateTime)> _chatRecord = [];
        public List<(string, string, DateTime)> ChatRecord
        {
            get { return _chatRecord; }
            set
            {
                _chatRecord = value;
                OnPropertyChanged(nameof(ChatRecord));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
