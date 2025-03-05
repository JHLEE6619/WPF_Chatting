using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.Sockets;
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
        private ObservableCollection<User> userList = [];
        private Client clnt;
        private byte roomId;

        public Invite()
        {
            InitializeComponent();
        }
        public Invite(byte roomId, Client clnt)
        {
            InitializeComponent();
            this.clnt = clnt;
            this.roomId = roomId;
            userList = Create_userList(roomId);
            LV_user_list.ItemsSource = userList;
             
        }

        private ObservableCollection<User> Create_userList(byte roomId)
        {
            ObservableCollection<User> userList = Global_Data.UserList;
            string[] memberList = [];
            foreach (var chatRoom in Global_Data.ChatRoomList)
            {
                if (chatRoom.RoomId == roomId)
                {
                    memberList = chatRoom.MemberId.Split(", ");
                    break;
                }
            }

            List<User> removeList = [];
            foreach (var user in userList)
            {
                foreach(var member in memberList)
                {
                    if (member.Equals(user.UserId))
                        removeList.Add(user);
                }
            }

            foreach(var removeUser in removeList)
            {
                userList.Remove(removeUser);
            }

            return userList;
        }

        private void btn_invite_Click(object sender, RoutedEventArgs e)
        {
            List<string> memberId = [];
            List<User> removeList = [];
            foreach (var user in userList)
            {
                if (user.IsChecked)
                {
                    memberId.Add(user.UserId);
                    removeList.Add(user);
                }
            }

            foreach (var removeUser in removeList)
            {
                userList.Remove(removeUser);
            }

            clnt.Send_msg(Invite_member(memberId));
            MessageBox.Show("초대가 완료되었습니다.", "초대 완료");
        }

        private Send_Message Invite_member(List<string> memberId)
        {
            Send_Message msg = new() { MsgId = (byte)Client.MsgId.INVITE, MemberId = memberId, RoomId = roomId};
            return msg;
        }


    }
}
