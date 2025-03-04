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
        static NetworkStream stream = tc.GetStream();
        Main main = new();

        private readonly object thisLock = new();

        public enum MsgId : byte
        {
            JOIN, LOGIN, ADD_USER, CREATE_ROOM, ENTER_CHAT_ROOM, SEND_CHAT, SEND_CHAT_RECORD, SEND_FILE, INVITE, EXIT, REMOVE_MEMBER, LOGOUT, REMOVE_USER
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
                    await stream.ReadAsync(buf, 0, buf.Length);
                    string json = Encoding.UTF8.GetString(buf);
                    msg = JsonConvert.DeserializeObject<Receive_Message>(json);
                    // 데이터를 수신하면 스레드를 생성해 처리
                    Task.Run(() => Handler(msg));
                }
            }
            catch { }
            finally
            {
                stream.Close();
                tc.Close();
            }
        }


        private void Handler(Receive_Message msg)
        {
            lock (thisLock)
            {
                switch (msg.MsgId)
                {
                    case (byte)MsgId.LOGIN: // 할당
                        Receive_userList(msg.ConnectedUser);
                        break;
                    case (byte)MsgId.ADD_USER: // Add
                        Add_user(msg.UserId);
                        break;
                    case (byte)MsgId.CREATE_ROOM: // Add
                        Create_room(msg.RoomId, msg.UserId);
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
            //this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            //{
            //}));
            Global_Data.UserList.Add(user);


        }

        private void Create_room(byte roomId, string memberId)
        {
            ChatRoom chatRoom = new() { RoomId = roomId, MemberId = memberId };
            Global_Data.ChatRoomList.Add(chatRoom);
        }

        private void Add_chat(Receive_Message msg)
        {
            Chat chat = new()
            {
                UserId = msg.UserId,
                Content = msg.Chat,
                Time = msg.Time
            };

            if (Global_Data.ChatRecord.ContainsKey(msg.RoomId))
            {
                Global_Data.ChatRecord[msg.RoomId].Add(chat);
            }
            else
            {
                ObservableCollection<Chat> chatRecord = [];
                chatRecord.Add(chat);
                Global_Data.ChatRecord.Add(msg.RoomId, chatRecord);
            }
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

        private Receive_Message DeserializeToJson(byte[] buf)
        {
            Receive_Message rcvMsg;
            string json = Encoding.UTF8.GetString(buf);
            rcvMsg = JsonConvert.DeserializeObject<Receive_Message>(json);
            return rcvMsg;
        }

        public void Send_msg(Send_Message msg)
        {
            byte[] sendMsg = SerializeToJson(msg);
            stream.WriteAsync(sendMsg).ConfigureAwait(false);
        }
    }
}
