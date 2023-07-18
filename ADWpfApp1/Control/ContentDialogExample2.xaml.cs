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

        public void UpdateProgress(ProgressData progress)
        {
            ProgressBar1.Value = (double)progress.Position / progress.Length * 100;

            string v = StringHelper.ToSizeString(progress.Position);
            string v2 = StringHelper.ToSizeString(progress.Length);
            TextBlock3.Text = $"{v}/{v2}";
        }
    }
}
