namespace Test.Streamer.Controls
{
    partial class NetworkSettingsForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.multicastPanel = new System.Windows.Forms.TableLayoutPanel();
            this.multicastPort2Numeric = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.multicastAddressTextBox = new System.Windows.Forms.TextBox();
            this.multicastPort1Numeric = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.unicastRadioButton = new System.Windows.Forms.RadioButton();
            this.multicastRadioButton = new System.Windows.Forms.RadioButton();
            this.transportComboBox = new System.Windows.Forms.ComboBox();
            this.textBoxStreamName = new System.Windows.Forms.TextBox();
            this.labelDevice = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.videoSourceUpdateButton = new System.Windows.Forms.Button();
            this.networkComboBox = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.label23 = new System.Windows.Forms.Label();
            this.findFreePortButton = new System.Windows.Forms.Button();
            this.communicationPortNumeric = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.multicastPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.multicastPort2Numeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.multicastPort1Numeric)).BeginInit();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.communicationPortNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 11, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(431, 416);
            this.tableLayoutPanel1.TabIndex = 84;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(2, 134);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 15, 6, 15);
            this.groupBox1.Size = new System.Drawing.Size(427, 242);
            this.groupBox1.TabIndex = 91;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Streaming Settings";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.textBoxStreamName, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelDevice, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(6, 31);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(415, 196);
            this.tableLayoutPanel3.TabIndex = 85;
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.SetColumnSpan(this.panel2, 2);
            this.panel2.Controls.Add(this.multicastPanel);
            this.panel2.Controls.Add(this.unicastRadioButton);
            this.panel2.Controls.Add(this.multicastRadioButton);
            this.panel2.Controls.Add(this.transportComboBox);
            this.panel2.Location = new System.Drawing.Point(10, 42);
            this.panel2.Margin = new System.Windows.Forms.Padding(10, 15, 2, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(215, 152);
            this.panel2.TabIndex = 86;
            // 
            // multicastPanel
            // 
            this.multicastPanel.AutoSize = true;
            this.multicastPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.multicastPanel.ColumnCount = 2;
            this.multicastPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.multicastPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.multicastPanel.Controls.Add(this.multicastPort2Numeric, 1, 2);
            this.multicastPanel.Controls.Add(this.label3, 0, 2);
            this.multicastPanel.Controls.Add(this.label1, 0, 0);
            this.multicastPanel.Controls.Add(this.multicastAddressTextBox, 1, 0);
            this.multicastPanel.Controls.Add(this.multicastPort1Numeric, 1, 1);
            this.multicastPanel.Controls.Add(this.label2, 0, 1);
            this.multicastPanel.Location = new System.Drawing.Point(15, 69);
            this.multicastPanel.Margin = new System.Windows.Forms.Padding(2);
            this.multicastPanel.Name = "multicastPanel";
            this.multicastPanel.RowCount = 4;
            this.multicastPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.multicastPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.multicastPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.multicastPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.multicastPanel.Size = new System.Drawing.Size(198, 81);
            this.multicastPanel.TabIndex = 85;
            // 
            // multicastPort2Numeric
            // 
            this.multicastPort2Numeric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.multicastPort2Numeric.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.multicastPort2Numeric.Location = new System.Drawing.Point(84, 56);
            this.multicastPort2Numeric.Margin = new System.Windows.Forms.Padding(2);
            this.multicastPort2Numeric.Maximum = new decimal(new int[] {
            5555,
            0,
            0,
            0});
            this.multicastPort2Numeric.Minimum = new decimal(new int[] {
            1234,
            0,
            0,
            0});
            this.multicastPort2Numeric.Name = "multicastPort2Numeric";
            this.multicastPort2Numeric.Size = new System.Drawing.Size(112, 23);
            this.multicastPort2Numeric.TabIndex = 36;
            this.multicastPort2Numeric.Value = new decimal(new int[] {
            1235,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(2, 54);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 27);
            this.label3.TabIndex = 95;
            this.label3.Text = "Audio Port:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(2, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 27);
            this.label1.TabIndex = 72;
            this.label1.Text = "Address:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // multicastAddressTextBox
            // 
            this.multicastAddressTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.multicastAddressTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.multicastAddressTextBox.Location = new System.Drawing.Point(84, 2);
            this.multicastAddressTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.multicastAddressTextBox.Name = "multicastAddressTextBox";
            this.multicastAddressTextBox.Size = new System.Drawing.Size(112, 23);
            this.multicastAddressTextBox.TabIndex = 32;
            this.multicastAddressTextBox.Text = "239.0.0.1";
            // 
            // multicastPort1Numeric
            // 
            this.multicastPort1Numeric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.multicastPort1Numeric.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.multicastPort1Numeric.Location = new System.Drawing.Point(84, 29);
            this.multicastPort1Numeric.Margin = new System.Windows.Forms.Padding(2);
            this.multicastPort1Numeric.Maximum = new decimal(new int[] {
            5555,
            0,
            0,
            0});
            this.multicastPort1Numeric.Minimum = new decimal(new int[] {
            1234,
            0,
            0,
            0});
            this.multicastPort1Numeric.Name = "multicastPort1Numeric";
            this.multicastPort1Numeric.Size = new System.Drawing.Size(112, 23);
            this.multicastPort1Numeric.TabIndex = 34;
            this.multicastPort1Numeric.Value = new decimal(new int[] {
            1234,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(2, 27);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 27);
            this.label2.TabIndex = 73;
            this.label2.Text = "Video Port:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // unicastRadioButton
            // 
            this.unicastRadioButton.AutoSize = true;
            this.unicastRadioButton.Checked = true;
            this.unicastRadioButton.Location = new System.Drawing.Point(2, 2);
            this.unicastRadioButton.Margin = new System.Windows.Forms.Padding(2);
            this.unicastRadioButton.Name = "unicastRadioButton";
            this.unicastRadioButton.Size = new System.Drawing.Size(76, 21);
            this.unicastRadioButton.TabIndex = 27;
            this.unicastRadioButton.TabStop = true;
            this.unicastRadioButton.Text = "Unicast";
            this.unicastRadioButton.UseVisualStyleBackColor = true;
            this.unicastRadioButton.CheckedChanged += new System.EventHandler(this.unicastRadioButton_CheckedChanged);
            // 
            // multicastRadioButton
            // 
            this.multicastRadioButton.AutoSize = true;
            this.multicastRadioButton.Location = new System.Drawing.Point(2, 40);
            this.multicastRadioButton.Margin = new System.Windows.Forms.Padding(2);
            this.multicastRadioButton.Name = "multicastRadioButton";
            this.multicastRadioButton.Size = new System.Drawing.Size(84, 21);
            this.multicastRadioButton.TabIndex = 29;
            this.multicastRadioButton.Text = "Multicast";
            this.multicastRadioButton.UseVisualStyleBackColor = true;
            this.multicastRadioButton.CheckedChanged += new System.EventHandler(this.multicastRadioButton_CheckedChanged);
            // 
            // transportComboBox
            // 
            this.transportComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.transportComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.transportComboBox.FormattingEnabled = true;
            this.transportComboBox.Location = new System.Drawing.Point(105, 2);
            this.transportComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.transportComboBox.Name = "transportComboBox";
            this.transportComboBox.Size = new System.Drawing.Size(65, 25);
            this.transportComboBox.TabIndex = 28;
            // 
            // textBoxStreamName
            // 
            this.textBoxStreamName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxStreamName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxStreamName.Location = new System.Drawing.Point(104, 2);
            this.textBoxStreamName.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxStreamName.Name = "textBoxStreamName";
            this.textBoxStreamName.Size = new System.Drawing.Size(309, 23);
            this.textBoxStreamName.TabIndex = 24;
            // 
            // labelDevice
            // 
            this.labelDevice.AutoSize = true;
            this.labelDevice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelDevice.Location = new System.Drawing.Point(2, 2);
            this.labelDevice.Margin = new System.Windows.Forms.Padding(2);
            this.labelDevice.Name = "labelDevice";
            this.labelDevice.Size = new System.Drawing.Size(98, 23);
            this.labelDevice.TabIndex = 93;
            this.labelDevice.Text = "Stream Name:";
            this.labelDevice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(158, 382);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 4, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.panel1.Size = new System.Drawing.Size(271, 32);
            this.panel1.TabIndex = 81;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(160, 2);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(109, 28);
            this.cancelButton.TabIndex = 50;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(41, 2);
            this.applyButton.Margin = new System.Windows.Forms.Padding(2);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(109, 28);
            this.applyButton.TabIndex = 40;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel2);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(2, 2);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(6, 15, 6, 15);
            this.groupBox2.Size = new System.Drawing.Size(427, 128);
            this.groupBox2.TabIndex = 85;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Server Settings";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.videoSourceUpdateButton, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.networkComboBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 31);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(415, 82);
            this.tableLayoutPanel2.TabIndex = 92;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(2, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 29);
            this.label5.TabIndex = 17;
            this.label5.Text = "Network:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // videoSourceUpdateButton
            // 
            this.videoSourceUpdateButton.AutoSize = true;
            this.videoSourceUpdateButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.videoSourceUpdateButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoSourceUpdateButton.Image = global::ScreenStreamer.WinForms.App.Properties.Resources.baseline_cached_black_18dp;
            this.videoSourceUpdateButton.Location = new System.Drawing.Point(390, 1);
            this.videoSourceUpdateButton.Margin = new System.Windows.Forms.Padding(1);
            this.videoSourceUpdateButton.Name = "videoSourceUpdateButton";
            this.videoSourceUpdateButton.Size = new System.Drawing.Size(24, 27);
            this.videoSourceUpdateButton.TabIndex = 18;
            this.videoSourceUpdateButton.UseVisualStyleBackColor = true;
            this.videoSourceUpdateButton.Click += new System.EventHandler(this.videoSourceUpdateButton_Click);
            // 
            // networkComboBox
            // 
            this.networkComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.networkComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.networkComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.networkComboBox.FormattingEnabled = true;
            this.networkComboBox.Location = new System.Drawing.Point(69, 2);
            this.networkComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.networkComboBox.Name = "networkComboBox";
            this.networkComboBox.Size = new System.Drawing.Size(318, 25);
            this.networkComboBox.TabIndex = 16;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel2.SetColumnSpan(this.tableLayoutPanel4, 3);
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.label23, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.findFreePortButton, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.communicationPortNumeric, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(4, 39);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4, 10, 4, 4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(407, 39);
            this.tableLayoutPanel4.TabIndex = 85;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label23.Location = new System.Drawing.Point(4, 4);
            this.label23.Margin = new System.Windows.Forms.Padding(4);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(138, 23);
            this.label23.TabIndex = 20;
            this.label23.Text = "Communication Port:";
            this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // findFreePortButton
            // 
            this.findFreePortButton.AutoSize = true;
            this.findFreePortButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.findFreePortButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.findFreePortButton.Location = new System.Drawing.Point(289, 2);
            this.findFreePortButton.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.findFreePortButton.Name = "findFreePortButton";
            this.findFreePortButton.Size = new System.Drawing.Size(118, 27);
            this.findFreePortButton.TabIndex = 22;
            this.findFreePortButton.Text = "Find Available...";
            this.findFreePortButton.UseVisualStyleBackColor = true;
            this.findFreePortButton.Click += new System.EventHandler(this.findFreePortButton_Click);
            // 
            // communicationPortNumeric
            // 
            this.communicationPortNumeric.AutoSize = true;
            this.communicationPortNumeric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.communicationPortNumeric.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.communicationPortNumeric.Location = new System.Drawing.Point(150, 4);
            this.communicationPortNumeric.Margin = new System.Windows.Forms.Padding(4);
            this.communicationPortNumeric.Maximum = new decimal(new int[] {
            100500,
            0,
            0,
            0});
            this.communicationPortNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.communicationPortNumeric.Name = "communicationPortNumeric";
            this.communicationPortNumeric.Size = new System.Drawing.Size(133, 23);
            this.communicationPortNumeric.TabIndex = 19;
            this.communicationPortNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // NetworkSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(500, 497);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NetworkSettingsForm";
            this.Text = "Network Settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.multicastPanel.ResumeLayout(false);
            this.multicastPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.multicastPort2Numeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.multicastPort1Numeric)).EndInit();
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.communicationPortNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton unicastRadioButton;
        private System.Windows.Forms.RadioButton multicastRadioButton;
        private System.Windows.Forms.ComboBox transportComboBox;
        private System.Windows.Forms.TextBox multicastAddressTextBox;
        private System.Windows.Forms.Button videoSourceUpdateButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelDevice;
        private System.Windows.Forms.TextBox textBoxStreamName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ComboBox networkComboBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.NumericUpDown multicastPort2Numeric;
        private System.Windows.Forms.NumericUpDown multicastPort1Numeric;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel multicastPanel;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Button findFreePortButton;
        private System.Windows.Forms.NumericUpDown communicationPortNumeric;
    }
}