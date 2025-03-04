﻿using System;
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
            JOIN, LOGIN, ADD_USER, CREATE_ROOM, CHAT_ROOM_lIST, ENTER_CHAT_ROOM, SEND_CHAT, SEND_CHAT_RECORD, SEND_FILE, INVITE, EXIT, REMOVE_MEMBER, LOGOUT, REMOVE_USER
        }

        public async Task StartServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 10000);
            Console.WriteLine(" 서버 시작 ");
            listener.Start();
            Console.WriteLine("1");
            while (true)
            {
                TcpClient client =
                    await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                Console.WriteLine("2");

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
                Console.WriteLine(" 클라이언트 연결 종료 ");
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
                    connectedUser.Remove(user.Key);
                    break;
                }
            }
        }

        private void Handler(Receive_Message msg, NetworkStream stream)
        {

            switch (msg.MsgId)
            {
                case (byte)MsgId.LOGIN: // 할당
                    Console.WriteLine(" 로그인 ");
                    Add_user(msg.UserId, stream);
                    Send_userList(stream);
                    break;
                case (byte)MsgId.CREATE_ROOM:
                    Console.WriteLine(" 방 생성 ");
                    Create_chatRoom(msg.MemberId);
                    Send_add_chatRoom(msg.MemberId);
                    lock (thisLock)
                    {
                        roomId++;
                    }
                    //Create_chatRoomList_per_user();
                    break;
                case (byte)MsgId.SEND_CHAT:
                    Console.WriteLine(" 채팅 전송 ");
                    Add_chat(msg);
                    Send_add_chat(msg);
                    break;
                case (byte)MsgId.SEND_FILE:
                    Console.WriteLine(" 파일 전송 ");
                    break;
                case (byte)MsgId.INVITE:
                    Console.WriteLine(" 초대 ");
                    Add_member_to_chatRoom(msg.RoomId, msg.MemberId);
                    Send_add_invited_chatRoom(msg.RoomId);
                    Send_chatRecord(msg.RoomId, msg.MemberId);
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

        private void Add_user(string userId, NetworkStream stream)
        {
            lock (thisLock)
            {
                connectedUser.Add(userId, stream);
            }
            Send_add_user(userId, stream);
        }

        private void Send_add_user(string userId, NetworkStream stream)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.ADD_USER, UserId = userId};
            byte[] bytes = Serialize_to_json(msg);
            stream.WriteAsync(bytes).ConfigureAwait(false);

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
            stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false); // await?  
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
            List<NetworkStream> socket = Search_socket(memberId);
            Send_Message msg = new() { MsgId = (byte)MsgId.CREATE_ROOM, RoomId = roomId, UserId = memberList };
            byte[] buf = Serialize_to_json(msg);
            foreach(var stream in socket)
            {
                stream.WriteAsync(buf).ConfigureAwait(false);
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
                    List<(string, string, DateTime)> chat = [];
                    chat.Add((msg.UserId, msg.Chat, msg.Time));
                    chatRecord.Add(msg.RoomId, chat);
                }
            }
        }

        private void Send_add_chat(Receive_Message receive_msg)
        {
            // 채팅방 번호 구성원의 ID로 채팅방 번호와 채팅 내용 add 할 수 있도록 전송
            Send_Message send_msg = new() {
                MsgId = (byte)MsgId.SEND_CHAT, RoomId = receive_msg.RoomId, 
                UserId = receive_msg.UserId, Chat = receive_msg.Chat };
            byte[] buf = Serialize_to_json(send_msg);
            List<NetworkStream> socket = Search_socket(chat_room_list[receive_msg.RoomId]);
            foreach (var stream in socket)
            {
                stream.WriteAsync(buf).ConfigureAwait(false);
            }
        }


        private void Send_chatRecord(byte roomId, List<string> memberList)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.SEND_CHAT_RECORD, RoomId = roomId  };
            List<NetworkStream> socket = Search_socket(memberList);
            List<(string, string, DateTime)> chat = [];
            foreach(var ch in chatRecord[roomId])
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
            List<NetworkStream> socket = Search_socket(memberId);
            Send_Message msg = new() { MsgId = (byte)MsgId.INVITE, RoomId = roomId, UserId = memberList };
            byte[] buf = Serialize_to_json(msg);
            foreach (var stream in socket)
            {
                stream.WriteAsync(buf).ConfigureAwait(false);
            }
        }

        private void Exit_chatRoom(byte roomId, string userId)
        {
            chat_room_list[roomId].Remove(userId);
            Send_exit_chatRoom(roomId, userId);
        }

        private void Send_exit_chatRoom(byte roomId, string userId)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.EXIT, RoomId = roomId, UserId = userId};
            byte[] buf = Serialize_to_json(msg);
            List<NetworkStream> socket = Search_socket(chat_room_list[roomId]);
            foreach (var stream in socket)
            {
                stream.WriteAsync(buf).ConfigureAwait(false);
            }


        }

        private void LogOut(string userId)
        {
            lock (thisLock) 
            {
                connectedUser.Remove(userId);
            }
            Send_logOut(userId);
        }

        private void Send_logOut(string userId)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.LOGOUT, UserId = userId };
            byte[] bytes = Serialize_to_json(msg);
            foreach (var user in connectedUser)
            {
                user.Value.WriteAsync(bytes).ConfigureAwait(false);
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
