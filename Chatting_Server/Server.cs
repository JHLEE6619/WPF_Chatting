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
        static Dictionary<byte, List<(string, string, string)>> chatRecord = []; // <roomId, chatRecord>
        static byte roomId = 0;

        private readonly object thisLock = new();

        private enum MsgId : byte
        {
            JOIN, LOGIN, ADD_USER, CREATE_ROOM, SEND_CHAT, SEND_FILE, INVITE, SEND_CHAT_RECORD, EXIT, LOGOUT
        }

        public async Task StartServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 10000);
            Console.WriteLine(" 서버 시작 ");
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
            Console.WriteLine(" 클라이언트 연결됨 ");
            byte[] buf = new byte[5000];
            TcpClient tc = (TcpClient)client;
            NetworkStream stream = tc.GetStream();
            Receive_Message msg = new();

            try
            {
                while (true)
                {
                    int len = await stream.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false);
                    string json = Encoding.UTF8.GetString(buf, 0, len);
                    msg = JsonConvert.DeserializeObject<Receive_Message>(json);
                    if(msg == null)
                    {
                        Console.WriteLine("메세지가 null 입니다.");
                        continue;
                    }
                    Handler(msg, stream);
                }
            }
            catch
            { }
            finally
            {
                Disconnect_client(stream);
                stream.Close();
                tc.Close();
            }
        }

        private void Disconnect_client(NetworkStream stream)
        {
            foreach(var user in connectedUser)
            {
                if(user.Value == stream)
                {
                    Console.WriteLine($"{user.Key} 연결 종료 ");
                    LogOut(user.Key);
                    break;
                }
            }
        }

        private void Handler(Receive_Message msg, NetworkStream stream)
        {

            switch (msg.MsgId)  
            {
                case (byte)MsgId.LOGIN: // 할당
                    Send_userListAsync(stream);
                    Add_user(msg.UserId, stream);
                    break;
                case (byte)MsgId.CREATE_ROOM:
                    Console.WriteLine(" 방 생성 ");
                    Create_chatRoom(msg.MemberId);
                    Send_add_chatRoomAsync(msg.MemberId);
                    lock (thisLock)
                    {
                        roomId++;
                    }
                    break;
                case (byte)MsgId.SEND_CHAT:
                    Console.WriteLine(" 채팅 전송 ");
                    Add_chat(msg);
                    Send_add_chatAsync(msg);
                    break;
                case (byte)MsgId.SEND_FILE:
                    Console.WriteLine(" 파일 전송 ");
                    break;
                case (byte)MsgId.INVITE:
                    Console.WriteLine(" 초대 ");
                    Add_member_to_chatRoom(msg.RoomId, msg.MemberId);
                    Send_add_invited_chatRoomAsync(msg.RoomId);
                    Send_chatRecordAsync(msg.RoomId, msg.MemberId);
                    break;
                case (byte)MsgId.EXIT:
                    Console.WriteLine(" 퇴장 ");
                    Exit_chatRoom(msg.RoomId, msg.UserId);
                    break;
                case (byte)MsgId.LOGOUT:
                    Console.WriteLine(" 로그아웃 ");
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

        private async Task Send_userListAsync(NetworkStream stream)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.LOGIN};
            foreach (var userId in connectedUser)
            {
                msg.ConnectedUser.Add(userId.Key);
            }

            byte[] bytes = Serialize_to_json(msg);
            // 로그인한 유저 소켓으로 접속유저 리스트 전송
            await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false); // await?  
        }

        private void Add_user(string userId, NetworkStream stream)
        {
            lock (thisLock)
            {
                connectedUser.Add(userId, stream);
                foreach(var user in connectedUser)
                {
                    Console.WriteLine($"{user.Key}");
                }
            }

            Send_add_userAsync(userId);
        }

        private async Task Send_add_userAsync(string userId)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.ADD_USER, UserId = userId};
            byte[] bytes = Serialize_to_json(msg);
            foreach (var user in connectedUser)
            {
                await user.Value.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }

        }

        private void Create_chatRoom(List<string> memberId)
        {
            List<(string, string, string)> chat = [];
            lock (thisLock)
            {
                chat_room_list.Add(roomId, memberId);
                chatRecord.Add(roomId, chat);
            }

        }

        private async Task Send_add_chatRoomAsync(List<string> memberId)
        {
            string memberList = String.Join(", ", memberId.ToArray());
            List<NetworkStream> socket = Search_socket(memberId);
            Send_Message msg = new() { MsgId = (byte)MsgId.CREATE_ROOM, RoomId = roomId, UserId = memberList };
            byte[] buf = Serialize_to_json(msg);
            foreach(var stream in socket)
            {
                await stream.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            }
        }

        private void Add_chat(Receive_Message msg)
        {
            // 방번호를 탐색 -> 그 방의 ChatRecord에 chat 추가
            lock (thisLock)
            {
                if (chatRecord.ContainsKey(msg.RoomId)) // room_Id가 chatRecord 키로 존재하면 true 반환
                {
                    chatRecord[msg.RoomId].Add((msg.UserId, msg.Chat, msg.Time));
                }
                else
                {
                    List<(string, string, string)> chat = [];
                    chat.Add((msg.UserId, msg.Chat, msg.Time));
                    chatRecord.Add(msg.RoomId, chat);
                }
            }
        }

        private async Task Send_add_chatAsync(Receive_Message receive_msg)
        {
            // 채팅방 번호 구성원의 ID로 채팅방 번호와 채팅 내용 add 할 수 있도록 전송
            Send_Message send_msg = new() {
                MsgId = (byte)MsgId.SEND_CHAT, RoomId = receive_msg.RoomId, 
                UserId = receive_msg.UserId, Chat = receive_msg.Chat, Time = receive_msg.Time };
            byte[] buf = Serialize_to_json(send_msg);
            List<NetworkStream> socket = Search_socket(chat_room_list[receive_msg.RoomId]);
            foreach (var stream in socket)
            {
                await stream.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            }
        }

        private void Add_member_to_chatRoom(byte room_id, List<string> memberId)
        {
            foreach(string userId in memberId)
            {
                chat_room_list[room_id].Add(userId);
            }
            Console.WriteLine("멤버 추가");
        }

        // 서버의 방 리스트에 추가 -> 서버의 방 리스트 인덱싱 -> 방의 멤버id만 가져옴 ->

        private async Task Send_add_invited_chatRoomAsync(byte roomId)
        {
            List<string> memberId = chat_room_list[roomId]; // 채팅방의 모든 구성원으로 정보 전송
            string memberList = String.Join(", ", memberId.ToArray());
            List<NetworkStream> socket = Search_socket(memberId);
            Send_Message msg = new() { MsgId = (byte)MsgId.INVITE, RoomId = roomId, UserId = memberList };
            byte[] buf = Serialize_to_json(msg);
            foreach (var stream in socket)
            {
               await stream.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            }
            Console.WriteLine("모든 멤버에게 멤버id 전송");
        }

        private async Task Send_chatRecordAsync(byte roomId, List<string> memberList) // 여기서 memberList는 초대된 멤버
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.SEND_CHAT_RECORD, RoomId = roomId };
            List<NetworkStream> socket = Search_socket(memberList);
            List<(string, string, string)> chat = [];
            foreach(var ch in chatRecord[roomId])
            {
                chat.Add((ch.Item1, ch.Item2, ch.Item3));
            }
            msg.ChatRecord = chat;
            byte[] buf = Serialize_to_json(msg);
            foreach (var stream in socket)
            {
                await stream.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            }
            Console.WriteLine("초대받은 유저들에게 채팅 내용 전송");
        }

        private void Exit_chatRoom(byte roomId, string userId)
        {
            chat_room_list[roomId].Remove(userId);
            Send_exit_chatRoomAsync(roomId, userId);
        }

        private async Task Send_exit_chatRoomAsync(byte roomId, string userId)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.EXIT, RoomId = roomId, UserId = userId};
            byte[] buf = Serialize_to_json(msg);
            List<NetworkStream> socket = Search_socket(chat_room_list[roomId]);
            foreach (var stream in socket)
            {
                await stream.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
            }


        }

        private void LogOut(string userId)
        {
            lock (thisLock) 
            {
                connectedUser.Remove(userId);
            }
            Send_logOutAsync(userId);
        }

        private async Task Send_logOutAsync(string userId)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.LOGOUT, UserId = userId };
            byte[] bytes = Serialize_to_json(msg);
            foreach (var user in connectedUser)
            {
                await user.Value.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }
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
    }
}
