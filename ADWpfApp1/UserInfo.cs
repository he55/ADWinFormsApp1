using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ADWpfApp1
{
    public class UserInfo : INotifyPropertyChanged
    {
        private bool isSel;

        public string ImagePath { get; set; }
        public string UserName { get; set; }
        public long IP { get; set; }
        public string IPString { get; set; }
        public bool IsSel
        {
            get => isSel;
            set
            {
                if (isSel != value)
                {
                    isSel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{UserName}: {IPString}";
        }
    }
}
