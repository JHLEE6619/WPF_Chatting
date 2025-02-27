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

            LV_chat_room_list.ItemsSource = Global_Data.UI.ChatRoomList;
            
        }

        // 목록에서 클릭한 채팅방의 인덱스를 이용해 방번호를 찾는다.
        private void Chat_room_click(object sender, SelectionChangedEventArgs e)
        {
            if (LV_chat_room_list.SelectedIndex == -1)
                return;
            int idx = LV_chat_room_list.SelectedIndex; // idx는 채팅방목록의 idx와 같다.
            LV_chat_room_list.SelectedItem = null;
            byte roomId = Global_Data.UI.ChatRoomList[idx].Item1; // idx로 방번호를 찾음
            Chat_room chat_room = new(clnt ,roomId); // 방 번호를 채팅방 화면으로 넘겨줌
            chat_room.Show();
        }
    }
}
