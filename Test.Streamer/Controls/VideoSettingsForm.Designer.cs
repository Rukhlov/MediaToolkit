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
            this.EncoderSettingsGroup = new System.Windows.Forms.GroupBox();
            this.adjustAspectRatioButton = new System.Windows.Forms.Button();
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
            this.ScreenCaptureGroup = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.displayTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.captureTypesComboBox = new System.Windows.Forms.ComboBox();
            this.captureRegionTextBox = new System.Windows.Forms.TextBox();
            this.captureMouseCheckBox = new System.Windows.Forms.CheckBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.WebCamGroup = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.CaptureDeviceProfilesComboBox = new System.Windows.Forms.ComboBox();
            this.CaptureDeviceTextBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.showCaptureBorderCheckBox = new System.Windows.Forms.CheckBox();
            this.showDebugInfoCheckBox = new System.Windows.Forms.CheckBox();
            this.EncoderSettingsGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.destWidthNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxBitrateNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.destHeightNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateNumeric)).BeginInit();
            this.ScreenCaptureGroup.SuspendLayout();
            this.WebCamGroup.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // EncoderSettingsGroup
            // 
            this.EncoderSettingsGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.EncoderSettingsGroup.Controls.Add(this.adjustAspectRatioButton);
            this.EncoderSettingsGroup.Controls.Add(this.label6);
            this.EncoderSettingsGroup.Controls.Add(this.encoderComboBox);
            this.EncoderSettingsGroup.Controls.Add(this.label16);
            this.EncoderSettingsGroup.Controls.Add(this.label14);
            this.EncoderSettingsGroup.Controls.Add(this.label13);
            this.EncoderSettingsGroup.Controls.Add(this.encProfileComboBox);
            this.EncoderSettingsGroup.Controls.Add(this.label15);
            this.EncoderSettingsGroup.Controls.Add(this.destWidthNumeric);
            this.EncoderSettingsGroup.Controls.Add(this.MaxBitrateNumeric);
            this.EncoderSettingsGroup.Controls.Add(this.bitrateModeComboBox);
            this.EncoderSettingsGroup.Controls.Add(this.aspectRatioCheckBox);
            this.EncoderSettingsGroup.Controls.Add(this.label12);
            this.EncoderSettingsGroup.Controls.Add(this.destHeightNumeric);
            this.EncoderSettingsGroup.Controls.Add(this.fpsNumeric);
            this.EncoderSettingsGroup.Controls.Add(this.bitrateNumeric);
            this.EncoderSettingsGroup.Controls.Add(this.label7);
            this.EncoderSettingsGroup.Controls.Add(this.label1);
            this.EncoderSettingsGroup.Controls.Add(this.latencyModeCheckBox);
            this.EncoderSettingsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EncoderSettingsGroup.Location = new System.Drawing.Point(3, 288);
            this.EncoderSettingsGroup.Name = "EncoderSettingsGroup";
            this.EncoderSettingsGroup.Size = new System.Drawing.Size(379, 268);
            this.EncoderSettingsGroup.TabIndex = 74;
            this.EncoderSettingsGroup.TabStop = false;
            this.EncoderSettingsGroup.Text = "EncoderSettings";
            // 
            // adjustAspectRatioButton
            // 
            this.adjustAspectRatioButton.Location = new System.Drawing.Point(264, 26);
            this.adjustAspectRatioButton.Name = "adjustAspectRatioButton";
            this.adjustAspectRatioButton.Size = new System.Drawing.Size(99, 27);
            this.adjustAspectRatioButton.TabIndex = 74;
            this.adjustAspectRatioButton.Text = "Adjust";
            this.adjustAspectRatioButton.UseVisualStyleBackColor = true;
            this.adjustAspectRatioButton.Click += new System.EventHandler(this.adjustAspectRatioButton_Click);
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
            this.encoderComboBox.Size = new System.Drawing.Size(264, 24);
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
            this.encProfileComboBox.Size = new System.Drawing.Size(264, 24);
            this.encProfileComboBox.TabIndex = 53;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(205, 175);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(82, 17);
            this.label15.TabIndex = 55;
            this.label15.Text = "Bitrate Max:";
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
            this.MaxBitrateNumeric.Location = new System.Drawing.Point(293, 173);
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
            this.bitrateModeComboBox.Size = new System.Drawing.Size(264, 24);
            this.bitrateModeComboBox.TabIndex = 55;
            // 
            // aspectRatioCheckBox
            // 
            this.aspectRatioCheckBox.AutoSize = true;
            this.aspectRatioCheckBox.Checked = true;
            this.aspectRatioCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.aspectRatioCheckBox.Location = new System.Drawing.Point(17, 54);
            this.aspectRatioCheckBox.Name = "aspectRatioCheckBox";
            this.aspectRatioCheckBox.Size = new System.Drawing.Size(106, 21);
            this.aspectRatioCheckBox.TabIndex = 72;
            this.aspectRatioCheckBox.Text = "AspectRatio";
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
            this.latencyModeCheckBox.Location = new System.Drawing.Point(18, 229);
            this.latencyModeCheckBox.Name = "latencyModeCheckBox";
            this.latencyModeCheckBox.Size = new System.Drawing.Size(105, 21);
            this.latencyModeCheckBox.TabIndex = 51;
            this.latencyModeCheckBox.Text = "LowLatency";
            this.latencyModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // snippingToolButton
            // 
            this.snippingToolButton.Location = new System.Drawing.Point(17, 135);
            this.snippingToolButton.Name = "snippingToolButton";
            this.snippingToolButton.Size = new System.Drawing.Size(160, 27);
            this.snippingToolButton.TabIndex = 73;
            this.snippingToolButton.Text = "SelectRegion";
            this.snippingToolButton.UseVisualStyleBackColor = true;
            this.snippingToolButton.Click += new System.EventHandler(this.snippingToolButton_Click);
            // 
            // ScreenCaptureGroup
            // 
            this.ScreenCaptureGroup.Controls.Add(this.showDebugInfoCheckBox);
            this.ScreenCaptureGroup.Controls.Add(this.showCaptureBorderCheckBox);
            this.ScreenCaptureGroup.Controls.Add(this.label5);
            this.ScreenCaptureGroup.Controls.Add(this.label9);
            this.ScreenCaptureGroup.Controls.Add(this.displayTextBox);
            this.ScreenCaptureGroup.Controls.Add(this.label8);
            this.ScreenCaptureGroup.Controls.Add(this.captureTypesComboBox);
            this.ScreenCaptureGroup.Controls.Add(this.captureRegionTextBox);
            this.ScreenCaptureGroup.Controls.Add(this.captureMouseCheckBox);
            this.ScreenCaptureGroup.Controls.Add(this.snippingToolButton);
            this.ScreenCaptureGroup.Location = new System.Drawing.Point(3, 100);
            this.ScreenCaptureGroup.Name = "ScreenCaptureGroup";
            this.ScreenCaptureGroup.Size = new System.Drawing.Size(379, 182);
            this.ScreenCaptureGroup.TabIndex = 70;
            this.ScreenCaptureGroup.TabStop = false;
            this.ScreenCaptureGroup.Text = "CaptureSettings";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 81);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 17);
            this.label5.TabIndex = 82;
            this.label5.Text = "Capture:";
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
            // displayTextBox
            // 
            this.displayTextBox.Location = new System.Drawing.Point(96, 50);
            this.displayTextBox.Name = "displayTextBox";
            this.displayTextBox.ReadOnly = true;
            this.displayTextBox.Size = new System.Drawing.Size(267, 22);
            this.displayTextBox.TabIndex = 80;
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
            // captureTypesComboBox
            // 
            this.captureTypesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.captureTypesComboBox.Enabled = false;
            this.captureTypesComboBox.FormattingEnabled = true;
            this.captureTypesComboBox.Location = new System.Drawing.Point(96, 78);
            this.captureTypesComboBox.Name = "captureTypesComboBox";
            this.captureTypesComboBox.Size = new System.Drawing.Size(267, 24);
            this.captureTypesComboBox.TabIndex = 77;
            // 
            // captureRegionTextBox
            // 
            this.captureRegionTextBox.Location = new System.Drawing.Point(96, 22);
            this.captureRegionTextBox.Name = "captureRegionTextBox";
            this.captureRegionTextBox.ReadOnly = true;
            this.captureRegionTextBox.Size = new System.Drawing.Size(267, 22);
            this.captureRegionTextBox.TabIndex = 76;
            // 
            // captureMouseCheckBox
            // 
            this.captureMouseCheckBox.AutoSize = true;
            this.captureMouseCheckBox.Checked = true;
            this.captureMouseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.captureMouseCheckBox.Location = new System.Drawing.Point(17, 108);
            this.captureMouseCheckBox.Name = "captureMouseCheckBox";
            this.captureMouseCheckBox.Size = new System.Drawing.Size(126, 21);
            this.captureMouseCheckBox.TabIndex = 69;
            this.captureMouseCheckBox.Text = "Capture Mouse";
            this.captureMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(37, 3);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(100, 27);
            this.applyButton.TabIndex = 76;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(146, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 27);
            this.cancelButton.TabIndex = 77;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // WebCamGroup
            // 
            this.WebCamGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.WebCamGroup.Controls.Add(this.label2);
            this.WebCamGroup.Controls.Add(this.label3);
            this.WebCamGroup.Controls.Add(this.CaptureDeviceProfilesComboBox);
            this.WebCamGroup.Controls.Add(this.CaptureDeviceTextBox);
            this.WebCamGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WebCamGroup.Location = new System.Drawing.Point(3, 3);
            this.WebCamGroup.Name = "WebCamGroup";
            this.WebCamGroup.Size = new System.Drawing.Size(379, 91);
            this.WebCamGroup.TabIndex = 78;
            this.WebCamGroup.TabStop = false;
            this.WebCamGroup.Text = "CaptureSettings";
            this.WebCamGroup.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 17);
            this.label2.TabIndex = 81;
            this.label2.Text = "Device:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 17);
            this.label3.TabIndex = 79;
            this.label3.Text = "Profile:";
            // 
            // CaptureDeviceProfilesComboBox
            // 
            this.CaptureDeviceProfilesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CaptureDeviceProfilesComboBox.Enabled = false;
            this.CaptureDeviceProfilesComboBox.FormattingEnabled = true;
            this.CaptureDeviceProfilesComboBox.Location = new System.Drawing.Point(96, 50);
            this.CaptureDeviceProfilesComboBox.Name = "CaptureDeviceProfilesComboBox";
            this.CaptureDeviceProfilesComboBox.Size = new System.Drawing.Size(267, 24);
            this.CaptureDeviceProfilesComboBox.TabIndex = 77;
            // 
            // CaptureDeviceTextBox
            // 
            this.CaptureDeviceTextBox.Location = new System.Drawing.Point(96, 22);
            this.CaptureDeviceTextBox.Name = "CaptureDeviceTextBox";
            this.CaptureDeviceTextBox.ReadOnly = true;
            this.CaptureDeviceTextBox.Size = new System.Drawing.Size(267, 22);
            this.CaptureDeviceTextBox.TabIndex = 76;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.WebCamGroup, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.ScreenCaptureGroup, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.EncoderSettingsGroup, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 3, 11, 11);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(385, 598);
            this.tableLayoutPanel1.TabIndex = 79;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(133, 562);
            this.panel1.Name = "panel1";
            this.panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.panel1.Size = new System.Drawing.Size(249, 33);
            this.panel1.TabIndex = 80;
            // 
            // showCaptureBorderCheckBox
            // 
            this.showCaptureBorderCheckBox.AutoSize = true;
            this.showCaptureBorderCheckBox.Checked = true;
            this.showCaptureBorderCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showCaptureBorderCheckBox.Location = new System.Drawing.Point(201, 114);
            this.showCaptureBorderCheckBox.Name = "showCaptureBorderCheckBox";
            this.showCaptureBorderCheckBox.Size = new System.Drawing.Size(162, 21);
            this.showCaptureBorderCheckBox.TabIndex = 83;
            this.showCaptureBorderCheckBox.Text = "Show Capture Frame";
            this.showCaptureBorderCheckBox.UseVisualStyleBackColor = true;
            // 
            // showDebugInfoCheckBox
            // 
            this.showDebugInfoCheckBox.AutoSize = true;
            this.showDebugInfoCheckBox.Checked = true;
            this.showDebugInfoCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showDebugInfoCheckBox.Location = new System.Drawing.Point(201, 141);
            this.showDebugInfoCheckBox.Name = "showDebugInfoCheckBox";
            this.showDebugInfoCheckBox.Size = new System.Drawing.Size(137, 21);
            this.showDebugInfoCheckBox.TabIndex = 84;
            this.showDebugInfoCheckBox.Text = "Show Debug Info";
            this.showDebugInfoCheckBox.UseVisualStyleBackColor = true;
            // 
            // VideoSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(482, 657);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VideoSettingsForm";
            this.Text = "VideoSettings";
            this.EncoderSettingsGroup.ResumeLayout(false);
            this.EncoderSettingsGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.destWidthNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxBitrateNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.destHeightNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateNumeric)).EndInit();
            this.ScreenCaptureGroup.ResumeLayout(false);
            this.ScreenCaptureGroup.PerformLayout();
            this.WebCamGroup.ResumeLayout(false);
            this.WebCamGroup.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox EncoderSettingsGroup;
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
        private System.Windows.Forms.GroupBox ScreenCaptureGroup;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox displayTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox captureTypesComboBox;
        private System.Windows.Forms.TextBox captureRegionTextBox;
        private System.Windows.Forms.CheckBox captureMouseCheckBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button adjustAspectRatioButton;
        private System.Windows.Forms.GroupBox WebCamGroup;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox CaptureDeviceProfilesComboBox;
        private System.Windows.Forms.TextBox CaptureDeviceTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox showDebugInfoCheckBox;
        private System.Windows.Forms.CheckBox showCaptureBorderCheckBox;
    }
}