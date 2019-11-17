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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.sampleRateNumeric = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.channelsNumeric = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.encoderComboBox = new System.Windows.Forms.ComboBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sampleRateNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.channelsNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.sampleRateNumeric);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.channelsNumeric);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.encoderComboBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(449, 139);
            this.groupBox1.TabIndex = 74;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "AudioSettings";
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
            this.encoderComboBox.Location = new System.Drawing.Point(108, 21);
            this.encoderComboBox.Name = "encoderComboBox";
            this.encoderComboBox.Size = new System.Drawing.Size(203, 24);
            this.encoderComboBox.TabIndex = 47;
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(244, 157);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(100, 23);
            this.applyButton.TabIndex = 76;
            this.applyButton.Text = "_Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(353, 157);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 23);
            this.cancelButton.TabIndex = 77;
            this.cancelButton.Text = "_Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // AudioSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 194);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AudioSettingsForm";
            this.Text = "_AudioSettings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sampleRateNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.channelsNumeric)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox encoderComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown sampleRateNumeric;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown channelsNumeric;
    }
}