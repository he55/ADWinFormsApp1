namespace ADWpfApp1
{
    public class UserInfo 
    {
        public string ImagePath { get; set; }
        public string UserName { get; set; }
        public long IP { get; set; }
        public string IPString { get; set; }
        public override string ToString()
        {
            return $"{UserName}: {IPString}";
        }
    }
}
