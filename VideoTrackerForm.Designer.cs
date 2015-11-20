namespace VideoTracker
{
    partial class VideoTrackerForm
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
            this.components = new System.ComponentModel.Container();
            this.mainPanel = new System.Windows.Forms.TableLayoutPanel();
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.fileMenuItem = new System.Windows.Forms.MenuItem();
            this.loadMenuItem = new System.Windows.Forms.MenuItem();
            this.saveMenuItem = new System.Windows.Forms.MenuItem();
            this.saveAsMenuItem = new System.Windows.Forms.MenuItem();
            this.autoSaveMenuItem = new System.Windows.Forms.MenuItem();
            this.exitMenuItem = new System.Windows.Forms.MenuItem();
            this.editMenuItem = new System.Windows.Forms.MenuItem();
            this.addProgramMenuItem = new System.Windows.Forms.MenuItem();
            this.addAmazonVideoOnDemandMenuItem = new System.Windows.Forms.MenuItem();
            this.addCrunchyRollVideoMenuItem = new System.Windows.Forms.MenuItem();
            this.refreshMenuItem = new System.Windows.Forms.MenuItem();
            this.settingsMenuItem = new System.Windows.Forms.MenuItem();
            this.helpMenuItem = new System.Windows.Forms.MenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.AutoSize = true;
            this.mainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.mainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(341, 207);
            this.mainPanel.TabIndex = 0;
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileMenuItem,
            this.editMenuItem,
            this.helpMenuItem});
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.Index = 0;
            this.fileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.loadMenuItem,
            this.saveMenuItem,
            this.saveAsMenuItem,
            this.autoSaveMenuItem,
            this.exitMenuItem});
            this.fileMenuItem.Text = "File";
            // 
            // loadMenuItem
            // 
            this.loadMenuItem.Index = 0;
            this.loadMenuItem.Text = "Load";
            this.loadMenuItem.Click += new System.EventHandler(this.loadMenuItem_Click);
            // 
            // saveMenuItem
            // 
            this.saveMenuItem.Index = 1;
            this.saveMenuItem.Text = "Save";
            this.saveMenuItem.Click += new System.EventHandler(this.saveMenuItem_Click);
            // 
            // saveAsMenuItem
            // 
            this.saveAsMenuItem.Index = 2;
            this.saveAsMenuItem.Text = "Save As";
            this.saveAsMenuItem.Click += new System.EventHandler(this.saveAsMenuItem_Click);
            // 
            // autoSaveMenuItem
            // 
            this.autoSaveMenuItem.Checked = true;
            this.autoSaveMenuItem.Index = 3;
            this.autoSaveMenuItem.Text = "Auto Save";
            this.autoSaveMenuItem.Click += new System.EventHandler(this.autoSaveMenuItem_Click);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Index = 4;
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // editMenuItem
            // 
            this.editMenuItem.Index = 1;
            this.editMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.addProgramMenuItem,
            this.addAmazonVideoOnDemandMenuItem,
            this.addCrunchyRollVideoMenuItem,
            this.refreshMenuItem,
            this.settingsMenuItem});
            this.editMenuItem.Text = "Edit";
            // 
            // addProgramMenuItem
            // 
            this.addProgramMenuItem.Index = 0;
            this.addProgramMenuItem.Text = "Add Video Files";
            this.addProgramMenuItem.Click += new System.EventHandler(this.addVideoFileMenuItem_Click);
            // 
            // addAmazonVideoOnDemandMenuItem
            // 
            this.addAmazonVideoOnDemandMenuItem.Index = 1;
            this.addAmazonVideoOnDemandMenuItem.Text = "Add Amazon Video On-Demand";
            this.addAmazonVideoOnDemandMenuItem.Click += new System.EventHandler(this.addAmazonVideoOnDemandMenuItem_Click);
            // 
            // addCrunchyRollVideoMenuItem
            // 
            this.addCrunchyRollVideoMenuItem.Index = 2;
            this.addCrunchyRollVideoMenuItem.Text = "Add CrunchyRoll Video";
            this.addCrunchyRollVideoMenuItem.Click += new System.EventHandler(this.addCrunchyRollVideoMenuItem_Click);
            // 
            // refreshMenuItem
            // 
            this.refreshMenuItem.Index = 3;
            this.refreshMenuItem.Shortcut = System.Windows.Forms.Shortcut.F5;
            this.refreshMenuItem.Text = "Refresh";
            this.refreshMenuItem.Click += new System.EventHandler(this.refreshMenuItem_Click);
            // 
            // settingsMenuItem
            // 
            this.settingsMenuItem.Index = 4;
            this.settingsMenuItem.Text = "Settings";
            this.settingsMenuItem.Click += new System.EventHandler(this.settingsMenuItem_Click);
            // 
            // helpMenuItem
            // 
            this.helpMenuItem.Index = 2;
            this.helpMenuItem.Text = "Help";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(220, 6);
            // 
            // openFileDialog
            // 
            this.openFileDialog.AutoUpgradeEnabled = false;
            this.openFileDialog.DefaultExt = "vtr";
            this.openFileDialog.Filter = "VTR files|*.vtr|All files|*.*";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "vtr";
            this.saveFileDialog.Filter = "VTR files|*.vtr|All files|*.*";
            // 
            // VideoTrackerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(341, 207);
            this.Controls.Add(this.mainPanel);
            this.Menu = this.mainMenu;
            this.Name = "VideoTrackerForm";
            this.Text = "VideoTracker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VideoTrackerForm_FormClosing);
            this.ResizeEnd += new System.EventHandler(this.VideoTrackerForm_ResizeEnd);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel mainPanel;
        private System.Windows.Forms.MainMenu mainMenu;
        private System.Windows.Forms.MenuItem fileMenuItem;
        private System.Windows.Forms.MenuItem editMenuItem;
        private System.Windows.Forms.MenuItem helpMenuItem;
        private System.Windows.Forms.MenuItem loadMenuItem;
        private System.Windows.Forms.MenuItem autoSaveMenuItem;
        private System.Windows.Forms.MenuItem addProgramMenuItem;
        private System.Windows.Forms.MenuItem exitMenuItem;
        private System.Windows.Forms.MenuItem settingsMenuItem;
        private System.Windows.Forms.MenuItem saveAsMenuItem;
        private System.Windows.Forms.MenuItem saveMenuItem;
        private System.Windows.Forms.MenuItem addAmazonVideoOnDemandMenuItem;
        private System.Windows.Forms.MenuItem addCrunchyRollVideoMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.MenuItem refreshMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}

