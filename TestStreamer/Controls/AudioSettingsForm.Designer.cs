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
            this.label3 = new System.Windows.Forms.Label();
            this.captureComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.captFormatTextBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.encoderComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.channelsNumeric = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.sampleRateNumeric = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.channelsNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sampleRateNumeric)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(97, 254);
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
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(37, 3);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(100, 27);
            this.applyButton.TabIndex = 76;
            this.applyButton.Text = "_Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.captFormatTextBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.captureComboBox);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(343, 100);
            this.groupBox2.TabIndex = 82;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "_Capture Settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 17);
            this.label3.TabIndex = 46;
            this.label3.Text = "_Capture:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // captureComboBox
            // 
            this.captureComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.captureComboBox.Enabled = false;
            this.captureComboBox.FormattingEnabled = true;
            this.captureComboBox.Location = new System.Drawing.Point(108, 49);
            this.captureComboBox.Name = "captureComboBox";
            this.captureComboBox.Size = new System.Drawing.Size(229, 24);
            this.captureComboBox.TabIndex = 47;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 17);
            this.label2.TabIndex = 83;
            this.label2.Text = "_Format:";
            // 
            // captFormatTextBox
            // 
            this.captFormatTextBox.Location = new System.Drawing.Point(108, 21);
            this.captFormatTextBox.Name = "captFormatTextBox";
            this.captFormatTextBox.ReadOnly = true;
            this.captFormatTextBox.Size = new System.Drawing.Size(229, 22);
            this.captFormatTextBox.TabIndex = 82;
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(349, 290);
            this.tableLayoutPanel1.TabIndex = 83;
            // 
            // encoderComboBox
            // 
            this.encoderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encoderComboBox.Enabled = false;
            this.encoderComboBox.FormattingEnabled = true;
            this.encoderComboBox.Location = new System.Drawing.Point(108, 21);
            this.encoderComboBox.Name = "encoderComboBox";
            this.encoderComboBox.Size = new System.Drawing.Size(229, 24);
            this.encoderComboBox.TabIndex = 47;
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
            // channelsNumeric
            // 
            this.channelsNumeric.Enabled = false;
            this.channelsNumeric.Location = new System.Drawing.Point(108, 80);
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
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(13, 80);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 17);
            this.label10.TabIndex = 74;
            this.label10.Text = "Channels:";
            // 
            // sampleRateNumeric
            // 
            this.sampleRateNumeric.Enabled = false;
            this.sampleRateNumeric.Location = new System.Drawing.Point(108, 51);
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
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 51);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 17);
            this.label7.TabIndex = 76;
            this.label7.Text = "SampleRate:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.sampleRateNumeric);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.channelsNumeric);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.encoderComboBox);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 109);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(343, 139);
            this.groupBox1.TabIndex = 74;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "_Encoder Settings";
            // 
            // AudioSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(405, 354);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AudioSettingsForm";
            this.Text = "_AudioSettings";
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.channelsNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sampleRateNumeric)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
    }
}