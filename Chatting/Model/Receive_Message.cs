using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatting.Model
{
    class Receive_Message
    {
        public byte MsgId { get; set; }
        // 접속중인 유저 목록(id)
        public ObservableCollection<string> ConnectedUser { get; set; } = [];
        // 대화방 목록(방번호, 구성원id) 
        public ObservableCollection<(byte, List<string>)> ChatRoomList { get; set; } = [];
        // 대화기록(방번호, 아이디, 대화내용, 시간)
        public ObservableCollection<(string, string, DateTime)> ChatRecord { get; set; } = [];

    }
}
