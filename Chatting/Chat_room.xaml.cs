﻿using System;
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
        private Client clnt;
        private byte roomId;
        Chat_room_list chat_room_list;

        public Chat_room()
        {
            InitializeComponent();
        }

        public Chat_room(Client clnt, byte roomId, Chat_room_list chat_room_list)
        {
            InitializeComponent();
            this.clnt = clnt;
            this.clnt.chat_room = this;
            this.roomId = roomId;
            this.chat_room_list = chat_room_list;
            // 채팅방 입장 시 채팅기록 전송
            LV_chat_record.ItemsSource = Global_Data.ChatRecord[roomId];
        }

        private void btn_send_chat_Click(object sender, RoutedEventArgs e)
        {
            clnt.Send_msgAsync(Send_chat(Tbox_chat.Text));
            Tbox_chat.Text = "";
        }

        private Send_Message Send_chat(string chat)
        {
            DateTime time = DateTime.Now;
            string hhmm = time.ToString("hh:mm");

            Send_Message msg = new() { MsgId = (byte)Client.MsgId.SEND_CHAT, UserId = Global_Data.UserId, RoomId = roomId, Chat = chat, Time = hhmm };
            return msg;
        }

        private void btn_invite_Click(object sender, RoutedEventArgs e)
        {
            Invite invite = new(roomId, clnt);
            invite.Show();
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            clnt.Send_msgAsync(Exit(roomId));
            this.Close();

        }

        private Send_Message Exit(byte roomId)
        {
            Send_Message msg = new() { MsgId = (byte)Client.MsgId.EXIT, UserId = Global_Data.UserId, RoomId = roomId };
            return msg;
        }
    }
}
