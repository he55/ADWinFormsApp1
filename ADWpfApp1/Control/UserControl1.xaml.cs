using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ADWpfApp1
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public Action<string> ImageChanged;
        string imagePath = Helper.ImagePath();

        public UserControl1()
        {
            InitializeComponent();

            if (File.Exists(imagePath))
                img.ProfilePicture = new BitmapImage(new Uri(imagePath));
        }

        public ImageSource ProfilePicture { get => img.ProfilePicture; set => img.ProfilePicture = value; }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.png;*.jpg;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                img.ProfilePicture = new BitmapImage(new Uri(openFileDialog.FileName));
                File.Copy(openFileDialog.FileName, imagePath, true);
                ImageChanged?.Invoke(openFileDialog.FileName);
            }
        }
    }
}
