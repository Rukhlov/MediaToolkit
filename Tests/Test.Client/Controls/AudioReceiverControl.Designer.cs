namespace TestClient.Controls
{
    partial class AudioReceiverControl
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
            this.volumeSlider = new NAudio.Gui.VolumeSlider();
            this.audioStopButton = new System.Windows.Forms.Button();
            this.waveformPainter1 = new NAudio.Gui.WaveformPainter();
            this.audioPlayButton = new System.Windows.Forms.Button();
            this.label21 = new System.Windows.Forms.Label();
            this.audioUpdateButton = new System.Windows.Forms.Button();
            this.label19 = new System.Windows.Forms.Label();
            this.audioRenderComboBox = new System.Windows.Forms.ComboBox();
            this.sampleRateNumeric = new System.Windows.Forms.NumericUpDown();
            this.label20 = new System.Windows.Forms.Label();
            this.channelsNumeric = new System.Windows.Forms.NumericUpDown();
            this.label17 = new System.Windows.Forms.Label();
            this.audioAddrTextBox = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.audioPortNumeric = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.transportComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.sampleRateNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.channelsNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.audioPortNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // volumeSlider
            // 
            this.volumeSlider.Location = new System.Drawing.Point(398, 245);
            this.volumeSlider.Name = "volumeSlider";
            this.volumeSlider.Size = new System.Drawing.Size(158, 16);
            this.volumeSlider.TabIndex = 32;
            this.volumeSlider.VolumeChanged += new System.EventHandler(this.volumeSlider_VolumeChanged);
            // 
            // audioStopButton
            // 
            this.audioStopButton.Location = new System.Drawing.Point(177, 239);
            this.audioStopButton.Margin = new System.Windows.Forms.Padding(4);
            this.audioStopButton.Name = "audioStopButton";
            this.audioStopButton.Size = new System.Drawing.Size(100, 28);
            this.audioStopButton.TabIndex = 29;
            this.audioStopButton.Text = "Stop";
            this.audioStopButton.UseVisualStyleBackColor = true;
            this.audioStopButton.Click += new System.EventHandler(this.audioStopButton_Click);
            // 
            // waveformPainter1
            // 
            this.waveformPainter1.BackColor = System.Drawing.SystemColors.Info;
            this.waveformPainter1.Location = new System.Drawing.Point(18, 274);
            this.waveformPainter1.Name = "waveformPainter1";
            this.waveformPainter1.Size = new System.Drawing.Size(467, 69);
            this.waveformPainter1.TabIndex = 31;
            this.waveformPainter1.Text = "waveformPainter1";
            // 
            // audioPlayButton
            // 
            this.audioPlayButton.Location = new System.Drawing.Point(18, 239);
            this.audioPlayButton.Margin = new System.Windows.Forms.Padding(4);
            this.audioPlayButton.Name = "audioPlayButton";
            this.audioPlayButton.Size = new System.Drawing.Size(151, 28);
            this.audioPlayButton.TabIndex = 28;
            this.audioPlayButton.Text = "Play";
            this.audioPlayButton.UseVisualStyleBackColor = true;
            this.audioPlayButton.Click += new System.EventHandler(this.audioPlayButton_Click);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(15, 35);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(95, 17);
            this.label21.TabIndex = 29;
            this.label21.Text = "AudioRender:";
            // 
            // audioUpdateButton
            // 
            this.audioUpdateButton.Location = new System.Drawing.Point(512, 32);
            this.audioUpdateButton.Name = "audioUpdateButton";
            this.audioUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.audioUpdateButton.TabIndex = 27;
            this.audioUpdateButton.Text = "_Update";
            this.audioUpdateButton.UseVisualStyleBackColor = true;
            this.audioUpdateButton.Click += new System.EventHandler(this.audioUpdateButton_Click);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(20, 170);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(89, 17);
            this.label19.TabIndex = 21;
            this.label19.Text = "SampleRate:";
            // 
            // audioRenderComboBox
            // 
            this.audioRenderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.audioRenderComboBox.FormattingEnabled = true;
            this.audioRenderComboBox.Location = new System.Drawing.Point(115, 32);
            this.audioRenderComboBox.Name = "audioRenderComboBox";
            this.audioRenderComboBox.Size = new System.Drawing.Size(391, 24);
            this.audioRenderComboBox.TabIndex = 28;
            this.audioRenderComboBox.SelectedValueChanged += new System.EventHandler(this.audioRenderComboBox_SelectedValueChanged);
            // 
            // sampleRateNumeric
            // 
            this.sampleRateNumeric.Enabled = false;
            this.sampleRateNumeric.Location = new System.Drawing.Point(115, 170);
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
            this.sampleRateNumeric.Size = new System.Drawing.Size(97, 22);
            this.sampleRateNumeric.TabIndex = 20;
            this.sampleRateNumeric.Value = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(20, 199);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(71, 17);
            this.label20.TabIndex = 19;
            this.label20.Text = "Channels:";
            // 
            // channelsNumeric
            // 
            this.channelsNumeric.Enabled = false;
            this.channelsNumeric.Location = new System.Drawing.Point(115, 199);
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
            this.channelsNumeric.Size = new System.Drawing.Size(97, 22);
            this.channelsNumeric.TabIndex = 18;
            this.channelsNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(20, 77);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(64, 17);
            this.label17.TabIndex = 8;
            this.label17.Text = "Address:";
            // 
            // audioAddrTextBox
            // 
            this.audioAddrTextBox.Location = new System.Drawing.Point(101, 74);
            this.audioAddrTextBox.Name = "audioAddrTextBox";
            this.audioAddrTextBox.Size = new System.Drawing.Size(325, 22);
            this.audioAddrTextBox.TabIndex = 6;
            this.audioAddrTextBox.Text = "239.0.0.1";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(20, 105);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(38, 17);
            this.label18.TabIndex = 9;
            this.label18.Text = "Port:";
            // 
            // audioPortNumeric
            // 
            this.audioPortNumeric.Location = new System.Drawing.Point(101, 102);
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
            this.audioPortNumeric.Size = new System.Drawing.Size(123, 22);
            this.audioPortNumeric.TabIndex = 7;
            this.audioPortNumeric.Value = new decimal(new int[] {
            1235,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 134);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 17);
            this.label3.TabIndex = 50;
            this.label3.Text = "Transport:";
            // 
            // transportComboBox
            // 
            this.transportComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.transportComboBox.FormattingEnabled = true;
            this.transportComboBox.Location = new System.Drawing.Point(101, 130);
            this.transportComboBox.Name = "transportComboBox";
            this.transportComboBox.Size = new System.Drawing.Size(123, 24);
            this.transportComboBox.TabIndex = 49;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(325, 245);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 17);
            this.label1.TabIndex = 51;
            this.label1.Text = "_Volume:";
            // 
            // AudioReceiverControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.transportComboBox);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.audioUpdateButton);
            this.Controls.Add(this.volumeSlider);
            this.Controls.Add(this.audioRenderComboBox);
            this.Controls.Add(this.audioStopButton);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.audioAddrTextBox);
            this.Controls.Add(this.waveformPainter1);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.sampleRateNumeric);
            this.Controls.Add(this.audioPortNumeric);
            this.Controls.Add(this.audioPlayButton);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.channelsNumeric);
            this.Name = "AudioReceiverControl";
            this.Size = new System.Drawing.Size(601, 365);
            ((System.ComponentModel.ISupportInitialize)(this.sampleRateNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.channelsNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.audioPortNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NAudio.Gui.VolumeSlider volumeSlider;
        private System.Windows.Forms.Button audioStopButton;
        private NAudio.Gui.WaveformPainter waveformPainter1;
        private System.Windows.Forms.Button audioPlayButton;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Button audioUpdateButton;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.ComboBox audioRenderComboBox;
        private System.Windows.Forms.NumericUpDown sampleRateNumeric;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.NumericUpDown channelsNumeric;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox audioAddrTextBox;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.NumericUpDown audioPortNumeric;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox transportComboBox;
        private System.Windows.Forms.Label label1;
    }
}
