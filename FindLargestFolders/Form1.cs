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
        Dictionary<string, long> parentSizeMap = new Dictionary<string, long>();
        Dictionary<string, Tuple<string, long>[]> childSizeMap = new Dictionary<string, Tuple<string, long>[]>();
        List<string> avoidedFolderNames = new List<string> { "Documents and Settings", "System Volume Information", "Windows" };
        bool adminToolTipActive = false;
        bool maxfolderstooltipActive = false;
        DirectoryInfo lastWorkingItem;
        DirectoryInfo lastWorkingParentDirectory;
        ControlBar activeControlBar;
        ControlBar.ControlBarOptions activeControlBarOpts = new ControlBar.ControlBarOptions();
        bool scannedAll = false;
        long accessOldThresholdInDays = 90;

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
            lastWorkingParentDirectory = Utilities.GetDirectoryInfoFromPath(comboBox1.Text);
            flowLayoutPanel1.Controls.Clear();
            DrawParentFolders(comboBox1.Text, true);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();

            lastWorkingParentDirectory = Utilities.GetDirectoryInfoFromPath(folderBrowserDialog1.SelectedPath);
            flowLayoutPanel1.Controls.Clear();
            DrawParentFolders(folderBrowserDialog1.SelectedPath, true);
        }
        private void investigationButton_Click(object sender, EventArgs e)
        {
            progressUI.SetProgress(0);
            lastWorkingParentDirectory = Utilities.GetDirectoryInfoFromPath(comboBox1.Text);
            if (backgroundWorker2.IsBusy)
                backgroundWorker2.CancelAsync();
            if (backgroundWorker3.IsBusy)
                backgroundWorker3.CancelAsync();
            backgroundWorker1.RunWorkerAsync(CreateMagicBGWData(Utilities.GetDirectoryInfoFromPath(comboBox1.Text)));
        }
        private void scanAllButton_Click(object sender, EventArgs e)
        {
            scannedAll = true;
            if (backgroundWorker2.IsBusy)
                backgroundWorker2.CancelAsync();
            if (backgroundWorker1.IsBusy)
                backgroundWorker1.CancelAsync();
            ClearMaps();
            flowLayoutPanel1.Controls.Clear();
            DrawParentFolders(comboBox1.Text, false);
            lastWorkingParentDirectory = Utilities.GetDirectoryInfoFromPath(comboBox1.Text);
            var subdirs = Utilities.GetDirectoriesFromDirectoryInfo(new DirectoryInfo(comboBox1.Text));
            progressUI.SetProgress(0);

            backgroundWorker3.RunWorkerAsync(new Tuple<DirectoryInfo[], bool>(subdirs, false));
        }
        private void lastAccessScanButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker3.IsBusy)
                backgroundWorker3.CancelAsync();
            if (backgroundWorker1.IsBusy)
                backgroundWorker1.CancelAsync();
            progressUI.SetProgress(0);
            TimePicker timePicker = new TimePicker();
            timePicker.ShowDialog();
            var res = timePicker.GetResult();
            if (res.Ok)
            {
                accessOldThresholdInDays = res.daysOld;
                lastWorkingParentDirectory = Utilities.GetDirectoryInfoFromPath(comboBox1.Text);
                backgroundWorker2.RunWorkerAsync(CreateMagicBGWData(Utilities.GetDirectoryInfoFromPath(comboBox1.Text)));
            }
        }
        private void UiComponent_ScanClickEvent(DirectoryInfo dir)
        {
            directoryUI[dir.FullName].SetDirSize(Utilities.GetDirSize(dir));
        }

        private void UiComponent_ExtendedClickEvent(DirectoryInfo dir)
        {
            var tempdirs = Utilities.GetDirectoriesFromDirectoryInfoWithMessageBox(dir);
            if (tempdirs == null)
            {
                FindRelevantUi(dir.FullName).RevertExtendEventUi();
                return;
            }
            if (CheckIfWorkerIsRunning() && !scannedAll)
            {
                MessageBox.Show("Please wait for the process to finish", "Busy",
    MessageBoxButtons.OK, MessageBoxIcon.Error);
                FindRelevantUi(dir.FullName).RevertExtendEventUi();
                return;
            }
            if (!CheckFoldersPrepared(dir, true))

            {
                DirectoryInfo[] dirinfo = new DirectoryInfo[1];
                dirinfo[0] = dir;
                Tuple<DirectoryInfo[], bool> drawInfoPair = new Tuple<DirectoryInfo[], bool>(dirinfo, true);
                directoryUI[dir.FullName].Enabled = false;
                backgroundWorker3.RunWorkerAsync(drawInfoPair); // Invalid Operation Exception
            }
            else
            {
                flowLayoutPanel1.SuspendLayout();
                DrawFoldersForFolder(dir, false);
                flowLayoutPanel1.ResumeLayout();
            }

        }
        private void DeleteDirectory(DirectoryInfo dir)
        {
            var result = MessageBox.Show("Do you want to PERMANENTLY delete folder \n\r" + dir.Name, "PERMANENTLY Deleting File",
       MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Cancel) { return; }
            else
            {
                try
                {
                    Directory.Delete(dir.FullName, true);
                    RemoveItemFromUI(dir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void UiComponent_DeleteClickEvent(DirectoryInfo dir)
        {
            DeleteDirectory(dir);
        }
        private void UiComponent_InvestigateClickEvent(DirectoryInfo dir)
        {
            flowLayoutPanel1.Controls.Clear();
            //ClearMaps();
            directoryUI.Clear();
            //
            progressUI.SetProgress(0);
            lastWorkingParentDirectory = dir;
            backgroundWorker1.RunWorkerAsync(CreateMagicBGWData(dir));
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
        private void RemoveItemFromUI(DirectoryInfo dir)
        {
            var relui = FindRelevantUi(dir.FullName);
            if (!directoryUI.ContainsKey(dir.Parent.FullName))
            {
                if (flowLayoutPanel1.Controls.Contains(relui))
                {
                    flowLayoutPanel1.Controls.Remove(relui);
                }
            }
            else if (directoryUI.ContainsKey(dir.FullName))
            {

                if (flowLayoutPanel1.Controls.Contains(directoryUI[dir.Parent.FullName]))
                {
                    directoryUI[dir.Parent.FullName].RemoveLayout(relui);
                }
                directoryUI.Remove(dir.FullName);
            }
            if (parentSizeMap.ContainsKey(dir.Parent.FullName))
            {
                parentSizeMap.Remove(dir.Parent.FullName);
            }
            if (childSizeMap.ContainsKey(dir.Parent.FullName))
            {
                childSizeMap.Remove(dir.Parent.FullName);
            }
        }
        private bool CheckIfWorkerIsRunning()
        {
            return backgroundWorker3.IsBusy;
        }
        private DirectoryItemUI FindRelevantUi(string path)
        {
            if (directoryUI.ContainsKey(path))
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
        private void AddDirectoryToDirectoryUI(Tuple<string, long> pathAndSize, bool isParent, bool isEnabled)
        {
            var uiComponent = MakeDirectoryItemUi(Utilities.GetDirectoryInfoFromPath(pathAndSize.Item1), Utilities.FormatBytes(pathAndSize.Item2), true);
            uiComponent.Enabled = isEnabled;
            if (!directoryUI.ContainsKey(pathAndSize.Item1)) directoryUI.Add(pathAndSize.Item1, uiComponent);
        }
        private void AddChildDirectoriesToDirectoryUI(string parentDirPath, List<Tuple<string, long>> sizeMapandSizeList, int numberofDirsToShow)
        {
            if (sizeMapandSizeList.Count < numberofDirsToShow) numberofDirsToShow = sizeMapandSizeList.Count;
            if (childSizeMap.ContainsKey(parentDirPath)) return;
            childSizeMap.Add(parentDirPath, new Tuple<string, long>[numberofDirsToShow]);
            for (int i = 0; i < numberofDirsToShow; ++i)
            {
                AddDirectoryToDirectoryUI(sizeMapandSizeList[i], false, true);
                childSizeMap[parentDirPath][i] = sizeMapandSizeList[i];
            }
        }
        private void AddParentDirectoryToDirectoryUI(Tuple<string, long> pathAndSize, bool isEnabled)
        {
            AddDirectoryToDirectoryUI(pathAndSize, true, isEnabled);
        }

        private void PrepareFoldersForFolder(DirectoryInfo dir, bool recursive, bool showMsgbox)
        {
            if (avoidedFolderNames.Contains(dir.Name)) return;
            var dirs = (showMsgbox ? Utilities.GetDirectoriesFromDirectoryInfoWithMessageBox(dir) : Utilities.GetDirectoriesFromDirectoryInfo(dir));
            if (dirs == null || dirs.Length == 0) return;

            var sizemap = Utilities.CreateSizeMap(dir.FullName);
            AddChildDirectoriesToDirectoryUI(dir.FullName, sizemap, (int)numericUpDown1.Value);
            if (!recursive) return;
            foreach (var subdir in dirs)
            {
                PrepareFoldersForFolder(subdir, recursive, showMsgbox);
            }
        }
        private void DrawFoldersForFolder(DirectoryInfo dir, bool recursive)
        {
            if (childSizeMap.ContainsKey(dir.FullName) && directoryUI.ContainsKey(dir.FullName))
            {
                foreach (var ui in childSizeMap[dir.FullName])
                {

                    if (!directoryUI.ContainsKey(ui.Item1))
                    {
                        var uiComponent = MakeDirectoryItemUi(dir, Utilities.FormatBytes(ui.Item2), false);
                        directoryUI.Add(ui.Item1, uiComponent);                        
                    }
                    directoryUI[dir.FullName].AddLayout(directoryUI[ui.Item1]);
                    if (recursive)
                    {
                        DrawFoldersForFolder(Utilities.GetDirectoryInfoFromPath(ui.Item1), recursive);
                    }
                }
            }
        }
        private bool CheckFoldersPrepared(DirectoryInfo dir, bool showMsgbox)
        {
            var dirs = (showMsgbox ? Utilities.GetDirectoriesFromDirectoryInfoWithMessageBox(dir) : Utilities.GetDirectoriesFromDirectoryInfo(dir));
            if (dirs == null) return false;
            if (childSizeMap.ContainsKey(dir.FullName) || parentSizeMap.ContainsKey(dir.FullName)) return true;
            return false;
        }
        private void DrawParentFolders(string path, bool isEnabled)
        {
            //ClearMaps();
            //directoryUI.Clear();
            //
            var givenPath = new DirectoryInfo(path);
            var subdirs = Utilities.GetDirectoriesFromDirectoryInfo(givenPath);

            flowLayoutPanel1.SuspendLayout();
            if (subdirs != null)
            {
                foreach (var dir in subdirs)
                {
                    if (avoidedFolderNames.Contains(dir.Name)) continue;
                    long folderSize = 0;
                    if (parentSizeMap.ContainsKey(dir.FullName))
                        folderSize = Convert.ToInt64(parentSizeMap[dir.FullName]);
                    else if (childSizeMap.ContainsKey(dir.Parent.FullName))
                    {
                        foreach (var child in childSizeMap[dir.Parent.FullName])
                        {
                            if (child.Item1 == dir.FullName)
                            {
                                folderSize = child.Item2;
                                break;
                            }
                        }
                    }
                    else if (childSizeMap.ContainsKey(dir.FullName))
                    {
                        foreach (var child in childSizeMap[dir.FullName])
                        {
                            folderSize += child.Item2;
                        }
                    }
                    DrawFolder(new Tuple<string, long>(dir.FullName, folderSize), isEnabled);
                }
            }
            flowLayoutPanel1.ResumeLayout();
        }
        private void DrawFolders(List<Tuple<string, long>> pathSizeList, bool isEnabled)
        {
            flowLayoutPanel1.SuspendLayout();
            foreach (var dirpair in pathSizeList)
            {
                DrawFolder(dirpair, isEnabled);
            }
            flowLayoutPanel1.ResumeLayout();
        }
        private void DrawFolder(Tuple<string, long> pathandSize, bool isEnabled)
        {
            var dir = Utilities.GetDirectoryInfoFromPath(pathandSize.Item1);
            if (avoidedFolderNames.Contains(dir.Name)) return;
            //AddDirectoryToDirectoryUI(pathandSize, true, isEnabled);
            var uiComponent = MakeDirectoryItemUi(dir, Utilities.FormatBytes(pathandSize.Item2), true);
            uiComponent.Enabled = isEnabled;
            if (!directoryUI.ContainsKey(dir.FullName)) directoryUI.Add(dir.FullName, uiComponent);
            flowLayoutPanel1.Controls.Add(uiComponent);
        }
        private void DrawControlBar(DirectoryInfo parentDir)
        {
            if (flowLayoutPanel1.Controls.Contains(activeControlBar))
                flowLayoutPanel1.Controls.Remove(activeControlBar);
            var uiComponent = MakeControlBar(parentDir, activeControlBarOpts);

            activeControlBar = uiComponent;
            flowLayoutPanel1.Controls.Add(activeControlBar);
        }
        private void ClearMaps()
        {
            directoryUI.Clear();
            childSizeMap.Clear();
            parentSizeMap.Clear();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var tuple = (Tuple<DirectoryInfo[], bool>)(e.Argument);
            var subdirs = tuple.Item1;
            if (subdirs == null || subdirs.Length == 0) return;
            for (int i = 0; i < subdirs.Length; ++i)
            {
                if (avoidedFolderNames.Contains(subdirs[i].Name)) continue;
                parentSizeMap.Add(subdirs[i].FullName, Utilities.GetDirSize(subdirs[i]));
                lastWorkingItem = subdirs[i];
                backgroundWorker3.ReportProgress(Utilities.GetPercentage(i + 1, subdirs.Length));
            }
            for (int i = 0; i < subdirs.Length; ++i)
            {
                PrepareFoldersForFolder(subdirs[i], false, false);
                lastWorkingItem = subdirs[i];
                backgroundWorker3.ReportProgress(Utilities.GetPercentage(i + 1, subdirs.Length) + 100);
            }
            e.Result = tuple;
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage < 100)
            {
                progressUI.SetOperationName("Calculating Parent Directories...");
                progressUI.SetItemName(lastWorkingItem.Name);
                progressUI.SetProgress(e.ProgressPercentage);
                if (directoryUI.ContainsKey(lastWorkingItem.FullName) || parentSizeMap.ContainsKey(lastWorkingItem.FullName)) directoryUI[lastWorkingItem.FullName].SetDirSize(parentSizeMap[lastWorkingItem.FullName]);
            }
            else
            {
                progressUI.SetOperationName("Calculating Sub Directory Sizes...");
                progressUI.SetProgress(e.ProgressPercentage - 100);
                progressUI.SetItemName(lastWorkingItem.Name);
                if (directoryUI.ContainsKey(lastWorkingItem.FullName)) directoryUI[lastWorkingItem.FullName].Enabled = true;
            }
        }
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var tuple = (Tuple<DirectoryInfo[], bool>)(e.Result);
            var result = tuple.Item1;
            var drawAtEnd = tuple.Item2;
            if (result == null) return;
            foreach (var dir in result)
            {
                if (directoryUI.ContainsKey(dir.FullName))
                {
                    directoryUI[dir.FullName].Enabled = true;
                    if (parentSizeMap.ContainsKey(dir.FullName)) directoryUI[dir.FullName].SetDirSize(parentSizeMap[dir.FullName]);
                    else if(dir.Parent != null)
                    {
                        if(childSizeMap.ContainsKey(dir.Parent.FullName))
                        {
                            foreach(var child in childSizeMap[dir.Parent.FullName])
                            {
                                if (child.Item1 == dir.FullName)
                                    directoryUI[dir.FullName].SetDirSize(child.Item2);
                            }
                        }
                    }
                }
            }           
            progressUI.SetOperationName("Scan Complete");
            progressUI.SetItemName("......");
            if (drawAtEnd)
            {
                flowLayoutPanel1.SuspendLayout();
                DrawFoldersForFolder(result[0], false);
                flowLayoutPanel1.ResumeLayout();
            }
            if (scannedAll)
            {
                SortDescendingDirectoryUI();
                scannedAll = false;
            }
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
                sizeList.Add(new Tuple<string, long>(subdirs[i].FullName, dirSizeLong));
                lastWorkingItem = subdirs[i];
                backgroundWorker1.ReportProgress(Utilities.GetPercentage(i + 1, subdirs.Length));
            }
            sizeList.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            List<Tuple<string, long>> culpritList = new List<Tuple<string, long>>();
            for (int i = 0; i < numberOfFoldersToFocus && i < sizeList.Count; ++i)
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
                AddParentDirectoryToDirectoryUI(culpritList[i], true);
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
            else if (e.ProgressPercentage > 100 && e.ProgressPercentage < 200)
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
                if (directoryUI.ContainsKey(lastWorkingItem.FullName))
                {
                    directoryUI[lastWorkingItem.FullName].Enabled = true;
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var datain = (MagicBGWData)(e.Result);
            if (datain.CulpritList == null)
                MessageBox.Show("Investigation resulted in 0 folders.", "No Result",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            var result = datain.CulpritList;
            var drawAtEnd = datain.DrawAtEnd;
            progressUI.SetOperationName("Done...");
            progressUI.SetItemName("......");
            if (drawAtEnd)
            {
                flowLayoutPanel1.SuspendLayout();
                ClearMaps();
                flowLayoutPanel1.Controls.Clear();
                DrawControlBar(lastWorkingParentDirectory);
                DrawFolders(result, true);
                flowLayoutPanel1.ResumeLayout();
            }
        }
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            var datain = (MagicBGWData)(e.Argument);
            var subdirs = datain.Directories;
            var folderDepth = datain.FolderDepth;
            var numberOfFoldersToFocus = datain.NumberOfTotalFoldersToFocus;
            var subPercentage = datain.SubFolderSizeMatchPercentage;

            var lastAccessedDayThreshold = accessOldThresholdInDays;


            if (subdirs == null || subdirs.Length == 0) return;
            List<Tuple<DirectoryInfo, long>> sizeList = new List<Tuple<DirectoryInfo, long>>();
            Dictionary<DirectoryInfo, List<FileInfo>> lastAccessed = new Dictionary<DirectoryInfo, List<FileInfo>>();
            List<DirectoryInfo> flaggedList = new List<DirectoryInfo>();

            for (int i = 0; i < subdirs.Length; ++i)
            {
                lastWorkingItem = subdirs[i];
                backgroundWorker2.ReportProgress(Utilities.GetPercentage(i + 1, subdirs.Length));

                if (avoidedFolderNames.Contains(subdirs[i].Name)) continue;
                lastAccessed = Utilities.MergeDictionary(Utilities.RecursiveLastAccessedFolderFinder(subdirs[i], (int)lastAccessedDayThreshold), lastAccessed);

            }

            for(int i = 0; i < lastAccessed.Keys.Count; ++i)
            {
                lastWorkingItem = lastAccessed.ElementAt(i).Key;
                backgroundWorker2.ReportProgress(Utilities.GetPercentage(i + 1, lastAccessed.Keys.Count) + 100);
                if (avoidedFolderNames.Contains(lastWorkingItem.Name)) continue;
                if (lastAccessed.ContainsKey(lastWorkingItem))
                {
                    var lastAccessedFiles = lastAccessed[lastWorkingItem].ToArray();
                    long dirSize = 0;
                    if (lastAccessedFiles.Length == 0)
                    {
                        if (!flaggedList.Contains(lastWorkingItem))
                        {
                            flaggedList.Add(lastWorkingItem);
                            dirSize = Utilities.GetDirSize(lastWorkingItem);
                        }
                    }
                    else
                        dirSize = Utilities.TotalFileSize(lastAccessedFiles);

                    sizeList.Add(new Tuple<DirectoryInfo, long>(lastWorkingItem, dirSize));
                }
            }
            sizeList.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            List<Tuple<string, long>> culpritList = new List<Tuple<string, long>>();

            for (int i = 0; i < sizeList.Count && i < numberOfFoldersToFocus; ++i)
            {

                culpritList.Add(new Tuple<string, long>(sizeList[i].Item1.FullName, sizeList[i].Item2));
                var culpritDirInf = Utilities.GetDirectoryInfoFromPath(culpritList[i].Item1);
                lastWorkingItem = culpritDirInf;
                backgroundWorker2.ReportProgress(Utilities.GetPercentage(i + 1, sizeList.Count) + 200);

                AddParentDirectoryToDirectoryUI(culpritList[i], true);
                PrepareFoldersForFolder(culpritDirInf, false, false);
            }
            datain.CulpritList = culpritList;
            e.Result = datain;
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage <= 100)
            {
                progressUI.SetOperationName("Finding Old Files...");
                progressUI.SetItemName(lastWorkingItem.Name);
                progressUI.SetProgress(e.ProgressPercentage);
            }
            else if (e.ProgressPercentage > 100 && e.ProgressPercentage < 200)
            {
                progressUI.SetOperationName("Calculating Sizes...");
                progressUI.SetProgress(e.ProgressPercentage - 100);
                progressUI.SetItemName(lastWorkingItem.Name);
            }
            else
            {
                progressUI.SetOperationName("Preparing folders...");
                progressUI.SetProgress(e.ProgressPercentage - 200);
                progressUI.SetItemName(lastWorkingItem.Name);
                if (directoryUI.ContainsKey(lastWorkingItem.FullName))
                {
                    directoryUI[lastWorkingItem.FullName].Enabled = true;
                }
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var datain = (MagicBGWData)(e.Result);
            if (datain.CulpritList == null)
                MessageBox.Show("Investigation resulted in 0 folders.", "No Result",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            var result = datain.CulpritList;
            var drawAtEnd = datain.DrawAtEnd;
            progressUI.SetOperationName("Done...");
            progressUI.SetItemName("......");
            if (drawAtEnd)
            {
                flowLayoutPanel1.SuspendLayout();
                ClearMaps();
                flowLayoutPanel1.Controls.Clear();
                DrawControlBar(lastWorkingParentDirectory);
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
        private ControlBar MakeControlBar(DirectoryInfo parentDir, ControlBar.ControlBarOptions options)
        {
            var controlUi = new ControlBar(parentDir, options);
            controlUi.ParentClickEvent += ControlUi_ParentClickEvent;
            controlUi.BackClickEvent += ControlUi_BackClickEvent;
            controlUi.RefreshClickEvent += ControlUi_RefreshClickEvent;
            controlUi.InvestigateClickEvent += ControlUi_InvestigateClickEvent;
            controlUi.DeleteClickEvent += ControlUi_DeleteClickEvent;
            return controlUi;
        }

        private void ControlUi_DeleteClickEvent(DirectoryInfo dir)
        {
            DeleteDirectory(dir);
            flowLayoutPanel1.Controls.Clear();
            if (dir.Parent != null)
            {
                DrawControlBar(dir.Parent);
                DrawParentFolders(dir.Parent.FullName, true);
            }
        }

        private void ControlUi_InvestigateClickEvent(DirectoryInfo dir)
        {
            progressUI.SetProgress(0);
            lastWorkingParentDirectory = dir;
            backgroundWorker1.RunWorkerAsync(CreateMagicBGWData(dir));
        }

        private void ControlUi_RefreshClickEvent(DirectoryInfo dir)
        {
            ClearMaps();
            flowLayoutPanel1.Controls.Clear();
            DrawControlBar(dir);
            DrawParentFolders(dir.FullName, false);
            lastWorkingParentDirectory = dir;
            var subdirs = Utilities.GetDirectoriesFromDirectoryInfo(dir);
            progressUI.SetProgress(0);
            backgroundWorker3.RunWorkerAsync(new Tuple<DirectoryInfo[], bool>(subdirs, false));
        }

        private void ControlUi_BackClickEvent(DirectoryInfo dir)
        {
            flowLayoutPanel1.Controls.Clear();
            if (dir!= null && dir.Parent != null)
                DrawControlBar(dir);
            DrawParentFolders(dir.FullName, true);
        }

        private void ControlUi_ParentClickEvent(DirectoryInfo dir)
        {
            flowLayoutPanel1.Controls.Clear();
            if (dir != null && dir.Parent != null)
                DrawControlBar(dir);
            DrawParentFolders(dir.FullName, true);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lastWorkingParentDirectory = Utilities.GetDirectoryInfoFromPath(comboBox1.Text);
            flowLayoutPanel1.Controls.Clear();
            DrawControlBar(lastWorkingParentDirectory);
            DrawParentFolders(comboBox1.Text, true);
        }

        private void label2_MouseClick(object sender, MouseEventArgs e)
        {
            if (maxfolderstooltipActive)
            {
                maxfolderstooltip.Hide(label2);
                maxfolderstooltipActive = false;
            }
            else
            {
                maxfolderstooltip.Show("Determines the maximum amount of folders to show. This also changes the number of folders to investigate.", label2);
                maxfolderstooltipActive = true;
            }
        }
        private void SortDescendingDirectoryUI()
        {
            var sizeMap = CreateSizeMapFromScannedDirectoryUI(directoryUI);
            flowLayoutPanel1.Controls.Clear();
            for (int i = 0; i < sizeMap.Count; ++i)
            {
                if(parentSizeMap.ContainsKey(sizeMap[i].Item1))
                    flowLayoutPanel1.Controls.Add(directoryUI[sizeMap[i].Item1]);
            }
        }
        public static List<Tuple<string, long>> CreateSizeMapFromScannedDirectoryUI(Dictionary<string, DirectoryItemUI> directories)
        {
            List<Tuple<string, long>> sizeList = new List<Tuple<string, long>>();
            foreach (var ui in directories)
            {
                sizeList.Add(new Tuple<string, long>(ui.Key, ui.Value.DirBytes));
            }
            sizeList.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            return sizeList;
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            DragDropEffects effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (Directory.Exists(path))
                {
                    effects = DragDropEffects.Copy;
                    lastWorkingParentDirectory = Utilities.GetDirectoryInfoFromPath(path);
                    flowLayoutPanel1.Controls.Clear();
                    DrawParentFolders(path, true);
                }

            }
            e.Effect = effects;
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Link;
        }

        private void flowLayoutPanel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Link;
        }

        private void flowLayoutPanel1_DragDrop(object sender, DragEventArgs e)
        {
            DragDropEffects effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (Directory.Exists(path))
                {
                    effects = DragDropEffects.Copy;
                    lastWorkingParentDirectory = Utilities.GetDirectoryInfoFromPath(path);
                    flowLayoutPanel1.Controls.Clear();
                    DrawParentFolders(path, true);
                }

            }
            e.Effect = effects;
        }

    }
}
