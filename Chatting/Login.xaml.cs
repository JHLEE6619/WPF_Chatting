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
    /// Login.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Btn_join_click(object sender, RoutedEventArgs e)
        {
            Join join = new();
            this.NavigationService.Navigate(join);
        }

        private void btn_login_Click(object sender, RoutedEventArgs e)
        {
            Global_Data.UserId = TBox_id.Text;
            Main main = new();
            this.NavigationService.Navigate(main);
        }
    }
}
