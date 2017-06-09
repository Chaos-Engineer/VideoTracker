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
    /// Interaction logic for CrunchyRollVideoSeriesForm.xaml
    /// </summary>
    public partial class CrunchyRollVideoSeriesForm : Window
    {
        private VideoTrackerData videoTrackerData;
        private CrunchyRollVideoSeries crunchyRollVideoSeries;

        public CrunchyRollVideoSeriesForm(VideoTrackerData vtd)
        {
            this.videoTrackerData = vtd;
            this.Owner = vtd.videoTrackerForm;
            InitializeComponent();
        }

        public CrunchyRollVideoSeriesForm(VideoTrackerData vtd, CrunchyRollVideoSeries cs)
        {
            InitializeComponent();
            this.crunchyRollVideoSeries = cs;
            this.videoTrackerData = vtd;
            this.Owner = vtd.videoTrackerForm;
            this.titleBox.Text = cs.seriesTitle;
            this.URLBox.Text = cs.URL;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Check input for validity
            if (titleBox.Text.Equals(""))
            {
                ErrorDialog.Show("Title must be set");
                return;
            }

            //
            // If no URL is specified, try to deduce it from the title.
            //
            // Convert the URL to a standard all-lowercase form for future pattern-matching.
            //
            if (URLBox.Text.Equals("")) URLBox.Text = titleBox.Text.Replace(' ', '-').ToLower();
            int index = URLBox.Text.LastIndexOf('/');
            if (index < 0)
            {
                URLBox.Text = CrunchyRollVideoSeries.CrunchyRollUrlPrefix + "/" + URLBox.Text.ToLower();
            }
            else
            {
                URLBox.Text = CrunchyRollVideoSeries.CrunchyRollUrlPrefix + URLBox.Text.Substring(index).ToLower();
            }

            // Create a new series object, or update an existing one.

            
            if (crunchyRollVideoSeries == null)
            {
                crunchyRollVideoSeries = new CrunchyRollVideoSeries(URLBox.Text, titleBox.Text);
            }
            else
            {
                crunchyRollVideoSeries.seriesTitle = titleBox.Text;
                crunchyRollVideoSeries.URL = URLBox.Text;
            }
            crunchyRollVideoSeries.LoadSeries(titleBox.Text, "", videoTrackerData);
            this.DialogResult = true;
        }
    }
}
