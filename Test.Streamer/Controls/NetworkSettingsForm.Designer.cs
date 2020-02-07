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
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.findFreePortButton = new System.Windows.Forms.Button();
            this.networkComboBox = new System.Windows.Forms.ComboBox();
            this.videoSourceUpdateButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.communicationPortNumeric = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelDevice = new System.Windows.Forms.Label();
            this.multicastPanel = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.multicastAddressTextBox = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.unicastRadioButton = new System.Windows.Forms.RadioButton();
            this.textBoxStreamName = new System.Windows.Forms.TextBox();
            this.transportComboBox = new System.Windows.Forms.ComboBox();
            this.multicastRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.communicationPortNumeric)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.multicastPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 3, 11, 11);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(422, 359);
            this.tableLayoutPanel1.TabIndex = 84;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(416, 314);
            this.panel2.TabIndex = 85;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.findFreePortButton);
            this.groupBox2.Controls.Add(this.networkComboBox);
            this.groupBox2.Controls.Add(this.videoSourceUpdateButton);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label23);
            this.groupBox2.Controls.Add(this.communicationPortNumeric);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(407, 102);
            this.groupBox2.TabIndex = 85;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Server Settings";
            // 
            // findFreePortButton
            // 
            this.findFreePortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.findFreePortButton.Location = new System.Drawing.Point(270, 54);
            this.findFreePortButton.Name = "findFreePortButton";
            this.findFreePortButton.Size = new System.Drawing.Size(125, 27);
            this.findFreePortButton.TabIndex = 91;
            this.findFreePortButton.Text = "Find Available...";
            this.findFreePortButton.UseVisualStyleBackColor = true;
            this.findFreePortButton.Click += new System.EventHandler(this.findFreePortButton_Click);
            // 
            // networkComboBox
            // 
            this.networkComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.networkComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.networkComboBox.FormattingEnabled = true;
            this.networkComboBox.Location = new System.Drawing.Point(113, 21);
            this.networkComboBox.Name = "networkComboBox";
            this.networkComboBox.Size = new System.Drawing.Size(252, 24);
            this.networkComboBox.TabIndex = 16;
            // 
            // videoSourceUpdateButton
            // 
            this.videoSourceUpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.videoSourceUpdateButton.AutoSize = true;
            this.videoSourceUpdateButton.Image = global::Test.Streamer.Properties.Resources.baseline_cached_black_18dp;
            this.videoSourceUpdateButton.Location = new System.Drawing.Point(369, 21);
            this.videoSourceUpdateButton.Margin = new System.Windows.Forms.Padding(1);
            this.videoSourceUpdateButton.Name = "videoSourceUpdateButton";
            this.videoSourceUpdateButton.Size = new System.Drawing.Size(26, 26);
            this.videoSourceUpdateButton.TabIndex = 90;
            this.videoSourceUpdateButton.UseVisualStyleBackColor = true;
            this.videoSourceUpdateButton.Click += new System.EventHandler(this.videoSourceUpdateButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 17);
            this.label5.TabIndex = 17;
            this.label5.Text = "Network:";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(10, 59);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(138, 17);
            this.label23.TabIndex = 20;
            this.label23.Text = "Communication Port:";
            // 
            // communicationPortNumeric
            // 
            this.communicationPortNumeric.Location = new System.Drawing.Point(160, 57);
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
            this.communicationPortNumeric.Size = new System.Drawing.Size(80, 22);
            this.communicationPortNumeric.TabIndex = 19;
            this.communicationPortNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.labelDevice);
            this.groupBox1.Controls.Add(this.multicastPanel);
            this.groupBox1.Controls.Add(this.unicastRadioButton);
            this.groupBox1.Controls.Add(this.textBoxStreamName);
            this.groupBox1.Controls.Add(this.transportComboBox);
            this.groupBox1.Controls.Add(this.multicastRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(4, 115);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(406, 196);
            this.groupBox1.TabIndex = 91;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Streaming Settings";
            // 
            // labelDevice
            // 
            this.labelDevice.AutoSize = true;
            this.labelDevice.Location = new System.Drawing.Point(13, 24);
            this.labelDevice.Name = "labelDevice";
            this.labelDevice.Size = new System.Drawing.Size(98, 17);
            this.labelDevice.TabIndex = 93;
            this.labelDevice.Text = "Stream Name:";
            // 
            // multicastPanel
            // 
            this.multicastPanel.AutoSize = true;
            this.multicastPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.multicastPanel.Controls.Add(this.label3);
            this.multicastPanel.Controls.Add(this.multicastAddressTextBox);
            this.multicastPanel.Controls.Add(this.textBox2);
            this.multicastPanel.Controls.Add(this.textBox1);
            this.multicastPanel.Controls.Add(this.label2);
            this.multicastPanel.Controls.Add(this.label1);
            this.multicastPanel.Location = new System.Drawing.Point(18, 118);
            this.multicastPanel.Name = "multicastPanel";
            this.multicastPanel.Size = new System.Drawing.Size(241, 56);
            this.multicastPanel.TabIndex = 85;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(159, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(13, 17);
            this.label3.TabIndex = 75;
            this.label3.Text = "-";
            // 
            // multicastAddressTextBox
            // 
            this.multicastAddressTextBox.Location = new System.Drawing.Point(93, 3);
            this.multicastAddressTextBox.Name = "multicastAddressTextBox";
            this.multicastAddressTextBox.Size = new System.Drawing.Size(145, 22);
            this.multicastAddressTextBox.TabIndex = 70;
            this.multicastAddressTextBox.Text = "239.0.0.1";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(178, 31);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(60, 22);
            this.textBox2.TabIndex = 74;
            this.textBox2.Text = "5555";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(93, 31);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(60, 22);
            this.textBox1.TabIndex = 71;
            this.textBox1.Text = "1234";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 17);
            this.label2.TabIndex = 73;
            this.label2.Text = "Port Range:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 17);
            this.label1.TabIndex = 72;
            this.label1.Text = "Address:";
            // 
            // unicastRadioButton
            // 
            this.unicastRadioButton.AutoSize = true;
            this.unicastRadioButton.Checked = true;
            this.unicastRadioButton.Location = new System.Drawing.Point(18, 58);
            this.unicastRadioButton.Name = "unicastRadioButton";
            this.unicastRadioButton.Size = new System.Drawing.Size(76, 21);
            this.unicastRadioButton.TabIndex = 67;
            this.unicastRadioButton.TabStop = true;
            this.unicastRadioButton.Text = "Unicast";
            this.unicastRadioButton.UseVisualStyleBackColor = true;
            this.unicastRadioButton.CheckedChanged += new System.EventHandler(this.unicastRadioButton_CheckedChanged);
            // 
            // textBoxStreamName
            // 
            this.textBoxStreamName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStreamName.Location = new System.Drawing.Point(112, 21);
            this.textBoxStreamName.Name = "textBoxStreamName";
            this.textBoxStreamName.Size = new System.Drawing.Size(282, 22);
            this.textBoxStreamName.TabIndex = 92;
            // 
            // transportComboBox
            // 
            this.transportComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.transportComboBox.FormattingEnabled = true;
            this.transportComboBox.Location = new System.Drawing.Point(111, 58);
            this.transportComboBox.Name = "transportComboBox";
            this.transportComboBox.Size = new System.Drawing.Size(145, 24);
            this.transportComboBox.TabIndex = 69;
            // 
            // multicastRadioButton
            // 
            this.multicastRadioButton.AutoSize = true;
            this.multicastRadioButton.Location = new System.Drawing.Point(18, 91);
            this.multicastRadioButton.Name = "multicastRadioButton";
            this.multicastRadioButton.Size = new System.Drawing.Size(84, 21);
            this.multicastRadioButton.TabIndex = 68;
            this.multicastRadioButton.Text = "Multicast";
            this.multicastRadioButton.UseVisualStyleBackColor = true;
            this.multicastRadioButton.CheckedChanged += new System.EventHandler(this.multicastRadioButton_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(170, 323);
            this.panel1.Name = "panel1";
            this.panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.panel1.Size = new System.Drawing.Size(249, 33);
            this.panel1.TabIndex = 81;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(146, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 27);
            this.cancelButton.TabIndex = 77;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
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
            // NetworkSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(556, 552);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NetworkSettingsForm";
            this.Text = "Network Settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.communicationPortNumeric)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.multicastPanel.ResumeLayout(false);
            this.multicastPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.NumericUpDown communicationPortNumeric;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton unicastRadioButton;
        private System.Windows.Forms.RadioButton multicastRadioButton;
        private System.Windows.Forms.ComboBox transportComboBox;
        private System.Windows.Forms.TextBox multicastAddressTextBox;
        private System.Windows.Forms.ComboBox networkComboBox;
        private System.Windows.Forms.Button videoSourceUpdateButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Panel multicastPanel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button findFreePortButton;
        private System.Windows.Forms.Label labelDevice;
        private System.Windows.Forms.TextBox textBoxStreamName;
    }
}