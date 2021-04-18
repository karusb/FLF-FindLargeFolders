namespace FindLargestFolders
{
    partial class Progress
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fileProgressBar = new System.Windows.Forms.ProgressBar();
            this.operationNameLabel = new System.Windows.Forms.Label();
            this.itemNameLabel = new System.Windows.Forms.Label();
            this.percentageLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileProgressBar
            // 
            this.fileProgressBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.fileProgressBar.Dock = System.Windows.Forms.DockStyle.Left;
            this.fileProgressBar.Location = new System.Drawing.Point(0, 0);
            this.fileProgressBar.Name = "fileProgressBar";
            this.fileProgressBar.Size = new System.Drawing.Size(327, 17);
            this.fileProgressBar.TabIndex = 0;
            // 
            // operationNameLabel
            // 
            this.operationNameLabel.AutoSize = true;
            this.operationNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.operationNameLabel.ForeColor = System.Drawing.Color.Chocolate;
            this.operationNameLabel.Location = new System.Drawing.Point(3, 0);
            this.operationNameLabel.Name = "operationNameLabel";
            this.operationNameLabel.Size = new System.Drawing.Size(24, 13);
            this.operationNameLabel.TabIndex = 1;
            this.operationNameLabel.Text = "Idle";
            // 
            // itemNameLabel
            // 
            this.itemNameLabel.AutoSize = true;
            this.itemNameLabel.ForeColor = System.Drawing.Color.Chocolate;
            this.itemNameLabel.Location = new System.Drawing.Point(185, 0);
            this.itemNameLabel.Name = "itemNameLabel";
            this.itemNameLabel.Size = new System.Drawing.Size(103, 13);
            this.itemNameLabel.TabIndex = 2;
            this.itemNameLabel.Text = "................................";
            // 
            // percentageLabel
            // 
            this.percentageLabel.AutoSize = true;
            this.percentageLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.percentageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.percentageLabel.ForeColor = System.Drawing.Color.Chocolate;
            this.percentageLabel.Location = new System.Drawing.Point(347, 0);
            this.percentageLabel.Name = "percentageLabel";
            this.percentageLabel.Size = new System.Drawing.Size(25, 15);
            this.percentageLabel.TabIndex = 3;
            this.percentageLabel.Text = "%0";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.tableLayoutPanel1.Controls.Add(this.operationNameLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.itemNameLabel, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(372, 16);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.fileProgressBar);
            this.panel1.Controls.Add(this.percentageLabel);
            this.panel1.Location = new System.Drawing.Point(4, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(372, 17);
            this.panel1.TabIndex = 5;
            // 
            // Progress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Progress";
            this.Size = new System.Drawing.Size(379, 48);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar fileProgressBar;
        private System.Windows.Forms.Label operationNameLabel;
        private System.Windows.Forms.Label itemNameLabel;
        private System.Windows.Forms.Label percentageLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
    }
}
