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

namespace Chatting
{
    /// <summary>
    /// Main.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Main : Page
    {
        public Main()
        {
            InitializeComponent();
        }
        public Main(string userId)
        {
            InitializeComponent();
            Client clnt = new();
            clnt.ConnectServer();
            clnt.Login(userId);
            LV_user_list.ItemsSource = Client.UI.ConnectedUser;
        }

        private void btn_chat_room_list_Click(object sender, RoutedEventArgs e)
        {
            Chat_room_list chat_room_list = new();
            chat_room_list.Show();
        }

        private void btn_create_chat_room_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
