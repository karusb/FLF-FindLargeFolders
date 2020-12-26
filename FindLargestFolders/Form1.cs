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

namespace FindLargestFolders
{

    public partial class Form1 : Form
    {
        Dictionary<string, DirectoryItemUI> directoryUI = new Dictionary<string, DirectoryItemUI>();
        Dictionary<string, string> parentSizeMap = new Dictionary<string, string>();
        Dictionary<string, Tuple<string, long>[]> childSizeMap = new Dictionary<string, Tuple<string, long>[]>();
        List<string> avoidedFolderNames = new List<string> { "Documents and Settings", "System Volume Information" };
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
            var givenPath = new DirectoryInfo(comboBox1.Text);
            flowLayoutPanel1.Controls.Clear();
            foreach (var dir in givenPath.GetDirectories())
            {
                if (avoidedFolderNames.Contains(dir.Name))continue;
                var uiComponent = MakeDirectoryItemUi(dir, "0", true);
                directoryUI.Add(dir.FullName, uiComponent);
                flowLayoutPanel1.Controls.Add(uiComponent);
            }

        }

        private void UiComponent_ScanClickEvent(DirectoryInfo dir)
        {
            var dirSize = Utilities.FormatBytes(Utilities.GetDirSize(dir));
            directoryUI[dir.FullName].SetDirSize(dirSize);
        }

        private void UiComponent_ExtendedClickEvent(DirectoryInfo dir)
        {
            var tempdirs = Utilities.GetDirectoriesFromDirectoryInfoWithMessageBox(dir);
            if (tempdirs == null) return;
            if (!CheckFoldersPrepared(dir,true))

            {
                DirectoryInfo[] dirinfo = new DirectoryInfo[1];
                dirinfo[0] = dir;
                Tuple<DirectoryInfo[], bool> drawInfoPair = new Tuple<DirectoryInfo[], bool>(dirinfo,true);
                directoryUI[dir.FullName].Enabled = false;
                backgroundWorker3.RunWorkerAsync(drawInfoPair);
                //PrepareFoldersForFolder(dir, false,true);
            }
            else
            {
                DrawFoldersForFolder(dir, false);
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
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {

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
                var uiComponent = new DirectoryItemUI(ldir, Utilities.FormatBytes(dirSizes[i]), false);
                uiComponent.ExtendedClickEvent += UiComponent_ExtendedClickEvent;
                uiComponent.ScanClickEvent += UiComponent_ScanClickEvent;
                uiComponent.RemoveClickEvent += UiComponent_DeleteClickEvent;
                uiComponent.Name = ldir.FullName;
                directoryUI.Add(ldir.FullName, uiComponent);
                childSizeMap[parentDir.FullName][itemcount] = new Tuple<string,long>(ldir.FullName,dirSizes[i]);
                itemcount++;
            }
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

        private void adminLabel_MouseClick(object sender, MouseEventArgs e)
        {
            if(adminToolTipActive)
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

        private void scanAllButton_Click(object sender, EventArgs e)
        {
            
            var givenPath = new DirectoryInfo(comboBox1.Text);
            var subdirs = Utilities.GetDirectoriesFromDirectoryInfo(givenPath);
            flowLayoutPanel1.Controls.Clear();

            int dirCount = 0;
            foreach (var dir in subdirs)
            {
                dirCount++;

                var uiComponent = MakeDirectoryItemUi(dir, "0", true);
                uiComponent.Enabled = false;
                if(!directoryUI.ContainsKey(dir.FullName))directoryUI.Add(dir.FullName, uiComponent);

                flowLayoutPanel1.Controls.Add(uiComponent);
            }
           
            progressUI.SetProgress(0);
            backgroundWorker3.RunWorkerAsync(new Tuple<DirectoryInfo[],bool>(subdirs,false));
            

            scannedAll = true;
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
            if (e.ProgressPercentage < 100)
            {
                progressUI.SetOperationName("Calculating Parent Directories...");
                progressUI.SetItemName(lastWorkingItem.Name);
                progressUI.SetProgress(e.ProgressPercentage);
                directoryUI[lastWorkingItem.FullName].SetDirSize(parentSizeMap[lastWorkingItem.FullName]);
            }
            else
            {
                progressUI.SetOperationName("Calculating SubDirectory Sizes...");
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
                directoryUI[dir.FullName].Enabled = true;
            }
            progressUI.SetOperationName("Scan Complete");
            progressUI.SetItemName("......");
            if(drawAtEnd)
            {
                DrawFoldersForFolder(result[0], false);
            }
        }
        private DirectoryItemUI MakeDirectoryItemUi(DirectoryInfo dir, string size, bool parent)
        {
            var uiComponent = new DirectoryItemUI(dir, size, true);
            uiComponent.ExtendedClickEvent += UiComponent_ExtendedClickEvent;
            uiComponent.ScanClickEvent += UiComponent_ScanClickEvent;
            uiComponent.RemoveClickEvent += UiComponent_DeleteClickEvent;
            uiComponent.Name = dir.FullName;
            return uiComponent;
        }
    }
}
