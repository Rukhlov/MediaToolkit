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
            this.snippingToolButton = new System.Windows.Forms.Button();
            this.ScreenCaptureGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.captureMouseCheckBox = new System.Windows.Forms.CheckBox();
            this.showDebugInfoCheckBox = new System.Windows.Forms.CheckBox();
            this.showCaptureBorderCheckBox = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.displayTextBox = new System.Windows.Forms.TextBox();
            this.labelDisplay = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.captureRegionTextBox = new System.Windows.Forms.TextBox();
            this.captureTypesComboBox = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.applyButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.WebCamGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CaptureDeviceProfilesComboBox = new System.Windows.Forms.ComboBox();
            this.CaptureDeviceTextBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.EncoderSettingsGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.label16 = new System.Windows.Forms.Label();
            this.encoderComboBox = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.bitrateModeComboBox = new System.Windows.Forms.ComboBox();
            this.encProfileComboBox = new System.Windows.Forms.ComboBox();
            this.latencyModeCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.fpsNumeric = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.MaxBitrateNumeric = new System.Windows.Forms.NumericUpDown();
            this.bitrateNumeric = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.panelEncoderResoulution = new System.Windows.Forms.Panel();
            this.destHeightNumeric = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.destWidthNumeric = new System.Windows.Forms.NumericUpDown();
            this.adjustAspectRatioButton = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.aspectRatioCheckBox = new System.Windows.Forms.CheckBox();
            this.checkBoxResoulutionFromSource = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ScreenCaptureGroup.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.WebCamGroup.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.EncoderSettingsGroup.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxBitrateNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateNumeric)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            this.panelEncoderResoulution.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.destHeightNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.destWidthNumeric)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // snippingToolButton
            // 
            this.snippingToolButton.Location = new System.Drawing.Point(303, 58);
            this.snippingToolButton.Margin = new System.Windows.Forms.Padding(2);
            this.snippingToolButton.Name = "snippingToolButton";
            this.tableLayoutPanel4.SetRowSpan(this.snippingToolButton, 2);
            this.snippingToolButton.Size = new System.Drawing.Size(151, 30);
            this.snippingToolButton.TabIndex = 88;
            this.snippingToolButton.Text = "Select Region";
            this.snippingToolButton.UseVisualStyleBackColor = true;
            this.snippingToolButton.Visible = false;
            this.snippingToolButton.Click += new System.EventHandler(this.snippingToolButton_Click);
            // 
            // ScreenCaptureGroup
            // 
            this.ScreenCaptureGroup.AutoSize = true;
            this.ScreenCaptureGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ScreenCaptureGroup.Controls.Add(this.tableLayoutPanel3);
            this.ScreenCaptureGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScreenCaptureGroup.Location = new System.Drawing.Point(2, 136);
            this.ScreenCaptureGroup.Margin = new System.Windows.Forms.Padding(2);
            this.ScreenCaptureGroup.Name = "ScreenCaptureGroup";
            this.ScreenCaptureGroup.Padding = new System.Windows.Forms.Padding(6, 15, 6, 15);
            this.ScreenCaptureGroup.Size = new System.Drawing.Size(472, 248);
            this.ScreenCaptureGroup.TabIndex = 70;
            this.ScreenCaptureGroup.TabStop = false;
            this.ScreenCaptureGroup.Text = "Capture Settings";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.displayTextBox, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelDisplay, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.captureRegionTextBox, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.captureTypesComboBox, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.label9, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(6, 35);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 4;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(460, 198);
            this.tableLayoutPanel3.TabIndex = 80;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel3.SetColumnSpan(this.tableLayoutPanel4, 2);
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.captureMouseCheckBox, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.showDebugInfoCheckBox, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.showCaptureBorderCheckBox, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.snippingToolButton, 2, 2);
            this.tableLayoutPanel4.Controls.Add(this.button1, 2, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(2, 104);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(2, 10, 2, 2);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(456, 92);
            this.tableLayoutPanel4.TabIndex = 81;
            // 
            // captureMouseCheckBox
            // 
            this.captureMouseCheckBox.AutoSize = true;
            this.captureMouseCheckBox.Checked = true;
            this.captureMouseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.captureMouseCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.captureMouseCheckBox.Location = new System.Drawing.Point(5, 2);
            this.captureMouseCheckBox.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.captureMouseCheckBox.Name = "captureMouseCheckBox";
            this.captureMouseCheckBox.Size = new System.Drawing.Size(168, 24);
            this.captureMouseCheckBox.TabIndex = 80;
            this.captureMouseCheckBox.Text = "Capture Mouse";
            this.captureMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // showDebugInfoCheckBox
            // 
            this.showDebugInfoCheckBox.AutoSize = true;
            this.showDebugInfoCheckBox.Checked = true;
            this.showDebugInfoCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showDebugInfoCheckBox.Location = new System.Drawing.Point(5, 30);
            this.showDebugInfoCheckBox.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.showDebugInfoCheckBox.Name = "showDebugInfoCheckBox";
            this.showDebugInfoCheckBox.Size = new System.Drawing.Size(146, 24);
            this.showDebugInfoCheckBox.TabIndex = 82;
            this.showDebugInfoCheckBox.Text = "Show Debug Info";
            this.showDebugInfoCheckBox.UseVisualStyleBackColor = true;
            this.showDebugInfoCheckBox.CheckedChanged += new System.EventHandler(this.showDebugInfoCheckBox_CheckedChanged);
            // 
            // showCaptureBorderCheckBox
            // 
            this.showCaptureBorderCheckBox.AutoSize = true;
            this.showCaptureBorderCheckBox.Checked = true;
            this.showCaptureBorderCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showCaptureBorderCheckBox.Location = new System.Drawing.Point(5, 58);
            this.showCaptureBorderCheckBox.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.showCaptureBorderCheckBox.Name = "showCaptureBorderCheckBox";
            this.showCaptureBorderCheckBox.Size = new System.Drawing.Size(168, 24);
            this.showCaptureBorderCheckBox.TabIndex = 84;
            this.showCaptureBorderCheckBox.Text = "Show Capture Frame";
            this.showCaptureBorderCheckBox.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(303, 2);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.tableLayoutPanel4.SetRowSpan(this.button1, 2);
            this.button1.Size = new System.Drawing.Size(150, 30);
            this.button1.TabIndex = 86;
            this.button1.Text = "Preview";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // displayTextBox
            // 
            this.displayTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.displayTextBox.Location = new System.Drawing.Point(70, 2);
            this.displayTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.displayTextBox.Name = "displayTextBox";
            this.displayTextBox.ReadOnly = true;
            this.displayTextBox.Size = new System.Drawing.Size(388, 27);
            this.displayTextBox.TabIndex = 73;
            // 
            // labelDisplay
            // 
            this.labelDisplay.AutoSize = true;
            this.labelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelDisplay.Location = new System.Drawing.Point(2, 0);
            this.labelDisplay.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDisplay.Name = "labelDisplay";
            this.labelDisplay.Size = new System.Drawing.Size(64, 31);
            this.labelDisplay.TabIndex = 79;
            this.labelDisplay.Text = "Display:";
            this.labelDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(2, 62);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 32);
            this.label5.TabIndex = 82;
            this.label5.Text = "Capture:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // captureRegionTextBox
            // 
            this.captureRegionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.captureRegionTextBox.Location = new System.Drawing.Point(70, 33);
            this.captureRegionTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.captureRegionTextBox.Name = "captureRegionTextBox";
            this.captureRegionTextBox.ReadOnly = true;
            this.captureRegionTextBox.Size = new System.Drawing.Size(388, 27);
            this.captureRegionTextBox.TabIndex = 76;
            // 
            // captureTypesComboBox
            // 
            this.captureTypesComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.captureTypesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.captureTypesComboBox.Enabled = false;
            this.captureTypesComboBox.FormattingEnabled = true;
            this.captureTypesComboBox.Location = new System.Drawing.Point(70, 64);
            this.captureTypesComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.captureTypesComboBox.Name = "captureTypesComboBox";
            this.captureTypesComboBox.Size = new System.Drawing.Size(388, 28);
            this.captureTypesComboBox.TabIndex = 77;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(2, 31);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(64, 31);
            this.label9.TabIndex = 81;
            this.label9.Text = "Region:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // applyButton
            // 
            this.applyButton.AutoSize = true;
            this.applyButton.Location = new System.Drawing.Point(18, 2);
            this.applyButton.Margin = new System.Windows.Forms.Padding(2);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(109, 30);
            this.applyButton.TabIndex = 110;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(134, 2);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(109, 30);
            this.cancelButton.TabIndex = 112;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // WebCamGroup
            // 
            this.WebCamGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.WebCamGroup.Controls.Add(this.tableLayoutPanel2);
            this.WebCamGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WebCamGroup.Location = new System.Drawing.Point(2, 2);
            this.WebCamGroup.Margin = new System.Windows.Forms.Padding(2);
            this.WebCamGroup.Name = "WebCamGroup";
            this.WebCamGroup.Padding = new System.Windows.Forms.Padding(6, 15, 6, 15);
            this.WebCamGroup.Size = new System.Drawing.Size(472, 130);
            this.WebCamGroup.TabIndex = 78;
            this.WebCamGroup.TabStop = false;
            this.WebCamGroup.Text = "Capture Settings";
            this.WebCamGroup.Visible = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.CaptureDeviceProfilesComboBox, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.CaptureDeviceTextBox, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 35);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(460, 80);
            this.tableLayoutPanel2.TabIndex = 80;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(2, 31);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 32);
            this.label3.TabIndex = 79;
            this.label3.Text = "Profile:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(2, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 31);
            this.label2.TabIndex = 81;
            this.label2.Text = "Device:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CaptureDeviceProfilesComboBox
            // 
            this.CaptureDeviceProfilesComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CaptureDeviceProfilesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CaptureDeviceProfilesComboBox.Enabled = false;
            this.CaptureDeviceProfilesComboBox.FormattingEnabled = true;
            this.CaptureDeviceProfilesComboBox.Location = new System.Drawing.Point(63, 33);
            this.CaptureDeviceProfilesComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.CaptureDeviceProfilesComboBox.Name = "CaptureDeviceProfilesComboBox";
            this.CaptureDeviceProfilesComboBox.Size = new System.Drawing.Size(395, 28);
            this.CaptureDeviceProfilesComboBox.TabIndex = 62;
            // 
            // CaptureDeviceTextBox
            // 
            this.CaptureDeviceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CaptureDeviceTextBox.Location = new System.Drawing.Point(63, 2);
            this.CaptureDeviceTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.CaptureDeviceTextBox.Name = "CaptureDeviceTextBox";
            this.CaptureDeviceTextBox.ReadOnly = true;
            this.CaptureDeviceTextBox.Size = new System.Drawing.Size(395, 27);
            this.CaptureDeviceTextBox.TabIndex = 60;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.ScreenCaptureGroup, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.EncoderSettingsGroup, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.WebCamGroup, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(11, 12);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 10, 11);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(476, 723);
            this.tableLayoutPanel1.TabIndex = 79;
            // 
            // EncoderSettingsGroup
            // 
            this.EncoderSettingsGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.EncoderSettingsGroup.Controls.Add(this.tableLayoutPanel7);
            this.EncoderSettingsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EncoderSettingsGroup.Location = new System.Drawing.Point(2, 388);
            this.EncoderSettingsGroup.Margin = new System.Windows.Forms.Padding(2);
            this.EncoderSettingsGroup.Name = "EncoderSettingsGroup";
            this.EncoderSettingsGroup.Padding = new System.Windows.Forms.Padding(6, 15, 6, 15);
            this.EncoderSettingsGroup.Size = new System.Drawing.Size(472, 295);
            this.EncoderSettingsGroup.TabIndex = 74;
            this.EncoderSettingsGroup.TabStop = false;
            this.EncoderSettingsGroup.Text = "Encoder Settings";
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel9, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(6, 35);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 2;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(460, 245);
            this.tableLayoutPanel7.TabIndex = 82;
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 3;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.Controls.Add(this.tableLayoutPanel6, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.latencyModeCheckBox, 0, 1);
            this.tableLayoutPanel9.Controls.Add(this.tableLayoutPanel8, 2, 0);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(4, 100);
            this.tableLayoutPanel9.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 2;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(455, 141);
            this.tableLayoutPanel9.TabIndex = 83;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.AutoSize = true;
            this.tableLayoutPanel6.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.label16, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.encoderComboBox, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.label14, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.bitrateModeComboBox, 1, 2);
            this.tableLayoutPanel6.Controls.Add(this.encProfileComboBox, 1, 1);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 7;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.Size = new System.Drawing.Size(199, 97);
            this.tableLayoutPanel6.TabIndex = 81;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Location = new System.Drawing.Point(2, 64);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(66, 32);
            this.label16.TabIndex = 55;
            this.label16.Text = "Mode:";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // encoderComboBox
            // 
            this.encoderComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.encoderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encoderComboBox.Enabled = false;
            this.encoderComboBox.FormattingEnabled = true;
            this.encoderComboBox.Location = new System.Drawing.Point(72, 2);
            this.encoderComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.encoderComboBox.Name = "encoderComboBox";
            this.encoderComboBox.Size = new System.Drawing.Size(125, 28);
            this.encoderComboBox.TabIndex = 98;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Location = new System.Drawing.Point(2, 32);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(66, 32);
            this.label14.TabIndex = 52;
            this.label14.Text = "Profile:";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(2, 0);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 32);
            this.label6.TabIndex = 46;
            this.label6.Text = "Encoder:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // bitrateModeComboBox
            // 
            this.bitrateModeComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bitrateModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bitrateModeComboBox.FormattingEnabled = true;
            this.bitrateModeComboBox.Location = new System.Drawing.Point(72, 66);
            this.bitrateModeComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.bitrateModeComboBox.Name = "bitrateModeComboBox";
            this.bitrateModeComboBox.Size = new System.Drawing.Size(125, 28);
            this.bitrateModeComboBox.TabIndex = 102;
            // 
            // encProfileComboBox
            // 
            this.encProfileComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.encProfileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encProfileComboBox.FormattingEnabled = true;
            this.encProfileComboBox.Location = new System.Drawing.Point(72, 34);
            this.encProfileComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.encProfileComboBox.Name = "encProfileComboBox";
            this.encProfileComboBox.Size = new System.Drawing.Size(125, 28);
            this.encProfileComboBox.TabIndex = 100;
            // 
            // latencyModeCheckBox
            // 
            this.latencyModeCheckBox.AutoSize = true;
            this.latencyModeCheckBox.Checked = true;
            this.latencyModeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.latencyModeCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.latencyModeCheckBox.Location = new System.Drawing.Point(5, 113);
            this.latencyModeCheckBox.Margin = new System.Windows.Forms.Padding(5, 12, 2, 2);
            this.latencyModeCheckBox.Name = "latencyModeCheckBox";
            this.latencyModeCheckBox.Size = new System.Drawing.Size(196, 26);
            this.latencyModeCheckBox.TabIndex = 51;
            this.latencyModeCheckBox.Text = "Low Latency Mode";
            this.latencyModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.AutoSize = true;
            this.tableLayoutPanel8.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel8.ColumnCount = 2;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Controls.Add(this.fpsNumeric, 1, 2);
            this.tableLayoutPanel8.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel8.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.label15, 0, 1);
            this.tableLayoutPanel8.Controls.Add(this.MaxBitrateNumeric, 1, 1);
            this.tableLayoutPanel8.Controls.Add(this.bitrateNumeric, 1, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(255, 4);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 3;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel8.Size = new System.Drawing.Size(196, 93);
            this.tableLayoutPanel8.TabIndex = 82;
            // 
            // fpsNumeric
            // 
            this.fpsNumeric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fpsNumeric.Location = new System.Drawing.Point(94, 64);
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
            this.fpsNumeric.Size = new System.Drawing.Size(100, 27);
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
            this.label1.Location = new System.Drawing.Point(2, 62);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 31);
            this.label1.TabIndex = 6;
            this.label1.Text = "FPS:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(2, 0);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 31);
            this.label7.TabIndex = 50;
            this.label7.Text = "Bitrate:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Location = new System.Drawing.Point(2, 31);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(88, 31);
            this.label15.TabIndex = 55;
            this.label15.Text = "Bitrate Max:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MaxBitrateNumeric
            // 
            this.MaxBitrateNumeric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MaxBitrateNumeric.Location = new System.Drawing.Point(94, 33);
            this.MaxBitrateNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.MaxBitrateNumeric.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.MaxBitrateNumeric.Name = "MaxBitrateNumeric";
            this.MaxBitrateNumeric.Size = new System.Drawing.Size(100, 27);
            this.MaxBitrateNumeric.TabIndex = 106;
            this.MaxBitrateNumeric.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // bitrateNumeric
            // 
            this.bitrateNumeric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bitrateNumeric.Location = new System.Drawing.Point(94, 2);
            this.bitrateNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.bitrateNumeric.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.bitrateNumeric.Name = "bitrateNumeric";
            this.bitrateNumeric.Size = new System.Drawing.Size(100, 27);
            this.bitrateNumeric.TabIndex = 104;
            this.bitrateNumeric.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 3;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.Controls.Add(this.panelEncoderResoulution, 1, 1);
            this.tableLayoutPanel5.Controls.Add(this.label13, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.aspectRatioCheckBox, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.checkBoxResoulutionFromSource, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(4, 0);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 3;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.Size = new System.Drawing.Size(455, 92);
            this.tableLayoutPanel5.TabIndex = 80;
            // 
            // panelEncoderResoulution
            // 
            this.panelEncoderResoulution.AutoSize = true;
            this.panelEncoderResoulution.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelEncoderResoulution.Controls.Add(this.destHeightNumeric);
            this.panelEncoderResoulution.Controls.Add(this.label12);
            this.panelEncoderResoulution.Controls.Add(this.destWidthNumeric);
            this.panelEncoderResoulution.Controls.Add(this.adjustAspectRatioButton);
            this.panelEncoderResoulution.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelEncoderResoulution.Location = new System.Drawing.Point(86, 28);
            this.panelEncoderResoulution.Margin = new System.Windows.Forms.Padding(0);
            this.panelEncoderResoulution.Name = "panelEncoderResoulution";
            this.panelEncoderResoulution.Size = new System.Drawing.Size(369, 33);
            this.panelEncoderResoulution.TabIndex = 81;
            // 
            // destHeightNumeric
            // 
            this.destHeightNumeric.Location = new System.Drawing.Point(98, 4);
            this.destHeightNumeric.Margin = new System.Windows.Forms.Padding(2);
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
            this.destHeightNumeric.Size = new System.Drawing.Size(69, 27);
            this.destHeightNumeric.TabIndex = 94;
            this.destHeightNumeric.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(79, 6);
            this.label12.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(16, 20);
            this.label12.TabIndex = 28;
            this.label12.Text = "x";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // destWidthNumeric
            // 
            this.destWidthNumeric.Location = new System.Drawing.Point(2, 4);
            this.destWidthNumeric.Margin = new System.Windows.Forms.Padding(2);
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
            this.destWidthNumeric.Size = new System.Drawing.Size(74, 27);
            this.destWidthNumeric.TabIndex = 92;
            this.destWidthNumeric.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // adjustAspectRatioButton
            // 
            this.adjustAspectRatioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.adjustAspectRatioButton.AutoSize = true;
            this.adjustAspectRatioButton.Location = new System.Drawing.Point(215, 1);
            this.adjustAspectRatioButton.Margin = new System.Windows.Forms.Padding(2);
            this.adjustAspectRatioButton.Name = "adjustAspectRatioButton";
            this.adjustAspectRatioButton.Size = new System.Drawing.Size(151, 30);
            this.adjustAspectRatioButton.TabIndex = 96;
            this.adjustAspectRatioButton.Text = "Adjust";
            this.adjustAspectRatioButton.UseVisualStyleBackColor = true;
            this.adjustAspectRatioButton.Click += new System.EventHandler(this.adjustAspectRatioButton_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label13.Location = new System.Drawing.Point(2, 28);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(82, 33);
            this.label13.TabIndex = 30;
            this.label13.Text = "Resolution:";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // aspectRatioCheckBox
            // 
            this.aspectRatioCheckBox.AutoSize = true;
            this.aspectRatioCheckBox.Checked = true;
            this.aspectRatioCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel5.SetColumnSpan(this.aspectRatioCheckBox, 2);
            this.aspectRatioCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aspectRatioCheckBox.Location = new System.Drawing.Point(5, 63);
            this.aspectRatioCheckBox.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.aspectRatioCheckBox.Name = "aspectRatioCheckBox";
            this.aspectRatioCheckBox.Size = new System.Drawing.Size(448, 27);
            this.aspectRatioCheckBox.TabIndex = 97;
            this.aspectRatioCheckBox.Text = "Aspect Ratio";
            this.aspectRatioCheckBox.UseVisualStyleBackColor = true;
            // 
            // checkBoxResoulutionFromSource
            // 
            this.tableLayoutPanel5.SetColumnSpan(this.checkBoxResoulutionFromSource, 2);
            this.checkBoxResoulutionFromSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxResoulutionFromSource.Location = new System.Drawing.Point(5, 2);
            this.checkBoxResoulutionFromSource.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.checkBoxResoulutionFromSource.Name = "checkBoxResoulutionFromSource";
            this.checkBoxResoulutionFromSource.Size = new System.Drawing.Size(448, 24);
            this.checkBoxResoulutionFromSource.TabIndex = 89;
            this.checkBoxResoulutionFromSource.Text = "Use Resolution From Capture Source";
            this.checkBoxResoulutionFromSource.UseVisualStyleBackColor = true;
            this.checkBoxResoulutionFromSource.CheckedChanged += new System.EventHandler(this.checkBoxResoulutionFromSource_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(229, 687);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.panel1.Size = new System.Drawing.Size(245, 34);
            this.panel1.TabIndex = 80;
            // 
            // VideoSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(555, 774);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VideoSettingsForm";
            this.Text = "VideoSettings";
            this.ScreenCaptureGroup.ResumeLayout(false);
            this.ScreenCaptureGroup.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.WebCamGroup.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.EncoderSettingsGroup.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel9.ResumeLayout(false);
            this.tableLayoutPanel9.PerformLayout();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxBitrateNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateNumeric)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.panelEncoderResoulution.ResumeLayout(false);
            this.panelEncoderResoulution.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.destHeightNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.destWidthNumeric)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button snippingToolButton;
        private System.Windows.Forms.GroupBox ScreenCaptureGroup;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox displayTextBox;
        private System.Windows.Forms.Label labelDisplay;
        private System.Windows.Forms.ComboBox captureTypesComboBox;
        private System.Windows.Forms.TextBox captureRegionTextBox;
        private System.Windows.Forms.CheckBox captureMouseCheckBox;
        private System.Windows.Forms.Label label9;
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.GroupBox EncoderSettingsGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox encoderComboBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox bitrateModeComboBox;
        private System.Windows.Forms.ComboBox encProfileComboBox;
        private System.Windows.Forms.CheckBox latencyModeCheckBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.NumericUpDown fpsNumeric;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown MaxBitrateNumeric;
        private System.Windows.Forms.NumericUpDown bitrateNumeric;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Panel panelEncoderResoulution;
        private System.Windows.Forms.NumericUpDown destHeightNumeric;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown destWidthNumeric;
        private System.Windows.Forms.Button adjustAspectRatioButton;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox aspectRatioCheckBox;
        private System.Windows.Forms.CheckBox checkBoxResoulutionFromSource;
        private System.Windows.Forms.Button button1;
    }
}