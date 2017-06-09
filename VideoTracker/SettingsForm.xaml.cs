using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VideoTracker
{
    public partial class SettingsForm : Window
    {
        private VideoTrackerData videoTrackerData;
        private VideoTrackerForm videoTrackerForm;
        private VistaFolderBrowserDialog openDefaultDirectoryDialog;


        public SettingsForm(VideoTrackerData vtd)
        {
            InitializeComponent();
            this.videoTrackerData = vtd;
            this.videoTrackerForm = vtd.videoTrackerForm;
            this.Owner = vtd.videoTrackerForm;

            this.openDefaultDirectoryDialog = new VistaFolderBrowserDialog();
            this.openDefaultDirectoryDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Global settings

            // The "ConvertToInt" call assigns a default value of 1 if currently undefined.
            columnsTextBox.Text = vtd.globals.GetInt(gdg.MAIN, gdk.COLUMNS, 1).ToString();

            // File series settings
            if (vtd.globals[gdg.FILE][gdk.DEFDIRLIST] != null && vtd.globals.GetArray(gdg.FILE, gdk.DEFDIRLIST).Length > 0)
            {
                foreach (string dir in vtd.globals.GetArray(gdg.FILE, gdk.DEFDIRLIST))
                {
                    defaultDirectoryListBox.Items.Add(dir);
                }
            }

            // Amazon series settings
            publicKeyTextBox.Text = vtd.globals[gdg.AMAZON][gdk.PUBLICKEY];
            secretKeyTextBox.Text = vtd.globals[gdg.AMAZON][gdk.SECRETKEY];
            affiliateIdTextBox.Text = vtd.globals[gdg.AMAZON][gdk.AFFILIATEID];
        }


        private void OKButton_Click(object sender, EventArgs e)
        {
            // Apply changes. If there were validation failures, then prevent the 
            // form from closing.
            bool settingsValid = ApplyChanges();
            if (settingsValid)
            {
                this.DialogResult = true;
            }
            return;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        private bool ApplyChanges()
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

            return true;
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
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        }


        private void addDefaultDirectoryButton_Click(object sender, EventArgs e)
        {
            if (openDefaultDirectoryDialog.ShowDialog() == true)
            {
                AddDirectoryToListBox(openDefaultDirectoryDialog.SelectedPath);
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
                ErrorDialog.Show(directory + " does not exist.");
                return;
            }
            if (!defaultDirectoryListBox.Items.Contains(directory))
            {
                defaultDirectoryListBox.Items.Add(directory);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}