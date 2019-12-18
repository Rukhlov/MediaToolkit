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
            this.panel1 = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label23 = new System.Windows.Forms.Label();
            this.communicationPortNumeric = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.unicastRadioButton = new System.Windows.Forms.RadioButton();
            this.multicastRadioButton = new System.Windows.Forms.RadioButton();
            this.transportComboBox = new System.Windows.Forms.ComboBox();
            this.multicastAddressTextBox = new System.Windows.Forms.TextBox();
            this.networkComboBox = new System.Windows.Forms.ComboBox();
            this.videoSourceUpdateButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.communicationPortNumeric)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(376, 265);
            this.tableLayoutPanel1.TabIndex = 84;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(124, 229);
            this.panel1.Name = "panel1";
            this.panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.panel1.Size = new System.Drawing.Size(249, 33);
            this.panel1.TabIndex = 81;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(146, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 27);
            this.cancelButton.TabIndex = 77;
            this.cancelButton.Text = "_Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(37, 3);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(100, 27);
            this.applyButton.TabIndex = 76;
            this.applyButton.Text = "_Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(370, 220);
            this.panel2.TabIndex = 85;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(7, 62);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(46, 17);
            this.label23.TabIndex = 20;
            this.label23.Text = "_Port:";
            // 
            // communicationPortNumeric
            // 
            this.communicationPortNumeric.Location = new System.Drawing.Point(100, 60);
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
            this.communicationPortNumeric.Size = new System.Drawing.Size(96, 22);
            this.communicationPortNumeric.TabIndex = 19;
            this.communicationPortNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 17);
            this.label5.TabIndex = 17;
            this.label5.Text = "_Networks:";
            // 
            // unicastRadioButton
            // 
            this.unicastRadioButton.AutoSize = true;
            this.unicastRadioButton.Checked = true;
            this.unicastRadioButton.Location = new System.Drawing.Point(10, 33);
            this.unicastRadioButton.Name = "unicastRadioButton";
            this.unicastRadioButton.Size = new System.Drawing.Size(76, 21);
            this.unicastRadioButton.TabIndex = 67;
            this.unicastRadioButton.TabStop = true;
            this.unicastRadioButton.Text = "Unicast";
            this.unicastRadioButton.UseVisualStyleBackColor = true;
            // 
            // multicastRadioButton
            // 
            this.multicastRadioButton.AutoSize = true;
            this.multicastRadioButton.Location = new System.Drawing.Point(10, 63);
            this.multicastRadioButton.Name = "multicastRadioButton";
            this.multicastRadioButton.Size = new System.Drawing.Size(84, 21);
            this.multicastRadioButton.TabIndex = 68;
            this.multicastRadioButton.Text = "Multicast";
            this.multicastRadioButton.UseVisualStyleBackColor = true;
            // 
            // transportComboBox
            // 
            this.transportComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.transportComboBox.Enabled = false;
            this.transportComboBox.FormattingEnabled = true;
            this.transportComboBox.Location = new System.Drawing.Point(100, 32);
            this.transportComboBox.Name = "transportComboBox";
            this.transportComboBox.Size = new System.Drawing.Size(96, 24);
            this.transportComboBox.TabIndex = 69;
            // 
            // multicastAddressTextBox
            // 
            this.multicastAddressTextBox.Location = new System.Drawing.Point(100, 62);
            this.multicastAddressTextBox.Name = "multicastAddressTextBox";
            this.multicastAddressTextBox.Size = new System.Drawing.Size(96, 22);
            this.multicastAddressTextBox.TabIndex = 70;
            this.multicastAddressTextBox.Text = "239.0.0.1";
            // 
            // networkComboBox
            // 
            this.networkComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.networkComboBox.FormattingEnabled = true;
            this.networkComboBox.Location = new System.Drawing.Point(100, 30);
            this.networkComboBox.Name = "networkComboBox";
            this.networkComboBox.Size = new System.Drawing.Size(225, 24);
            this.networkComboBox.TabIndex = 16;
            // 
            // videoSourceUpdateButton
            // 
            this.videoSourceUpdateButton.AutoSize = true;
            this.videoSourceUpdateButton.Image = global::Test.Streamer.Properties.Resources.baseline_cached_black_18dp;
            this.videoSourceUpdateButton.Location = new System.Drawing.Point(329, 28);
            this.videoSourceUpdateButton.Margin = new System.Windows.Forms.Padding(1);
            this.videoSourceUpdateButton.Name = "videoSourceUpdateButton";
            this.videoSourceUpdateButton.Size = new System.Drawing.Size(26, 26);
            this.videoSourceUpdateButton.TabIndex = 90;
            this.videoSourceUpdateButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.unicastRadioButton);
            this.groupBox1.Controls.Add(this.multicastAddressTextBox);
            this.groupBox1.Controls.Add(this.transportComboBox);
            this.groupBox1.Controls.Add(this.multicastRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(3, 109);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(363, 100);
            this.groupBox1.TabIndex = 91;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "_Streaming Mode";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.networkComboBox);
            this.groupBox2.Controls.Add(this.videoSourceUpdateButton);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label23);
            this.groupBox2.Controls.Add(this.communicationPortNumeric);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(363, 100);
            this.groupBox2.TabIndex = 85;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "_Server Settings";
            // 
            // NetworkSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(485, 359);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NetworkSettingsForm";
            this.Text = "_Network Settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.communicationPortNumeric)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}