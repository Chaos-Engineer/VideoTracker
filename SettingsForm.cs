using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Ookii.Dialogs;

namespace VideoTracker
{
    public partial class SettingsForm : Form
    {
        private const string UNREGISTER = "Unregister";
        private const string CONFIGURE = "Configure";

        private bool settingsValid;
        private VideoTrackerData videoTrackerData;
        private VideoTrackerForm videoTrackerForm;

        public SettingsForm(VideoTrackerData vtd)
        {
            InitializeComponent();
            this.videoTrackerData = vtd;
            this.videoTrackerForm = vtd.videoTrackerForm;

            // Global settings

            // The "ConvertToInt" call assigns a default value of 1 if currently undefined.
            columnsTextBox.Text = vtd.globals.GetInt(gdg.MAIN, gdk.COLUMNS, 1).ToString();

            // File series settings
            if (vtd.globals[gdg.FILE][gdk.DEFDIRLIST] != null && vtd.globals.GetArray(gdg.FILE, gdk.DEFDIRLIST).Length > 0)
            {
                defaultDirectoryListBox.Items.AddRange(vtd.globals.GetArray(gdg.FILE, gdk.DEFDIRLIST).ToArray<string>());
            }

            // Amazon series settings
            publicKeyTextBox.Text = vtd.globals[gdg.AMAZON][gdk.PUBLICKEY];
            secretKeyTextBox.Text = vtd.globals[gdg.AMAZON][gdk.SECRETKEY];
            affiliateIdTextBox.Text = vtd.globals[gdg.AMAZON][gdk.AFFILIATEID];

            // Plugin settings
            BuildPluginTable();
        }

        //
        // The Windows library draws text vertically down left-aligned tabs. This routine
        // allows text to be drawn horizontally instead. (If the tab width is too 
        // narrow, increase the TabControl object's *height* attribute.)
        //
        private void tabControl_DrawItem(Object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush brush;
            Font font;

            TabPage tabPage = tabControl.TabPages[e.Index];
            Rectangle tabBounds = tabControl.GetTabRect(e.Index);

            brush = new SolidBrush(Color.Black);
            if (e.State == DrawItemState.Selected)
            {
                font = new Font(this.Font, FontStyle.Bold);
            }
            else
            {
                font = this.Font;
            }

            StringFormat stringFlags = new StringFormat();
            stringFlags.Alignment = StringAlignment.Center;
            stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(tabPage.Text, font, brush, tabBounds, new StringFormat(stringFlags));
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // User selected cancel, don't validate or save results.
            if (this.DialogResult == DialogResult.Cancel)
            {
                e.Cancel = false;
                return;
            }

            // Apply changes. If there were validation failures, then prevent the 
            // form from closing.
            ApplyChanges();
            settingsValid = true;
            if (settingsValid)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
            return;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        private void ApplyChanges()
        {
            // Program-wide globals
            int columns;
            if (!Int32.TryParse(columnsTextBox.Text, out columns))
            {
                columns = 1;
            }
            videoTrackerData.globals[gdg.MAIN][gdk.COLUMNS] = columns.ToString();

            videoTrackerData.videoTrackerForm.ResizeMainPanel();

            // File series globals
            videoTrackerData.globals.Set(gdg.FILE, gdk.DEFDIRLIST, defaultDirectoryListBox.Items.OfType<string>().ToList());

            // Amazon series globals
            videoTrackerData.globals.Set(gdg.AMAZON, gdk.PUBLICKEY, publicKeyTextBox.Text);
            videoTrackerData.globals.Set(gdg.AMAZON, gdk.SECRETKEY, secretKeyTextBox.Text);
            videoTrackerData.globals.Set(gdg.AMAZON, gdk.AFFILIATEID, affiliateIdTextBox.Text);
            this.settingsValid = true;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel l = sender as LinkLabel;
            Process.Start(l.Text);
        }

        private void defaultDirectoryListBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                AddDirectoryToListBox(file);
            }
        }

