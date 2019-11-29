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
        public event ExtendClickedEventDelegate ScanClickEvent;
        public DirectoryInfo Dir;
        private bool isExtended = false;
        public DirectoryItemUI(DirectoryInfo directory,string size,bool isParent)
        {
            Dir = directory;
            InitializeComponent();
            label1.Text = directory.FullName;
            label2.Text = size;
            if (!isParent) button2.Visible = false;
        }
        public void SetDirSize(string size)
        {
            label2.Text = size;
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            if (isExtended)
            {
                button1.Text = "+";
                belowFlow.Controls.Clear();
                isExtended = false;
            }
            else
            {
                button1.Text = "-";
                ExtendedClickEvent?.Invoke(Dir);
                isExtended = true;
            }
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
    }
}
