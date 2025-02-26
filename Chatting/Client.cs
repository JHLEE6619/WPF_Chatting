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
        public static Receive_Message UI { get; set; }
        public enum MsgId : byte
        {
            JOIN, LOGIN, CREATE_ROOM, SEND_CHAT, SEND_FILE, INVITE, EXIT, LOGOUT
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
                    Handler(msg);
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
            switch (msg.MsgId)
            {
                case (byte)MsgId.LOGIN:
                    UI.ConnectedUser = rcv_itemList(msg.ConnectedUser);
                    break;
                case (byte)MsgId.CREATE_ROOM:
                    UI.ChatRoomList = rcv_itemList(msg.ChatRoomList);
                    break;
                case (byte)MsgId.SEND_CHAT:
                    UI.ChatRecord = rcv_itemList(msg.ChatRecord);
                    break;
                case (byte)MsgId.SEND_FILE: 
                    
                    break;
                case (byte)MsgId.EXIT:
                    UI.ConnectedUser = rcv_itemList(msg.ConnectedUser);
                    break;
            }
        }

        private byte[] SerializeToJson(Send_Message msg)
        {
            string json = JsonConvert.SerializeObject(msg);
            byte[] sendMsg = Encoding.UTF8.GetBytes(json);
            return sendMsg;
        }

        public void Login(string userId)
        {
            Send_Message msg = new() { MsgId = (byte)MsgId.LOGIN, UserId = userId};
            byte[] sendMsg = SerializeToJson(msg);
            stream.WriteAsync(sendMsg, 0, sendMsg.Length).ConfigureAwait(false);
        }

        // 일반화 메소드
        private ObservableCollection<T> rcv_itemList<T>(ObservableCollection<T> itemList)
        {
            return itemList;
        }
    }
}
