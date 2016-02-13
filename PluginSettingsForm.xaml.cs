﻿using Microsoft.Win32;
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
    /// <summary>
    /// Interaction logic for PluginSettingsForm.xaml
    /// </summary>
    /// 
    public partial class PluginSettingsForm : Window
    {
        private const string UNREGISTER = "Unregister";
        private const string CONFIGURE = "Configure";
        private VideoTrackerData videoTrackerData;
        private VideoTrackerForm videoTrackerForm;
        private OpenFileDialog openPluginFileDialog;
        private VistaFolderBrowserDialog openPythonDirectoryDialog;

        private const string LIB1 = @"C:\Program Files\IronPython 2.7\Lib";
        private const string LIB2 = @"C:\Program Files (x86)\IronPython 2.7\Lib";

        public PluginSettingsForm(VideoTrackerData vtd)
        {
            InitializeComponent();
            this.videoTrackerData = vtd;
            this.videoTrackerForm = vtd.videoTrackerForm;
            this.Owner = vtd.videoTrackerForm;
            this.openPluginFileDialog = new OpenFileDialog();
            this.openPluginFileDialog.InitialDirectory = Environment.CurrentDirectory + @"\Plugins";
            this.openPluginFileDialog.DefaultExt = "py";
            this.openPluginFileDialog.Filter = "Python files|*.py|All files|*.*";

            this.openPythonDirectoryDialog = new VistaFolderBrowserDialog();
            openPythonDirectoryDialog.SelectedPath = @"C:\";

            CheckLibValidity();
            BuildPluginTable();
        }

        private void CheckLibValidity()
        {
            this.pythonPluginHelp.Visibility = Visibility.Collapsed;
            if (this.pythonDirTextBox.Text == "" || !Directory.Exists(this.pythonDirTextBox.Text))
            {
                if (Directory.Exists(LIB1))
                {
                    this.pythonDirTextBox.Text = LIB1;
                }
                else if (Directory.Exists(LIB2))
                {
                    this.pythonDirTextBox.Text = LIB2;
                }
                else
                {
                    this.pythonPluginHelp.Visibility = Visibility.Visible;
                }
            }
        }

        private void BuildPluginTable()
        {

            using (var d = Dispatcher.DisableProcessing())
            {
                this.pythonDirTextBox.Text = videoTrackerData.globals[gdg.PLUGIN_GLOBALS][gdk.PYTHONPATH];

                pluginPanel.Children.Add(new CustomLabel("Plug-In", 0, 0));
                pluginPanel.Children.Add(new CustomLabel("Description", 0, 1));
                foreach (string key in videoTrackerData.globals[gdg.PLUGINS].Keys)
                {
                    AddPluginRow(key, videoTrackerData.globals[key][gpk.ADD], videoTrackerData.globals[key][gpk.DESC]);
                }
            }
        }

        private void AddPluginRow(string pluginName, string pluginAdd, string pluginDesc)
        {
            int index = pluginPanel.RowDefinitions.Count;
            RowDefinition def = new RowDefinition();
            pluginPanel.RowDefinitions.Add(def);
            CustomLabel keyLabel = new CustomLabel(pluginName, index, 0);
            CustomLabel fileLabel = new CustomLabel(pluginDesc, index, 1);
            pluginPanel.Children.Add(keyLabel);
            pluginPanel.Children.Add(fileLabel);
            CustomButton configButton = new CustomButton(this, pluginName, pluginAdd, CONFIGURE, index, 2);
            CustomButton deleteButton = new CustomButton(this, pluginName, pluginAdd, UNREGISTER, index, 3);
            pluginPanel.Children.Add(configButton);
            pluginPanel.Children.Add(deleteButton);
        }

        private void okButtonClick(object sender, EventArgs e)
        {
            videoTrackerData.globals[gdg.PLUGIN_GLOBALS][gdk.PYTHONPATH] = pythonDirTextBox.Text;
            this.DialogResult = true;
        }


        private void registerButton_Click(object sender, EventArgs e)
        {
            if (openPluginFileDialog.ShowDialog() != true) return;

            Plugin plugin = new Plugin();
            if (plugin.Register(openPluginFileDialog.FileName, videoTrackerData))
            {
                string pluginName = plugin.pluginName;
                string pluginAdd = videoTrackerData.globals[pluginName][gpk.ADD];
                string pluginDesc = videoTrackerData.globals[pluginName][gpk.DESC];
                AddPluginRow(pluginName, pluginAdd, pluginDesc);
                this.videoTrackerForm.InsertPluginMenuItem(pluginName, pluginAdd);
            }
        }

        public void customButton_Click(object sender, EventArgs e)
        {
            CustomButton cb = (CustomButton)sender;
            string pluginName = cb.pluginName;
            string pluginAdd = cb.pluginAdd;
            int rowNum = cb.rowNum;

            if ((string) cb.operation  == UNREGISTER)
            {
                this.videoTrackerForm.DeletePluginMenuItem(pluginAdd);
                videoTrackerData.globals[gdg.PLUGINS].Remove(pluginName);
                foreach (string key in videoTrackerData.globals[pluginName].Keys)
                {
                    videoTrackerData.globals.Remove(pluginName);
                }
                pluginPanel.RowDefinitions[rowNum].Height = new GridLength(0); // Deleting rows is difficult - just hide it.

            }
            if ((string) cb.operation == CONFIGURE) {
                string errorString;
                Plugin plugin = new Plugin(pluginName, videoTrackerData);
                plugin.ConfigureGlobals(videoTrackerData, out errorString);
                if (errorString != "") App.ErrorBox(errorString);
            }
        }

        private void pythonDirectoryButtonClick(object sender, EventArgs e)
        {
            if (openPythonDirectoryDialog.ShowDialog() == true)
            {
                pythonDirTextBox.Text = openPythonDirectoryDialog.SelectedPath;
                videoTrackerData.globals[gdg.PLUGIN_GLOBALS][gdk.PYTHONPATH] = pythonDirTextBox.Text;
                CheckLibValidity();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

    }

    // Utility class to create a bordered label, with AutoSize set to true and with the text 
    // passed into the constructor. In order to keep the border thickness constant, we only
    // set the top/left border in the first row/column.
  
    public class CustomLabel : Label
    {
        public CustomLabel(string text, int row, int column)
        {
            this.Padding = new Thickness(5);

            Thickness borderThickness = new Thickness(0, 0, 1, 1);

            if (row == 0)
            {
                borderThickness.Top = 1.0;
                this.FontWeight = FontWeights.Bold;
            }
            if (column == 0)
            {
                borderThickness.Left = 1.0;
            }

            this.BorderThickness = borderThickness;
            this.BorderBrush = Brushes.Black;
            this.Content = text;
            Grid.SetRow(this, row);
            Grid.SetColumn(this, column);
        }
    }

    // Utility class to create a custom button and place it in the grid. Information about
    // the button's function is coded into the public properties for use by customButton_Click
    public class CustomButton : Button
    {
        public int rowNum;
        public string pluginName;
        public string pluginAdd;
        public string operation;

        public CustomButton(PluginSettingsForm pluginSettingsForm, string pluginName, string pluginAdd, string operation, int row, int column)
        {
            this.Content = operation;
            this.Click += new RoutedEventHandler(pluginSettingsForm.customButton_Click);

            this.operation = operation;
            this.pluginName = pluginName;
            this.pluginAdd = pluginAdd;
            this.rowNum = row;

            this.Padding = new Thickness(10, 2, 10, 2);
            this.Margin = new Thickness(2);

            Grid.SetRow(this, row);
            Grid.SetColumn(this, column);
        }
    }
}
