namespace ADWpfApp1
{
    public class MyDownloadFileInfo
    {
        public static MyDownloadFileInfo DownloadFileInfo { get; set; }

        public long Len { get; set; }
        public string FileName { get; set; }
        public int Hash { get; set; }
    }
}
