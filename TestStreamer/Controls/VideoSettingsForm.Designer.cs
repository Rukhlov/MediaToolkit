namespace TestStreamer.Controls
{
    partial class VideoSettingsForm
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.transportComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.portNumeric = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.encoderComboBox = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.encProfileComboBox = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.destWidthNumeric = new System.Windows.Forms.NumericUpDown();
            this.MaxBitrateNumeric = new System.Windows.Forms.NumericUpDown();
            this.bitrateModeComboBox = new System.Windows.Forms.ComboBox();
            this.aspectRatioCheckBox = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.destHeightNumeric = new System.Windows.Forms.NumericUpDown();
            this.fpsNumeric = new System.Windows.Forms.NumericUpDown();
            this.bitrateNumeric = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.latencyModeCheckBox = new System.Windows.Forms.CheckBox();
            this.snippingToolButton = new System.Windows.Forms.Button();
            this.srcRectGroupBox = new System.Windows.Forms.GroupBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.captureTypesComboBox = new System.Windows.Forms.ComboBox();
            this.displayTextBox = new System.Windows.Forms.TextBox();
            this.captureMouseCheckBox = new System.Windows.Forms.CheckBox();
            this.captureRegionTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.portNumeric)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.destWidthNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxBitrateNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.destHeightNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateNumeric)).BeginInit();
            this.srcRectGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.addressTextBox);
            this.groupBox2.Controls.Add(this.transportComboBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.portNumeric);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(449, 120);
            this.groupBox2.TabIndex = 75;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "_NetworkSettings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 17);
            this.label3.TabIndex = 58;
            this.label3.Text = "Address:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // addressTextBox
            // 
            this.addressTextBox.Location = new System.Drawing.Point(99, 24);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(344, 22);
            this.addressTextBox.TabIndex = 56;
            this.addressTextBox.Text = "0.0.0.0";
            // 
            // transportComboBox
            // 
            this.transportComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.transportComboBox.FormattingEnabled = true;
            this.transportComboBox.Location = new System.Drawing.Point(99, 80);
            this.transportComboBox.Name = "transportComboBox";
            this.transportComboBox.Size = new System.Drawing.Size(107, 24);
            this.transportComboBox.TabIndex = 63;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 59;
            this.label2.Text = "Port:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 83);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 17);
            this.label5.TabIndex = 64;
            this.label5.Text = "Transport:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // portNumeric
            // 
            this.portNumeric.Location = new System.Drawing.Point(99, 52);
            this.portNumeric.Maximum = new decimal(new int[] {
            100500,
            0,
            0,
            0});
            this.portNumeric.Name = "portNumeric";
            this.portNumeric.Size = new System.Drawing.Size(107, 22);
            this.portNumeric.TabIndex = 57;
            this.portNumeric.Value = new decimal(new int[] {
            1234,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.encoderComboBox);
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.encProfileComboBox);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.destWidthNumeric);
            this.groupBox1.Controls.Add(this.MaxBitrateNumeric);
            this.groupBox1.Controls.Add(this.bitrateModeComboBox);
            this.groupBox1.Controls.Add(this.aspectRatioCheckBox);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.destHeightNumeric);
            this.groupBox1.Controls.Add(this.fpsNumeric);
            this.groupBox1.Controls.Add(this.bitrateNumeric);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.latencyModeCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 283);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(449, 267);
            this.groupBox1.TabIndex = 74;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "VideoSettings";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 84);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 17);
            this.label6.TabIndex = 46;
            this.label6.Text = "Encoder:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // encoderComboBox
            // 
            this.encoderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encoderComboBox.Enabled = false;
            this.encoderComboBox.FormattingEnabled = true;
            this.encoderComboBox.Location = new System.Drawing.Point(99, 81);
            this.encoderComboBox.Name = "encoderComboBox";
            this.encoderComboBox.Size = new System.Drawing.Size(214, 24);
            this.encoderComboBox.TabIndex = 47;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(15, 144);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(47, 17);
            this.label16.TabIndex = 55;
            this.label16.Text = "Mode:";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(15, 114);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(52, 17);
            this.label14.TabIndex = 52;
            this.label14.Text = "Profile:";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(14, 28);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(79, 17);
            this.label13.TabIndex = 30;
            this.label13.Text = "Resolution:";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // encProfileComboBox
            // 
            this.encProfileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encProfileComboBox.FormattingEnabled = true;
            this.encProfileComboBox.Location = new System.Drawing.Point(99, 111);
            this.encProfileComboBox.Name = "encProfileComboBox";
            this.encProfileComboBox.Size = new System.Drawing.Size(214, 24);
            this.encProfileComboBox.TabIndex = 53;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(199, 175);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(37, 17);
            this.label15.TabIndex = 55;
            this.label15.Text = "Max:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // destWidthNumeric
            // 
            this.destWidthNumeric.Location = new System.Drawing.Point(99, 26);
            this.destWidthNumeric.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.destWidthNumeric.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.destWidthNumeric.Name = "destWidthNumeric";
            this.destWidthNumeric.Size = new System.Drawing.Size(60, 22);
            this.destWidthNumeric.TabIndex = 26;
            this.destWidthNumeric.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // MaxBitrateNumeric
            // 
            this.MaxBitrateNumeric.Location = new System.Drawing.Point(243, 173);
            this.MaxBitrateNumeric.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.MaxBitrateNumeric.Name = "MaxBitrateNumeric";
            this.MaxBitrateNumeric.Size = new System.Drawing.Size(70, 22);
            this.MaxBitrateNumeric.TabIndex = 56;
            this.MaxBitrateNumeric.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // bitrateModeComboBox
            // 
            this.bitrateModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bitrateModeComboBox.FormattingEnabled = true;
            this.bitrateModeComboBox.Location = new System.Drawing.Point(99, 141);
            this.bitrateModeComboBox.Name = "bitrateModeComboBox";
            this.bitrateModeComboBox.Size = new System.Drawing.Size(214, 24);
            this.bitrateModeComboBox.TabIndex = 55;
            // 
            // aspectRatioCheckBox
            // 
            this.aspectRatioCheckBox.AutoSize = true;
            this.aspectRatioCheckBox.Checked = true;
            this.aspectRatioCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.aspectRatioCheckBox.Location = new System.Drawing.Point(17, 54);
            this.aspectRatioCheckBox.Name = "aspectRatioCheckBox";
            this.aspectRatioCheckBox.Size = new System.Drawing.Size(114, 21);
            this.aspectRatioCheckBox.TabIndex = 72;
            this.aspectRatioCheckBox.Text = "_AspectRatio";
            this.aspectRatioCheckBox.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(163, 28);
            this.label12.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(14, 17);
            this.label12.TabIndex = 28;
            this.label12.Text = "x";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // destHeightNumeric
            // 
            this.destHeightNumeric.Location = new System.Drawing.Point(181, 26);
            this.destHeightNumeric.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.destHeightNumeric.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.destHeightNumeric.Name = "destHeightNumeric";
            this.destHeightNumeric.Size = new System.Drawing.Size(60, 22);
            this.destHeightNumeric.TabIndex = 29;
            this.destHeightNumeric.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // fpsNumeric
            // 
            this.fpsNumeric.Location = new System.Drawing.Point(99, 201);
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
            this.fpsNumeric.Size = new System.Drawing.Size(70, 22);
            this.fpsNumeric.TabIndex = 5;
            this.fpsNumeric.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // bitrateNumeric
            // 
            this.bitrateNumeric.Location = new System.Drawing.Point(99, 173);
            this.bitrateNumeric.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.bitrateNumeric.Name = "bitrateNumeric";
            this.bitrateNumeric.Size = new System.Drawing.Size(70, 22);
            this.bitrateNumeric.TabIndex = 49;
            this.bitrateNumeric.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 175);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 17);
            this.label7.TabIndex = 50;
            this.label7.Text = "Bitrate:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 203);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "FPS:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // latencyModeCheckBox
            // 
            this.latencyModeCheckBox.AutoSize = true;
            this.latencyModeCheckBox.Checked = true;
            this.latencyModeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.latencyModeCheckBox.Location = new System.Drawing.Point(17, 229);
            this.latencyModeCheckBox.Name = "latencyModeCheckBox";
            this.latencyModeCheckBox.Size = new System.Drawing.Size(113, 21);
            this.latencyModeCheckBox.TabIndex = 51;
            this.latencyModeCheckBox.Text = "_LowLatency";
            this.latencyModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // snippingToolButton
            // 
            this.snippingToolButton.Location = new System.Drawing.Point(349, 21);
            this.snippingToolButton.Name = "snippingToolButton";
            this.snippingToolButton.Size = new System.Drawing.Size(94, 23);
            this.snippingToolButton.TabIndex = 73;
            this.snippingToolButton.Text = "_Select";
            this.snippingToolButton.UseVisualStyleBackColor = true;
            this.snippingToolButton.Click += new System.EventHandler(this.snippingToolButton_Click);
            // 
            // srcRectGroupBox
            // 
            this.srcRectGroupBox.Controls.Add(this.label9);
            this.srcRectGroupBox.Controls.Add(this.displayTextBox);
            this.srcRectGroupBox.Controls.Add(this.label8);
            this.srcRectGroupBox.Controls.Add(this.label4);
            this.srcRectGroupBox.Controls.Add(this.captureTypesComboBox);
            this.srcRectGroupBox.Controls.Add(this.captureRegionTextBox);
            this.srcRectGroupBox.Controls.Add(this.captureMouseCheckBox);
            this.srcRectGroupBox.Controls.Add(this.snippingToolButton);
            this.srcRectGroupBox.Location = new System.Drawing.Point(12, 138);
            this.srcRectGroupBox.Name = "srcRectGroupBox";
            this.srcRectGroupBox.Size = new System.Drawing.Size(449, 139);
            this.srcRectGroupBox.TabIndex = 70;
            this.srcRectGroupBox.TabStop = false;
            this.srcRectGroupBox.Text = "CaptureSettings";
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(246, 565);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(100, 23);
            this.applyButton.TabIndex = 76;
            this.applyButton.Text = "_Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(355, 565);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 23);
            this.cancelButton.TabIndex = 77;
            this.cancelButton.Text = "_Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 17);
            this.label4.TabIndex = 78;
            this.label4.Text = "Capture:";
            // 
            // captureTypesComboBox
            // 
            this.captureTypesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.captureTypesComboBox.Enabled = false;
            this.captureTypesComboBox.FormattingEnabled = true;
            this.captureTypesComboBox.Location = new System.Drawing.Point(96, 78);
            this.captureTypesComboBox.Name = "captureTypesComboBox";
            this.captureTypesComboBox.Size = new System.Drawing.Size(244, 24);
            this.captureTypesComboBox.TabIndex = 77;
            // 
            // displayTextBox
            // 
            this.displayTextBox.Location = new System.Drawing.Point(96, 50);
            this.displayTextBox.Name = "displayTextBox";
            this.displayTextBox.ReadOnly = true;
            this.displayTextBox.Size = new System.Drawing.Size(244, 22);
            this.displayTextBox.TabIndex = 80;
            // 
            // captureMouseCheckBox
            // 
            this.captureMouseCheckBox.AutoSize = true;
            this.captureMouseCheckBox.Checked = true;
            this.captureMouseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.captureMouseCheckBox.Location = new System.Drawing.Point(17, 108);
            this.captureMouseCheckBox.Name = "captureMouseCheckBox";
            this.captureMouseCheckBox.Size = new System.Drawing.Size(134, 21);
            this.captureMouseCheckBox.TabIndex = 69;
            this.captureMouseCheckBox.Text = "_Capture Mouse";
            this.captureMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // captureRegionTextBox
            // 
            this.captureRegionTextBox.Location = new System.Drawing.Point(96, 22);
            this.captureRegionTextBox.Name = "captureRegionTextBox";
            this.captureRegionTextBox.ReadOnly = true;
            this.captureRegionTextBox.Size = new System.Drawing.Size(244, 22);
            this.captureRegionTextBox.TabIndex = 76;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(15, 53);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 17);
            this.label8.TabIndex = 79;
            this.label8.Text = "Display:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(15, 24);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(57, 17);
            this.label9.TabIndex = 81;
            this.label9.Text = "Region:";
            // 
            // VideoSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 604);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.srcRectGroupBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VideoSettingsForm";
            this.Text = "_VideoSettings";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.portNumeric)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.destWidthNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxBitrateNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.destHeightNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateNumeric)).EndInit();
            this.srcRectGroupBox.ResumeLayout(false);
            this.srcRectGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox transportComboBox;
        private System.Windows.Forms.NumericUpDown portNumeric;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox addressTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown MaxBitrateNumeric;
        private System.Windows.Forms.ComboBox encoderComboBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox encProfileComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown bitrateNumeric;
        private System.Windows.Forms.ComboBox bitrateModeComboBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown fpsNumeric;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox latencyModeCheckBox;
        private System.Windows.Forms.Button snippingToolButton;
        private System.Windows.Forms.CheckBox aspectRatioCheckBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.NumericUpDown destWidthNumeric;
        private System.Windows.Forms.NumericUpDown destHeightNumeric;
        private System.Windows.Forms.GroupBox srcRectGroupBox;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox displayTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox captureTypesComboBox;
        private System.Windows.Forms.TextBox captureRegionTextBox;
        private System.Windows.Forms.CheckBox captureMouseCheckBox;
        private System.Windows.Forms.Label label9;
    }
}