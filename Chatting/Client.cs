using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Chatting.Model;
using Newtonsoft.Json;

namespace Chatting
{
    public class Client
    {
        static TcpClient tc = new TcpClient("127.0.0.1", 10000);
        static NetworkStream stream = tc.GetStream();
        private readonly object thisLock = new();

        public enum MsgId : byte
        {
            JOIN, LOGIN, ADD_USER, CREATE_ROOM, CHAT_ROOM_lIST, ENTER_CHAT_ROOM, SEND_CHAT, SEND_FILE, INVITE, EXIT, REMOVE_MEMBER, LOGOUT, REMOVE_USER
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
                        Global_Data.UI.ConnectedUser = msg.ConnectedUser;
                        break;
                    case (byte)MsgId.ADD_USER: // Add
                        //
                        break;
                    case (byte)MsgId.CREATE_ROOM: // Add
                        Global_Data.UI.ChatRoomList = msg.ChatRoomList;
                        break;
                    case (byte)MsgId.CHAT_ROOM_lIST: // 할당
                        //
                        break;
                    case (byte)MsgId.ENTER_CHAT_ROOM: // 할당
                        //
                        break;
                    case (byte)MsgId.SEND_CHAT: // Add
                        Global_Data.UI.ChatRecord = msg.ChatRecord;
                        break;
                    case (byte)MsgId.SEND_FILE:

                        break;
                    case (byte)MsgId.EXIT: // Remove
                        Global_Data.UI.ConnectedUser = msg.ConnectedUser;
                        break;
                }
            }
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
