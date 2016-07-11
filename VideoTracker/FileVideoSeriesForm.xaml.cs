using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
// FileVideoSeriesForm
//
// Fields:
// - The name of the series [REQUIRED]
// - The name of the file to start with [OPTIONAL, use first file if this is not specified.]
// - The directories to search for files [OPTIONAL, if not specified then we'll try to populate this from the
//       global list of default directories. If we're not able to find any matching files in the default directories,
//       then this field must be specified.
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VideoTracker
{
    public partial class FileVideoSeriesForm : Window
    {
        private FileVideoSeries fileVideoSeries;
        private VideoTrackerData videoTrackerData;
        private string regexWildcard;
        private string fileWildcard;

        private OpenFileDialog openFileDialog;
        private VistaFolderBrowserDialog openDirectoryDialog;

        public FileVideoSeriesForm(VideoTrackerData vtd)
        {
            InitializeComponent();
            InitializeDialogs();
            this.videoTrackerData = vtd;
            this.Owner = vtd.videoTrackerForm;
        }

        public FileVideoSeriesForm(VideoTrackerData vtd, FileVideoSeries vs)
        {
            InitializeComponent();
            InitializeDialogs();
            this.fileVideoSeries = vs;
            this.videoTrackerData = vtd;
            this.Owner = vtd.videoTrackerForm;

            this.titleBox.Text = vs.seriesTitle;
            if (vs.currentVideo != null)
            {
                this.fileNameBox.Text = vs.currentVideo.episodeTitle;
            }
            foreach (string dir in vs.directoryList)
            {
                AddDirectoryToListBox(dir);
            }
        }

        private void InitializeDialogs()
        {
            this.openFileDialog = new OpenFileDialog();
            this.openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.openFileDialog.Filter = "Video files|*.avi;*.mp4;*.mkv|All files|*.*";
            this.openFileDialog.Title = "Current File From Series";

            this.openDirectoryDialog = new VistaFolderBrowserDialog();
            this.openDirectoryDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {

            if (!ValidateForm())
            {
                return;
            }

            // Create a new series object, or update an existing one.

            if (fileVideoSeries == null)
            {
                fileVideoSeries = new FileVideoSeries();
            }

            fileVideoSeries.InitializeFromForm(directoryListBox.Items.OfType<String>().ToList(),
                this.fileNameBox.Text);
            fileVideoSeries.LoadSeries(titleBox.Text, fileNameBox.Text, videoTrackerData);
            this.DialogResult = true;
        }

        // All validation operations go here.
        private bool ValidateForm()
        {
            if (titleBox.Text.Equals(""))
            {
                ErrorDialog.Show("Title must be set");
                return (false);
            }

            Regex whitespace = new Regex(@"\s+");
            regexWildcard = whitespace.Replace(titleBox.Text, ".*");
            fileWildcard = "*" + whitespace.Replace(titleBox.Text, "*") + "*";
            if (fileNameBox.Text != "" && !Regex.Match(fileNameBox.Text, regexWildcard, RegexOptions.IgnoreCase).Success)
            {
                ErrorDialog.Show("Title string must be contained in filename");
                return (false);
            }

            if (directoryListBox.Items.Count == 0)
            {
                if (videoTrackerData.globals.GetList(gdg.FILE, gdk.DEFDIRLIST).Count == 0)
                {
                    ErrorDialog.Show("Directory list (or default directory list) must be set.");
                    return (false);
                }
                GetDirectoriesFromDefaultList(fileWildcard);
                if (directoryListBox.Items.Count == 0)
                {
                    ErrorDialog.Show("No matching files found in default directory list.");
                    return (false);
                }
            }
            return (true);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                AddFileToForm(openFileDialog.FileName);
            }
        }

        private void addDirectoryButton_Click(object sender, EventArgs e)
        {
            if (openDirectoryDialog.ShowDialog() == true)
            {
                AddDirectoryToListBox(openDirectoryDialog.SelectedPath);
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
            foreach (string file in files)
            {
                AddFileToForm(file);
            }
        }

        private void fileNameBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
            e.Handled = true;
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
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        }

        private void AddFileToForm(string file)
        {
            if (Directory.Exists(file))
            {
                AddDirectoryToListBox(file);
            }
            else if (File.Exists(file))
            {
                fileNameBox.Text = file;
                AddDirectoryToListBox(System.IO.Path.GetDirectoryName(file));
            }
            else
            {
                ErrorDialog.Show(file + " does not exist.");
            }
        }

        private void AddDirectoryToListBox(string directory)
        {
            if (File.Exists(directory))
            {
                ErrorDialog.Show(directory + " is not a directory.");
                return;
            }
            if (!Directory.Exists(directory))
            {
                ErrorDialog.Show(directory + " does not exist.");
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
            using (new WaitCursor())
            {
                List<string> l = directoryListBox.Items.Cast<string>().ToList();
                foreach (String d in l)
                {
                    if (!Directory.Exists(d))
                    {
                        directoryListBox.Items.Remove(d);
                    }
                }
                foreach (String d in videoTrackerData.globals.GetList(gdg.FILE, gdk.DEFDIRLIST))
                {
                    foreach (String f in Directory.GetFiles(d, search, SearchOption.AllDirectories))
                    {
                        AddDirectoryToListBox(System.IO.Path.GetDirectoryName(f));
                    }
                }
            }
        }
    }
}
