namespace TestStreamer.Controls
{
    partial class AudioSettingsForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.captFormatTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.captureComboBox = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.sampleRateNumeric = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.channelsNumeric = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.encoderComboBox = new System.Windows.Forms.ComboBox();
            this.labelDevice = new System.Windows.Forms.Label();
            this.textBoxDevice = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sampleRateNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.channelsNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(97, 246);
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.labelDevice);
            this.groupBox2.Controls.Add(this.textBoxDevice);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.captFormatTextBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.captureComboBox);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(343, 118);
            this.groupBox2.TabIndex = 82;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Capture Settings";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 17);
            this.label2.TabIndex = 83;
            this.label2.Text = "Format:";
            // 
            // captFormatTextBox
            // 
            this.captFormatTextBox.Location = new System.Drawing.Point(94, 49);
            this.captFormatTextBox.Name = "captFormatTextBox";
            this.captFormatTextBox.ReadOnly = true;
            this.captFormatTextBox.Size = new System.Drawing.Size(243, 22);
            this.captFormatTextBox.TabIndex = 82;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 17);
            this.label3.TabIndex = 46;
            this.label3.Text = "Capture:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // captureComboBox
            // 
            this.captureComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.captureComboBox.Enabled = false;
            this.captureComboBox.FormattingEnabled = true;
            this.captureComboBox.Location = new System.Drawing.Point(94, 79);
            this.captureComboBox.Name = "captureComboBox";
            this.captureComboBox.Size = new System.Drawing.Size(243, 24);
            this.captureComboBox.TabIndex = 47;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 3, 11, 11);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(349, 282);
            this.tableLayoutPanel1.TabIndex = 83;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.sampleRateNumeric);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.channelsNumeric);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.encoderComboBox);
            this.groupBox1.Location = new System.Drawing.Point(3, 127);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(343, 113);
            this.groupBox1.TabIndex = 74;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Encoder Settings";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 54);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(93, 17);
            this.label7.TabIndex = 76;
            this.label7.Text = "Sample Rate:";
            // 
            // sampleRateNumeric
            // 
            this.sampleRateNumeric.Enabled = false;
            this.sampleRateNumeric.Location = new System.Drawing.Point(112, 52);
            this.sampleRateNumeric.Maximum = new decimal(new int[] {
            96000,
            0,
            0,
            0});
            this.sampleRateNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.sampleRateNumeric.Name = "sampleRateNumeric";
            this.sampleRateNumeric.Size = new System.Drawing.Size(107, 22);
            this.sampleRateNumeric.TabIndex = 75;
            this.sampleRateNumeric.Value = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(13, 80);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 17);
            this.label10.TabIndex = 74;
            this.label10.Text = "Channels:";
            // 
            // channelsNumeric
            // 
            this.channelsNumeric.Enabled = false;
            this.channelsNumeric.Location = new System.Drawing.Point(112, 80);
            this.channelsNumeric.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.channelsNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.channelsNumeric.Name = "channelsNumeric";
            this.channelsNumeric.Size = new System.Drawing.Size(107, 22);
            this.channelsNumeric.TabIndex = 73;
            this.channelsNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 24);
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
            this.encoderComboBox.Location = new System.Drawing.Point(112, 21);
            this.encoderComboBox.Name = "encoderComboBox";
            this.encoderComboBox.Size = new System.Drawing.Size(225, 24);
            this.encoderComboBox.TabIndex = 47;
            // 
            // labelDevice
            // 
            this.labelDevice.AutoSize = true;
            this.labelDevice.Location = new System.Drawing.Point(13, 24);
            this.labelDevice.Name = "labelDevice";
            this.labelDevice.Size = new System.Drawing.Size(55, 17);
            this.labelDevice.TabIndex = 85;
            this.labelDevice.Text = "Device:";
            // 
            // textBoxDevice
            // 
            this.textBoxDevice.Location = new System.Drawing.Point(94, 21);
            this.textBoxDevice.Name = "textBoxDevice";
            this.textBoxDevice.ReadOnly = true;
            this.textBoxDevice.Size = new System.Drawing.Size(243, 22);
            this.textBoxDevice.TabIndex = 84;
            // 
            // AudioSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(414, 368);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AudioSettingsForm";
            this.Text = "AudioSettings";
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sampleRateNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.channelsNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox captureComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox captFormatTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown sampleRateNumeric;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown channelsNumeric;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox encoderComboBox;
        private System.Windows.Forms.Label labelDevice;
        private System.Windows.Forms.TextBox textBoxDevice;
    }
}