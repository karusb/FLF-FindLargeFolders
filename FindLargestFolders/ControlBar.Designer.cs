
namespace FindLargestFolders
{
    partial class ControlBar
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
            this.components = new System.ComponentModel.Container();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.backButton = new System.Windows.Forms.Button();
            this.parentButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.parentLabel = new System.Windows.Forms.Label();
            this.refreshButton = new System.Windows.Forms.Button();
            this.investigateButton = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.deleteButton = new System.Windows.Forms.Button();
            this.investigateToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.refreshToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.UpToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.backToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.deleteToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.pathToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel1.Controls.Add(this.backButton);
            this.flowLayoutPanel1.Controls.Add(this.parentButton);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(160, 40);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // backButton
            // 
            this.backButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.backButton.BackgroundImage = global::FindLargestFolders.Properties.Resources.icons8_back_64;
            this.backButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.backButton.Location = new System.Drawing.Point(3, 3);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(35, 35);
            this.backButton.TabIndex = 0;
            this.UpToolTip.SetToolTip(this.backButton, "Back To Folder");
            this.backButton.UseVisualStyleBackColor = false;
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // parentButton
            // 
            this.parentButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.parentButton.BackgroundImage = global::FindLargestFolders.Properties.Resources.icons8_up_64;
            this.parentButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.parentButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.parentButton.Location = new System.Drawing.Point(44, 3);
            this.parentButton.Name = "parentButton";
            this.parentButton.Size = new System.Drawing.Size(35, 35);
            this.parentButton.TabIndex = 1;
            this.UpToolTip.SetToolTip(this.parentButton, "Parent Folder");
            this.parentButton.UseVisualStyleBackColor = false;
            this.parentButton.Click += new System.EventHandler(this.parentButton_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.investigateButton);
            this.panel1.Controls.Add(this.refreshButton);
            this.panel1.Location = new System.Drawing.Point(166, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(201, 40);
            this.panel1.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.parentLabel);
            this.panel2.Location = new System.Drawing.Point(0, 40);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(647, 26);
            this.panel2.TabIndex = 4;
            this.pathToolTip.SetToolTip(this.panel2, "Double click to open file in explorer");
            // 
            // parentLabel
            // 
            this.parentLabel.AutoSize = true;
            this.parentLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.parentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parentLabel.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.parentLabel.ForeColor = System.Drawing.Color.Chocolate;
            this.parentLabel.Location = new System.Drawing.Point(0, 0);
            this.parentLabel.Name = "parentLabel";
            this.parentLabel.Size = new System.Drawing.Size(71, 15);
            this.parentLabel.TabIndex = 0;
            this.parentLabel.Text = "ParentPath";
            this.pathToolTip.SetToolTip(this.parentLabel, "Double click to open file in explorer");
            this.parentLabel.Click += new System.EventHandler(this.parentLabel_Click);
            // 
            // refreshButton
            // 
            this.refreshButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.refreshButton.BackgroundImage = global::FindLargestFolders.Properties.Resources.icons8_refresh_48;
            this.refreshButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.refreshButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refreshButton.Location = new System.Drawing.Point(3, 2);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(35, 35);
            this.refreshButton.TabIndex = 2;
            this.refreshToolTip.SetToolTip(this.refreshButton, "Refresh/Recalculate");
            this.refreshButton.UseVisualStyleBackColor = false;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // investigateButton
            // 
            this.investigateButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.investigateButton.BackgroundImage = global::FindLargestFolders.Properties.Resources.icons8_search_property_50;
            this.investigateButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.investigateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.investigateButton.Location = new System.Drawing.Point(44, 2);
            this.investigateButton.Name = "investigateButton";
            this.investigateButton.Size = new System.Drawing.Size(35, 35);
            this.investigateButton.TabIndex = 3;
            this.investigateToolTip.SetToolTip(this.investigateButton, "Find Large Folders");
            this.investigateButton.UseVisualStyleBackColor = false;
            this.investigateButton.Click += new System.EventHandler(this.investigateButton_Click);
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.deleteButton);
            this.panel3.Location = new System.Drawing.Point(373, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(272, 40);
            this.panel3.TabIndex = 5;
            // 
            // deleteButton
            // 
            this.deleteButton.BackColor = System.Drawing.Color.Maroon;
            this.deleteButton.BackgroundImage = global::FindLargestFolders.Properties.Resources.icons8_delete_bin_50;
            this.deleteButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.deleteButton.Location = new System.Drawing.Point(234, 2);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(35, 35);
            this.deleteButton.TabIndex = 4;
            this.deleteToolTip.SetToolTip(this.deleteButton, "Delete Folder");
            this.deleteButton.UseVisualStyleBackColor = false;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // ControlBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "ControlBar";
            this.Size = new System.Drawing.Size(648, 65);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label parentLabel;
        private System.Windows.Forms.Button parentButton;
        private System.Windows.Forms.Button investigateButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.ToolTip UpToolTip;
        private System.Windows.Forms.ToolTip investigateToolTip;
        private System.Windows.Forms.ToolTip refreshToolTip;
        private System.Windows.Forms.ToolTip deleteToolTip;
        private System.Windows.Forms.ToolTip backToolTip;
        private System.Windows.Forms.ToolTip pathToolTip;
    }
}
