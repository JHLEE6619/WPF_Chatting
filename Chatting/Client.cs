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
            JOIN, LOGIN, ADD_USER, CREATE_ROOM, SEND_CHAT, SEND_FILE, INVITE, SEND_CHAT_RECORD, EXIT, LOGOUT
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
                case (byte)MsgId.INVITE: // 구성원 정보 수신 -> 기존 방을 제거하고 추가
                    Add_members(msg.RoomId, msg.UserId);
                    break;
                case (byte)MsgId.SEND_CHAT_RECORD:
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


        private void Receive_userList(List<string> connectedUser)
        {
            foreach (var userId in connectedUser)
            {
                Add_user(userId);
            }
        }

        private void Add_user(string userId)
        {
            if (!userId.Equals(Global_Data.UserId))
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
        }

        private void Create_room(byte roomId, string memberId)
        {
            ChatRoom chatRoom = new() { RoomId = roomId, MemberId = memberId };
            ObservableCollection<Chat> chat = [];
            if (chat_room_list != null)
            {
                chat_room_list.Dispatcher.BeginInvoke(() =>
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
                        if (Global_Data.ChatRecord.ContainsKey(roomId))
                        {
                            Global_Data.ChatRecord[roomId] = chat;
                        }
                        else
                        {
                            Global_Data.ChatRecord.Add(roomId, chat);
                        }
                    }
                });
            }
            else
            {
                lock (thisLock)
                {
                    if (Global_Data.ChatRecord.ContainsKey(roomId))
                    {
                        Global_Data.ChatRecord[roomId] = chat;
                    }
                    else
                    {
                        Global_Data.ChatRecord.Add(roomId, chat);
                    }
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
        }

        private void Add_members(byte roomId, string memberId)
        {
            int idx = Search_Room(roomId);
            // 방이 없는 초대받은 유저는 방 추가
            if (idx == -1)
            {
                Create_room(roomId, memberId);
            }
            else // 기존 구성원은 멤버 수정
            {
                if (chat_room_list != null)
                {
                    chat_room_list.Dispatcher.BeginInvoke(() =>
                    {
                        lock (thisLock)
                        {
                            Global_Data.ChatRoomList[Search_Room(roomId)].MemberId = memberId;
                        }

                        chat_room_list.InvalidateVisual();
                    });
                }
                else
                {
                    lock (thisLock)
                    {
                        Global_Data.ChatRoomList[Search_Room(roomId)].MemberId = memberId;
                    }
                }
            }
        }

        private int Search_Room(byte roomId)
        {
            int idx = -1;
            foreach(var room in Global_Data.ChatRoomList)
            {
                if(room.RoomId == roomId)
                {
                    idx = Global_Data.ChatRoomList.IndexOf(room);
                    break;
                }
            }
            return idx;
        }


        //private void Add_members(byte roomId, string memberId)
        //{
        //    // 기존 방이 있으면 제거
        //    foreach (var room in Global_Data.ChatRoomList)
        //    {
        //        if (room.RoomId == roomId)
        //        {
        //            if (chat_room_list != null)
        //            {
        //                chat_room_list.Dispatcher.BeginInvoke(() =>
        //                {
        //                    lock (thisLock)
        //                    {
        //                        Global_Data.ChatRoomList.Remove(room);
        //                    }
        //                });
        //            }
        //            else
        //            {
        //                lock (thisLock)
        //                {
        //                    Global_Data.ChatRoomList.Remove(room);
        //                }
        //            }
        //            break;
        //        }
        //    }
        //    // 다시 방 추가
        //    Create_room(roomId, memberId);
        //}

        //private void Receive_chatRecord(byte roomId, List<(string, string, string)> chatRecord)
        //{
        //    ObservableCollection<Chat> chatList = new();
        //    foreach (var item in chatRecord)
        //    {
        //        Chat chat = new()
        //        {
        //            UserId = item.Item1,
        //            Content = item.Item2,
        //            Time = item.Item3
        //        };
        //        chatList.Add(chat);
        //    }

        //    Global_Data.ChatRecord.Add(roomId, chatList);
        //}

        private void Receive_chatRecord(byte roomId, List<(string, string, string)> chatRecord)
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
                Global_Data.ChatRecord[roomId].Add(chat);
            }

        }

        private void Receive_exit_room(byte roomId, string userId)
        {
            if (Global_Data.UserId.Equals(userId))
            {
                chat_room.Dispatcher.BeginInvoke(() =>
                {
                    Global_Data.ChatRecord.Remove(roomId);
                });
                chat_room_list.Dispatcher.BeginInvoke(() =>
                {
                    foreach(var room in Global_Data.ChatRoomList)
                    {
                        if (room.RoomId == roomId)
                        {
                            Global_Data.ChatRoomList.Remove(room);
                            break;
                        }
                    }
                });
            }
            else
            {
                foreach (var chatRoom in Global_Data.ChatRoomList)
                {
                    if (chatRoom.RoomId == roomId)
                    {
                        chatRoom.MemberId = chatRoom.MemberId.Replace($"{userId}, ", "");
                        chatRoom.MemberId = chatRoom.MemberId.Replace($", {userId}", "");
                        break;
                    }
                }
            }
        }

        private void Receive_logout(string userId)
        {

            main.Dispatcher.BeginInvoke(() =>
            {
                foreach (var user in Global_Data.UserList)
                {
                    if (userId.Equals(user.UserId))
                    {
                        Global_Data.UserList.Remove(user);
                        break;
                    }
                }
            });
        }

        private byte[] SerializeToJson(Send_Message msg)
        {
            string json = JsonConvert.SerializeObject(msg);
            byte[] sendMsg = Encoding.UTF8.GetBytes(json);
            return sendMsg;
        }

        public async Task Send_msgAsync(Send_Message msg)
        {
            byte[] sendMsg = SerializeToJson(msg);
            await stream.WriteAsync(sendMsg, 0, sendMsg.Length).ConfigureAwait(false);
        }
    }
}
