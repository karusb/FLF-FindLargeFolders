using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;
using System.Management.Instrumentation;
using System.Net.Sockets;

namespace FindLargestFolders
{

    public partial class Form1 : Form
    {
        Dictionary<string, DirectoryItemUI> directoryUI = new Dictionary<string, DirectoryItemUI>();
        Dictionary<string, string> parentSizeMap = new Dictionary<string, string>();
        Dictionary<string, Tuple<string, long>[]> childSizeMap = new Dictionary<string, Tuple<string, long>[]>();
        List<string> avoidedFolderNames = new List<string> { "Documents and Settings", "System Volume Information" ,"Windows" };
        bool adminToolTipActive = false;
        DirectoryInfo lastWorkingItem;
        bool scannedAll = false;
        private class ProgressData
        {
            public string Name;
            public string Item;
            public int Percentage;
        }
        private class BGWorkerData
        {
            DirectoryInfo[] dirInfo;
            bool DrawAtEnd;
        }
        public class MagicBGWData
        {
            public DirectoryInfo[] Directories;
            public int NumberOfTotalFoldersToFocus;
            public int FolderDepth;
            public int SubFolderSizeMatchPercentage;
            public bool DrawAtEnd;
            public List<Tuple<string, long>> CulpritList;
        }
        public Form1()
        {
            InitializeComponent();
            var drives = Utilities.GetDrives();
            comboBox1.Text = drives[0].Name;
            if (Utilities.isRunAsAdmin()) adminLabel.ForeColor = Color.Green;
            foreach (var drive in drives)
            {
                comboBox1.Items.Add(drive.Name);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            DrawParentFolders(comboBox1.Text, true);
        }
        private void investigationButton_Click(object sender, EventArgs e)
        {
            progressUI.SetProgress(0);
            backgroundWorker1.RunWorkerAsync(CreateMagicBGWData(Utilities.GetDirectoryInfoFromPath(comboBox1.Text)));
        }
        private void scanAllButton_Click(object sender, EventArgs e)
        {
            scannedAll = true;
            DrawParentFolders(comboBox1.Text, false);
            var subdirs = Utilities.GetDirectoriesFromDirectoryInfo(new DirectoryInfo(comboBox1.Text));
            progressUI.SetProgress(0);
            backgroundWorker3.RunWorkerAsync(new Tuple<DirectoryInfo[], bool>(subdirs, false));
        }
        private void UiComponent_ScanClickEvent(DirectoryInfo dir)
        {
            var dirSize = Utilities.FormatBytes(Utilities.GetDirSize(dir));
            directoryUI[dir.FullName].SetDirSize(dirSize);
        }

        private void UiComponent_ExtendedClickEvent(DirectoryInfo dir)
        {
            var tempdirs = Utilities.GetDirectoriesFromDirectoryInfoWithMessageBox(dir);
            if (tempdirs == null)
            {
               FindRelevantUi(dir.FullName).RevertExtendEventUi();
                return;
            }
            if(CheckIfWorkerIsRunning() && !scannedAll)
            {
                MessageBox.Show("Please wait for the process to finish", "Busy",
    MessageBoxButtons.OK, MessageBoxIcon.Error);
                FindRelevantUi(dir.FullName).RevertExtendEventUi();
                return;
            }
            if (!CheckFoldersPrepared(dir,true))

            {
                DirectoryInfo[] dirinfo = new DirectoryInfo[1];
                dirinfo[0] = dir;
                Tuple<DirectoryInfo[], bool> drawInfoPair = new Tuple<DirectoryInfo[], bool>(dirinfo,true);
                directoryUI[dir.FullName].Enabled = false;
                backgroundWorker3.RunWorkerAsync(drawInfoPair); // Invalid Operation Exception
                //PrepareFoldersForFolder(dir, false,true);
            }
            else
            {
                flowLayoutPanel1.SuspendLayout();
                DrawFoldersForFolder(dir, false);
                flowLayoutPanel1.ResumeLayout();
            }

        }
        private void UiComponent_DeleteClickEvent(DirectoryInfo dir)
        {
             var result = MessageBox.Show("Do you want to delete folder \n\r" + dir.Name, "Deleting File",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Cancel) { return; }
            else
            {
                try
                {
                    Directory.Delete(dir.FullName, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }
        private void UiComponent_InvestigateClickEvent(DirectoryInfo dir)
        {
            progressUI.SetProgress(0);

            //var investigatedItemUi = directoryUI[dir.FullName];
            ClearMaps();
            //directoryUI.Add(dir.FullName,investigatedItemUi);
            backgroundWorker1.RunWorkerAsync(CreateMagicBGWData(dir));
        }
        private MagicBGWData CreateMagicBGWData(DirectoryInfo dir)
        {
            MagicBGWData datain = new MagicBGWData();
            datain.Directories = Utilities.GetDirectoriesFromDirectoryInfo(dir);
            datain.DrawAtEnd = true;
            datain.FolderDepth = 10;
            datain.NumberOfTotalFoldersToFocus = (int)numericUpDown1.Value;
            datain.SubFolderSizeMatchPercentage = 70;
            return datain;
        }
        private void adminLabel_MouseClick(object sender, MouseEventArgs e)
        {
            if (adminToolTipActive)
            {
                adminLabelTooltip.Hide(adminLabel);
                adminToolTipActive = false;
            }
            else
            {
                adminLabelTooltip.Show("Run with administrator privileges to access more features and folders", adminLabel);
                adminToolTipActive = true;
            }
        }
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {

        }
        private bool CheckIfWorkerIsRunning()
        {
            return backgroundWorker3.IsBusy;
        }
        private DirectoryItemUI FindRelevantUi(string path)
        {
            if(directoryUI.ContainsKey(path))
            {
                if (directoryUI[path].isParent)
                {
                    return directoryUI[path];
                }
                else
                {
                    var parentName = Utilities.GetDirectoryInfoFromPath(path).Parent.FullName;
                    if (directoryUI.ContainsKey(parentName))
                    {
                        return directoryUI[parentName];
                    }
                }
            }
            return null;
        }
        private void AddDirectoriesToDirectoryUI(DirectoryInfo parentDir,Tuple<Dictionary<long, DirectoryInfo>, List<long>> sizeMapandSizeList,int numberofDirsToShow)
        {
            var dirSizeMap = sizeMapandSizeList.Item1;
            var dirSizes = sizeMapandSizeList.Item2;
            if (dirSizes.Count < numberofDirsToShow) numberofDirsToShow = dirSizes.Count;
            if (childSizeMap.ContainsKey(parentDir.FullName)) return;
            childSizeMap.Add(parentDir.FullName, new Tuple<string, long>[numberofDirsToShow]);
            int itemcount = 0;
            for (int i = dirSizes.Count - 1; i > (dirSizes.Count - numberofDirsToShow - 1); --i)
            {
                var ldir = dirSizeMap[dirSizes[i]];
                var uiComponent = MakeDirectoryItemUi(ldir, Utilities.FormatBytes(dirSizes[i]), false);
                directoryUI.Add(ldir.FullName, uiComponent);
                childSizeMap[parentDir.FullName][itemcount] = new Tuple<string,long>(ldir.FullName,dirSizes[i]);
                itemcount++;
            }
        }
        private void AddChildDirectoriesToDirectoryUI(string parentDirPath, List<Tuple<string,long>> sizeMapandSizeList, int numberofDirsToShow)
        {
            if (sizeMapandSizeList.Count < numberofDirsToShow) numberofDirsToShow = sizeMapandSizeList.Count;
            if (childSizeMap.ContainsKey(parentDirPath)) return;
            childSizeMap.Add(parentDirPath, new Tuple<string, long>[numberofDirsToShow]);
            int itemcount = 0;
            for(int i = 0; i < numberofDirsToShow; ++i)
            {
                var uiComponent = MakeDirectoryItemUi(Utilities.GetDirectoryInfoFromPath(sizeMapandSizeList[i].Item1), Utilities.FormatBytes(sizeMapandSizeList[i].Item2), false);
                directoryUI.Add(sizeMapandSizeList[i].Item1, uiComponent);
                childSizeMap[parentDirPath][itemcount] = sizeMapandSizeList[i];
            }
        }
        private void AddParentDirectoryToDirectoryUI(Tuple<string,long> pathAndSize, bool isEnabled)
        {
            var uiComponent = MakeDirectoryItemUi(Utilities.GetDirectoryInfoFromPath(pathAndSize.Item1), Utilities.FormatBytes(pathAndSize.Item2), true);
            if(!directoryUI.ContainsKey(pathAndSize.Item1))directoryUI.Add(pathAndSize.Item1, uiComponent);
        }
        private void PrepareFoldersForFolder(DirectoryInfo dir , bool recursive,bool showMsgbox)
        {
            if (avoidedFolderNames.Contains(dir.Name)) return;
            var dirs = (showMsgbox ? Utilities.GetDirectoriesFromDirectoryInfoWithMessageBox(dir) : Utilities.GetDirectoriesFromDirectoryInfo(dir));
            if (dirs == null || dirs.Length == 0) return;

            var sizeTuple = Utilities.CreateDirSizeMap(dirs);

            AddDirectoriesToDirectoryUI(dir, sizeTuple, (int)numericUpDown1.Value);
            if (!recursive) return;
            foreach (var subdir in dirs)
            {
                PrepareFoldersForFolder(subdir,recursive, showMsgbox);
            }
        }
        private void DrawFoldersForFolder(DirectoryInfo dir , bool recursive)
        {
           if(childSizeMap.ContainsKey(dir.FullName)&& directoryUI.ContainsKey(dir.FullName))
           {
              foreach (var ui in childSizeMap[dir.FullName])
              {
                directoryUI[dir.FullName].AddLayout(directoryUI[ui.Item1]);
                if (recursive)
                 {
                   DrawFoldersForFolder(Utilities.GetDirectoryInfoFromPath(ui.Item1), recursive);
                 }
              }
           }
        }
        private bool CheckFoldersPrepared(DirectoryInfo dir,bool showMsgbox)
        {
            var dirs = (showMsgbox ? Utilities.GetDirectoriesFromDirectoryInfoWithMessageBox(dir) : Utilities.GetDirectoriesFromDirectoryInfo(dir));
            if (dirs == null) return false;
            if (childSizeMap.ContainsKey(dir.FullName)) return true;
            return false;
        }
        private void DrawParentFolders(string path, bool isEnabled)
        {
            ClearMaps();
            var givenPath = new DirectoryInfo(path);
            var subdirs = Utilities.GetDirectoriesFromDirectoryInfo(givenPath);
            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel1.SuspendLayout();
            foreach (var dir in subdirs)
            {
                DrawFolder(dir, "0", isEnabled);
            }
            flowLayoutPanel1.ResumeLayout();
        }
        private void DrawFolders(List<Tuple<string,long>> pathSizeList,bool isEnabled)
        {
            ClearMaps();
            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel1.SuspendLayout();
            foreach (var dirpair in pathSizeList)
            {
                DrawFolder(dirpair.Item1, Utilities.FormatBytes(dirpair.Item2), isEnabled);
            }
            flowLayoutPanel1.ResumeLayout();
        }
        private void DrawFolder(string path,string size,bool isEnabled)
        {
            var dir = Utilities.GetDirectoryInfoFromPath(path);
            if (avoidedFolderNames.Contains(dir.Name)) return;
            var uiComponent = MakeDirectoryItemUi(dir, size, true);
            uiComponent.Enabled = isEnabled;
            if (!directoryUI.ContainsKey(dir.FullName)) directoryUI.Add(dir.FullName, uiComponent);
            flowLayoutPanel1.Controls.Add(uiComponent);
        }
        private void DrawFolderFromDirectoryUI(string fullpath)
        {
            flowLayoutPanel1.Controls.Add(directoryUI[fullpath]);
        }
        private void DrawFolder(DirectoryInfo dir, string size, bool isEnabled)
        {
            if (avoidedFolderNames.Contains(dir.Name)) return;
            var uiComponent = MakeDirectoryItemUi(dir, size, true);
            uiComponent.Enabled = isEnabled;
            if (!directoryUI.ContainsKey(dir.FullName)) directoryUI.Add(dir.FullName, uiComponent);
            flowLayoutPanel1.Controls.Add(uiComponent);
        }
        private void ClearMaps()
        {
            directoryUI.Clear();
            childSizeMap.Clear();
            parentSizeMap.Clear();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var tuple = (Tuple<DirectoryInfo[],bool>)(e.Argument);
            var subdirs = tuple.Item1;
            if (subdirs == null || subdirs.Length == 0) return;
            for(int i = 0; i < subdirs.Length;++i)
            {             
                var dirSize = Utilities.FormatBytes(Utilities.GetDirSize(subdirs[i]));
                parentSizeMap.Add(subdirs[i].FullName, dirSize);
                lastWorkingItem = subdirs[i];
                backgroundWorker3.ReportProgress(Utilities.GetPercentage(i + 1, subdirs.Length));
            }
            for (int i = 0; i < subdirs.Length;++i)
            {
                lastWorkingItem = subdirs[i];
                backgroundWorker3.ReportProgress(Utilities.GetPercentage(i + 1, subdirs.Length) + 100);
                PrepareFoldersForFolder(subdirs[i], false,false);
            }
            e.Result = tuple;
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!directoryUI.ContainsKey(lastWorkingItem.FullName) || !parentSizeMap.ContainsKey(lastWorkingItem.FullName)) return;
            if (e.ProgressPercentage < 100)
            {
                progressUI.SetOperationName("Calculating Parent Directories...");
                progressUI.SetItemName(lastWorkingItem.Name);
                progressUI.SetProgress(e.ProgressPercentage);
                directoryUI[lastWorkingItem.FullName].SetDirSize(parentSizeMap[lastWorkingItem.FullName]);
            }
            else
            {
                progressUI.SetOperationName("Calculating Sub Directory Sizes...");
                progressUI.SetProgress(e.ProgressPercentage - 100);
                progressUI.SetItemName(lastWorkingItem.Name);
                directoryUI[lastWorkingItem.FullName].Enabled = true;
            }
        }
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var tuple = (Tuple<DirectoryInfo[], bool>)(e.Result);
            var result = tuple.Item1;
            var drawAtEnd = tuple.Item2;
            if (result == null) return;
            foreach(var dir in result)
            {
                if(directoryUI.ContainsKey(dir.FullName))directoryUI[dir.FullName].Enabled = true;
            }
            progressUI.SetOperationName("Scan Complete");
            progressUI.SetItemName("......");
            if(drawAtEnd)
            {
                flowLayoutPanel1.SuspendLayout();
                DrawFoldersForFolder(result[0], false);
                flowLayoutPanel1.ResumeLayout();
            }
            if (scannedAll) scannedAll = false;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            const int HEADERHEIGHT = 28;
            const int XPADDING = 43;
            const int YPADDING = 90;
            System.Drawing.Size padding = new Size();
            padding.Width = XPADDING;
            padding.Height = YPADDING;
            flowLayoutPanel1.Size = this.Size - padding;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var datain = (MagicBGWData)(e.Argument);
            var subdirs = datain.Directories;
            var folderDepth = datain.FolderDepth;
            var numberOfFoldersToFocus = datain.NumberOfTotalFoldersToFocus;
            var subPercentage = datain.SubFolderSizeMatchPercentage;

            if (subdirs == null || subdirs.Length == 0) return;
            List<Tuple<string, long>> sizeList = new List<Tuple<string, long>>();
            for (int i = 0; i < subdirs.Length; ++i)
            {
                if (avoidedFolderNames.Contains(subdirs[i].Name)) continue;
                var dirSizeLong = Utilities.GetDirSize(subdirs[i]);
                sizeList.Add(new Tuple<string,long>(subdirs[i].FullName, dirSizeLong));
                lastWorkingItem = subdirs[i];
                backgroundWorker1.ReportProgress(Utilities.GetPercentage(i + 1, subdirs.Length));
            }
            sizeList.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            List<Tuple<string, long>> culpritList = new List<Tuple<string, long>>();
            for(int i=0; i < numberOfFoldersToFocus && i < sizeList.Count;++i)
            {
                if (sizeList[i].Item2 == 0) continue;
                lastWorkingItem = Utilities.GetDirectoryInfoFromPath(sizeList[i].Item1);
                backgroundWorker1.ReportProgress(Utilities.GetPercentage(i + 1, subdirs.Length) + 100);
                var culprit = Utilities.RecursiveDirectoryCulpritFinder(sizeList[i].Item1, sizeList[i].Item2, datain.FolderDepth, datain.SubFolderSizeMatchPercentage);
                if (culprit == null) culprit = sizeList[i];
                culpritList.Add(culprit);
            }
            for (int i = 0; i < culpritList.Count; ++i)
            {
                var culpritDirInf = Utilities.GetDirectoryInfoFromPath(culpritList[i].Item1);
                AddParentDirectoryToDirectoryUI(culpritList[i],true);
                PrepareFoldersForFolder(culpritDirInf, false, false);
                lastWorkingItem = culpritDirInf;
                backgroundWorker1.ReportProgress(Utilities.GetPercentage(i + 1, culpritList.Count) + 200);
            }
            datain.CulpritList = culpritList;
            e.Result = datain;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage <= 100)
            {
                progressUI.SetOperationName("Calculating Parent Directories...");
                progressUI.SetItemName(lastWorkingItem.Name);
                progressUI.SetProgress(e.ProgressPercentage);
            }
            else if(e.ProgressPercentage > 100 && e.ProgressPercentage < 200)
            {
                progressUI.SetOperationName("Finding large folders...");
                progressUI.SetProgress(e.ProgressPercentage - 100);
                progressUI.SetItemName(lastWorkingItem.Name);
            }
            else
            {
                progressUI.SetOperationName("Preparing large folders...");
                progressUI.SetProgress(e.ProgressPercentage - 200);
                progressUI.SetItemName(lastWorkingItem.Name);
                directoryUI[lastWorkingItem.FullName].Enabled = true;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var datain = (MagicBGWData)(e.Result);
            var result = datain.CulpritList;
            var drawAtEnd = datain.DrawAtEnd;
            progressUI.SetOperationName("Done...");
            progressUI.SetItemName("......");
            if (drawAtEnd)
            {
                flowLayoutPanel1.SuspendLayout();
                DrawFolders(result, true);
                flowLayoutPanel1.ResumeLayout();
            }
        }

        private DirectoryItemUI MakeDirectoryItemUi(DirectoryInfo dir, string size, bool parent)
        {
            var uiComponent = new DirectoryItemUI(dir, size, true);
            uiComponent.ExtendedClickEvent += UiComponent_ExtendedClickEvent;
            uiComponent.ScanClickEvent += UiComponent_ScanClickEvent;
            uiComponent.RemoveClickEvent += UiComponent_DeleteClickEvent;
            uiComponent.InvestigateClickEvent += UiComponent_InvestigateClickEvent;
            uiComponent.Name = dir.FullName;
            return uiComponent;
        }
    }
}
