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
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.captureSettingsButton = new System.Windows.Forms.Button();
			this.snippingToolButton = new System.Windows.Forms.Button();
			this.previewButton = new System.Windows.Forms.Button();
			this.screenCaptureTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.captureRegionTextBox = new System.Windows.Forms.TextBox();
			this.displayTextBox = new System.Windows.Forms.TextBox();
			this.labelDisplay = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.captInfoTextBox = new System.Windows.Forms.TextBox();
			this.cameraTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.CaptureDeviceTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.uvcProfileInfotextBox = new System.Windows.Forms.TextBox();
			this.windowsCaptureTLPanel = new System.Windows.Forms.TableLayoutPanel();
			this.label6 = new System.Windows.Forms.Label();
			this.windowsUpdateButton = new System.Windows.Forms.Button();
			this.windowsComboBox = new System.Windows.Forms.ComboBox();
			this.applyButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.EncoderSettingsGroup = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.label4 = new System.Windows.Forms.Label();
			this.panelEncoderResoulution = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.encHeightNumeric = new System.Windows.Forms.NumericUpDown();
			this.label13 = new System.Windows.Forms.Label();
			this.encWidthNumeric = new System.Windows.Forms.NumericUpDown();
			this.adjustAspectRatioButton = new System.Windows.Forms.Button();
			this.encoderComboBox = new System.Windows.Forms.ComboBox();
			this.encoderSettingsButton = new System.Windows.Forms.Button();
			this.checkBoxResoulutionFromSource = new System.Windows.Forms.CheckBox();
			this.aspectRatioCheckBox = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.ScreenCaptureGroup.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.screenCaptureTableLayoutPanel.SuspendLayout();
			this.cameraTableLayoutPanel.SuspendLayout();
			this.windowsCaptureTLPanel.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.EncoderSettingsGroup.SuspendLayout();
			this.tableLayoutPanel4.SuspendLayout();
			this.panelEncoderResoulution.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.encHeightNumeric)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.encWidthNumeric)).BeginInit();
			this.panel1.SuspendLayout();
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
			this.ScreenCaptureGroup.Size = new System.Drawing.Size(464, 256);
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
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 4);
			this.tableLayoutPanel2.Controls.Add(this.screenCaptureTableLayoutPanel, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.cameraTableLayoutPanel, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.windowsCaptureTLPanel, 0, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 31);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 4);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 6;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(452, 210);
			this.tableLayoutPanel2.TabIndex = 82;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.AutoSize = true;
			this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel3.ColumnCount = 4;
			this.tableLayoutPanel2.SetColumnSpan(this.tableLayoutPanel3, 2);
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.Controls.Add(this.captureSettingsButton, 3, 0);
			this.tableLayoutPanel3.Controls.Add(this.snippingToolButton, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.previewButton, 2, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Right;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(144, 178);
			this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(308, 32);
			this.tableLayoutPanel3.TabIndex = 82;
			// 
			// captureSettingsButton
			// 
			this.captureSettingsButton.AutoSize = true;
			this.captureSettingsButton.Location = new System.Drawing.Point(216, 2);
			this.captureSettingsButton.Margin = new System.Windows.Forms.Padding(2);
			this.captureSettingsButton.Name = "captureSettingsButton";
			this.captureSettingsButton.Size = new System.Drawing.Size(90, 28);
			this.captureSettingsButton.TabIndex = 114;
			this.captureSettingsButton.Text = "Settings";
			this.captureSettingsButton.UseVisualStyleBackColor = true;
			this.captureSettingsButton.Click += new System.EventHandler(this.captureSettingsButton_Click);
			// 
			// snippingToolButton
			// 
			this.snippingToolButton.Location = new System.Drawing.Point(2, 2);
			this.snippingToolButton.Margin = new System.Windows.Forms.Padding(2);
			this.snippingToolButton.Name = "snippingToolButton";
			this.snippingToolButton.Size = new System.Drawing.Size(112, 28);
			this.snippingToolButton.TabIndex = 88;
			this.snippingToolButton.Text = "Select Region";
			this.snippingToolButton.UseVisualStyleBackColor = true;
			this.snippingToolButton.Visible = false;
			this.snippingToolButton.Click += new System.EventHandler(this.snippingToolButton_Click);
			// 
			// previewButton
			// 
			this.previewButton.AutoSize = true;
			this.previewButton.Location = new System.Drawing.Point(118, 2);
			this.previewButton.Margin = new System.Windows.Forms.Padding(2);
			this.previewButton.Name = "previewButton";
			this.previewButton.Size = new System.Drawing.Size(94, 28);
			this.previewButton.TabIndex = 86;
			this.previewButton.Text = "Preview";
			this.previewButton.UseVisualStyleBackColor = true;
			this.previewButton.Click += new System.EventHandler(this.previewButton_Click);
			// 
			// screenCaptureTableLayoutPanel
			// 
			this.screenCaptureTableLayoutPanel.AutoSize = true;
			this.screenCaptureTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.screenCaptureTableLayoutPanel.ColumnCount = 2;
			this.tableLayoutPanel2.SetColumnSpan(this.screenCaptureTableLayoutPanel, 2);
			this.screenCaptureTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.screenCaptureTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.screenCaptureTableLayoutPanel.Controls.Add(this.captureRegionTextBox, 1, 1);
			this.screenCaptureTableLayoutPanel.Controls.Add(this.displayTextBox, 1, 0);
			this.screenCaptureTableLayoutPanel.Controls.Add(this.labelDisplay, 0, 0);
			this.screenCaptureTableLayoutPanel.Controls.Add(this.label5, 0, 2);
			this.screenCaptureTableLayoutPanel.Controls.Add(this.label9, 0, 1);
			this.screenCaptureTableLayoutPanel.Controls.Add(this.captInfoTextBox, 1, 2);
			this.screenCaptureTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.screenCaptureTableLayoutPanel.Location = new System.Drawing.Point(2, 95);
			this.screenCaptureTableLayoutPanel.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this.screenCaptureTableLayoutPanel.Name = "screenCaptureTableLayoutPanel";
			this.screenCaptureTableLayoutPanel.RowCount = 4;
			this.screenCaptureTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.screenCaptureTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.screenCaptureTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.screenCaptureTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.screenCaptureTableLayoutPanel.Size = new System.Drawing.Size(450, 81);
			this.screenCaptureTableLayoutPanel.TabIndex = 80;
			// 
			// captureRegionTextBox
			// 
			this.captureRegionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.captureRegionTextBox.Location = new System.Drawing.Point(68, 29);
			this.captureRegionTextBox.Margin = new System.Windows.Forms.Padding(2);
			this.captureRegionTextBox.Name = "captureRegionTextBox";
			this.captureRegionTextBox.ReadOnly = true;
			this.captureRegionTextBox.Size = new System.Drawing.Size(380, 23);
			this.captureRegionTextBox.TabIndex = 76;
			// 
			// displayTextBox
			// 
			this.displayTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.displayTextBox.Location = new System.Drawing.Point(68, 2);
			this.displayTextBox.Margin = new System.Windows.Forms.Padding(2);
			this.displayTextBox.Name = "displayTextBox";
			this.displayTextBox.ReadOnly = true;
			this.displayTextBox.Size = new System.Drawing.Size(380, 23);
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
			this.label5.Size = new System.Drawing.Size(62, 27);
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
			// captInfoTextBox
			// 
			this.captInfoTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.captInfoTextBox.Location = new System.Drawing.Point(68, 56);
			this.captInfoTextBox.Margin = new System.Windows.Forms.Padding(2);
			this.captInfoTextBox.Name = "captInfoTextBox";
			this.captInfoTextBox.ReadOnly = true;
			this.captInfoTextBox.Size = new System.Drawing.Size(380, 23);
			this.captInfoTextBox.TabIndex = 83;
			// 
			// cameraTableLayoutPanel
			// 
			this.cameraTableLayoutPanel.AutoSize = true;
			this.cameraTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.cameraTableLayoutPanel.ColumnCount = 2;
			this.tableLayoutPanel2.SetColumnSpan(this.cameraTableLayoutPanel, 2);
			this.cameraTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.cameraTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.cameraTableLayoutPanel.Controls.Add(this.CaptureDeviceTextBox, 1, 0);
			this.cameraTableLayoutPanel.Controls.Add(this.label3, 0, 1);
			this.cameraTableLayoutPanel.Controls.Add(this.label2, 0, 0);
			this.cameraTableLayoutPanel.Controls.Add(this.uvcProfileInfotextBox, 1, 1);
			this.cameraTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cameraTableLayoutPanel.Location = new System.Drawing.Point(2, 2);
			this.cameraTableLayoutPanel.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this.cameraTableLayoutPanel.Name = "cameraTableLayoutPanel";
			this.cameraTableLayoutPanel.RowCount = 3;
			this.cameraTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.cameraTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.cameraTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.cameraTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.cameraTableLayoutPanel.Size = new System.Drawing.Size(450, 56);
			this.cameraTableLayoutPanel.TabIndex = 80;
			// 
			// CaptureDeviceTextBox
			// 
			this.CaptureDeviceTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CaptureDeviceTextBox.Location = new System.Drawing.Point(61, 2);
			this.CaptureDeviceTextBox.Margin = new System.Windows.Forms.Padding(2);
			this.CaptureDeviceTextBox.Name = "CaptureDeviceTextBox";
			this.CaptureDeviceTextBox.ReadOnly = true;
			this.CaptureDeviceTextBox.Size = new System.Drawing.Size(387, 23);
			this.CaptureDeviceTextBox.TabIndex = 60;
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
			// uvcProfileInfotextBox
			// 
			this.uvcProfileInfotextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.uvcProfileInfotextBox.Location = new System.Drawing.Point(62, 30);
			this.uvcProfileInfotextBox.Name = "uvcProfileInfotextBox";
			this.uvcProfileInfotextBox.ReadOnly = true;
			this.uvcProfileInfotextBox.Size = new System.Drawing.Size(385, 23);
			this.uvcProfileInfotextBox.TabIndex = 82;
			// 
			// windowsCaptureTLPanel
			// 
			this.windowsCaptureTLPanel.AutoSize = true;
			this.windowsCaptureTLPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.windowsCaptureTLPanel.ColumnCount = 3;
			this.windowsCaptureTLPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.windowsCaptureTLPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.windowsCaptureTLPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.windowsCaptureTLPanel.Controls.Add(this.label6, 0, 0);
			this.windowsCaptureTLPanel.Controls.Add(this.windowsUpdateButton, 2, 0);
			this.windowsCaptureTLPanel.Controls.Add(this.windowsComboBox, 1, 0);
			this.windowsCaptureTLPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.windowsCaptureTLPanel.Location = new System.Drawing.Point(2, 62);
			this.windowsCaptureTLPanel.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this.windowsCaptureTLPanel.Name = "windowsCaptureTLPanel";
			this.windowsCaptureTLPanel.RowCount = 2;
			this.windowsCaptureTLPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.windowsCaptureTLPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.windowsCaptureTLPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.windowsCaptureTLPanel.Size = new System.Drawing.Size(447, 29);
			this.windowsCaptureTLPanel.TabIndex = 93;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label6.Location = new System.Drawing.Point(2, 0);
			this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(68, 29);
			this.label6.TabIndex = 17;
			this.label6.Text = "Windows:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// windowsUpdateButton
			// 
			this.windowsUpdateButton.AutoSize = true;
			this.windowsUpdateButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.windowsUpdateButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.windowsUpdateButton.Image = global::ScreenStreamer.WinForms.App.Properties.Resources.baseline_cached_black_18dp;
			this.windowsUpdateButton.Location = new System.Drawing.Point(422, 1);
			this.windowsUpdateButton.Margin = new System.Windows.Forms.Padding(1);
			this.windowsUpdateButton.Name = "windowsUpdateButton";
			this.windowsUpdateButton.Size = new System.Drawing.Size(24, 27);
			this.windowsUpdateButton.TabIndex = 18;
			this.windowsUpdateButton.UseVisualStyleBackColor = true;
			this.windowsUpdateButton.Click += new System.EventHandler(this.windowsUpdateButton_Click);
			// 
			// windowsComboBox
			// 
			this.windowsComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.windowsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.windowsComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.windowsComboBox.FormattingEnabled = true;
			this.windowsComboBox.Location = new System.Drawing.Point(74, 2);
			this.windowsComboBox.Margin = new System.Windows.Forms.Padding(2);
			this.windowsComboBox.Name = "windowsComboBox";
			this.windowsComboBox.Size = new System.Drawing.Size(345, 25);
			this.windowsComboBox.TabIndex = 16;
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
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(468, 538);
			this.tableLayoutPanel1.TabIndex = 79;
			// 
			// EncoderSettingsGroup
			// 
			this.EncoderSettingsGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.EncoderSettingsGroup.Controls.Add(this.tableLayoutPanel4);
			this.EncoderSettingsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EncoderSettingsGroup.Location = new System.Drawing.Point(2, 262);
			this.EncoderSettingsGroup.Margin = new System.Windows.Forms.Padding(2);
			this.EncoderSettingsGroup.Name = "EncoderSettingsGroup";
			this.EncoderSettingsGroup.Padding = new System.Windows.Forms.Padding(6, 15, 6, 15);
			this.EncoderSettingsGroup.Size = new System.Drawing.Size(464, 236);
			this.EncoderSettingsGroup.TabIndex = 74;
			this.EncoderSettingsGroup.TabStop = false;
			this.EncoderSettingsGroup.Text = "Encoder Settings";
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.AutoSize = true;
			this.tableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel4.ColumnCount = 2;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.Controls.Add(this.label4, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this.panelEncoderResoulution, 0, 4);
			this.tableLayoutPanel4.Controls.Add(this.encoderComboBox, 1, 0);
			this.tableLayoutPanel4.Controls.Add(this.encoderSettingsButton, 1, 1);
			this.tableLayoutPanel4.Controls.Add(this.checkBoxResoulutionFromSource, 0, 2);
			this.tableLayoutPanel4.Controls.Add(this.aspectRatioCheckBox, 0, 3);
			this.tableLayoutPanel4.Location = new System.Drawing.Point(13, 33);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 6;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(445, 186);
			this.tableLayoutPanel4.TabIndex = 81;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(2, 0);
			this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(65, 29);
			this.label4.TabIndex = 114;
			this.label4.Text = "Encoder:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelEncoderResoulution
			// 
			this.panelEncoderResoulution.AutoSize = true;
			this.panelEncoderResoulution.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panelEncoderResoulution.ColumnCount = 4;
			this.tableLayoutPanel4.SetColumnSpan(this.panelEncoderResoulution, 2);
			this.panelEncoderResoulution.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.panelEncoderResoulution.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.panelEncoderResoulution.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.panelEncoderResoulution.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.panelEncoderResoulution.Controls.Add(this.label1, 2, 0);
			this.panelEncoderResoulution.Controls.Add(this.encHeightNumeric, 3, 0);
			this.panelEncoderResoulution.Controls.Add(this.label13, 0, 0);
			this.panelEncoderResoulution.Controls.Add(this.encWidthNumeric, 1, 0);
			this.panelEncoderResoulution.Controls.Add(this.adjustAspectRatioButton, 3, 1);
			this.panelEncoderResoulution.Location = new System.Drawing.Point(7, 124);
			this.panelEncoderResoulution.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
			this.panelEncoderResoulution.Name = "panelEncoderResoulution";
			this.panelEncoderResoulution.RowCount = 3;
			this.panelEncoderResoulution.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.panelEncoderResoulution.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.panelEncoderResoulution.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.panelEncoderResoulution.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.panelEncoderResoulution.Size = new System.Drawing.Size(243, 59);
			this.panelEncoderResoulution.TabIndex = 80;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(154, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(14, 27);
			this.label1.TabIndex = 114;
			this.label1.Text = "x";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// encHeightNumeric
			// 
			this.encHeightNumeric.AutoSize = true;
			this.encHeightNumeric.Dock = System.Windows.Forms.DockStyle.Fill;
			this.encHeightNumeric.Location = new System.Drawing.Point(172, 2);
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
			this.encHeightNumeric.Size = new System.Drawing.Size(69, 23);
			this.encHeightNumeric.TabIndex = 94;
			this.encHeightNumeric.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
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
			this.encWidthNumeric.Size = new System.Drawing.Size(69, 23);
			this.encWidthNumeric.TabIndex = 92;
			this.encWidthNumeric.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
			// 
			// adjustAspectRatioButton
			// 
			this.panelEncoderResoulution.SetColumnSpan(this.adjustAspectRatioButton, 5);
			this.adjustAspectRatioButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.adjustAspectRatioButton.Location = new System.Drawing.Point(7, 29);
			this.adjustAspectRatioButton.Margin = new System.Windows.Forms.Padding(2);
			this.adjustAspectRatioButton.Name = "adjustAspectRatioButton";
			this.adjustAspectRatioButton.Size = new System.Drawing.Size(234, 28);
			this.adjustAspectRatioButton.TabIndex = 96;
			this.adjustAspectRatioButton.Text = "Adjust Resolution To Capture Size";
			this.adjustAspectRatioButton.UseVisualStyleBackColor = true;
			this.adjustAspectRatioButton.Click += new System.EventHandler(this.adjustAspectRatioButton_Click);
			// 
			// encoderComboBox
			// 
			this.encoderComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.encoderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.encoderComboBox.FormattingEnabled = true;
			this.encoderComboBox.Location = new System.Drawing.Point(71, 2);
			this.encoderComboBox.Margin = new System.Windows.Forms.Padding(2);
			this.encoderComboBox.Name = "encoderComboBox";
			this.encoderComboBox.Size = new System.Drawing.Size(372, 25);
			this.encoderComboBox.TabIndex = 98;
			this.encoderComboBox.SelectedValueChanged += new System.EventHandler(this.encoderComboBox_SelectedValueChanged);
			// 
			// encoderSettingsButton
			// 
			this.encoderSettingsButton.AutoSize = true;
			this.encoderSettingsButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.encoderSettingsButton.Location = new System.Drawing.Point(353, 31);
			this.encoderSettingsButton.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
			this.encoderSettingsButton.Name = "encoderSettingsButton";
			this.encoderSettingsButton.Size = new System.Drawing.Size(90, 27);
			this.encoderSettingsButton.TabIndex = 113;
			this.encoderSettingsButton.Text = "Settings";
			this.encoderSettingsButton.UseVisualStyleBackColor = true;
			this.encoderSettingsButton.Click += new System.EventHandler(this.encoderSettingsButton_Click);
			// 
			// checkBoxResoulutionFromSource
			// 
			this.checkBoxResoulutionFromSource.AutoSize = true;
			this.tableLayoutPanel4.SetColumnSpan(this.checkBoxResoulutionFromSource, 2);
			this.checkBoxResoulutionFromSource.Location = new System.Drawing.Point(5, 70);
			this.checkBoxResoulutionFromSource.Margin = new System.Windows.Forms.Padding(5, 10, 2, 2);
			this.checkBoxResoulutionFromSource.Name = "checkBoxResoulutionFromSource";
			this.checkBoxResoulutionFromSource.Size = new System.Drawing.Size(265, 21);
			this.checkBoxResoulutionFromSource.TabIndex = 89;
			this.checkBoxResoulutionFromSource.Text = "Use Resolution From Capture Source";
			this.checkBoxResoulutionFromSource.UseVisualStyleBackColor = true;
			this.checkBoxResoulutionFromSource.CheckedChanged += new System.EventHandler(this.checkBoxResoulutionFromSource_CheckedChanged);
			// 
			// aspectRatioCheckBox
			// 
			this.aspectRatioCheckBox.AutoSize = true;
			this.tableLayoutPanel4.SetColumnSpan(this.aspectRatioCheckBox, 2);
			this.aspectRatioCheckBox.Location = new System.Drawing.Point(5, 98);
			this.aspectRatioCheckBox.Margin = new System.Windows.Forms.Padding(5, 5, 2, 2);
			this.aspectRatioCheckBox.Name = "aspectRatioCheckBox";
			this.aspectRatioCheckBox.Size = new System.Drawing.Size(147, 21);
			this.aspectRatioCheckBox.TabIndex = 115;
			this.aspectRatioCheckBox.Text = "Keep Aspect Ratio";
			this.aspectRatioCheckBox.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.AutoSize = true;
			this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Controls.Add(this.applyButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new System.Drawing.Point(230, 504);
			this.panel1.Margin = new System.Windows.Forms.Padding(2, 4, 2, 2);
			this.panel1.Name = "panel1";
			this.panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.panel1.Size = new System.Drawing.Size(236, 32);
			this.panel1.TabIndex = 60;
			// 
			// VideoSettingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(605, 684);
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
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.screenCaptureTableLayoutPanel.ResumeLayout(false);
			this.screenCaptureTableLayoutPanel.PerformLayout();
			this.cameraTableLayoutPanel.ResumeLayout(false);
			this.cameraTableLayoutPanel.PerformLayout();
			this.windowsCaptureTLPanel.ResumeLayout(false);
			this.windowsCaptureTLPanel.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.EncoderSettingsGroup.ResumeLayout(false);
			this.EncoderSettingsGroup.PerformLayout();
			this.tableLayoutPanel4.ResumeLayout(false);
			this.tableLayoutPanel4.PerformLayout();
			this.panelEncoderResoulution.ResumeLayout(false);
			this.panelEncoderResoulution.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.encHeightNumeric)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.encWidthNumeric)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox ScreenCaptureGroup;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox displayTextBox;
        private System.Windows.Forms.Label labelDisplay;
        private System.Windows.Forms.TextBox captureRegionTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox CaptureDeviceTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TableLayoutPanel cameraTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel screenCaptureTableLayoutPanel;
        private System.Windows.Forms.GroupBox EncoderSettingsGroup;
        private System.Windows.Forms.NumericUpDown encHeightNumeric;
        private System.Windows.Forms.NumericUpDown encWidthNumeric;
        private System.Windows.Forms.Button adjustAspectRatioButton;
        private System.Windows.Forms.CheckBox checkBoxResoulutionFromSource;
        private System.Windows.Forms.Button snippingToolButton;
        private System.Windows.Forms.Button previewButton;
        private System.Windows.Forms.ComboBox encoderComboBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel panelEncoderResoulution;
        private System.Windows.Forms.Button encoderSettingsButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button captureSettingsButton;
		private System.Windows.Forms.TextBox captInfoTextBox;
        private System.Windows.Forms.CheckBox aspectRatioCheckBox;
        private System.Windows.Forms.TextBox uvcProfileInfotextBox;
		private System.Windows.Forms.TableLayoutPanel windowsCaptureTLPanel;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button windowsUpdateButton;
		private System.Windows.Forms.ComboBox windowsComboBox;
	}
}