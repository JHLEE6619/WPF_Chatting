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
using System.Reflection.Metadata;
using System.IO;


namespace Chatting_Server
{
    public class Server
    {
        static Dictionary<string, NetworkStream> connectedUser = []; // <userId, socket>
        static Dictionary<byte, List<string>> chat_room_list = []; // <roomId , memberId>
        static Dictionary<byte, List<(string, string, DateTime)>> chatRecord = []; // <roomId, chatRecord>
        static byte roomId = 0;

        private readonly object thisLock = new();

        private enum MsgId : byte
        {
            JOIN, LOGIN, ADD_USER, CREATE_ROOM, CHAT_ROOM_lIST, ENTER_CHAT_ROOM, SEND_CHAT, SEND_FILE, INVITE, EXIT, REMOVE_MEMBER, LOGOUT, REMOVE_USER
        }

        public async Task StartServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 10000);
            listener.Start();
            while (true)
            {
                TcpClient client =
                    await listener.AcceptTcpClientAsync().ConfigureAwait(false);

                Task.Run(()=>ServerMain(client));
            }
        }

        // 클라이언트 별로 다른 스레드가 실행하는 메소드
        private async Task ServerMain(Object client)
        {
            byte[] buf = new byte[5000];
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
                case (byte)MsgId.LOGIN: // 할당
                    Add_user(msg.UserId, stream);
                    Send_userList(stream);
                    break;
                case (byte)MsgId.CREATE_ROOM:
                    Create_chatRoom(msg.MemberId);

                    lock (thisLock)
                    {
                        roomId++;
                    }
                    //Create_chatRoomList_per_user();
                    break;
                case (byte)MsgId.SEND_CHAT:
                    Add_chat(msg);
                    Search_memberId(msg.RoomId);
                    break;
                case (byte)MsgId.SEND_FILE:
                    break;
                case (byte)MsgId.INVITE: 
                    Invite_member_to_chatRoom(msg.RoomId, msg.MemberId);
                    break;
                case (byte)MsgId.EXIT:
                    Exit_chatRoom(msg.RoomId, msg.UserId);
                    break;
                case (byte)MsgId.LOGOUT:
                    LogOut(msg.UserId);
                    break;
            }
        }

        private void Add_user(string userId, NetworkStream stream)
        {
            lock (thisLock)
            {
                connectedUser.Add(userId, stream);
            }
            Send_add_user(userId, stream);
        }

        private void Send_userList(NetworkStream stream)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.LOGIN };
            foreach (var userId in connectedUser)
            {
                msg.ConnectedUser.Add(userId.Key);
            }
            byte[] bytes = Serialize_to_json(msg);

            // 로그인한 유저 소켓으로 접속유저 리스트 전송
            stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false) ; // await?  
        }

        private void Send_add_user(string userId, NetworkStream stream)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.ADD_USER, UserId = userId};
            byte[] bytes = Serialize_to_json(msg);
            stream.Write(bytes);

        }

        private void Create_chatRoom(List<string> memberId)
        {
            //static Dictionary<byte, List<string>> chat_room_list = [];
            lock (thisLock)
            {
                chat_room_list.Add(roomId, memberId);
            }
            // 만들어진 방 구성원에게만 채팅방리스트에 add 하도록 메세지 전송 roomId++
            Send_add_chatRoom(roomId, memberId);
        }

        private void Send_add_chatRoom(byte roomId, List<string> memberId)
        {
            string memberList = String.Join(", ", memberId.ToArray());
            Dictionary<byte, string> chatRoomList = [];
            chatRoomList.Add(roomId, memberList);
            Send_Message msg = new() { MsgId = (byte)MsgId.CREATE_ROOM, ChatRoomList = chatRoomList };
            byte[] buf = Serialize_to_json(msg);

            foreach(var member in memberId)
            {
                foreach(var user in connectedUser)
                {
                    if(user.Equals(member))
                        user.Value.Write(buf);
                }
                
            }
        }




        //private void Create_chatRoomList_per_user()
        //{
        //    foreach (var user in connectedUser) // 유저당 대화방리스트를 만듦
        //    {
        //        Send_Message msg = new() { MsgId = (byte)MsgId.CREATE_ROOM };
        //        Dictionary<byte, string> chatRoomList = [];
        //        foreach (var chatRoom in chat_room_list)
        //        {
        //            foreach (var memberId in chatRoom.Value)
        //            {
        //                // 접속한 유저id와 구성원id가 같으면, 유저당 대화방리스트에 추가
        //                if (user.Key.Equals(memberId))
        //                {
        //                    // 리스트를 "A, B, C" 와 같은 string으로 변환
        //                    string memberList = String.Join(", ", chatRoom.Value.ToArray());
        //                    chatRoomList.Add(chatRoom.Key, memberList);
        //                }
        //            }
        //        }
        //        // 모든 대화방 리스트를 탐색했으면 유저당 대화방 리스트를 해당 유저에게 send
        //        msg.ChatRoomList = chatRoomList;
        //        Send_chatRoomList(user.Value, msg);
        //    }
        //}


        private byte[] Serialize_to_json(Send_Message msg)
        {
            string json = JsonConvert.SerializeObject(msg);
            byte[] buf = Encoding.UTF8.GetBytes(json);
            return buf;
        }

        private void Add_chat(Receive_Message msg)
        {
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

        private void Send_chatRecord(NetworkStream stream, byte room_id)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.SEND_CHAT };
            List<(string, string, DateTime)> chat = [];
            foreach(var ch in chatRecord[room_id])
            {
                chat.Add((ch.Item1, ch.Item2, ch.Item3));
            }
            msg.ChatRecord = chat;
            byte[] buf = Serialize_to_json(msg);
            stream.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
        }

        private void Invite_member_to_chatRoom(byte room_id, List<string> memberId)
        {
            foreach(string userId in memberId)
            {
                chat_room_list[room_id].Add(userId);
            }
            //Create_chatRoomList_per_user();
        }

        private void Exit_chatRoom(byte room_id, string userId)
        {
            chat_room_list[room_id].Remove(userId);
            //Create_chatRoomList_per_user();
        }

        private void LogOut(string userId)
        {
            lock (thisLock) 
            {
                connectedUser.Remove(userId);
            }
            Send_remove_user(userId);
        }

        private void Send_remove_user(string userId)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.REMOVE_USER, UserId = userId };
            byte[] bytes = Serialize_to_json(msg);
            foreach (var user in connectedUser)
            {
                user.Value.Write(bytes);
            }
        }

    }
}
