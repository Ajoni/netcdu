using System.Runtime.InteropServices;

namespace netcdu
{
    public static class Extensions
    {
        private static char _pathSeparator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/';
        public static string GetFileNameFromPath(this string path)
        {
            return path.Substring(path.LastIndexOf(_pathSeparator) + 1);
        }

        public static string LongToStringSize(this long size)
        {
            short ext = 0;
            string unit = "B";
            while (size > 1024 && ext <=4)
            {
                ext++;
                size /= 1024;
            }
            if (ext == 1)
                unit = "KB";
            if (ext == 2)
                unit = "MB";
            if (ext == 3)
                unit = "GB";
            if (ext == 4)
                unit = "TB";


            return $"{size}{unit}";
        }
    }
}
