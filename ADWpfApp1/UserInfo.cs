namespace ADWpfApp1
{
    public class UserInfo
    {
        public string Name { get; set; }
        public long IP { get; set; }
        public string IPString { get; set; }

        public override string ToString()
        {
            return $"{Name}: {IPString}";
        }
    }
}
