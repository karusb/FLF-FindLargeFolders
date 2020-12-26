using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FindLargestFolders
{
    public partial class Progress : UserControl
    {
        private string ItemName = "";
        private int ProgressPercentage = 0;
        private string OperationName = "";
        public Progress()
        {
            InitializeComponent();
        }
        public void SetItemName(string itemName)
        {
            ItemName = itemName;
            itemNameLabel.Text = ItemName;
        }
        public void SetProgress(int progress)
        {
            ProgressPercentage = progress;
            fileProgressBar.Value = ProgressPercentage;
            percentageLabel.Text = "%" + ProgressPercentage.ToString();
        }
        public void SetOperationName(string operation)
        {
            OperationName = operation;
            operationNameLabel.Text = OperationName;
        }
        public string GetOperationName()
        {
            return OperationName;
        }
        public string GetItemName()
        {
            return ItemName;
        }
        public int GetProgressPercentage()
        {
            return ProgressPercentage;
        }
    }
}
