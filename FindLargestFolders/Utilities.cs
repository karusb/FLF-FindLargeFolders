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
        public static long GetPercentage(long currentNumber, long maxNumber)
        {
            return currentNumber * 100L / maxNumber;
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
        public static List<Tuple<string, long>> CreateSizeMap(string path)
        {
            var depth2 = GetDirectoriesFromPath(path);
            List<Tuple<string, long>> sizeList = new List<Tuple<string, long>>();
            for (int j = 0; j < depth2.Length; ++j)
            {
                sizeList.Add(new Tuple<string, long>(depth2[j].FullName, Utilities.GetDirSize(depth2[j])));
            }
            sizeList.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            sizeList.Reverse();
            return sizeList;
        }
        public static List<Tuple<string, long>> GetFoldersMatchingSizePercentage(long folderSize,int matchPercentage, List<Tuple<string,long>> sizeMap)
        {
            List<Tuple<string,long>> ret = new List<Tuple<string,long>>();
            foreach (var folder in sizeMap)
            {
                if (Utilities.GetPercentage(folder.Item2, folderSize) >= matchPercentage)
                {
                    ret.Add(folder);
                }
            }
            return ret;
        }
        public static Tuple<string,long> RecursiveDirectoryCulpritFinder(string folderName,long folderSize,int folderDepth,int folderMatchPercentage)
        {
            var parentFolder = folderName;
            var parentFolderSize = folderSize;
            bool hasMatchingFolders = false;
            var searchDepth = 0;
            do
            {
                var matchSizeList = Utilities.CreateSizeMap(parentFolder);
                var matchFolders = Utilities.GetFoldersMatchingSizePercentage(parentFolderSize, folderMatchPercentage, matchSizeList);
                if (matchFolders.Count != 0)
                {
                    searchDepth++;
                    parentFolder = matchFolders[0].Item1;
                    parentFolderSize = matchFolders[0].Item2;
                    hasMatchingFolders = true;
                }
                else
                {
                    hasMatchingFolders = false;
                }
            }
            while (hasMatchingFolders && searchDepth < folderDepth);
            if (parentFolder == folderName) return null;
            return new Tuple<string, long>(parentFolder, parentFolderSize);
        }
    }
}
