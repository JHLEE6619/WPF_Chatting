using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Chatting.Model;

namespace Chatting
{
    /// <summary>
    /// Chat_room.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Chat_room : Window
    {
        private Client clnt = new();
        private byte roomId;

        public Chat_room()
        {
            InitializeComponent();
        }

        public Chat_room(Client clnt, byte roomId)
        {
            InitializeComponent();
            this.clnt = clnt;
            this.roomId = roomId;
            // 채팅방 입장 시 채팅기록 전송
            LV_chat_record.ItemsSource = Global_Data.UI.ChatRecord;
        }

        private void btn_send_chat_Click(object sender, RoutedEventArgs e)
        {
            clnt.Send_msg(Send_chat(Tbox_chat.Text));
        }

        private Send_Message Send_chat(string chat)
        {
            DateTime time = DateTime.Now;
            Send_Message msg = new() { MsgId = (byte)Client.MsgId.SEND_CHAT, UserId = Global_Data.UserId, RoomId = roomId, Chat = chat, Time = time };
            return msg;
        }

        private void btn_invite_Click(object sender, RoutedEventArgs e)
        {
            Invite invite = new(roomId);
            invite.Show();
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            clnt.Send_msg(Exit(roomId));
        }

        private Send_Message Exit(byte roomId)
        {
            Send_Message msg = new() { MsgId = (byte)Client.MsgId.EXIT, UserId = Global_Data.UserId, RoomId = roomId };
            return msg;
        }
    }
}
