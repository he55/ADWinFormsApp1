using Microsoft.Win32;
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

namespace ADWpfApp1
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public Action<string> ImageChanged;

        public UserControl1()
        {
            InitializeComponent();
        }

        public ImageSource ProfilePicture { get => img.ProfilePicture; set => img.ProfilePicture = value; }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.png;*.jpg;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                img.ProfilePicture = new BitmapImage(new Uri(openFileDialog.FileName));
                ImageChanged?.Invoke(openFileDialog.FileName);
            }
        }
    }
}
