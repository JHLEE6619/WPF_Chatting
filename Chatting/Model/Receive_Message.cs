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

        private ObservableCollection<string> _connectedUser = new();
        public ObservableCollection<string> ConnectedUser
        {
            get => _connectedUser;
            set
            {
                _connectedUser = value;
                OnPropertyChanged(nameof(ConnectedUser));
            }
        }

        private ObservableCollection<(byte, List<string>)> _chatRoomList = new();
        public ObservableCollection<(byte, List<string>)> ChatRoomList
        {
            get => _chatRoomList;
            set
            {
                _chatRoomList = value;
                OnPropertyChanged(nameof(ChatRoomList));
            }
        }

        private ObservableCollection<(string, string, DateTime)> _chatRecord = new();
        public ObservableCollection<(string, string, DateTime)> ChatRecord
        {
            get => _chatRecord;
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
