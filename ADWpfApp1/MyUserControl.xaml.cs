using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

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
            // TODO: set image
            textBlock1.Text = userInfo.UserName;
            this.ToolTip = userInfo.IPString;
        }

        public void SetHi(bool val)
        {
            hiBorder.Visibility = val ? Visibility.Visible : Visibility.Hidden;
        }

        public void BeginStory()
        {
            Storyboard storyboard = (Storyboard)this.FindResource("story");
            storyboard.Begin();
        }
    }
}
