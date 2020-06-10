namespace ScreenStreamer.WinForms
{
    partial class AudioCaptSettingsForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CaptureDetailsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.captureTypesComboBox = new System.Windows.Forms.ComboBox();
            this.exclusiveModeCheckBox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.bufferSizeNumeric = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.eventSyncModeCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.CaptureDetailsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bufferSizeNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(11, 11);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 10, 11);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(419, 187);
            this.tableLayoutPanel1.TabIndex = 80;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(181, 153);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 4, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.panel1.Size = new System.Drawing.Size(236, 32);
            this.panel1.TabIndex = 60;
            // 
            // cancelButton
            // 
            this.cancelButton.AutoSize = true;
            this.cancelButton.Location = new System.Drawing.Point(134, 2);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 28);
            this.cancelButton.TabIndex = 210;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.AutoSize = true;
            this.applyButton.Location = new System.Drawing.Point(30, 2);
            this.applyButton.Margin = new System.Windows.Forms.Padding(2);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(100, 28);
            this.applyButton.TabIndex = 200;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.CaptureDetailsPanel);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 6, 6, 15);
            this.groupBox1.Size = new System.Drawing.Size(413, 143);
            this.groupBox1.TabIndex = 61;
            this.groupBox1.TabStop = false;
            // 
            // CaptureDetailsPanel
            // 
            this.CaptureDetailsPanel.AutoSize = true;
            this.CaptureDetailsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CaptureDetailsPanel.ColumnCount = 2;
            this.CaptureDetailsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.CaptureDetailsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.CaptureDetailsPanel.Controls.Add(this.captureTypesComboBox, 1, 0);
            this.CaptureDetailsPanel.Controls.Add(this.exclusiveModeCheckBox, 0, 2);
            this.CaptureDetailsPanel.Controls.Add(this.label5, 0, 0);
            this.CaptureDetailsPanel.Controls.Add(this.bufferSizeNumeric, 1, 1);
            this.CaptureDetailsPanel.Controls.Add(this.label1, 0, 1);
            this.CaptureDetailsPanel.Controls.Add(this.eventSyncModeCheckBox, 0, 3);
            this.CaptureDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CaptureDetailsPanel.Location = new System.Drawing.Point(6, 22);
            this.CaptureDetailsPanel.Margin = new System.Windows.Forms.Padding(2, 6, 0, 2);
            this.CaptureDetailsPanel.Name = "CaptureDetailsPanel";
            this.CaptureDetailsPanel.RowCount = 6;
            this.CaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.CaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.CaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.CaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.CaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.CaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.CaptureDetailsPanel.Size = new System.Drawing.Size(401, 106);
            this.CaptureDetailsPanel.TabIndex = 109;
            // 
            // captureTypesComboBox
            // 
            this.captureTypesComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.captureTypesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.captureTypesComboBox.FormattingEnabled = true;
            this.captureTypesComboBox.Location = new System.Drawing.Point(128, 2);
            this.captureTypesComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.captureTypesComboBox.Name = "captureTypesComboBox";
            this.captureTypesComboBox.Size = new System.Drawing.Size(271, 25);
            this.captureTypesComboBox.TabIndex = 83;
            // 
            // exclusiveModeCheckBox
            // 
            this.exclusiveModeCheckBox.AutoSize = true;
            this.exclusiveModeCheckBox.Checked = true;
            this.exclusiveModeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CaptureDetailsPanel.SetColumnSpan(this.exclusiveModeCheckBox, 2);
            this.exclusiveModeCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exclusiveModeCheckBox.Location = new System.Drawing.Point(5, 58);
            this.exclusiveModeCheckBox.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.exclusiveModeCheckBox.Name = "exclusiveModeCheckBox";
            this.exclusiveModeCheckBox.Size = new System.Drawing.Size(394, 21);
            this.exclusiveModeCheckBox.TabIndex = 80;
            this.exclusiveModeCheckBox.Text = "Exclusive Mode";
            this.exclusiveModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(2, 2);
            this.label5.Margin = new System.Windows.Forms.Padding(2);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(122, 25);
            this.label5.TabIndex = 84;
            this.label5.Text = "Capture Type:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bufferSizeNumeric
            // 
            this.bufferSizeNumeric.Dock = System.Windows.Forms.DockStyle.Right;
            this.bufferSizeNumeric.Location = new System.Drawing.Point(328, 31);
            this.bufferSizeNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.bufferSizeNumeric.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.bufferSizeNumeric.Name = "bufferSizeNumeric";
            this.bufferSizeNumeric.Size = new System.Drawing.Size(71, 23);
            this.bufferSizeNumeric.TabIndex = 108;
            this.bufferSizeNumeric.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(2, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 23);
            this.label1.TabIndex = 6;
            this.label1.Text = "Buffer Size, msec:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // eventSyncModeCheckBox
            // 
            this.eventSyncModeCheckBox.AutoSize = true;
            this.eventSyncModeCheckBox.Checked = true;
            this.eventSyncModeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CaptureDetailsPanel.SetColumnSpan(this.eventSyncModeCheckBox, 2);
            this.eventSyncModeCheckBox.Location = new System.Drawing.Point(5, 83);
            this.eventSyncModeCheckBox.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.eventSyncModeCheckBox.Name = "eventSyncModeCheckBox";
            this.eventSyncModeCheckBox.Size = new System.Drawing.Size(140, 21);
            this.eventSyncModeCheckBox.TabIndex = 82;
            this.eventSyncModeCheckBox.Text = "Event Sync Mode";
            this.eventSyncModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // AudioCaptSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(466, 268);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AudioCaptSettingsForm";
            this.ShowIcon = false;
            this.Text = "Audio Capture Settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.CaptureDetailsPanel.ResumeLayout(false);
            this.CaptureDetailsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bufferSizeNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown bufferSizeNumeric;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel CaptureDetailsPanel;
        private System.Windows.Forms.CheckBox exclusiveModeCheckBox;
        private System.Windows.Forms.CheckBox eventSyncModeCheckBox;
		private System.Windows.Forms.ComboBox captureTypesComboBox;
		private System.Windows.Forms.Label label5;
	}
}