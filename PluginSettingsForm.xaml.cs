using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
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


        public PluginSettingsForm(VideoTrackerData vtd)
        {
            InitializeComponent();
            this.videoTrackerData = vtd;
            this.videoTrackerForm = vtd.videoTrackerForm;
            this.openPluginFileDialog = new OpenFileDialog();
            this.openPluginFileDialog.DefaultExt = "py";
            this.openPluginFileDialog.Filter = "Python files|*.py|All files|*.*";
            BuildPluginTable();
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
            BorderedButton configButton = new BorderedButton(this, pluginName, pluginAdd, CONFIGURE, index, 2);
            BorderedButton deleteButton = new BorderedButton(this, pluginName, pluginAdd, UNREGISTER, index, 3);
            pluginPanel.Children.Add(configButton);
            pluginPanel.Children.Add(deleteButton);
        }

        private void okButtonClick(object sender, EventArgs e)
        {
            this.DialogResult = true;
        }


        private void registerButton_Click(object sender, EventArgs e)
        {

            OpenFileDialog fd = openPluginFileDialog;
            if (fd.ShowDialog() != true) return;

            Plugin plugin = new Plugin();
            if (plugin.Register(fd.FileName, videoTrackerData))
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
                pluginPanel.RowDefinitions[rowNum].Height = new GridLength(0); // Deleting row is difficult - just hide it.

            }
            if ((string) cb.operation == CONFIGURE) {
                string errorString;
                Plugin plugin = new Plugin(pluginName, videoTrackerData);
                plugin.ConfigureGlobals(videoTrackerData, out errorString);
                if (errorString != "") MessageBox.Show(errorString);
            }
        }

        private void pythonDirectoryButtonClick(object sender, EventArgs e)
        {
            openPythonDirectoryDialog = new VistaFolderBrowserDialog();
            if (openPythonDirectoryDialog.ShowDialog() == true)
            {
                pythonDirTextBox.Text = openPythonDirectoryDialog.SelectedPath;
            }
        }
    }

    // Utility class to create a label, with AutoSize set to true and with the text passed into the constructor.
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

    public class BorderedButton : Border
    {
        private CustomButton button;

        public BorderedButton(PluginSettingsForm pluginSettingsForm, string pluginName, string pluginAdd, string operation, int row, int column)
        {
            button = new CustomButton(pluginSettingsForm, pluginName, pluginAdd, operation, row, column);
            this.Child = button;
            this.Padding = new Thickness(5);
            Grid.SetRow(this, row);
            Grid.SetColumn(this, column);
        }
    }

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
        }
    }
}
