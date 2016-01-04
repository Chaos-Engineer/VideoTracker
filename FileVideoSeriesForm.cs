// FileVideoSeriesForm
//
// Fields:
// - The name of the series [REQUIRED]
// - The name of the file to start with [OPTIONAL, use first file if this is not specified.]
// - The directories to search for files [OPTIONAL, if not specified then we'll try to populate this from the
//       global list of default directories. If we're not able to find any matching files in the default directories,
//       then this field must be specified.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ookii.Dialogs;

namespace VideoTracker
{
    public partial class FileVideoSeriesForm : Form
    {
        private FileVideoSeries fileVideoSeries;
        private VideoTrackerData videoTrackerData;
        private string regexWildcard;
        private string fileWildcard;

        public FileVideoSeriesForm(VideoTrackerData vtd)
        {
            this.videoTrackerData = vtd;
            InitializeComponent();
        }

        public FileVideoSeriesForm(VideoTrackerData vtd, FileVideoSeries vs)
        {
            InitializeComponent();
            this.fileVideoSeries = vs;
            this.videoTrackerData = vtd;

            this.titleBox.Text = vs.title;
            if (vs.currentVideo != null)
            {
                this.fileNameBox.Text = vs.currentVideo.title;
            }
            foreach (string dir in vs.directoryList)
            {
                AddDirectoryToListBox(dir);
            }
        }

        private void FileVideoSeriesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // User selected cancel, don't validate or save results.
            if (this.DialogResult == DialogResult.Cancel)
            {
                e.Cancel = false;
                return;
            }

            if (!ValidateForm())
            {
                e.Cancel = true;
                return;
            }

            // Create a new series object, or update an existing one.

            if (fileVideoSeries == null) {
                fileVideoSeries = new FileVideoSeries();
            }

            fileVideoSeries.InitializeFromForm(directoryListBox.Items.OfType<String>().ToList(),
                this.fileNameBox.Text);
            fileVideoSeries.LoadFiles(titleBox.Text, fileNameBox.Text, videoTrackerData);
            e.Cancel = false;
        }

        // All validation operations go here.
        private bool ValidateForm()
        {
            if (titleBox.Text.Equals(""))
            {
                MessageBox.Show("Title must be set");
                return (false);
            }

            Regex whitespace = new Regex(@"\s+");
            regexWildcard = whitespace.Replace(titleBox.Text, ".*");
            fileWildcard = "*" + whitespace.Replace(titleBox.Text, "*") + "*";
            if (fileNameBox.Text != "" && !Regex.Match(fileNameBox.Text, regexWildcard, RegexOptions.IgnoreCase).Success)
            {
                MessageBox.Show("Title string must be contained in filename");
                return (false);
            }

            if (directoryListBox.Items.Count == 0)
            {
                if (videoTrackerData.globals.GetList(gdc.DEFDIRLIST).Count == 0)
                {
                    MessageBox.Show("Directory list (or default directory list) must be set.");
                    return (false);
                }
                GetDirectoriesFromDefaultList(fileWildcard);
                if (directoryListBox.Items.Count == 0)
                {
                    MessageBox.Show("No matching files found in default directory list.");
                    return(false);
                }
            }
            return (true);  
        }

        private void Browse_MouseClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog fd = openFileDialog;
            if (fd.ShowDialog() == DialogResult.OK)
            {
                AddFileToForm(fd.FileName);
            }
        }

        private void addDirButton_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog dd = openDirectoryDialog;
            if (dd.ShowDialog() == DialogResult.OK)
            {
                AddDirectoryToListBox(dd.SelectedPath);
            }
        }

        private void removeDirectoryButton_Click(object sender, EventArgs e)
        {
            directoryListBox.Items.RemoveAt(directoryListBox.SelectedIndex);
        }


        private void findDefaultDirButton_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }
            GetDirectoriesFromDefaultList(fileWildcard);
        }

        private void fileNameBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) {
                AddFileToForm(file);
            }
        }

        private void fileNameBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }


        private void directoryListBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                AddDirectoryToListBox(file);
            }
        }

        private void directoryListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void AddFileToForm(string file)
        {        
            if (Directory.Exists(file)) {
                AddDirectoryToListBox(file);
            } else if (File.Exists(file)) {
                fileNameBox.Text = file;
                AddDirectoryToListBox(Path.GetDirectoryName(file));
            }
            else
            {
                MessageBox.Show(file + " does not exist.");
            }
        }

        private void AddDirectoryToListBox(string directory)
        {
            if (!Directory.Exists(directory))
            {
                MessageBox.Show(directory + " does not exist.");
                return;
            }
            if (!directoryListBox.Items.Contains(directory))
            {
                directoryListBox.Items.Add(directory);
            }
        }

        // Search through the default directories and subdirectories for matching files. Add
        // each directory to the program-specific directory list.
        private void GetDirectoriesFromDefaultList(string search)
        {

            List<string> l = directoryListBox.Items.Cast<string>().ToList();
            foreach (String d in l)
            {
                if (!Directory.Exists(d))
                {
                    directoryListBox.Items.Remove(d);
                }
            }
            foreach (String d in videoTrackerData.globals.GetList(gdc.DEFDIRLIST)) {
                foreach (String f in Directory.GetFiles(d, search, SearchOption.AllDirectories )) {
                    AddDirectoryToListBox(Path.GetDirectoryName(f));
                }
            }
        }
    }
}
