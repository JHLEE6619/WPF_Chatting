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
    /// Chat_room_list.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Chat_room_list : Window
    {
        private Client clnt;
        public Chat_room_list()
        {
            InitializeComponent();
        }
        public Chat_room_list(Client clnt)
        {
            InitializeComponent();
            this.clnt = clnt;
            this.clnt.chat_room_list = this;
            LV_chat_room_list.ItemsSource = Global_Data.ChatRoomList;
            Tblock_userId.Text = $"{Global_Data.UserId}님 환영합니다.";

        }

        private void Chat_room_click(object sender, SelectionChangedEventArgs e)
        {
            if (LV_chat_room_list.SelectedIndex == -1)
                return;
            int idx = LV_chat_room_list.SelectedIndex;
            LV_chat_room_list.SelectedItem = null;
            byte roomId = Global_Data.ChatRoomList[idx].RoomId;
            Chat_room chat_room = new(clnt, roomId, this);
            chat_room.Show();

        }
    }
}
