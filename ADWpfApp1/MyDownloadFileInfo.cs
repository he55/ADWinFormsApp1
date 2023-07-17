using System;
using System.Collections.Generic;

namespace ADWpfApp1
{
    public class MyDownloadFileInfo
    {
        public static List<MyDownloadFileInfo> DownloadFileInfos = new List<MyDownloadFileInfo>();

        public static MyDownloadFileInfo Get(int hash)
        {
            for (int i = 0; i < DownloadFileInfos.Count; i++)
            {
                MyDownloadFileInfo item = DownloadFileInfos[i];
                if (item.Hash == hash)
                {
                    DownloadFileInfos.Remove(item);
                    return item;
                }
            }
            return null;
        }

        public long Len { get; set; }
        public string FileName { get; set; }
        public int Hash { get; set; }

        public string SaveFilePath { get; set; }
        public Action<ProgressData> ProgressCallback;
    }

    public class ProgressData
    {
        public long Length { get; set; }
        public long Position { get; set; }
        public bool Done => Length == Position;
    }
}
