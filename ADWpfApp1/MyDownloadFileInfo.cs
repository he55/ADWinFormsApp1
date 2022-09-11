using System.Collections.Generic;

namespace ADWpfApp1
{
    public class MyDownloadFileInfo
    {
        public static List<MyDownloadFileInfo> DownloadFileInfos = new List<MyDownloadFileInfo>();

        public long Len { get; set; }
        public string FileName { get; set; }
        public int Hash { get; set; }
    }
}
