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
                    Send_add_chatRoom(msg.MemberId);
                    lock (thisLock)
                    {
                        roomId++;
                    }
                    //Create_chatRoomList_per_user();
                    break;
                case (byte)MsgId.SEND_CHAT:
                    Add_chat(msg);
                    //
                    break;
                case (byte)MsgId.SEND_FILE:
                    break;
                case (byte)MsgId.INVITE: 
                    Add_member_to_chatRoom(msg.RoomId, msg.MemberId);
                    Send_add_invited_chatRoom(msg.RoomId);
                    Send_chatRecord(msg.RoomId, msg.MemberId);
                    break;
                case (byte)MsgId.EXIT:
                    Exit_chatRoom(msg.RoomId, msg.UserId);
                    break;
                case (byte)MsgId.LOGOUT:
                    LogOut(msg.UserId);
                    break;
            }
        }

        private byte[] Serialize_to_json(Send_Message msg)
        {
            string json = JsonConvert.SerializeObject(msg);
            byte[] buf = Encoding.UTF8.GetBytes(json);
            return buf;
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
            lock (thisLock)
            {
                chat_room_list.Add(roomId, memberId);
            }
        }

        private void Send_add_chatRoom(List<string> memberId)
        {
            string memberList = String.Join(", ", memberId.ToArray());
            Dictionary<byte, string> chatRoomList = [];
            List<NetworkStream> socket = Search_socket(memberId);
            chatRoomList.Add(roomId, memberList);
            Send_Message msg = new() { MsgId = (byte)MsgId.CREATE_ROOM, ChatRoomList = chatRoomList };
            byte[] buf = Serialize_to_json(msg);
            foreach(var stream in socket)
            {
                stream.Write(buf);
            }
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

        private void Send_add_chat(Receive_Message msg)
        {

        }

        private List<NetworkStream> Search_socket(List<string> userList)
        {
            List<NetworkStream> socket = [];
            
            foreach (var userId in userList)
            {
                foreach(var sock in connectedUser)
                {
                    if (sock.Key.Equals(userId))
                    {
                        socket.Add(sock.Value);
                        break;
                    }
                }
            }
            return socket;
        }

        private void Send_chatRecord(byte room_id, List<string> memberList)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.SEND_CHAT };
            List<NetworkStream> socket = Search_socket(memberList);
            List<(string, string, DateTime)> chat = [];
            foreach(var ch in chatRecord[room_id])
            {
                chat.Add((ch.Item1, ch.Item2, ch.Item3));
            }
            msg.ChatRecord = chat;
            byte[] buf = Serialize_to_json(msg);
            foreach (var stream in socket)
            {
                stream.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            }
        }

        private void Add_member_to_chatRoom(byte room_id, List<string> memberId)
        {
            foreach(string userId in memberId)
            {
                chat_room_list[room_id].Add(userId);
            }
        }

        private void Send_add_invited_chatRoom(byte roomId)
        {
            List<string> memberId = chat_room_list[roomId];
            string memberList = String.Join(", ", memberId.ToArray());
            Dictionary<byte, string> chatRoomList = [];
            List<NetworkStream> socket = Search_socket(memberId);
            chatRoomList.Add(roomId, memberList);
            Send_Message msg = new() { MsgId = (byte)MsgId.CREATE_ROOM, ChatRoomList = chatRoomList };
            byte[] buf = Serialize_to_json(msg);
            foreach (var stream in socket)
            {
                stream.Write(buf);
            }
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
