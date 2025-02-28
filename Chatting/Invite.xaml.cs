using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
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
    /// Invite.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Invite : Window
    {
        public Invite()
        {
            InitializeComponent();
        }
        public Invite(byte roomId)
        {
            InitializeComponent();
            List<string> userList = Create_userList(roomId);
            LV_user_list.ItemsSource = userList;
             
        }

        private List<string> Create_userList(byte roomId)
        {
            List<string> userList = Global_Data.UI.ConnectedUser;
            string[] memberList = Global_Data.UI.ChatRoomList[roomId].Split(", ");
            foreach(var user in userList)
            {
                foreach(var member in memberList)
                {
                    if (member.Equals(user))
                        userList.Remove(member);
                }
            }

            return userList;
        }

        private void check(object sender, RoutedEventArgs e)
        {


        }
    }
}
