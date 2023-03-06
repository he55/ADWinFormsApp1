using ModernWpf.Controls;

namespace ADWpfApp1
{
    public partial class ContentDialogExample2 : ContentDialog
    {
        public ContentDialogExample2(string msg1, string msg2)
        {
            InitializeComponent();
            TextBlock1.Text = msg1;
            TextBlock2.Text = msg2;
        }
    }
}
