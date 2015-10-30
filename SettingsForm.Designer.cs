namespace VideoTracker
{
    partial class SettingsForm
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.generalSettings = new System.Windows.Forms.TabPage();
            this.columnsTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.amazonSettings = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.affiliateIdTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.publicKeyTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.secretKeyTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.generalSettings.SuspendLayout();
            this.amazonSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tabControl.Controls.Add(this.generalSettings);
            this.tabControl.Controls.Add(this.amazonSettings);
            this.tabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl.ItemSize = new System.Drawing.Size(25, 75);
            this.tabControl.Location = new System.Drawing.Point(1, 2);
            this.tabControl.Multiline = true;
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(658, 153);
            this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl.TabIndex = 0;
            this.tabControl.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl_DrawItem);
            // 
            // generalSettings
            // 
            this.generalSettings.Controls.Add(this.columnsTextBox);
            this.generalSettings.Controls.Add(this.label7);
            this.generalSettings.Controls.Add(this.label1);
            this.generalSettings.Location = new System.Drawing.Point(79, 4);
            this.generalSettings.Name = "generalSettings";
            this.generalSettings.Padding = new System.Windows.Forms.Padding(3);
            this.generalSettings.Size = new System.Drawing.Size(575, 145);
            this.generalSettings.TabIndex = 0;
            this.generalSettings.Text = "General";
            this.generalSettings.UseVisualStyleBackColor = true;
            // 
            // columnsTextBox
            // 
            this.columnsTextBox.Location = new System.Drawing.Point(99, 27);
            this.columnsTextBox.Name = "columnsTextBox";
            this.columnsTextBox.Size = new System.Drawing.Size(31, 20);
            this.columnsTextBox.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 34);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Display Columns:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Global Settings";
            // 
            // amazonSettings
            // 
            this.amazonSettings.Controls.Add(this.label2);
            this.amazonSettings.Controls.Add(this.label3);
            this.amazonSettings.Controls.Add(this.linkLabel1);
            this.amazonSettings.Controls.Add(this.label4);
            this.amazonSettings.Controls.Add(this.affiliateIdTextBox);
            this.amazonSettings.Controls.Add(this.label5);
            this.amazonSettings.Controls.Add(this.publicKeyTextBox);
            this.amazonSettings.Controls.Add(this.label6);
            this.amazonSettings.Controls.Add(this.secretKeyTextBox);
            this.amazonSettings.Location = new System.Drawing.Point(79, 4);
            this.amazonSettings.Name = "amazonSettings";
            this.amazonSettings.Padding = new System.Windows.Forms.Padding(3);
            this.amazonSettings.Size = new System.Drawing.Size(575, 145);
            this.amazonSettings.TabIndex = 1;
            this.amazonSettings.Text = "Amazon";
            this.amazonSettings.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Amazon Settings";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(3, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(508, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "To import Amazon videos, you must obtain an affiliate ID and keys by following th" +
    "e instructions at this URL:";
            // 
            // linkLabel1
            // 
            this.linkLabel1.Location = new System.Drawing.Point(3, 26);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(560, 23);
            this.linkLabel1.TabIndex = 8;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://docs.aws.amazon.com/AWSECommerceService/latest/DG/becomingAssociate.html";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Affiliate ID:";
            // 
            // affiliateIdTextBox
            // 
            this.affiliateIdTextBox.Location = new System.Drawing.Point(75, 49);
            this.affiliateIdTextBox.Name = "affiliateIdTextBox";
            this.affiliateIdTextBox.Size = new System.Drawing.Size(100, 20);
            this.affiliateIdTextBox.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Public Key:";
            // 
            // publicKeyTextBox
            // 
            this.publicKeyTextBox.Location = new System.Drawing.Point(75, 78);
            this.publicKeyTextBox.Name = "publicKeyTextBox";
            this.publicKeyTextBox.Size = new System.Drawing.Size(292, 20);
            this.publicKeyTextBox.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 107);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Secret Key:";
            // 
            // secretKeyTextBox
            // 
            this.secretKeyTextBox.Location = new System.Drawing.Point(75, 104);
            this.secretKeyTextBox.Name = "secretKeyTextBox";
            this.secretKeyTextBox.Size = new System.Drawing.Size(292, 20);
            this.secretKeyTextBox.TabIndex = 12;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(260, 161);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(170, 161);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 2;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(80, 161);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(671, 196);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.cancelButton);
            this.Location = new System.Drawing.Point(6, 6);
            this.Name = "SettingsForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.tabControl.ResumeLayout(false);
            this.generalSettings.ResumeLayout(false);
            this.generalSettings.PerformLayout();
            this.amazonSettings.ResumeLayout(false);
            this.amazonSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage generalSettings;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TabPage amazonSettings;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox affiliateIdTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox publicKeyTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox secretKeyTextBox;
        private System.Windows.Forms.TextBox columnsTextBox;
        private System.Windows.Forms.Label label7;
    }
}
