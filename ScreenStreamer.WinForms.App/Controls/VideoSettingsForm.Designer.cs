namespace ScreenStreamer.WinForms.App
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
            this.ScreenCaptureGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.screenCaptureDetailsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.captureMouseCheckBox = new System.Windows.Forms.CheckBox();
            this.showDebugInfoCheckBox = new System.Windows.Forms.CheckBox();
            this.showCaptureBorderCheckBox = new System.Windows.Forms.CheckBox();
            this.snippingToolButton = new System.Windows.Forms.Button();
            this.screenCaptureTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.captureTypesComboBox = new System.Windows.Forms.ComboBox();
            this.captureRegionTextBox = new System.Windows.Forms.TextBox();
            this.displayTextBox = new System.Windows.Forms.TextBox();
            this.labelDisplay = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.previewButton = new System.Windows.Forms.Button();
            this.cameraTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CaptureDeviceProfilesComboBox = new System.Windows.Forms.ComboBox();
            this.CaptureDeviceTextBox = new System.Windows.Forms.TextBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.EncoderSettingsGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.aspectRatioCheckBox = new System.Windows.Forms.CheckBox();
            this.label16 = new System.Windows.Forms.Label();
            this.encoderComboBox = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.bitrateModeComboBox = new System.Windows.Forms.ComboBox();
            this.encProfileComboBox = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.bitrateNumeric = new System.Windows.Forms.NumericUpDown();
            this.MaxBitrateNumeric = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.panelEncoderResoulution = new System.Windows.Forms.TableLayoutPanel();
            this.label13 = new System.Windows.Forms.Label();
            this.encHeightNumeric = new System.Windows.Forms.NumericUpDown();
            this.encWidthNumeric = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.checkBoxResoulutionFromSource = new System.Windows.Forms.CheckBox();
            this.adjustAspectRatioButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.latencyModeCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.fpsNumeric = new System.Windows.Forms.NumericUpDown();
            this.ScreenCaptureGroup.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.screenCaptureDetailsPanel.SuspendLayout();
            this.screenCaptureTableLayoutPanel.SuspendLayout();
            this.cameraTableLayoutPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.EncoderSettingsGroup.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxBitrateNumeric)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            this.panelEncoderResoulution.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.encHeightNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.encWidthNumeric)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // ScreenCaptureGroup
            // 
            this.ScreenCaptureGroup.AutoSize = true;
            this.ScreenCaptureGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ScreenCaptureGroup.Controls.Add(this.tableLayoutPanel2);
            this.ScreenCaptureGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScreenCaptureGroup.Location = new System.Drawing.Point(2, 2);
            this.ScreenCaptureGroup.Margin = new System.Windows.Forms.Padding(2);
            this.ScreenCaptureGroup.Name = "ScreenCaptureGroup";
            this.ScreenCaptureGroup.Padding = new System.Windows.Forms.Padding(6, 15, 6, 15);
            this.ScreenCaptureGroup.Size = new System.Drawing.Size(458, 287);
            this.ScreenCaptureGroup.TabIndex = 70;
            this.ScreenCaptureGroup.TabStop = false;
            this.ScreenCaptureGroup.Text = "Capture Settings";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.screenCaptureDetailsPanel, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.screenCaptureTableLayoutPanel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.cameraTableLayoutPanel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.previewButton, 1, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 31);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(446, 241);
            this.tableLayoutPanel2.TabIndex = 82;
            // 
            // screenCaptureDetailsPanel
            // 
            this.screenCaptureDetailsPanel.AutoSize = true;
            this.screenCaptureDetailsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.screenCaptureDetailsPanel.ColumnCount = 3;
            this.screenCaptureDetailsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.screenCaptureDetailsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.screenCaptureDetailsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.screenCaptureDetailsPanel.Controls.Add(this.captureMouseCheckBox, 0, 0);
            this.screenCaptureDetailsPanel.Controls.Add(this.showDebugInfoCheckBox, 0, 1);
            this.screenCaptureDetailsPanel.Controls.Add(this.showCaptureBorderCheckBox, 0, 2);
            this.screenCaptureDetailsPanel.Controls.Add(this.snippingToolButton, 1, 0);
            this.screenCaptureDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.screenCaptureDetailsPanel.Location = new System.Drawing.Point(2, 157);
            this.screenCaptureDetailsPanel.Margin = new System.Windows.Forms.Padding(2, 10, 0, 2);
            this.screenCaptureDetailsPanel.Name = "screenCaptureDetailsPanel";
            this.screenCaptureDetailsPanel.RowCount = 3;
            this.screenCaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.screenCaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.screenCaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.screenCaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.screenCaptureDetailsPanel.Size = new System.Drawing.Size(285, 82);
            this.screenCaptureDetailsPanel.TabIndex = 81;
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
            this.captureMouseCheckBox.Size = new System.Drawing.Size(162, 28);
            this.captureMouseCheckBox.TabIndex = 80;
            this.captureMouseCheckBox.Text = "Capture Mouse";
            this.captureMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // showDebugInfoCheckBox
            // 
            this.showDebugInfoCheckBox.AutoSize = true;
            this.showDebugInfoCheckBox.Checked = true;
            this.showDebugInfoCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showDebugInfoCheckBox.Location = new System.Drawing.Point(5, 34);
            this.showDebugInfoCheckBox.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.showDebugInfoCheckBox.Name = "showDebugInfoCheckBox";
            this.showDebugInfoCheckBox.Size = new System.Drawing.Size(137, 21);
            this.showDebugInfoCheckBox.TabIndex = 82;
            this.showDebugInfoCheckBox.Text = "Show Debug Info";
            this.showDebugInfoCheckBox.UseVisualStyleBackColor = true;
            // 
            // showCaptureBorderCheckBox
            // 
            this.showCaptureBorderCheckBox.AutoSize = true;
            this.showCaptureBorderCheckBox.Checked = true;
            this.showCaptureBorderCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showCaptureBorderCheckBox.Location = new System.Drawing.Point(5, 59);
            this.showCaptureBorderCheckBox.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.showCaptureBorderCheckBox.Name = "showCaptureBorderCheckBox";
            this.showCaptureBorderCheckBox.Size = new System.Drawing.Size(162, 21);
            this.showCaptureBorderCheckBox.TabIndex = 84;
            this.showCaptureBorderCheckBox.Text = "Show Capture Frame";
            this.showCaptureBorderCheckBox.UseVisualStyleBackColor = true;
            // 
            // snippingToolButton
            // 
            this.snippingToolButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.snippingToolButton.Location = new System.Drawing.Point(171, 2);
            this.snippingToolButton.Margin = new System.Windows.Forms.Padding(2);
            this.snippingToolButton.Name = "snippingToolButton";
            this.snippingToolButton.Size = new System.Drawing.Size(112, 28);
            this.snippingToolButton.TabIndex = 88;
            this.snippingToolButton.Text = "Select Region";
            this.snippingToolButton.UseVisualStyleBackColor = true;
            this.snippingToolButton.Visible = false;
            this.snippingToolButton.Click += new System.EventHandler(this.snippingToolButton_Click);
            // 
            // screenCaptureTableLayoutPanel
            // 
            this.screenCaptureTableLayoutPanel.AutoSize = true;
            this.screenCaptureTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.screenCaptureTableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel2.SetColumnSpan(this.screenCaptureTableLayoutPanel, 2);
            this.screenCaptureTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.screenCaptureTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.screenCaptureTableLayoutPanel.Controls.Add(this.captureTypesComboBox, 1, 2);
            this.screenCaptureTableLayoutPanel.Controls.Add(this.captureRegionTextBox, 1, 1);
            this.screenCaptureTableLayoutPanel.Controls.Add(this.displayTextBox, 1, 0);
            this.screenCaptureTableLayoutPanel.Controls.Add(this.labelDisplay, 0, 0);
            this.screenCaptureTableLayoutPanel.Controls.Add(this.label5, 0, 2);
            this.screenCaptureTableLayoutPanel.Controls.Add(this.label9, 0, 1);
            this.screenCaptureTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.screenCaptureTableLayoutPanel.Location = new System.Drawing.Point(2, 62);
            this.screenCaptureTableLayoutPanel.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.screenCaptureTableLayoutPanel.Name = "screenCaptureTableLayoutPanel";
            this.screenCaptureTableLayoutPanel.RowCount = 4;
            this.screenCaptureTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.screenCaptureTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.screenCaptureTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.screenCaptureTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.screenCaptureTableLayoutPanel.Size = new System.Drawing.Size(444, 83);
            this.screenCaptureTableLayoutPanel.TabIndex = 80;
            // 
            // captureTypesComboBox
            // 
            this.captureTypesComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.captureTypesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.captureTypesComboBox.Enabled = false;
            this.captureTypesComboBox.FormattingEnabled = true;
            this.captureTypesComboBox.Location = new System.Drawing.Point(68, 56);
            this.captureTypesComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.captureTypesComboBox.Name = "captureTypesComboBox";
            this.captureTypesComboBox.Size = new System.Drawing.Size(374, 25);
            this.captureTypesComboBox.TabIndex = 77;
            // 
            // captureRegionTextBox
            // 
            this.captureRegionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.captureRegionTextBox.Location = new System.Drawing.Point(68, 29);
            this.captureRegionTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.captureRegionTextBox.Name = "captureRegionTextBox";
            this.captureRegionTextBox.ReadOnly = true;
            this.captureRegionTextBox.Size = new System.Drawing.Size(374, 23);
            this.captureRegionTextBox.TabIndex = 76;
            // 
            // displayTextBox
            // 
            this.displayTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.displayTextBox.Location = new System.Drawing.Point(68, 2);
            this.displayTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.displayTextBox.Name = "displayTextBox";
            this.displayTextBox.ReadOnly = true;
            this.displayTextBox.Size = new System.Drawing.Size(374, 23);
            this.displayTextBox.TabIndex = 73;
            // 
            // labelDisplay
            // 
            this.labelDisplay.AutoSize = true;
            this.labelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelDisplay.Location = new System.Drawing.Point(2, 0);
            this.labelDisplay.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDisplay.Name = "labelDisplay";
            this.labelDisplay.Size = new System.Drawing.Size(62, 27);
            this.labelDisplay.TabIndex = 79;
            this.labelDisplay.Text = "Display:";
            this.labelDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(2, 54);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 29);
            this.label5.TabIndex = 82;
            this.label5.Text = "Capture:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(2, 27);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(62, 27);
            this.label9.TabIndex = 81;
            this.label9.Text = "Region:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // previewButton
            // 
            this.previewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.previewButton.Location = new System.Drawing.Point(351, 147);
            this.previewButton.Margin = new System.Windows.Forms.Padding(2, 0, 1, 2);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(94, 28);
            this.previewButton.TabIndex = 86;
            this.previewButton.Text = "Preview";
            this.previewButton.UseVisualStyleBackColor = true;
            this.previewButton.Click += new System.EventHandler(this.previewButton_Click);
            // 
            // cameraTableLayoutPanel
            // 
            this.cameraTableLayoutPanel.AutoSize = true;
            this.cameraTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cameraTableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel2.SetColumnSpan(this.cameraTableLayoutPanel, 2);
            this.cameraTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.cameraTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cameraTableLayoutPanel.Controls.Add(this.CaptureDeviceProfilesComboBox, 1, 1);
            this.cameraTableLayoutPanel.Controls.Add(this.CaptureDeviceTextBox, 1, 0);
            this.cameraTableLayoutPanel.Controls.Add(this.label3, 0, 1);
            this.cameraTableLayoutPanel.Controls.Add(this.label2, 0, 0);
            this.cameraTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraTableLayoutPanel.Location = new System.Drawing.Point(2, 2);
            this.cameraTableLayoutPanel.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.cameraTableLayoutPanel.Name = "cameraTableLayoutPanel";
            this.cameraTableLayoutPanel.RowCount = 4;
            this.cameraTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.cameraTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.cameraTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.cameraTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.cameraTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cameraTableLayoutPanel.Size = new System.Drawing.Size(444, 56);
            this.cameraTableLayoutPanel.TabIndex = 80;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(2, 27);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 29);
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
            this.label2.Size = new System.Drawing.Size(55, 27);
            this.label2.TabIndex = 81;
            this.label2.Text = "Device:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CaptureDeviceProfilesComboBox
            // 
            this.CaptureDeviceProfilesComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CaptureDeviceProfilesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CaptureDeviceProfilesComboBox.Enabled = false;
            this.CaptureDeviceProfilesComboBox.FormattingEnabled = true;
            this.CaptureDeviceProfilesComboBox.Location = new System.Drawing.Point(61, 29);
            this.CaptureDeviceProfilesComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.CaptureDeviceProfilesComboBox.Name = "CaptureDeviceProfilesComboBox";
            this.CaptureDeviceProfilesComboBox.Size = new System.Drawing.Size(381, 25);
            this.CaptureDeviceProfilesComboBox.TabIndex = 62;
            // 
            // CaptureDeviceTextBox
            // 
            this.CaptureDeviceTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CaptureDeviceTextBox.Location = new System.Drawing.Point(61, 2);
            this.CaptureDeviceTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.CaptureDeviceTextBox.Name = "CaptureDeviceTextBox";
            this.CaptureDeviceTextBox.ReadOnly = true;
            this.CaptureDeviceTextBox.Size = new System.Drawing.Size(381, 23);
            this.CaptureDeviceTextBox.TabIndex = 60;
            // 
            // applyButton
            // 
            this.applyButton.AutoSize = true;
            this.applyButton.Location = new System.Drawing.Point(16, 2);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(112, 28);
            this.applyButton.TabIndex = 200;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.AutoSize = true;
            this.cancelButton.Location = new System.Drawing.Point(134, 2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(109, 28);
            this.cancelButton.TabIndex = 210;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.EncoderSettingsGroup, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.ScreenCaptureGroup, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(11, 11);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 10, 11);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(462, 656);
            this.tableLayoutPanel1.TabIndex = 79;
            // 
            // EncoderSettingsGroup
            // 
            this.EncoderSettingsGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.EncoderSettingsGroup.Controls.Add(this.tableLayoutPanel5);
            this.EncoderSettingsGroup.Controls.Add(this.tableLayoutPanel6);
            this.EncoderSettingsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EncoderSettingsGroup.Location = new System.Drawing.Point(2, 293);
            this.EncoderSettingsGroup.Margin = new System.Windows.Forms.Padding(2);
            this.EncoderSettingsGroup.Name = "EncoderSettingsGroup";
            this.EncoderSettingsGroup.Padding = new System.Windows.Forms.Padding(6, 15, 6, 15);
            this.EncoderSettingsGroup.Size = new System.Drawing.Size(458, 322);
            this.EncoderSettingsGroup.TabIndex = 74;
            this.EncoderSettingsGroup.TabStop = false;
            this.EncoderSettingsGroup.Text = "Encoder Settings";
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel6.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel6.ColumnCount = 5;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Controls.Add(this.aspectRatioCheckBox, 0, 7);
            this.tableLayoutPanel6.Controls.Add(this.label16, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.bitrateModeComboBox, 1, 2);
            this.tableLayoutPanel6.Controls.Add(this.latencyModeCheckBox, 0, 5);
            this.tableLayoutPanel6.Controls.Add(this.label7, 3, 1);
            this.tableLayoutPanel6.Controls.Add(this.bitrateNumeric, 4, 1);
            this.tableLayoutPanel6.Controls.Add(this.MaxBitrateNumeric, 4, 2);
            this.tableLayoutPanel6.Controls.Add(this.label15, 3, 2);
            this.tableLayoutPanel6.Controls.Add(this.fpsNumeric, 1, 3);
            this.tableLayoutPanel6.Controls.Add(this.label1, 0, 3);
            this.tableLayoutPanel6.Controls.Add(this.encoderComboBox, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.label14, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.encProfileComboBox, 1, 1);
            this.tableLayoutPanel6.Location = new System.Drawing.Point(6, 133);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(2, 14, 2, 2);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 8;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(445, 168);
            this.tableLayoutPanel6.TabIndex = 81;
            // 
            // aspectRatioCheckBox
            // 
            this.aspectRatioCheckBox.AutoSize = true;
            this.aspectRatioCheckBox.Checked = true;
            this.aspectRatioCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel6.SetColumnSpan(this.aspectRatioCheckBox, 4);
            this.aspectRatioCheckBox.Location = new System.Drawing.Point(6, 145);
            this.aspectRatioCheckBox.Margin = new System.Windows.Forms.Padding(6, 2, 2, 2);
            this.aspectRatioCheckBox.Name = "aspectRatioCheckBox";
            this.aspectRatioCheckBox.Size = new System.Drawing.Size(110, 21);
            this.aspectRatioCheckBox.TabIndex = 112;
            this.aspectRatioCheckBox.Text = "Aspect Ratio";
            this.aspectRatioCheckBox.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Location = new System.Drawing.Point(2, 60);
            this.label16.Margin = new System.Windows.Forms.Padding(2);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(65, 25);
            this.label16.TabIndex = 55;
            this.label16.Text = "Mode:";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // encoderComboBox
            // 
            this.tableLayoutPanel6.SetColumnSpan(this.encoderComboBox, 4);
            this.encoderComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.encoderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encoderComboBox.FormattingEnabled = true;
            this.encoderComboBox.Location = new System.Drawing.Point(71, 2);
            this.encoderComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.encoderComboBox.Name = "encoderComboBox";
            this.encoderComboBox.Size = new System.Drawing.Size(372, 25);
            this.encoderComboBox.TabIndex = 98;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Location = new System.Drawing.Point(2, 31);
            this.label14.Margin = new System.Windows.Forms.Padding(2);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(65, 25);
            this.label14.TabIndex = 52;
            this.label14.Text = "Profile:";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(2, 2);
            this.label6.Margin = new System.Windows.Forms.Padding(2);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 25);
            this.label6.TabIndex = 46;
            this.label6.Text = "Encoder:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bitrateModeComboBox
            // 
            this.bitrateModeComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bitrateModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bitrateModeComboBox.FormattingEnabled = true;
            this.bitrateModeComboBox.Location = new System.Drawing.Point(71, 60);
            this.bitrateModeComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.bitrateModeComboBox.Name = "bitrateModeComboBox";
            this.bitrateModeComboBox.Size = new System.Drawing.Size(120, 25);
            this.bitrateModeComboBox.TabIndex = 102;
            // 
            // encProfileComboBox
            // 
            this.encProfileComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.encProfileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encProfileComboBox.FormattingEnabled = true;
            this.encProfileComboBox.Location = new System.Drawing.Point(71, 31);
            this.encProfileComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.encProfileComboBox.Name = "encProfileComboBox";
            this.encProfileComboBox.Size = new System.Drawing.Size(120, 25);
            this.encProfileComboBox.TabIndex = 100;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(237, 31);
            this.label7.Margin = new System.Windows.Forms.Padding(2);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 25);
            this.label7.TabIndex = 50;
            this.label7.Text = "Bitrate:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bitrateNumeric
            // 
            this.bitrateNumeric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bitrateNumeric.Location = new System.Drawing.Point(323, 31);
            this.bitrateNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.bitrateNumeric.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.bitrateNumeric.Name = "bitrateNumeric";
            this.bitrateNumeric.Size = new System.Drawing.Size(120, 23);
            this.bitrateNumeric.TabIndex = 104;
            this.bitrateNumeric.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            // 
            // MaxBitrateNumeric
            // 
            this.MaxBitrateNumeric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MaxBitrateNumeric.Location = new System.Drawing.Point(323, 60);
            this.MaxBitrateNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.MaxBitrateNumeric.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.MaxBitrateNumeric.Name = "MaxBitrateNumeric";
            this.MaxBitrateNumeric.Size = new System.Drawing.Size(120, 23);
            this.MaxBitrateNumeric.TabIndex = 106;
            this.MaxBitrateNumeric.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Location = new System.Drawing.Point(237, 60);
            this.label15.Margin = new System.Windows.Forms.Padding(2);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(82, 25);
            this.label15.TabIndex = 55;
            this.label15.Text = "Bitrate Max:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel5.ColumnCount = 3;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.Controls.Add(this.panelEncoderResoulution, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.checkBoxResoulutionFromSource, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.adjustAspectRatioButton, 0, 2);
            this.tableLayoutPanel5.Location = new System.Drawing.Point(6, 31);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 4;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(443, 88);
            this.tableLayoutPanel5.TabIndex = 80;
            // 
            // panelEncoderResoulution
            // 
            this.panelEncoderResoulution.AutoSize = true;
            this.panelEncoderResoulution.ColumnCount = 5;
            this.tableLayoutPanel5.SetColumnSpan(this.panelEncoderResoulution, 2);
            this.panelEncoderResoulution.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.panelEncoderResoulution.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panelEncoderResoulution.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.panelEncoderResoulution.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panelEncoderResoulution.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.panelEncoderResoulution.Controls.Add(this.label13, 0, 0);
            this.panelEncoderResoulution.Controls.Add(this.encHeightNumeric, 3, 0);
            this.panelEncoderResoulution.Controls.Add(this.encWidthNumeric, 1, 0);
            this.panelEncoderResoulution.Controls.Add(this.label12, 2, 0);
            this.panelEncoderResoulution.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelEncoderResoulution.Location = new System.Drawing.Point(2, 27);
            this.panelEncoderResoulution.Margin = new System.Windows.Forms.Padding(2);
            this.panelEncoderResoulution.Name = "panelEncoderResoulution";
            this.panelEncoderResoulution.RowCount = 2;
            this.panelEncoderResoulution.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelEncoderResoulution.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelEncoderResoulution.Size = new System.Drawing.Size(439, 27);
            this.panelEncoderResoulution.TabIndex = 91;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label13.Location = new System.Drawing.Point(0, 0);
            this.label13.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(79, 27);
            this.label13.TabIndex = 30;
            this.label13.Text = "Resolution:";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // encHeightNumeric
            // 
            this.encHeightNumeric.AutoSize = true;
            this.encHeightNumeric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.encHeightNumeric.Location = new System.Drawing.Point(270, 2);
            this.encHeightNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.encHeightNumeric.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.encHeightNumeric.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.encHeightNumeric.Name = "encHeightNumeric";
            this.encHeightNumeric.Size = new System.Drawing.Size(167, 23);
            this.encHeightNumeric.TabIndex = 94;
            this.encHeightNumeric.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // encWidthNumeric
            // 
            this.encWidthNumeric.AutoSize = true;
            this.encWidthNumeric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.encWidthNumeric.Location = new System.Drawing.Point(83, 2);
            this.encWidthNumeric.Margin = new System.Windows.Forms.Padding(2);
            this.encWidthNumeric.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.encWidthNumeric.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.encWidthNumeric.Name = "encWidthNumeric";
            this.encWidthNumeric.Size = new System.Drawing.Size(167, 23);
            this.encWidthNumeric.TabIndex = 92;
            this.encWidthNumeric.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label12.Location = new System.Drawing.Point(253, 0);
            this.label12.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(14, 27);
            this.label12.TabIndex = 28;
            this.label12.Text = "x";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkBoxResoulutionFromSource
            // 
            this.checkBoxResoulutionFromSource.AutoSize = true;
            this.tableLayoutPanel5.SetColumnSpan(this.checkBoxResoulutionFromSource, 2);
            this.checkBoxResoulutionFromSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxResoulutionFromSource.Location = new System.Drawing.Point(5, 2);
            this.checkBoxResoulutionFromSource.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.checkBoxResoulutionFromSource.Name = "checkBoxResoulutionFromSource";
            this.checkBoxResoulutionFromSource.Size = new System.Drawing.Size(436, 21);
            this.checkBoxResoulutionFromSource.TabIndex = 89;
            this.checkBoxResoulutionFromSource.Text = "Use Resolution From Capture Source";
            this.checkBoxResoulutionFromSource.UseVisualStyleBackColor = true;
            this.checkBoxResoulutionFromSource.CheckedChanged += new System.EventHandler(this.checkBoxResoulutionFromSource_CheckedChanged);
            // 
            // adjustAspectRatioButton
            // 
            this.adjustAspectRatioButton.AutoSize = true;
            this.tableLayoutPanel5.SetColumnSpan(this.adjustAspectRatioButton, 2);
            this.adjustAspectRatioButton.Location = new System.Drawing.Point(5, 58);
            this.adjustAspectRatioButton.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.adjustAspectRatioButton.Name = "adjustAspectRatioButton";
            this.adjustAspectRatioButton.Size = new System.Drawing.Size(234, 28);
            this.adjustAspectRatioButton.TabIndex = 96;
            this.adjustAspectRatioButton.Text = "Adjust Resolution To Capture Size";
            this.adjustAspectRatioButton.UseVisualStyleBackColor = true;
            this.adjustAspectRatioButton.Click += new System.EventHandler(this.adjustAspectRatioButton_Click);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(214, 621);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 4, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.panel1.Size = new System.Drawing.Size(246, 33);
            this.panel1.TabIndex = 60;
            // 
            // latencyModeCheckBox
            // 
            this.latencyModeCheckBox.AutoSize = true;
            this.latencyModeCheckBox.Checked = true;
            this.latencyModeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel6.SetColumnSpan(this.latencyModeCheckBox, 5);
            this.latencyModeCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.latencyModeCheckBox.Location = new System.Drawing.Point(6, 120);
            this.latencyModeCheckBox.Margin = new System.Windows.Forms.Padding(6, 6, 2, 2);
            this.latencyModeCheckBox.Name = "latencyModeCheckBox";
            this.latencyModeCheckBox.Size = new System.Drawing.Size(437, 21);
            this.latencyModeCheckBox.TabIndex = 110;
            this.latencyModeCheckBox.Text = "Low Latency Mode";
            this.latencyModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(2, 89);
            this.label1.Margin = new System.Windows.Forms.Padding(2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 23);
            this.label1.TabIndex = 6;
            this.label1.Text = "FPS:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // fpsNumeric
            // 
            this.fpsNumeric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fpsNumeric.Location = new System.Drawing.Point(71, 89);
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
            this.fpsNumeric.Size = new System.Drawing.Size(120, 23);
            this.fpsNumeric.TabIndex = 108;
            this.fpsNumeric.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // VideoSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(496, 706);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VideoSettingsForm";
            this.Text = "VideoSettings";
            this.ScreenCaptureGroup.ResumeLayout(false);
            this.ScreenCaptureGroup.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.screenCaptureDetailsPanel.ResumeLayout(false);
            this.screenCaptureDetailsPanel.PerformLayout();
            this.screenCaptureTableLayoutPanel.ResumeLayout(false);
            this.screenCaptureTableLayoutPanel.PerformLayout();
            this.cameraTableLayoutPanel.ResumeLayout(false);
            this.cameraTableLayoutPanel.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.EncoderSettingsGroup.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxBitrateNumeric)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.panelEncoderResoulution.ResumeLayout(false);
            this.panelEncoderResoulution.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.encHeightNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.encWidthNumeric)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox ScreenCaptureGroup;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox displayTextBox;
        private System.Windows.Forms.Label labelDisplay;
        private System.Windows.Forms.ComboBox captureTypesComboBox;
        private System.Windows.Forms.TextBox captureRegionTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox CaptureDeviceProfilesComboBox;
        private System.Windows.Forms.TextBox CaptureDeviceTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TableLayoutPanel cameraTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel screenCaptureTableLayoutPanel;
        private System.Windows.Forms.GroupBox EncoderSettingsGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.NumericUpDown encHeightNumeric;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown encWidthNumeric;
        private System.Windows.Forms.Button adjustAspectRatioButton;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox aspectRatioCheckBox;
        private System.Windows.Forms.CheckBox checkBoxResoulutionFromSource;
        private System.Windows.Forms.TableLayoutPanel screenCaptureDetailsPanel;
        private System.Windows.Forms.CheckBox captureMouseCheckBox;
        private System.Windows.Forms.CheckBox showDebugInfoCheckBox;
        private System.Windows.Forms.CheckBox showCaptureBorderCheckBox;
        private System.Windows.Forms.Button snippingToolButton;
        private System.Windows.Forms.Button previewButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox encoderComboBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox bitrateModeComboBox;
        private System.Windows.Forms.ComboBox encProfileComboBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown MaxBitrateNumeric;
        private System.Windows.Forms.NumericUpDown bitrateNumeric;
		private System.Windows.Forms.TableLayoutPanel panelEncoderResoulution;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.CheckBox latencyModeCheckBox;
        private System.Windows.Forms.NumericUpDown fpsNumeric;
        private System.Windows.Forms.Label label1;
    }
}