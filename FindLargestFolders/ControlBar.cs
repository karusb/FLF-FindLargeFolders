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

    public partial class ControlBar : UserControl
    {
        public delegate void ExtendClickedEventDelegate(DirectoryInfo dir);
        public event ExtendClickedEventDelegate BackClickEvent;
        public event ExtendClickedEventDelegate ParentClickEvent;
        public event ExtendClickedEventDelegate RefreshClickEvent;
        public event ExtendClickedEventDelegate InvestigateClickEvent;
        public event ExtendClickedEventDelegate DeleteClickEvent;
        public DirectoryInfo Dir;
        public ControlBarOptions Opts;
        public ControlBar(DirectoryInfo ParentDir, ControlBarOptions options)
        {
            InitializeComponent();
            Dir = ParentDir;
            Opts = options;
            SetOptions();
            parentLabel.Text = Dir.FullName;
        }
        public class ControlBarOptions
        {
            public bool BackButtonEnabled = true;
            public bool UpButtonEnabled = true;
            public bool RefreshButtonEnabled = true;
            public bool InvestigateButtonEnabled = true;
            public bool DeleteButtonEnabled = true;
        }
        private void backButton_Click(object sender, EventArgs e)
        {
            BackClickEvent?.Invoke(Dir);
        }

        private void parentButton_Click(object sender, EventArgs e)
        {
            ParentClickEvent?.Invoke(Dir.Parent);
        }
        private void SetOptions()
        {
            backButton.Visible = Opts.BackButtonEnabled;
            parentButton.Visible = Opts.UpButtonEnabled;
            refreshButton.Visible = Opts.RefreshButtonEnabled;
            investigateButton.Visible = Opts.InvestigateButtonEnabled;
            deleteButton.Visible = Opts.DeleteButtonEnabled;
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            RefreshClickEvent?.Invoke(Dir);
        }

        private void investigateButton_Click(object sender, EventArgs e)
        {
            InvestigateClickEvent?.Invoke(Dir);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            DeleteClickEvent?.Invoke(Dir);
        }

        private void parentLabel_Click(object sender, EventArgs e)
        {
            Utilities.OpenFolderInExplorer(parentLabel.Text);
        }
    }
}
