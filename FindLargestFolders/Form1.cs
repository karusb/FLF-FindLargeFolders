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

namespace FindLargestFolders
{

    public partial class Form1 : Form
    {
        Dictionary<DirectoryInfo, DirectoryItemUI> directoryUI = new Dictionary<DirectoryInfo, DirectoryItemUI>();
        
        public Form1()
        {
            InitializeComponent();
            var drives = Utilities.GetDrives();
            comboBox1.Text = drives[0].Name;
            foreach (var drive in drives)
            {
                comboBox1.Items.Add(drive.Name);
            }
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var givenPath = new DirectoryInfo(comboBox1.Text);
            flowLayoutPanel1.Controls.Clear();
            foreach (var dir in givenPath.GetDirectories())
            {
                var uiComponent = new DirectoryItemUI(dir, "0",true);
                uiComponent.ExtendedClickEvent += UiComponent_ExtendedClickEvent;
                uiComponent.ScanClickEvent += UiComponent_ScanClickEvent;
                uiComponent.Name = dir.FullName;
                directoryUI.Add(dir, uiComponent);
                flowLayoutPanel1.Controls.Add(uiComponent);
            }

        }

        private void UiComponent_ScanClickEvent(DirectoryInfo dir)
        {
            DirectoryInfo[] dirs;
            try
            {
                dirs = dir.GetDirectories();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.Source,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var dirSize = Utilities.FormatBytes(Utilities.GetDirSize(dir));
            directoryUI[dir].SetDirSize(dirSize);
        }

        private void UiComponent_ExtendedClickEvent(DirectoryInfo dir)
        {
            int numberofDirsToShow = (int)numericUpDown1.Value;
            Dictionary<long, DirectoryInfo> dirSizeMap = new Dictionary<long, DirectoryInfo>();
            List<long> dirSizes = new List<long>();
            DirectoryInfo[] dirs;
            try
            {
                dirs = dir.GetDirectories();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.Source,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
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
            if (dirSizes.Count < numberofDirsToShow) numberofDirsToShow = dirSizes.Count;
            flowLayoutPanel1.SuspendLayout();
            for(int i= dirSizes.Count -1; i > (dirSizes.Count - numberofDirsToShow-1);--i)
            {
                var ldir = dirSizeMap[dirSizes[i]];
                var uiComponent = new DirectoryItemUI(ldir, Utilities.FormatBytes(dirSizes[i]),false);
                uiComponent.ExtendedClickEvent += UiComponent_ExtendedClickEvent;
                uiComponent.ScanClickEvent += UiComponent_ScanClickEvent;
                uiComponent.Name = dir.FullName;
                directoryUI.Add(ldir, uiComponent);
                directoryUI[dir].AddLayout(uiComponent);
            }
            flowLayoutPanel1.ResumeLayout();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {

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
    }
}
