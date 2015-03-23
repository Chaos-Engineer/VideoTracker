﻿using System;
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

namespace VideoTracker
{
    public partial class FileVideoSeriesForm : Form
    {
        public FileVideoSeries fileVideoSeries;
        private VideoTrackerData videoTrackerData;

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

        private void Browse_MouseClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog fd = openFileDialog;
            if (fd.ShowDialog() == DialogResult.OK)
            {
                AddFileToForm(fd.FileName);
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

            // Check input for validity
            if (titleBox.Text.Equals("") || fileNameBox.Text.Equals("") || directoryListBox.Items.Count == 0)
            {
                MessageBox.Show("Title, Filename, and Directory List must be set");
                e.Cancel = true;
                return;
            }

            Regex whitespace = new Regex(@"\s+");
            string wildcard = whitespace.Replace(titleBox.Text, ".*");
            if (!Regex.Match(fileNameBox.Text, wildcard, RegexOptions.IgnoreCase).Success) {
                MessageBox.Show("Title string must be contained in filename");
                e.Cancel = true;
                return;
            }

            // Create a new series object, or update an existing one.

            if (fileVideoSeries == null) {
                fileVideoSeries = new FileVideoSeries();
                if (!fileVideoSeries.LoadGlobalSettings(videoTrackerData))
                {
                    e.Cancel = true;
                    return;
                }
            }
            fileVideoSeries.Initialize( directoryListBox.Items.OfType<String>().ToList());
           
            fileVideoSeries.Load(titleBox.Text, fileNameBox.Text, fileVideoSeries.panel);
            e.Cancel = false;
        }

        private void addDirButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dd = openDirectoryDialog;
            if (dd.ShowDialog() == DialogResult.OK)
            {
                AddDirectoryToListBox(Path.GetDirectoryName(dd.FileName));
            }
        }

        private void removeDirectoryButton_Click(object sender, EventArgs e)
        {
            directoryListBox.Items.RemoveAt(directoryListBox.SelectedIndex);
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
    }
}
