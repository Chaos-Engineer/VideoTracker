namespace VideoTracker
{
    partial class VideoSeriesForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.titleBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Browse = new System.Windows.Forms.Button();
            this.fileNameBox = new System.Windows.Forms.TextBox();
            this.OK = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.directoryListBox = new System.Windows.Forms.ListBox();
            this.addDirButton = new System.Windows.Forms.Button();
            this.removeDirectoryButton = new System.Windows.Forms.Button();
            this.openDirectoryDialog = new System.Windows.Forms.OpenFileDialog();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Video files|*.avi;*.mp4;*.mkv|All files|*.*";
            this.openFileDialog.SupportMultiDottedExtensions = true;
            this.openFileDialog.Title = "Current File From Series";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Title:";
            // 
            // titleBox
            // 
            this.titleBox.Location = new System.Drawing.Point(108, 1);
            this.titleBox.Name = "titleBox";
            this.titleBox.Size = new System.Drawing.Size(350, 20);
            this.titleBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Current File:";
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(464, 25);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(102, 23);
            this.Browse.TabIndex = 3;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Browse_MouseClick);
            // 
            // fileNameBox
            // 
            this.fileNameBox.AllowDrop = true;
            this.fileNameBox.Location = new System.Drawing.Point(108, 27);
            this.fileNameBox.Name = "fileNameBox";
            this.fileNameBox.Size = new System.Drawing.Size(350, 20);
            this.fileNameBox.TabIndex = 4;
            this.fileNameBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.fileNameBox_DragDrop);
            this.fileNameBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.fileNameBox_DragEnter);
            // 
            // OK
            // 
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Location = new System.Drawing.Point(163, 147);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 5;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(327, 147);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 6;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // directoryListBox
            // 
            this.directoryListBox.AllowDrop = true;
            this.directoryListBox.FormattingEnabled = true;
            this.directoryListBox.Location = new System.Drawing.Point(108, 64);
            this.directoryListBox.Name = "directoryListBox";
            this.directoryListBox.Size = new System.Drawing.Size(350, 56);
            this.directoryListBox.TabIndex = 7;
            this.directoryListBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.directoryListBox_DragDrop);
            this.directoryListBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.directoryListBox_DragEnter);
            // 
            // addDirButton
            // 
            this.addDirButton.Location = new System.Drawing.Point(464, 64);
            this.addDirButton.Name = "addDirButton";
            this.addDirButton.Size = new System.Drawing.Size(102, 23);
            this.addDirButton.TabIndex = 8;
            this.addDirButton.Text = "Add Directory";
            this.addDirButton.UseVisualStyleBackColor = true;
            this.addDirButton.Click += new System.EventHandler(this.addDirButton_Click);
            // 
            // removeDirectoryButton
            // 
            this.removeDirectoryButton.Location = new System.Drawing.Point(464, 93);
            this.removeDirectoryButton.Name = "removeDirectoryButton";
            this.removeDirectoryButton.Size = new System.Drawing.Size(102, 23);
            this.removeDirectoryButton.TabIndex = 9;
            this.removeDirectoryButton.Text = "Remove Directory";
            this.removeDirectoryButton.UseVisualStyleBackColor = true;
            this.removeDirectoryButton.Click += new System.EventHandler(this.removeDirectoryButton_Click);
            // 
            // openDirectoryDialog
            // 
            this.openDirectoryDialog.CheckFileExists = false;
            this.openDirectoryDialog.CheckPathExists = false;
            this.openDirectoryDialog.Filter = "Video files|*.avi;*.mp4;*.mkv|All files|*.*";
            this.openDirectoryDialog.SupportMultiDottedExtensions = true;
            this.openDirectoryDialog.Title = "Any file from additional directory";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Search directories:";
            // 
            // VideoSeriesForm
            // 
            this.AcceptButton = this.OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(586, 182);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.removeDirectoryButton);
            this.Controls.Add(this.addDirButton);
            this.Controls.Add(this.directoryListBox);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.fileNameBox);
            this.Controls.Add(this.Browse);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.titleBox);
            this.Controls.Add(this.label1);
            this.Name = "VideoSeriesForm";
            this.Text = "Select Video Series";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VideoSeriesForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox titleBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.TextBox fileNameBox;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.ListBox directoryListBox;
        private System.Windows.Forms.Button addDirButton;
        private System.Windows.Forms.Button removeDirectoryButton;
        private System.Windows.Forms.OpenFileDialog openDirectoryDialog;
        private System.Windows.Forms.Label label3;

    }
}