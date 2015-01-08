namespace VideoTracker
{
    partial class VideoPlayerPanel
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
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.seriesName = new System.Windows.Forms.Label();
            this.videoSelector = new System.Windows.Forms.ComboBox();
            this.playButton = new System.Windows.Forms.Button();
            this.playNextButton = new System.Windows.Forms.Button();
            this.backButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.editButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.AutoSize = true;
            this.flowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel.Controls.Add(this.seriesName);
            this.flowLayoutPanel.Controls.Add(this.videoSelector);
            this.flowLayoutPanel.Controls.Add(this.playButton);
            this.flowLayoutPanel.Controls.Add(this.playNextButton);
            this.flowLayoutPanel.Controls.Add(this.backButton);
            this.flowLayoutPanel.Controls.Add(this.nextButton);
            this.flowLayoutPanel.Controls.Add(this.deleteButton);
            this.flowLayoutPanel.Controls.Add(this.editButton);
            this.flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(266, 87);
            this.flowLayoutPanel.TabIndex = 0;
            // 
            // seriesName
            // 
            this.seriesName.AutoSize = true;
            this.flowLayoutPanel.SetFlowBreak(this.seriesName, true);
            this.seriesName.Location = new System.Drawing.Point(3, 0);
            this.seriesName.Name = "seriesName";
            this.seriesName.Size = new System.Drawing.Size(62, 13);
            this.seriesName.TabIndex = 0;
            this.seriesName.Text = "seriesName";
            // 
            // videoSelector
            // 
            this.flowLayoutPanel.SetFlowBreak(this.videoSelector, true);
            this.videoSelector.FormattingEnabled = true;
            this.videoSelector.Location = new System.Drawing.Point(3, 30);
            this.videoSelector.Name = "videoSelector";
            this.videoSelector.Size = new System.Drawing.Size(149, 21);
            this.videoSelector.TabIndex = 1;
            this.videoSelector.DropDownClosed += new System.EventHandler(this.videoSelector_DropDownClosed);
            // 
            // playButton
            // 
            this.playButton.AutoSize = true;
            this.playButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.playButton.Location = new System.Drawing.Point(3, 59);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(37, 23);
            this.playButton.TabIndex = 2;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // playNextButton
            // 
            this.playNextButton.AutoSize = true;
            this.playNextButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.playNextButton.Location = new System.Drawing.Point(46, 59);
            this.playNextButton.Name = "playNextButton";
            this.playNextButton.Size = new System.Drawing.Size(62, 23);
            this.playNextButton.TabIndex = 3;
            this.playNextButton.Text = "Play Next";
            this.playNextButton.UseVisualStyleBackColor = true;
            this.playNextButton.Click += new System.EventHandler(this.playNextButton_Click);
            // 
            // backButton
            // 
            this.backButton.AutoSize = true;
            this.backButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.backButton.Location = new System.Drawing.Point(114, 59);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(23, 23);
            this.backButton.TabIndex = 4;
            this.backButton.Text = "<";
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.AutoSize = true;
            this.nextButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.nextButton.Location = new System.Drawing.Point(143, 59);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(23, 23);
            this.nextButton.TabIndex = 5;
            this.nextButton.Text = ">";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.AutoSize = true;
            this.deleteButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.deleteButton.Location = new System.Drawing.Point(172, 59);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(48, 23);
            this.deleteButton.TabIndex = 6;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // editButton
            // 
            this.editButton.AutoSize = true;
            this.editButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.editButton.Location = new System.Drawing.Point(226, 59);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(35, 23);
            this.editButton.TabIndex = 7;
            this.editButton.Text = "Edit";
            this.editButton.UseVisualStyleBackColor = true;
            this.editButton.Click += new System.EventHandler(this.editButton_Click);
            // 
            // VideoPlayerPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.flowLayoutPanel);
            this.Name = "VideoPlayerPanel";
            this.Size = new System.Drawing.Size(266, 87);
            this.flowLayoutPanel.ResumeLayout(false);
            this.flowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.Label seriesName;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.ComboBox videoSelector;
        private System.Windows.Forms.Button playNextButton;
        private System.Windows.Forms.Button editButton;
    }

        #endregion
}