        private void defaultDirectoryListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }


        private void addDefaultDirectoryButton_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog dd = openDefaultDirectoryDialog;
            if (dd.ShowDialog() == DialogResult.OK)
            {
                AddDirectoryToListBox(dd.SelectedPath);
            }
        }

        private void removeDefaultDirectoryButton_Click(object sender, EventArgs e)
        {
            defaultDirectoryListBox.Items.RemoveAt(defaultDirectoryListBox.SelectedIndex);
        }

        private void AddDirectoryToListBox(string directory)
        {
            if (!Directory.Exists(directory))
            {
                MessageBox.Show(directory + " does not exist.");
                return;
            }
            if (!defaultDirectoryListBox.Items.Contains(directory))
            {
                defaultDirectoryListBox.Items.Add(directory);
            }
        }

        private void BuildPluginTable()
        {
            pluginPanel.SuspendLayout();
            pluginPanel.Controls.Add(new CustomLabel("Plug-In"), 0, 0);
            pluginPanel.Controls.Add(new CustomLabel("Description"), 1, 0);
            foreach (string key in videoTrackerData.globals[gdg.PLUGINS].Keys)
            {
                AddPluginRow(key, videoTrackerData.globals[key][gpk.DESC]);
            }
            pluginPanel.ResumeLayout(false);
            pluginPanel.PerformLayout();
        }

        private void AddPluginRow(string key, string description)
        {
            int index = pluginPanel.RowCount++;
            RowStyle style = new RowStyle(SizeType.AutoSize);
            pluginPanel.RowStyles.Add(style);
            CustomLabel keyLabel = new CustomLabel(key);
            CustomLabel fileLabel = new CustomLabel(description);
            pluginPanel.Controls.Add(keyLabel, 0, index);
            pluginPanel.Controls.Add(fileLabel, 1, index);
            CustomButton configButton = new CustomButton(this, key, CONFIGURE, index);
            CustomButton deleteButton = new CustomButton(this, key, UNREGISTER, index);
            pluginPanel.Controls.Add(configButton, 2, index);
            pluginPanel.Controls.Add(deleteButton, 3, index);
        }

        private void registerButton_Click(object sender, EventArgs e)
        {

            OpenFileDialog fd = openPluginFileDialog;
            if (fd.ShowDialog() != DialogResult.OK) return;

            string pluginFile = fd.FileName;
            string pluginName;

            if (PluginSeries.Register(pluginFile, videoTrackerData, out pluginName))
            {
                string pluginAdd = videoTrackerData.globals[pluginName][gpk.ADD];
                string pluginDesc = videoTrackerData.globals[pluginName][gpk.DESC];
                AddPluginRow(pluginName, pluginDesc);
                this.videoTrackerForm.InsertPluginMenuItem(pluginName, pluginAdd);
            }

        }

        public void customButton_Click(object sender, EventArgs e)
        {
            CustomButton b = (CustomButton) sender;
            if (b.Text == UNREGISTER)
            {
                videoTrackerData.globals[gdg.PLUGINS].Remove(b.Name);
                foreach (string key in videoTrackerData.globals[b.Name].Keys)
                {
                    videoTrackerData.globals.Remove(b.Name);
                }
                pluginPanel.RowStyles[b.rowNum].SizeType = SizeType.Absolute;
                pluginPanel.RowStyles[b.rowNum].Height = 0; // Deleting row is difficult - just hide it.
                this.videoTrackerForm.DeletePluginMenuItem(b.Name);
            }
            if (b.Text == CONFIGURE) {
                string errorString;
                PluginSeries.PerformGlobalConfiguration(b.Name, videoTrackerData, out errorString);
                if (errorString != "") MessageBox.Show(errorString);
            }
        }
    }

    // Utility class to create a label, with AutoSize set to true and with the text passed into the constructor.
    public class CustomLabel : Label
    {
        public CustomLabel(string text)
        {
            this.Text = text;
            this.AutoSize = true;
        }
    }

    public class CustomButton : Button
    {
        public int rowNum;
        public CustomButton(SettingsForm settingsForm, string key, string text, int rowNum)
        {
            this.Name = key;
            this.Text = text;
            this.AutoSize = true;
            this.UseVisualStyleBackColor = true;
            this.rowNum = rowNum;
            this.Click += new System.EventHandler(settingsForm.customButton_Click);
        }
    }
}
