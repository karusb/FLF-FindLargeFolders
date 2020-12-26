using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FindLargestFolders
{
    public class Utilities
    {
        public static DriveInfo[] GetDrives()
        {
            return DriveInfo.GetDrives();
        }
        public static long GetDirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            try
            {
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += fi.Length;
                }
            }
            catch (UnauthorizedAccessException e)
            {
                return 0;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetDirSize(di);
            }
            return size;
        }
        public static long GetTotalFileSizeInDirectory(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            return size;
        }
        public static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
        public static int GetPercentage(int currentNumber,int maxNumber)
        {
            return (currentNumber * 100) / (maxNumber);
        }
        public static bool isRunAsAdmin()
        {
            bool isElevated;

            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            return isElevated;
        }
        public static DirectoryInfo[] GetDirectoriesFromPath(string path)
        {

            DirectoryInfo[] dirs;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                dirs = dir.GetDirectories();
            }
            catch (Exception ex)
            {
                return null;
            }
            return dirs;
        }
        public static DirectoryInfo GetDirectoryInfoFromPath(string path)
        {
            return new DirectoryInfo(path);
        }
        public static DirectoryInfo[] GetDirectoriesFromDirectoryInfo(DirectoryInfo dir)
        {
            DirectoryInfo[] dirs;
            try
            {
                dirs = dir.GetDirectories();
            }
            catch (Exception ex)
            {
                return null;
            }
            return dirs;
        }
        public static DirectoryInfo[] GetDirectoriesFromDirectoryInfoWithMessageBox(DirectoryInfo dir)
        {
            DirectoryInfo[] dirs;
            try
            {
                dirs = dir.GetDirectories();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return dirs;
        }
        public static Tuple<Dictionary<long, DirectoryInfo>, List<long>> CreateDirSizeMap(DirectoryInfo[] dirs)
        {
            Dictionary<long, DirectoryInfo> dirSizeMap = new Dictionary<long, DirectoryInfo>();
            List<long> dirSizes = new List<long>();
            foreach (var ldir in dirs)
            {
                long dirSize = Utilities.GetDirSize(ldir);
                if (dirSize > 0)
                {
                    if (!dirSizeMap.ContainsKey(dirSize))
                    {
                        dirSizeMap.Add(dirSize, ldir);
                        dirSizes.Add(dirSize);
                    }
                }
            }
            dirSizes.Sort();
            return new Tuple<Dictionary<long, DirectoryInfo>, List<long>>(dirSizeMap, dirSizes);
        }
    }
}
