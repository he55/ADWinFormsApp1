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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ADWpfApp1
{
    /// <summary>
    /// Interaction logic for MyUserControl.xaml
    /// </summary>
    public partial class MyUserControl : UserControl
    {
        public MyUserControl()
        {
            InitializeComponent();
        }

        public void SetUserInfo(UserInfo userInfo)
        {
            textBlock1.Text= userInfo.UserName;
           this.ToolTip= userInfo.IPString;

            BeginStory();
        }

        void BeginStory()
        {
            Storyboard storyboard = (Storyboard)this.FindResource("story");
            storyboard.Begin();
        }
    }
}
