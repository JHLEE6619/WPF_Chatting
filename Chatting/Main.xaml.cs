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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Chatting.Model;

namespace Chatting
{
    /// <summary>
    /// Main.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Main : Page
    {
        Client clnt = new();

        public Main()
        {
            InitializeComponent();
            clnt.ConnectServer();
            clnt.Send_msg(Login(Global_Data.UserId));
            LV_user_list.ItemsSource = Global_Data.UI.ConnectedUser;
        }

        private void btn_chat_room_list_Click(object sender, RoutedEventArgs e)
        {
            Chat_room_list chat_room_list = new(clnt);
            chat_room_list.Show();
        }

        private void btn_create_chat_room_Click(object sender, RoutedEventArgs e)
        {
            // 체크박스 true인 아이디 가져오는 구문 필요
            List<string> memberId = [];

            clnt.Send_msg(Create_chatRoom(memberId));
        }

        private Send_Message Login(string userId)
        {
            Send_Message msg = new() { MsgId = (byte)Client.MsgId.LOGIN, UserId = userId };
            return msg;
        }

        private Send_Message Create_chatRoom(List<string> memberId)
        {
            Send_Message msg = new() { MsgId = (byte)Client.MsgId.CREATE_ROOM, MemberId = memberId };
            return msg;
        }


    }
}
