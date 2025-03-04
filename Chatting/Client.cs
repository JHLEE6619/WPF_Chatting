using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Threading;
using Chatting.Model;
using Newtonsoft.Json;

namespace Chatting
{
    public class Client
    {
        static TcpClient tc = new TcpClient("127.0.0.1", 10000);
        NetworkStream stream = tc.GetStream();
        private readonly object thisLock = new();
        Main main;
        public Chat_room_list chat_room_list;
        public Chat_room chat_room;


        public Client(Main main)
        {
            this.main = main;
        }


        public enum MsgId : byte
        {
            JOIN, LOGIN, ADD_USER, CREATE_ROOM, SEND_CHAT, SEND_CHAT_RECORD, SEND_FILE, INVITE, EXIT, LOGOUT
        }

        public void ConnectServer()
        {
            Task.Run(() => Receive_msg(tc));
        }



        private async Task Receive_msg(TcpClient tc)
        {
            Receive_Message msg = new();
            byte[] buf = new byte[5000];
            try
            {
                while (true)
                {
                    int len = await stream.ReadAsync(buf, 0, buf.Length);
                    string json = Encoding.UTF8.GetString(buf, 0, len);
                    msg = JsonConvert.DeserializeObject<Receive_Message>(json);
                    // 데이터를 수신하면 스레드를 생성해 처리
                    await Handler(msg);
                }
            }
            catch { }
            finally
            {
                stream.Close();
                tc.Close();
            }
        }


        private async Task Handler(Receive_Message msg)
        {
            lock (thisLock)
            {
                switch (msg.MsgId)
                {
                    case (byte)MsgId.LOGIN: // 할당
                        Receive_userList(msg.ConnectedUser);
                        System.Diagnostics.Debug.WriteLine("로그인");
                        break;
                    case (byte)MsgId.ADD_USER: // Add
                        Add_user(msg.UserId);
                        System.Diagnostics.Debug.WriteLine("유저 추가");
                        break;
                    case (byte)MsgId.CREATE_ROOM: // Add
                        Create_room(msg.RoomId, msg.UserId);
                        System.Diagnostics.Debug.WriteLine("방 생성");
                        break;
                    case (byte)MsgId.SEND_CHAT: // Add
                        Add_chat(msg);
                        break;
                    case (byte)MsgId.SEND_FILE:
                        break;
                    case (byte)MsgId.INVITE:
                        Receive_chatRecord(msg.RoomId, msg.ChatRecord);
                        break;
                    case (byte)MsgId.EXIT: // Remove
                        Receive_exit_room(msg.RoomId, msg.UserId);
                        break;
                    case (byte)MsgId.LOGOUT:
                        Receive_logout(msg.UserId);
                        break;
                }
            }
        }


        private void Receive_userList(List<string> connectedUser)
        {
            foreach (var userId in connectedUser)
            {
                Add_user(userId);
            }
        }

        private void Add_user(string userId)
        {
            User user = new() { UserId = userId };

            main.Dispatcher.BeginInvoke(() =>
            {
                lock (thisLock)
                {
                    Global_Data.UserList.Add(user);
                }
            });


        }

        private void Create_room(byte roomId, string memberId)
        {
            ChatRoom chatRoom = new() { RoomId = roomId, MemberId = memberId };
            ObservableCollection<Chat> chat = [];
            // dispatcher
            if (chat_room_list != null)
            {
                chat_room.Dispatcher.BeginInvoke(() =>
                {
                    lock (thisLock)
                    {
                        Global_Data.ChatRoomList.Add(chatRoom);
                    }
                });
            }
            else
            {
                lock (thisLock)
                {
                    Global_Data.ChatRoomList.Add(chatRoom);
                }
            }

            if (chat_room != null)
            {
                chat_room.Dispatcher.BeginInvoke(() =>
                {
                    lock (thisLock)
                    {
                        Global_Data.ChatRecord.Add(roomId, chat);
                    }
                });
            }
            else
            {
                lock (thisLock)
                {
                    Global_Data.ChatRecord.Add(roomId, chat);
                }
            }
        }

        private void Add_chat(Receive_Message msg)
        {
            Chat chat = new()
            {
                UserId = msg.UserId,
                Content = msg.Chat,
                Time = msg.Time
            };

            if (chat_room != null)
            {
                chat_room.Dispatcher.BeginInvoke(() =>
                {
                    lock (thisLock)
                    {
                        Global_Data.ChatRecord[msg.RoomId].Add(chat);
                    }
                });
            }
            else
            {
                lock (thisLock)
                {
                    Global_Data.ChatRecord[msg.RoomId].Add(chat);
                }
            }

            //if (Global_Data.ChatRecord.ContainsKey(msg.RoomId))
            //{
            //    Global_Data.ChatRecord[msg.RoomId].Add(chat);
            //}
            //else
            //{
            //    ObservableCollection<Chat> chatRecord = [];
            //    chatRecord.Add(chat);
            //    Global_Data.ChatRecord.Add(msg.RoomId, chatRecord);
            //}
        }

        private void Receive_chatRecord(byte roomId, List<(string, string, DateTime)> chatRecord)
        {
            ObservableCollection<Chat> chatList = new();
            foreach (var item in chatRecord)
            {
                Chat chat = new()
                {
                    UserId = item.Item1,
                    Content = item.Item2,
                    Time = item.Item3
                };
                chatList.Add(chat);
            }

            Global_Data.ChatRecord.Add(roomId, chatList);
        }

        //// 유저가 방을 나갈때 서버로 보내는 명령
        //private void Send_exit_room(byte roomId, string userId)
        //{
        //    Send_Message msg = new() { MsgId = (byte)MsgId.EXIT, UserId = userId };
        //    byte[] buf = SerializeToJson(msg);
        //    stream.WriteAsync(buf).ConfigureAwait(false);
        //}

        // 방을 나갔음을 알리는 서버로 부터의 메세지

        private void Receive_exit_room(byte roomId, string userId)
        {
            foreach(var chatRoom in Global_Data.ChatRoomList)
            {
                if(chatRoom.RoomId == roomId)
                {
                    chatRoom.MemberId = chatRoom.MemberId.Replace($"{userId}, ", "");
                    chatRoom.MemberId = chatRoom.MemberId.Replace($", {userId}", "");
                    break;
                }
            }
        }

        private void Receive_logout(string userId)
        {
            User user = new() { UserId = userId };
            Global_Data.UserList.Remove(user);
        }
        // 채팅방 입장전 -> 다른유저 채팅 -> add
        // 채팅방 첫입장 -> 채팅기록 보내줌 -> 할당 -> 채팅방 창닫기 -> 다른유저가 채팅보냄 -> ADD -> 다시 채팅방 입장 -> 채팅기록 보내줌 ->할당
        // 채팅방 리스트 첫입장 -> 채팅방 리스트 보내줌 -> 할당 -> 창닫기 ->

        private byte[] SerializeToJson(Send_Message msg)
        {
            string json = JsonConvert.SerializeObject(msg);
            byte[] sendMsg = Encoding.UTF8.GetBytes(json);
            return sendMsg;
        }

        public void Send_msg(Send_Message msg)
        {
            byte[] sendMsg = SerializeToJson(msg);
            stream.WriteAsync(sendMsg).ConfigureAwait(false);
        }
    }
}
