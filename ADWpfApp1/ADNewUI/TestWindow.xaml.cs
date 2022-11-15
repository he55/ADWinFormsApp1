using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ADWpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
            this.AllowDrop=true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            return;
            Point point = e.GetPosition(leida);
            leida.SetPoint(point);
            Debug.WriteLine($"point: {point}");
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            Point point = e.GetPosition(leida);
            leida.SetPoint(point);
            Debug.WriteLine($"point: {point}");
        }

        protected override void OnDrop(DragEventArgs e)
        {
            Debug.WriteLine($"dev: {leida.SelectUserInfo?.UserName}");
            leida.ResetBackground();
        }

        int i = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            i++;
            leida.AddDevice(new UserInfo { UserName = $"dev: {i}", IPString = "1.2.3.4",IP=0 });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            leida.RemoveDevice(0);
        }
    }
}
