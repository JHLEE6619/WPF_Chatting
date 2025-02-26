using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Chatting_Server.Model;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Collections.ObjectModel;


namespace Chatting_Server
{
    class Server
    {
        static Dictionary<string, NetworkStream> connectedUser = []; // <userId, socket>
        static Dictionary<byte, List<string>> chat_room_list = []; // <roomId , memberId>
        static Dictionary<byte, List<(string, string, DateTime)>> chatRecord = []; // <roomId, chatRecord>
        static byte roomId = 0;

        private readonly object thisLock = new();
        byte[] buf = new byte[1024];

        private enum MsgId : byte
        {
            JOIN, LOGIN, SHOW_CHAT_ROOM, CREATE_ROOM, SEND_CHAT, SEND_FILE, INVITE, EXIT, LOGOUT
        }

        public async Task StartServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 10000);
            listener.Start();
            while (true)
            {
                TcpClient client =
                    await listener.AcceptTcpClientAsync().ConfigureAwait(false);

                Task.Factory.StartNew(ServerMain, client);
            }
        }

        // 클라이언트 별로 다른 스레드가 실행하는 메소드
        private async void ServerMain(Object client)
        {
            TcpClient tc = (TcpClient)client;
            NetworkStream stream = tc.GetStream();
            Receive_Message msg = new();
            try
            {
                while (true)
                {
                    await stream.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false);
                    string json = Encoding.UTF8.GetString(buf, 0, buf.Length);
                    msg = JsonConvert.DeserializeObject<Receive_Message>(json);
                    Handler(msg, stream);
                }
            }
            catch
            { }
            finally
            {
                stream.Close();
                tc.Close();
            }
        }

        private void Handler(Receive_Message msg, NetworkStream stream)
        {

            switch (msg.MsgId)
            {
                case (byte)MsgId.LOGIN:
                    Add_user(msg.UserId, stream);
                    Send_userList(stream);
                    break;
                case (byte)MsgId.SHOW_CHAT_ROOM: break;
                case (byte)MsgId.CREATE_ROOM:
                    Create_chatRoom(msg.MemberId);
                    Create_chatRoomList_per_user();
                    break;
                case (byte)MsgId.SEND_CHAT: break;
                case (byte)MsgId.SEND_FILE: break;
                case (byte)MsgId.INVITE: break;
                case (byte)MsgId.EXIT: break;
                case (byte)MsgId.LOGOUT: break;
            }
        }

        private void Add_user(string userId, NetworkStream stream)
        {
            lock (thisLock)
            {
                connectedUser.Add(userId, stream);
            }
        }

        private void Send_userList(NetworkStream stream)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.LOGIN };
            lock (thisLock)
            {
                foreach (var userId in connectedUser)
                {
                    msg.ConnectedUser.Add(userId.Key);
                }
            }
            string json = JsonConvert.SerializeObject(msg);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            stream.Write(bytes, 0, bytes.Length);
        }

        //private void Send_userList(NetworkStream stream)
        //{
        //    string userIdList = "";
        //    // 접속중인 유저 아이디들을 구분자를 콤마로 하여 string에 모두 담는다.
        //    foreach (var userId in connectedUser)
        //    {
        //        userIdList = $"{userId.Key},";
        //    }
        //    userIdList = userIdList.TrimEnd(','); // 마지막 구분자 콤마 제거
        //    buf = Encoding.UTF8.GetBytes(userIdList);
        //    stream.Write(buf, 0, buf.Length);
        //}

        private void Create_chatRoom(List<string> memberId)
        {
            //static Dictionary<byte, List<string>> chat_room_list = [];
            lock (thisLock)
            {
                chat_room_list.Add(roomId++, memberId);
            }
        }

        private void Create_chatRoomList_per_user()
        {
            // static Dictionary<roomId, List<memberId>> chat_room_list = [];
            // static Dictionary<userId, stream> connectedUser = [];
            //접속 유저 리스트와 대화방 구성원리스트 교차조회하여 send
            lock (thisLock)
            {
                foreach (var user in connectedUser) // 유저당 대화방리스트를 만듦
                {
                    Send_Message msg = new() { MsgId = (byte)MsgId.CREATE_ROOM };
                    ObservableCollection<(byte, List<string>)> chatRoomList = [];
                    foreach (var chatRoom in chat_room_list)
                    {
                        foreach (var memberId in chatRoom.Value)
                        {
                            // 접속한 유저id와 구성원id가 같으면, 유저당 대화방리스트에 추가
                            if (user.Key.Equals(memberId))
                            {
                                chatRoomList.Add((chatRoom.Key, chatRoom.Value));
                            }
                        }
                    }
                    // 모든 대화방 리스트를 탐색했으면 유저당 대화방 리스트를 해당 유저에게 send
                    msg.ChatRoomList = chatRoomList;
                    Send_chatRoomList(user.Value, msg);
                }
            }
        }

        private void Send_chatRoomList(NetworkStream stream, Send_Message msg)
        {
            string json = JsonConvert.SerializeObject(msg);
            byte[] buf = Encoding.UTF8.GetBytes(json);
            stream.Write(buf, 0, buf.Length);
        }

        private void Add_chat(Receive_Message msg)
        {
            // public ObservableCollection<(int, List<(string, string, DateTime)>)> ChatRecord
            // static Dictionary<byte, List<(string, string, DateTime)>> chatRecord = []; // <roomId, chatRecord>
            // 방번호를 탐색 -> 그 방의 ChatRecord에 chat 추가
            DateTime now = DateTime.Now;
            lock (thisLock)
            {
                if (chatRecord.ContainsKey(msg.RoomId)) // room_Id가 chatRecord 키로 존재하면 true 반환
                {
                    chatRecord[msg.RoomId].Add((msg.UserId, msg.Chat, now));
                }
                else
                {
                    List<(string, string, DateTime)> chat = [];
                    chat.Add((msg.UserId, msg.Chat, now));
                    chatRecord.Add(msg.RoomId, chat);
                }
            }
        }

        private void Search_memberId(byte room_id)
        {
            lock (thisLock)
            {
                // 해당 방id의 멤버id를 connectedUser에서 찾아서 그 유저의 소켓을 통해 채팅 전송
                foreach (var memberId in chat_room_list[room_id])
                {
                    foreach(var user in connectedUser)
                    {
                        if (user.Key.Equals(memberId))
                        {
                            Send_chatRecord(user.Value, room_id);
                        }
                    }
                }

            }
        }

        private void Send_chatRecord(NetworkStream stream, byte room_id)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.SEND_CHAT };
            ObservableCollection<(string, string, DateTime)> chat = [];
            foreach(var ch in chatRecord[room_id])
            {
                chat.Add((ch.Item1, ch.Item2, ch.Item3));
            }
            msg.ChatRecord = chat;
            string json = JsonConvert.SerializeObject(msg);
            byte[] buf = Encoding.UTF8.GetBytes(json);
            stream.Write(buf, 0, buf.Length);
        }
    }
}
