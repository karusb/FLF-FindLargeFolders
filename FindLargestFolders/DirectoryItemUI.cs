using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FindLargestFolders
{
    public partial class DirectoryItemUI : UserControl
    {
        public delegate void ExtendClickedEventDelegate(DirectoryInfo dir);
        public event ExtendClickedEventDelegate ExtendedClickEvent;
        public event ExtendClickedEventDelegate InvestigateClickEvent;
        public event ExtendClickedEventDelegate ScanClickEvent;
        public event ExtendClickedEventDelegate RemoveClickEvent;
        public DirectoryInfo Dir;
        private bool isExtended = false;
        public bool isParent;
        public long DirBytes = 0;
        public DirectoryItemUI(DirectoryInfo directory,string size,bool parent)
        {
            Dir = directory;
            InitializeComponent();
            label1.Text = directory.FullName;
            label2.Text = size;
            button1.BackgroundImage = FindLargestFolders.Properties.Resources.icons8_plus_math_26;
            isParent = parent;
            fileCountLabel.Text = "0";
            try
            {
                fileCountLabel.Text = directory.GetFiles().Length.ToString();
            }
            catch
            {

            }
            if (!isParent) button2.Visible = false;
        }
        public void SetDirSize(long size)
        {
            label2.Text = Utilities.FormatBytes(size);
            DirBytes = size;
        }
        public void RevertExtendEventUi()
        {
            if (isExtended)
            {
                button1.BackgroundImage = FindLargestFolders.Properties.Resources.icons8_plus_math_26;
                label1.BackColor = Color.LightSlateGray;
                isExtended = false;
            }
            else
            {
                button1.BackgroundImage = FindLargestFolders.Properties.Resources.icons8_minus_24;
                label1.BackColor = Color.CadetBlue;
                isExtended = true;
            }
        }
        private void ExtendEvent()
        {
            if (isExtended)
            {
                button1.BackgroundImage = FindLargestFolders.Properties.Resources.icons8_plus_math_26;
                label1.BackColor = Color.LightSlateGray;
                belowFlow.Controls.Clear();
                isExtended = false;
            }
            else
            {
                button1.BackgroundImage = FindLargestFolders.Properties.Resources.icons8_minus_24;
                label1.BackColor = Color.CadetBlue;
                isExtended = true;
                ExtendedClickEvent?.Invoke(Dir);

            }
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            ExtendEvent();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            ScanClickEvent?.Invoke(Dir);
        }
        public void AddLayout(DirectoryItemUI ui)
        {
            belowFlow.Controls.Add(ui);
        }
        public void RemoveLayout(DirectoryItemUI ui)
        {
            belowFlow.Controls.Remove(ui);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            RemoveClickEvent?.Invoke(Dir);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void label1_DoubleClick(object sender, EventArgs e)
        {
            ExtendEvent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            InvestigateClickEvent?.Invoke(Dir);
        }

        private void label2_Click(object sender, EventArgs e)
        {
            ScanClickEvent?.Invoke(Dir);
        }

        private void fileCountLabelPanel_Click(object sender, EventArgs e)
        {
            Utilities.OpenFolderInExplorer(label1.Text);
        }
    }
}
