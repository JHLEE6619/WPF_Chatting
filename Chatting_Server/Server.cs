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

namespace Chatting_Server
{
    class Server
    {
        byte[] buf = new byte[1024];
        byte roomId = 0;
        Dictionary<string, TcpClient> connectedUser = [];
        Dictionary<byte, List<string>> chat_room_list = [];

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

        private async void ServerMain(Object client)
        {
            TcpClient tc = (TcpClient)client;
            NetworkStream stream = tc.GetStream();
            Message msg = new();
            try
            {
                while (true)
                {
                    await stream.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false);
                    string json = Encoding.UTF8.GetString(buf, 0, buf.Length);
                    msg = JsonConvert.DeserializeObject<Message>(json);
                    await Handler(msg);
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

        private async Task Handler(Message msg)
        {

            switch (msg.MsgId)
            {
                case (byte)MsgId.LOGIN: 
                    break;
                case (byte)MsgId.SHOW_CHAT_ROOM: break;
                case (byte)MsgId.CREATE_ROOM: break;
                case (byte)MsgId.SEND_CHAT: break;
                case (byte)MsgId.SEND_FILE: break;
                case (byte)MsgId.INVITE: break;
                case (byte)MsgId.EXIT: break;
                case (byte)MsgId.LOGOUT: break;
            }
        }

        private void Add_user()
        {

            NetworkStream stream = tc.GetStream();
            stream.Read(buf, 0, buf.Length);
            connectedUser.Add();
        }

        // 회원가입하면 DB에 추가(생략)
        // 로그인하면 유저리스트에 추가하고 모든 접속된 유저 소켓에 유저리스트 전송
        // 대화방 생성하면 대화방 구성원 소켓으로 대화방 리스트 정보 전송(방 id 필요) -> 전송할때 방id도 같이 전송
        // 


        
    }
}
