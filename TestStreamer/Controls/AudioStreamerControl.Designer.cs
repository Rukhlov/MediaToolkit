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
            this.settingButton = new System.Windows.Forms.Button();
            this.waveformPainter2 = new NAudio.Gui.WaveformPainter();
            this.waveformPainter1 = new NAudio.Gui.WaveformPainter();
            this.label16 = new System.Windows.Forms.Label();
            this.audioUpdateButton = new System.Windows.Forms.Button();
            this.audioSrcComboBox = new System.Windows.Forms.ComboBox();
            this.audioStartButton = new System.Windows.Forms.Button();
            this.audioStopButton = new System.Windows.Forms.Button();
            this.settingPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // settingPanel
            // 
            this.settingPanel.Controls.Add(this.settingButton);
            this.settingPanel.Controls.Add(this.waveformPainter2);
            this.settingPanel.Controls.Add(this.waveformPainter1);
            this.settingPanel.Controls.Add(this.label16);
            this.settingPanel.Controls.Add(this.audioUpdateButton);
            this.settingPanel.Controls.Add(this.audioSrcComboBox);
            this.settingPanel.Location = new System.Drawing.Point(3, 26);
            this.settingPanel.Name = "settingPanel";
            this.settingPanel.Size = new System.Drawing.Size(552, 409);
            this.settingPanel.TabIndex = 23;
            // 
            // settingButton
            // 
            this.settingButton.Location = new System.Drawing.Point(453, 15);
            this.settingButton.Name = "settingButton";
            this.settingButton.Size = new System.Drawing.Size(75, 23);
            this.settingButton.TabIndex = 51;
            this.settingButton.Text = "_Settings";
            this.settingButton.UseVisualStyleBackColor = true;
            this.settingButton.Click += new System.EventHandler(this.settingButton_Click);
            // 
            // waveformPainter2
            // 
            this.waveformPainter2.BackColor = System.Drawing.SystemColors.Info;
            this.waveformPainter2.Location = new System.Drawing.Point(14, 137);
            this.waveformPainter2.Margin = new System.Windows.Forms.Padding(1);
            this.waveformPainter2.Name = "waveformPainter2";
            this.waveformPainter2.Size = new System.Drawing.Size(433, 38);
            this.waveformPainter2.TabIndex = 50;
            this.waveformPainter2.Text = "waveformPainter2";
            // 
            // waveformPainter1
            // 
            this.waveformPainter1.BackColor = System.Drawing.SystemColors.Info;
            this.waveformPainter1.Location = new System.Drawing.Point(14, 97);
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
            this.Size = new System.Drawing.Size(628, 549);
            this.settingPanel.ResumeLayout(false);
            this.settingPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel settingPanel;
        private NAudio.Gui.WaveformPainter waveformPainter1;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button audioUpdateButton;
        private System.Windows.Forms.ComboBox audioSrcComboBox;
        private System.Windows.Forms.Button audioStartButton;
        private System.Windows.Forms.Button audioStopButton;
        private NAudio.Gui.WaveformPainter waveformPainter2;
        private System.Windows.Forms.Button settingButton;
    }
}
