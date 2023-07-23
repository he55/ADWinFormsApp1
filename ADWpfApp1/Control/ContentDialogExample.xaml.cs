using ModernWpf.Controls;

namespace ADWpfApp1
{
    public partial class ContentDialogExample : ContentDialog
    {
        public ContentDialogExample(string msg1, string msg2)
        {
            InitializeComponent();
            TextBlock1.Text = msg1;
            TextBlock2.Text = msg2;
        }
    }
}
