namespace ADWpfApp1
{
    public class StringHelper
    {
        public static string ToSizeString(long size)
        {
            const double KB = 1024;
            const double MB = 1024*1024;
            const double GB = 1024*1024*1024;

            if (size >= GB)
                return $"{(size / GB):0.##} GB";
            else if (size >= MB)
                return $"{(size / MB):0.##} MB";
            else if (size >= KB)
                return $"{(size / KB):0.##} KB";
            else
                return $"{size} B";
        }
    }
}
