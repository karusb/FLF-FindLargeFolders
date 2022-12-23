using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FindLargestFolders
{
    public partial class TimePicker : Form
    {
        private long daysOld = 0;
        private bool ok = false;
        public class TimePickerResult
        {
            public long daysOld = 0;
            public bool Ok = false;
        }
        public TimePicker()
        {
            InitializeComponent();
            dateTimePicker1.MaxDate = DateTime.Now;
        }

        public TimePickerResult GetResult()
        {
            this.Invalidate();
            var res = new TimePickerResult();
            res.daysOld = this.daysOld;
            res.Ok = this.ok;
            return res;
        }
        private void OKbutton_Click(object sender, EventArgs e)
        {
            ok = true;
            if (dateSelected.Checked)
                daysOld = (long)(DateTime.Now - dateTimePicker1.Value).TotalDays;
            else
                daysOld = (long)numericUpDown1.Value;
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            ok = false;
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        private void dateSelected_CheckedChanged(object sender, EventArgs e)
        {
            if(dateSelected.Checked)numberSelected.Checked = false;
        }

        private void numberSelected_CheckedChanged(object sender, EventArgs e)
        {
            if (numberSelected.Checked) dateSelected.Checked = false;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dateSelected.Checked = true;
            numberSelected.Checked = false;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            dateSelected.Checked = false;
            numberSelected.Checked = true;
        }
    }
}
