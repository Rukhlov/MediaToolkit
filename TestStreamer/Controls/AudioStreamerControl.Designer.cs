namespace TestStreamer.Controls
{
    partial class AudioStreamerControl
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.settingPanel = new System.Windows.Forms.Panel();
            this.waveformPainter2 = new NAudio.Gui.WaveformPainter();
            this.encoderComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.transportComboBox = new System.Windows.Forms.ComboBox();
            this.waveformPainter1 = new NAudio.Gui.WaveformPainter();
            this.label16 = new System.Windows.Forms.Label();
            this.audioUpdateButton = new System.Windows.Forms.Button();
            this.audioSrcComboBox = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.sampleRateNumeric = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.channelsNumeric = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.audioPortNumeric = new System.Windows.Forms.NumericUpDown();
            this.audioAddrTextBox = new System.Windows.Forms.TextBox();
            this.audioStartButton = new System.Windows.Forms.Button();
            this.audioStopButton = new System.Windows.Forms.Button();
            this.settingPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sampleRateNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.channelsNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.audioPortNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // settingPanel
            // 
            this.settingPanel.Controls.Add(this.waveformPainter2);
            this.settingPanel.Controls.Add(this.encoderComboBox);
            this.settingPanel.Controls.Add(this.label1);
            this.settingPanel.Controls.Add(this.label5);
            this.settingPanel.Controls.Add(this.transportComboBox);
            this.settingPanel.Controls.Add(this.waveformPainter1);
            this.settingPanel.Controls.Add(this.label16);
            this.settingPanel.Controls.Add(this.audioUpdateButton);
            this.settingPanel.Controls.Add(this.audioSrcComboBox);
            this.settingPanel.Controls.Add(this.label7);
            this.settingPanel.Controls.Add(this.sampleRateNumeric);
            this.settingPanel.Controls.Add(this.label6);
            this.settingPanel.Controls.Add(this.channelsNumeric);
            this.settingPanel.Controls.Add(this.label19);
            this.settingPanel.Controls.Add(this.label21);
            this.settingPanel.Controls.Add(this.audioPortNumeric);
            this.settingPanel.Controls.Add(this.audioAddrTextBox);
            this.settingPanel.Location = new System.Drawing.Point(3, 26);
            this.settingPanel.Name = "settingPanel";
            this.settingPanel.Size = new System.Drawing.Size(458, 409);
            this.settingPanel.TabIndex = 23;
            // 
            // waveformPainter2
            // 
            this.waveformPainter2.BackColor = System.Drawing.SystemColors.Info;
            this.waveformPainter2.Location = new System.Drawing.Point(1, 370);
            this.waveformPainter2.Margin = new System.Windows.Forms.Padding(1);
            this.waveformPainter2.Name = "waveformPainter2";
            this.waveformPainter2.Size = new System.Drawing.Size(433, 38);
            this.waveformPainter2.TabIndex = 50;
            this.waveformPainter2.Text = "waveformPainter2";
            // 
            // encoderComboBox
            // 
            this.encoderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encoderComboBox.Enabled = false;
            this.encoderComboBox.FormattingEnabled = true;
            this.encoderComboBox.Location = new System.Drawing.Point(116, 176);
            this.encoderComboBox.Name = "encoderComboBox";
            this.encoderComboBox.Size = new System.Drawing.Size(107, 24);
            this.encoderComboBox.TabIndex = 49;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 179);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 17);
            this.label1.TabIndex = 48;
            this.label1.Text = "Encoder:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 125);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 17);
            this.label5.TabIndex = 42;
            this.label5.Text = "Transport:";
            // 
            // transportComboBox
            // 
            this.transportComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.transportComboBox.FormattingEnabled = true;
            this.transportComboBox.Location = new System.Drawing.Point(91, 125);
            this.transportComboBox.Name = "transportComboBox";
            this.transportComboBox.Size = new System.Drawing.Size(138, 24);
            this.transportComboBox.TabIndex = 41;
            // 
            // waveformPainter1
            // 
            this.waveformPainter1.BackColor = System.Drawing.SystemColors.Info;
            this.waveformPainter1.Location = new System.Drawing.Point(1, 330);
            this.waveformPainter1.Margin = new System.Windows.Forms.Padding(1);
            this.waveformPainter1.Name = "waveformPainter1";
            this.waveformPainter1.Size = new System.Drawing.Size(433, 38);
            this.waveformPainter1.TabIndex = 21;
            this.waveformPainter1.Text = "waveformPainter1";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(11, 18);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(69, 17);
            this.label16.TabIndex = 20;
            this.label16.Text = "AudioSrc:";
            // 
            // audioUpdateButton
            // 
            this.audioUpdateButton.Location = new System.Drawing.Point(372, 15);
            this.audioUpdateButton.Name = "audioUpdateButton";
            this.audioUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.audioUpdateButton.TabIndex = 18;
            this.audioUpdateButton.Text = "_Update";
            this.audioUpdateButton.UseVisualStyleBackColor = true;
            this.audioUpdateButton.Click += new System.EventHandler(this.audioUpdateButton_Click);
            // 
            // audioSrcComboBox
            // 
            this.audioSrcComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.audioSrcComboBox.FormattingEnabled = true;
            this.audioSrcComboBox.Location = new System.Drawing.Point(86, 15);
            this.audioSrcComboBox.Name = "audioSrcComboBox";
            this.audioSrcComboBox.Size = new System.Drawing.Size(281, 24);
            this.audioSrcComboBox.TabIndex = 19;
            this.audioSrcComboBox.SelectedValueChanged += new System.EventHandler(this.audioSrcComboBox_SelectedValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(21, 206);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 17);
            this.label7.TabIndex = 17;
            this.label7.Text = "SampleRate:";
            // 
            // sampleRateNumeric
            // 
            this.sampleRateNumeric.Enabled = false;
            this.sampleRateNumeric.Location = new System.Drawing.Point(116, 206);
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
            this.sampleRateNumeric.TabIndex = 16;
            this.sampleRateNumeric.Value = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(21, 235);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 17);
            this.label6.TabIndex = 15;
            this.label6.Text = "Channels:";
            // 
            // channelsNumeric
            // 
            this.channelsNumeric.Enabled = false;
            this.channelsNumeric.Location = new System.Drawing.Point(116, 235);
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
            this.channelsNumeric.TabIndex = 14;
            this.channelsNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(11, 96);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(38, 17);
            this.label19.TabIndex = 13;
            this.label19.Text = "Port:";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(11, 69);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(64, 17);
            this.label21.TabIndex = 12;
            this.label21.Text = "Address:";
            // 
            // audioPortNumeric
            // 
            this.audioPortNumeric.Location = new System.Drawing.Point(91, 94);
            this.audioPortNumeric.Maximum = new decimal(new int[] {
            100500,
            0,
            0,
            0});
            this.audioPortNumeric.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.audioPortNumeric.Name = "audioPortNumeric";
            this.audioPortNumeric.Size = new System.Drawing.Size(138, 22);
            this.audioPortNumeric.TabIndex = 11;
            this.audioPortNumeric.Value = new decimal(new int[] {
            1235,
            0,
            0,
            0});
            // 
            // audioAddrTextBox
            // 
            this.audioAddrTextBox.Location = new System.Drawing.Point(91, 66);
            this.audioAddrTextBox.Name = "audioAddrTextBox";
            this.audioAddrTextBox.Size = new System.Drawing.Size(276, 22);
            this.audioAddrTextBox.TabIndex = 10;
            this.audioAddrTextBox.Text = "239.0.0.1";
            // 
            // audioStartButton
            // 
            this.audioStartButton.Location = new System.Drawing.Point(49, 460);
            this.audioStartButton.Name = "audioStartButton";
            this.audioStartButton.Size = new System.Drawing.Size(177, 35);
            this.audioStartButton.TabIndex = 21;
            this.audioStartButton.Text = "_Start";
            this.audioStartButton.UseVisualStyleBackColor = true;
            this.audioStartButton.Click += new System.EventHandler(this.audioStartButton_Click);
            // 
            // audioStopButton
            // 
            this.audioStopButton.Location = new System.Drawing.Point(242, 460);
            this.audioStopButton.Name = "audioStopButton";
            this.audioStopButton.Size = new System.Drawing.Size(107, 35);
            this.audioStopButton.TabIndex = 22;
            this.audioStopButton.Text = "_Stop";
            this.audioStopButton.UseVisualStyleBackColor = true;
            this.audioStopButton.Click += new System.EventHandler(this.audioStopButton_Click);
            // 
            // AudioStreamerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.settingPanel);
            this.Controls.Add(this.audioStartButton);
            this.Controls.Add(this.audioStopButton);
            this.Name = "AudioStreamerControl";
            this.Size = new System.Drawing.Size(483, 549);
            this.settingPanel.ResumeLayout(false);
            this.settingPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sampleRateNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.channelsNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.audioPortNumeric)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel settingPanel;
        private NAudio.Gui.WaveformPainter waveformPainter1;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button audioUpdateButton;
        private System.Windows.Forms.ComboBox audioSrcComboBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown sampleRateNumeric;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown channelsNumeric;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.NumericUpDown audioPortNumeric;
        private System.Windows.Forms.TextBox audioAddrTextBox;
        private System.Windows.Forms.Button audioStartButton;
        private System.Windows.Forms.Button audioStopButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox transportComboBox;
        private System.Windows.Forms.ComboBox encoderComboBox;
        private System.Windows.Forms.Label label1;
        private NAudio.Gui.WaveformPainter waveformPainter2;
    }
}
