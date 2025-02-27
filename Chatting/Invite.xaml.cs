using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            List<string> userList = [];
            foreach (var item_chatRoom in Global_Data.UI.ChatRoomList)
            {
                // 방번호로 해당 방을 찾음
                if (item_chatRoom.Item1 == roomId)
                {
                    // 그 방 멤버와 접속중인 유저를 교차조회 하여
                    // 일치하지 않는 ID만 리스트에 추가
                    foreach (var item_userId in Global_Data.UI.ConnectedUser)
                    {
                        foreach (var item_memberId in item_chatRoom.Item2)
                        {
                            if (!item_memberId.Equals(item_userId))
                                userList.Add(item_memberId);
                        }
                    }
                    break;
                }
            }
            return userList;
        }
    }
}
