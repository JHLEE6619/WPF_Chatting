using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chatting
{
    class Client
    {
        TcpClient tc;
        byte[] buf = new byte[256];

        public enum MsgId : byte
        {
            JOIN, LOGIN, SHOW_CHAT_ROOM, CREATE_ROOM, SEND_CHAT, SEND_FILE, INVITE, EXIT, LOGOUT
        }

        public Client(string ip, int port)
        {
            // 1. 접속할 서버의 IP, PORT 정보를 넘겨받아 연결 요청
            tc = new TcpClient(ip, port);
        }

        public void SendCommand(byte[] command)
        {
            NetworkStream stream = tc.GetStream();
            stream.WriteAsync(command, 0, command.Length);
        }
    }
}
