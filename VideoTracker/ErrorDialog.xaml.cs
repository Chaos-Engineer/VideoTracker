using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Custom error dialog. This always displays an error message. An optional "detail"
    /// argument can also be specified. This is initially minimized, but can be expanded
    /// and then displayed as either text or HTML.
    /// 
    /// </summary>
    public partial class ErrorDialog : Window
    {
        private bool expanded = false;
        private bool htmlMode = false;
        private bool htmlPresent = false;
        private bool htmlLoaded = false;
        private string details = "";
        private string error = "";

        public static void Show(string error, string details)
        {
            ErrorDialog e = new ErrorDialog(error, details);
            e.ShowDialog();
        }

        public static void Show(string error)
        {
            ErrorDialog e = new ErrorDialog(error, "");
            e.ShowDialog();
        }
 
        private ErrorDialog(string error, string details)
        {
            InitializeComponent();
            this.details = details;
            this.error = error;

            this.Error.Text = error;
            if (this.details == null || this.details == "")
            {
                this.ExpandDetailsButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Details.Text = this.details;
                if (this.details.Contains('<'))  htmlPresent = true;
            }
            SetVisibility();
        }

        private void expandDetailsButtonClick(object sender, EventArgs e)
        {
            if (expanded)
            {
                expanded = false;
                SetVisibility();
            }
            else
            {
                expanded = true;
                SetVisibility();
            }
        }

        private void modeButtonClick(object sender, EventArgs e)
        {
            if (htmlMode)
            {
                htmlMode = false;
                SetVisibility();
            }
            else
            {
                htmlMode = true;
                SetVisibility();
            }

        }

        private void SetVisibility()
        {
            // The "reason" box can be expanded or hidden.
            // If it is expanded, and if it contains HTML (starts with '>'), then the
            // user is given the option of displaying as TEXT or HTML. Otherwise the
            // box can be displayed as TEXT only.

            if (expanded)
            {
                if (htmlPresent)
                {
                    this.ModeButton.Visibility = Visibility.Visible;
                }

                if (htmlMode)
                {
                    this.ModeButton.Content = "Display as text";
                    this.DetailsHTML.Visibility = Visibility.Visible;
                    this.Details.Visibility = Visibility.Collapsed;
                    if (!htmlLoaded)
                    {
                        // Undocumented Windows feature to disable Javascript error
                        // messages within WebBrowser objects
                        dynamic activeX = this.DetailsHTML.GetType().InvokeMember("ActiveXInstance",
                            BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                            null, this.DetailsHTML, new object[] { });
                        activeX.Silent = true;
                        using (new WaitCursor())
                        {
                            this.DetailsHTML.NavigateToString(this.details);
                        }
                        htmlLoaded = true;
                    }
                }
                else
                {
                    this.ModeButton.Content = "Display as HTML";
                    this.DetailsHTML.Visibility = Visibility.Collapsed;
                    this.Details.Visibility = Visibility.Visible;
                }
                this.ExpandDetailsButton.Content = "Reduce";
            }
            else
            {
                this.Details.Visibility = Visibility.Collapsed;
                this.DetailsHTML.Visibility = Visibility.Collapsed;
                this.ModeButton.Visibility = Visibility.Collapsed;
                this.ExpandDetailsButton.Content = "Expand";
            }
        }

        private void okButtonClick(object sender, EventArgs e)
        {
            this.DialogResult = true;
            return;
        }

    }
}
