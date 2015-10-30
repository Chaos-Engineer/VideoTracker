﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

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
            if (vtd.columns > 0)
            {
                columnsTextBox.Text = vtd.columns.ToString();
            }
            else
            {
                columnsTextBox.Text = "1";
            }

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
            if (!Int32.TryParse(columnsTextBox.Text, out videoTrackerData.columns))
            {
                videoTrackerData.columns = 1;
            }
            videoTrackerData.videoTrackerForm.ResizeMainPanel();

            videoTrackerData.awsPublicKey = publicKeyTextBox.Text;
            videoTrackerData.awsSecretKey = secretKeyTextBox.Text;
            videoTrackerData.awsAffiliateID = affiliateIdTextBox.Text;
            this.settingsValid = true;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel l = sender as LinkLabel;
            Process.Start(l.Text);
        }

    }
}
