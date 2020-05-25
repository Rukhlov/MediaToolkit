namespace ScreenStreamer.WinForms
{
    partial class VideoEncoderSettingsForm
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
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.aspectRatioComboBox = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.bitrateNumeric = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.bitrateModeComboBox = new System.Windows.Forms.ComboBox();
            this.latencyModeCheckBox = new System.Windows.Forms.CheckBox();
            this.MaxBitrateNumeric = new System.Windows.Forms.NumericUpDown();
            this.fpsNumeric = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.encProfileComboBox = new System.Windows.Forms.ComboBox();
            this.formatTextBox = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.gopSizeNumeric = new System.Windows.Forms.NumericUpDown();
            this.qualityNumeric = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxBitrateNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gopSizeNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityNumeric)).BeginInit();
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(339, 362);
            this.tableLayoutPanel1.TabIndex = 80;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(92, 328);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 4, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.panel1.Size = new System.Drawing.Size(245, 32);
            this.panel1.TabIndex = 60;
            // 
            // cancelButton
            // 
            this.cancelButton.AutoSize = true;
            this.cancelButton.Location = new System.Drawing.Point(134, 2);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(109, 28);
            this.cancelButton.TabIndex = 210;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.AutoSize = true;
            this.applyButton.Location = new System.Drawing.Point(16, 2);
            this.applyButton.Margin = new System.Windows.Forms.Padding(2);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(112, 28);
            this.applyButton.TabIndex = 200;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.tableLayoutPanel6);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 6, 6, 15);
            this.groupBox1.Size = new System.Drawing.Size(333, 318);
            this.groupBox1.TabIndex = 61;
            this.groupBox1.TabStop = false;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.AutoSize = true;
            this.tableLayoutPanel6.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.aspectRatioComboBox, 1, 3);
            this.tableLayoutPanel6.Controls.Add(this.label16, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.bitrateNumeric, 1, 4);
            this.tableLayoutPanel6.Controls.Add(this.label7, 0, 4);
            this.tableLayoutPanel6.Controls.Add(this.bitrateModeComboBox, 1, 2);
            this.tableLayoutPanel6.Controls.Add(this.latencyModeCheckBox, 0, 9);
            this.tableLayoutPanel6.Controls.Add(this.MaxBitrateNumeric, 1, 5);
            this.tableLayoutPanel6.Controls.Add(this.fpsNumeric, 1, 6);
            this.tableLayoutPanel6.Controls.Add(this.label1, 0, 6);
            this.tableLayoutPanel6.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.label14, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.encProfileComboBox, 1, 1);
            this.tableLayoutPanel6.Controls.Add(this.formatTextBox, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.label15, 0, 5);
            this.tableLayoutPanel6.Controls.Add(this.label2, 0, 8);
            this.tableLayoutPanel6.Controls.Add(this.gopSizeNumeric, 1, 8);
            this.tableLayoutPanel6.Controls.Add(this.qualityNumeric, 1, 7);
            this.tableLayoutPanel6.Controls.Add(this.label3, 0, 7);
            this.tableLayoutPanel6.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(6, 22);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 12;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(321, 281);
            this.tableLayoutPanel6.TabIndex = 81;
            // 
            // aspectRatioComboBox
            // 
            this.aspectRatioComboBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.aspectRatioComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.aspectRatioComboBox.FormattingEnabled = true;
            this.aspectRatioComboBox.Location = new System.Drawing.Point(127, 89);
            this.aspectRatioComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.aspectRatioComboBox.Name = "aspectRatioComboBox";
            this.aspectRatioComboBox.Size = new System.Drawing.Size(192, 25);
            this.aspectRatioComboBox.TabIndex = 120;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Location = new System.Drawing.Point(2, 60);
            this.label16.Margin = new System.Windows.Forms.Padding(2);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(120, 25);
            this.label16.TabIndex = 55;
            this.label16.Text = "Mode:";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bitrateNumeric
            // 
            this.bitrateNumeric.Dock = System.Windows.Forms.DockStyle.Right;
            this.bitrateNumeric.Location = new System.Drawing.Point(209, 119);
            this.bitrateNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.bitrateNumeric.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.bitrateNumeric.Name = "bitrateNumeric";
            this.bitrateNumeric.Size = new System.Drawing.Size(110, 23);
            this.bitrateNumeric.TabIndex = 104;
            this.bitrateNumeric.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(2, 119);
            this.label7.Margin = new System.Windows.Forms.Padding(2);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(120, 23);
            this.label7.TabIndex = 50;
            this.label7.Text = "Bitrate, kbps:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bitrateModeComboBox
            // 
            this.bitrateModeComboBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.bitrateModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bitrateModeComboBox.FormattingEnabled = true;
            this.bitrateModeComboBox.Location = new System.Drawing.Point(127, 60);
            this.bitrateModeComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.bitrateModeComboBox.Name = "bitrateModeComboBox";
            this.bitrateModeComboBox.Size = new System.Drawing.Size(192, 25);
            this.bitrateModeComboBox.TabIndex = 102;
            // 
            // latencyModeCheckBox
            // 
            this.latencyModeCheckBox.AutoSize = true;
            this.latencyModeCheckBox.Checked = true;
            this.latencyModeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel6.SetColumnSpan(this.latencyModeCheckBox, 2);
            this.latencyModeCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.latencyModeCheckBox.Location = new System.Drawing.Point(6, 258);
            this.latencyModeCheckBox.Margin = new System.Windows.Forms.Padding(6, 6, 2, 2);
            this.latencyModeCheckBox.Name = "latencyModeCheckBox";
            this.latencyModeCheckBox.Size = new System.Drawing.Size(313, 21);
            this.latencyModeCheckBox.TabIndex = 110;
            this.latencyModeCheckBox.Text = "Low Latency Mode";
            this.latencyModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // MaxBitrateNumeric
            // 
            this.MaxBitrateNumeric.Dock = System.Windows.Forms.DockStyle.Right;
            this.MaxBitrateNumeric.Location = new System.Drawing.Point(209, 146);
            this.MaxBitrateNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.MaxBitrateNumeric.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.MaxBitrateNumeric.Name = "MaxBitrateNumeric";
            this.MaxBitrateNumeric.Size = new System.Drawing.Size(110, 23);
            this.MaxBitrateNumeric.TabIndex = 106;
            this.MaxBitrateNumeric.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // fpsNumeric
            // 
            this.fpsNumeric.Dock = System.Windows.Forms.DockStyle.Right;
            this.fpsNumeric.Location = new System.Drawing.Point(209, 173);
            this.fpsNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.fpsNumeric.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.fpsNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.fpsNumeric.Name = "fpsNumeric";
            this.fpsNumeric.Size = new System.Drawing.Size(110, 23);
            this.fpsNumeric.TabIndex = 108;
            this.fpsNumeric.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(2, 173);
            this.label1.Margin = new System.Windows.Forms.Padding(2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 23);
            this.label1.TabIndex = 6;
            this.label1.Text = "FPS:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(2, 2);
            this.label6.Margin = new System.Windows.Forms.Padding(2);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 25);
            this.label6.TabIndex = 46;
            this.label6.Text = "Format:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Location = new System.Drawing.Point(2, 31);
            this.label14.Margin = new System.Windows.Forms.Padding(2);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(120, 25);
            this.label14.TabIndex = 52;
            this.label14.Text = "Profile:";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // encProfileComboBox
            // 
            this.encProfileComboBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.encProfileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encProfileComboBox.FormattingEnabled = true;
            this.encProfileComboBox.Location = new System.Drawing.Point(127, 31);
            this.encProfileComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.encProfileComboBox.Name = "encProfileComboBox";
            this.encProfileComboBox.Size = new System.Drawing.Size(192, 25);
            this.encProfileComboBox.TabIndex = 100;
            // 
            // formatTextBox
            // 
            this.formatTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formatTextBox.Location = new System.Drawing.Point(127, 3);
            this.formatTextBox.Name = "formatTextBox";
            this.formatTextBox.ReadOnly = true;
            this.formatTextBox.Size = new System.Drawing.Size(191, 23);
            this.formatTextBox.TabIndex = 113;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Location = new System.Drawing.Point(2, 146);
            this.label15.Margin = new System.Windows.Forms.Padding(2);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(120, 23);
            this.label15.TabIndex = 55;
            this.label15.Text = "Max Bitrate, kbps:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(2, 227);
            this.label2.Margin = new System.Windows.Forms.Padding(2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 23);
            this.label2.TabIndex = 114;
            this.label2.Text = "GOP Size:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gopSizeNumeric
            // 
            this.gopSizeNumeric.Dock = System.Windows.Forms.DockStyle.Right;
            this.gopSizeNumeric.Location = new System.Drawing.Point(209, 227);
            this.gopSizeNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.gopSizeNumeric.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.gopSizeNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.gopSizeNumeric.Name = "gopSizeNumeric";
            this.gopSizeNumeric.Size = new System.Drawing.Size(110, 23);
            this.gopSizeNumeric.TabIndex = 115;
            this.gopSizeNumeric.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            // 
            // qualityNumeric
            // 
            this.qualityNumeric.Dock = System.Windows.Forms.DockStyle.Right;
            this.qualityNumeric.Location = new System.Drawing.Point(209, 200);
            this.qualityNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.qualityNumeric.Name = "qualityNumeric";
            this.qualityNumeric.Size = new System.Drawing.Size(110, 23);
            this.qualityNumeric.TabIndex = 117;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(2, 200);
            this.label3.Margin = new System.Windows.Forms.Padding(2);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 23);
            this.label3.TabIndex = 118;
            this.label3.Text = "Quality:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(2, 89);
            this.label4.Margin = new System.Windows.Forms.Padding(2);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 26);
            this.label4.TabIndex = 119;
            this.label4.Text = "Aspect Ratio:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // VideoEncoderSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(421, 433);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VideoEncoderSettingsForm";
            this.ShowIcon = false;
            this.Text = "Video Encoder Settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxBitrateNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gopSizeNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.CheckBox latencyModeCheckBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown bitrateNumeric;
        private System.Windows.Forms.NumericUpDown MaxBitrateNumeric;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox bitrateModeComboBox;
        private System.Windows.Forms.NumericUpDown fpsNumeric;
        private System.Windows.Forms.ComboBox encProfileComboBox;
        private System.Windows.Forms.TextBox formatTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown gopSizeNumeric;
        private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.NumericUpDown qualityNumeric;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox aspectRatioComboBox;
		private System.Windows.Forms.Label label4;
	}
}