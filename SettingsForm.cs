using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoTracker
{
    public partial class SettingsForm : Form
    {
        private bool settingsValid;
        private VideoTrackerData videoTrackerData;

        public SettingsForm(VideoTrackerData vtd)
        {
            InitializeComponent();
            this.videoTrackerData = vtd;
            if (vtd.awsPublicKey != null)
            {
                publicKeyTextBox.Text = vtd.awsPublicKey;
            }
            if (vtd.awsSecretKey != null)
            {
                secretKeyTextBox.Text = vtd.awsSecretKey;
            }
            if (vtd.awsAffiliateID != null) {
                affiliateIdTextBox.Text = vtd.awsAffiliateID;
            }
        }

        //
        // The Windows library draws text vertically down left-aligned tabs. This routine
        // allows text to be drawn horizontally, as expected. (If the tab width is too 
        // narrow, increase the TabControl object's *height* attribute.)
        //
        private void tabControl_DrawItem(Object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush brush;
            Font font;

            // Get the item from the collection.
            TabPage tabPage = tabControl.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
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

            // Draw string. Center the text.
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
            applyButton.PerformClick();
            if (settingsValid)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            videoTrackerData.awsPublicKey = publicKeyTextBox.Text;
            videoTrackerData.awsSecretKey = secretKeyTextBox.Text;
            videoTrackerData.awsAffiliateID = affiliateIdTextBox.Text;
            this.settingsValid = true;
        }
    }
}
