using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatting_Server.Model
{
    class Send_Message
    {
        public byte MsgId { get; set; }
        // 로그인한 유저(id)
        public string UserId { get; set; }
        // 접속중인 유저 목록(id)
        public List<string> ConnectedUser { get; set; } = [];
        //// 대화방 목록(방번호, 구성원id) 
        //public Dictionary<byte, string> ChatRoomList { get; set; } = [];
        // 방 번호
        public byte RoomId { get; set; }
        // 채팅 내용
        public string Chat { get; set; }
        // 시간
        public string Time { get; set; }
        // 대화내용(아이디, 대화내용, 시간)
        public List<(string, string, string)> ChatRecord { get; set; } = [];
    }
}
