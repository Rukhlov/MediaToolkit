namespace TestStreamer.Controls
{
    partial class ScreenStreamerControl
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

            UsbManager.Close();
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
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.networkPanel = new System.Windows.Forms.Panel();
            this.unicastRadioButton = new System.Windows.Forms.RadioButton();
            this.multicastRadioButton = new System.Windows.Forms.RadioButton();
            this.transportComboBox = new System.Windows.Forms.ComboBox();
            this.multicastAddressTextBox = new System.Windows.Forms.TextBox();
            this.audioPreviewButton = new System.Windows.Forms.Button();
            this.videoEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.audioEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.videoPreviewButton = new System.Windows.Forms.Button();
            this.audioUpdateButton = new System.Windows.Forms.Button();
            this.videoUpdateButton = new System.Windows.Forms.Button();
            this.audioSettingButton = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.audioSourcesComboBox = new System.Windows.Forms.ComboBox();
            this.videoSettingsButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.videoSourcesComboBox = new System.Windows.Forms.ComboBox();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.statInfoCheckBox = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.settingPanel.SuspendLayout();
            this.networkPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // settingPanel
            // 
            this.settingPanel.Controls.Add(this.button2);
            this.settingPanel.Controls.Add(this.button1);
            this.settingPanel.Controls.Add(this.networkPanel);
            this.settingPanel.Controls.Add(this.audioPreviewButton);
            this.settingPanel.Controls.Add(this.videoEnabledCheckBox);
            this.settingPanel.Controls.Add(this.audioEnabledCheckBox);
            this.settingPanel.Controls.Add(this.videoPreviewButton);
            this.settingPanel.Controls.Add(this.audioUpdateButton);
            this.settingPanel.Controls.Add(this.videoUpdateButton);
            this.settingPanel.Controls.Add(this.audioSettingButton);
            this.settingPanel.Controls.Add(this.label16);
            this.settingPanel.Controls.Add(this.audioSourcesComboBox);
            this.settingPanel.Controls.Add(this.videoSettingsButton);
            this.settingPanel.Controls.Add(this.label4);
            this.settingPanel.Controls.Add(this.videoSourcesComboBox);
            this.settingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingPanel.Location = new System.Drawing.Point(0, 0);
            this.settingPanel.Name = "settingPanel";
            this.settingPanel.Size = new System.Drawing.Size(466, 412);
            this.settingPanel.TabIndex = 20;
            // 
            // button2
            // 
            this.button2.AutoSize = true;
            this.button2.Location = new System.Drawing.Point(275, 301);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(89, 27);
            this.button2.TabIndex = 78;
            this.button2.Text = "_Teardown";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.Location = new System.Drawing.Point(192, 301);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(77, 27);
            this.button1.TabIndex = 77;
            this.button1.Text = "_Play";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // networkPanel
            // 
            this.networkPanel.Controls.Add(this.unicastRadioButton);
            this.networkPanel.Controls.Add(this.multicastRadioButton);
            this.networkPanel.Controls.Add(this.transportComboBox);
            this.networkPanel.Controls.Add(this.multicastAddressTextBox);
            this.networkPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.networkPanel.Location = new System.Drawing.Point(0, 0);
            this.networkPanel.Name = "networkPanel";
            this.networkPanel.Size = new System.Drawing.Size(466, 49);
            this.networkPanel.TabIndex = 76;
            // 
            // unicastRadioButton
            // 
            this.unicastRadioButton.AutoSize = true;
            this.unicastRadioButton.Checked = true;
            this.unicastRadioButton.Location = new System.Drawing.Point(5, 15);
            this.unicastRadioButton.Name = "unicastRadioButton";
            this.unicastRadioButton.Size = new System.Drawing.Size(76, 21);
            this.unicastRadioButton.TabIndex = 67;
            this.unicastRadioButton.TabStop = true;
            this.unicastRadioButton.Text = "Unicast";
            this.unicastRadioButton.UseVisualStyleBackColor = true;
            this.unicastRadioButton.CheckedChanged += new System.EventHandler(this.unicastRadioButton_CheckedChanged);
            // 
            // multicastRadioButton
            // 
            this.multicastRadioButton.AutoSize = true;
            this.multicastRadioButton.Location = new System.Drawing.Point(266, 17);
            this.multicastRadioButton.Name = "multicastRadioButton";
            this.multicastRadioButton.Size = new System.Drawing.Size(84, 21);
            this.multicastRadioButton.TabIndex = 68;
            this.multicastRadioButton.Text = "Multicast";
            this.multicastRadioButton.UseVisualStyleBackColor = true;
            this.multicastRadioButton.CheckedChanged += new System.EventHandler(this.multicastRadioButton_CheckedChanged);
            // 
            // transportComboBox
            // 
            this.transportComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.transportComboBox.Enabled = false;
            this.transportComboBox.FormattingEnabled = true;
            this.transportComboBox.Location = new System.Drawing.Point(95, 14);
            this.transportComboBox.Name = "transportComboBox";
            this.transportComboBox.Size = new System.Drawing.Size(96, 24);
            this.transportComboBox.TabIndex = 69;
            this.transportComboBox.SelectedIndexChanged += new System.EventHandler(this.transportComboBox_SelectedIndexChanged);
            // 
            // multicastAddressTextBox
            // 
            this.multicastAddressTextBox.Location = new System.Drawing.Point(356, 16);
            this.multicastAddressTextBox.Name = "multicastAddressTextBox";
            this.multicastAddressTextBox.Size = new System.Drawing.Size(96, 22);
            this.multicastAddressTextBox.TabIndex = 70;
            this.multicastAddressTextBox.Text = "239.0.0.1";
            // 
            // audioPreviewButton
            // 
            this.audioPreviewButton.AutoSize = true;
            this.audioPreviewButton.Enabled = false;
            this.audioPreviewButton.Location = new System.Drawing.Point(247, 212);
            this.audioPreviewButton.Name = "audioPreviewButton";
            this.audioPreviewButton.Size = new System.Drawing.Size(77, 27);
            this.audioPreviewButton.TabIndex = 75;
            this.audioPreviewButton.Text = "_Preview";
            this.audioPreviewButton.UseVisualStyleBackColor = true;
            this.audioPreviewButton.Click += new System.EventHandler(this.audioPreviewButton_Click);
            // 
            // videoEnabledCheckBox
            // 
            this.videoEnabledCheckBox.AutoSize = true;
            this.videoEnabledCheckBox.Checked = true;
            this.videoEnabledCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.videoEnabledCheckBox.Location = new System.Drawing.Point(6, 107);
            this.videoEnabledCheckBox.Name = "videoEnabledCheckBox";
            this.videoEnabledCheckBox.Size = new System.Drawing.Size(82, 21);
            this.videoEnabledCheckBox.TabIndex = 74;
            this.videoEnabledCheckBox.Text = "Enabled";
            this.videoEnabledCheckBox.UseVisualStyleBackColor = true;
            this.videoEnabledCheckBox.CheckedChanged += new System.EventHandler(this.videoEnabledCheckBox_CheckedChanged);
            // 
            // audioEnabledCheckBox
            // 
            this.audioEnabledCheckBox.AutoSize = true;
            this.audioEnabledCheckBox.Checked = true;
            this.audioEnabledCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.audioEnabledCheckBox.Location = new System.Drawing.Point(6, 216);
            this.audioEnabledCheckBox.Name = "audioEnabledCheckBox";
            this.audioEnabledCheckBox.Size = new System.Drawing.Size(82, 21);
            this.audioEnabledCheckBox.TabIndex = 73;
            this.audioEnabledCheckBox.Text = "Enabled";
            this.audioEnabledCheckBox.UseVisualStyleBackColor = true;
            this.audioEnabledCheckBox.CheckedChanged += new System.EventHandler(this.audioEnabledCheckBox_CheckedChanged);
            // 
            // videoPreviewButton
            // 
            this.videoPreviewButton.AutoSize = true;
            this.videoPreviewButton.Location = new System.Drawing.Point(247, 103);
            this.videoPreviewButton.Name = "videoPreviewButton";
            this.videoPreviewButton.Size = new System.Drawing.Size(77, 27);
            this.videoPreviewButton.TabIndex = 19;
            this.videoPreviewButton.Text = "_Preview";
            this.videoPreviewButton.UseVisualStyleBackColor = true;
            this.videoPreviewButton.Click += new System.EventHandler(this.previewButton_Click);
            // 
            // audioUpdateButton
            // 
            this.audioUpdateButton.Location = new System.Drawing.Point(402, 180);
            this.audioUpdateButton.Name = "audioUpdateButton";
            this.audioUpdateButton.Size = new System.Drawing.Size(30, 27);
            this.audioUpdateButton.TabIndex = 72;
            this.audioUpdateButton.Text = "...";
            this.audioUpdateButton.UseVisualStyleBackColor = true;
            this.audioUpdateButton.Click += new System.EventHandler(this.audioUpdateButton_Click);
            // 
            // videoUpdateButton
            // 
            this.videoUpdateButton.AutoSize = true;
            this.videoUpdateButton.Location = new System.Drawing.Point(402, 70);
            this.videoUpdateButton.Name = "videoUpdateButton";
            this.videoUpdateButton.Size = new System.Drawing.Size(30, 27);
            this.videoUpdateButton.TabIndex = 71;
            this.videoUpdateButton.Text = "...";
            this.videoUpdateButton.UseVisualStyleBackColor = true;
            this.videoUpdateButton.Click += new System.EventHandler(this.videoUpdateButton_Click);
            // 
            // audioSettingButton
            // 
            this.audioSettingButton.Location = new System.Drawing.Point(330, 212);
            this.audioSettingButton.Name = "audioSettingButton";
            this.audioSettingButton.Size = new System.Drawing.Size(102, 27);
            this.audioSettingButton.TabIndex = 59;
            this.audioSettingButton.Text = "_Settings";
            this.audioSettingButton.UseVisualStyleBackColor = true;
            this.audioSettingButton.Click += new System.EventHandler(this.audioSettingButton_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(3, 162);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(97, 17);
            this.label16.TabIndex = 58;
            this.label16.Text = "Audio Source:";
            // 
            // audioSourcesComboBox
            // 
            this.audioSourcesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.audioSourcesComboBox.FormattingEnabled = true;
            this.audioSourcesComboBox.Location = new System.Drawing.Point(6, 182);
            this.audioSourcesComboBox.Name = "audioSourcesComboBox";
            this.audioSourcesComboBox.Size = new System.Drawing.Size(390, 24);
            this.audioSourcesComboBox.TabIndex = 57;
            this.audioSourcesComboBox.SelectedValueChanged += new System.EventHandler(this.audioSrcComboBox_SelectedValueChanged);
            // 
            // videoSettingsButton
            // 
            this.videoSettingsButton.AutoSize = true;
            this.videoSettingsButton.Location = new System.Drawing.Point(330, 103);
            this.videoSettingsButton.Name = "videoSettingsButton";
            this.videoSettingsButton.Size = new System.Drawing.Size(102, 27);
            this.videoSettingsButton.TabIndex = 55;
            this.videoSettingsButton.Text = "_Settings";
            this.videoSettingsButton.UseVisualStyleBackColor = true;
            this.videoSettingsButton.Click += new System.EventHandler(this.videoSettingsButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 17);
            this.label4.TabIndex = 14;
            this.label4.Text = "Video Source:";
            // 
            // videoSourcesComboBox
            // 
            this.videoSourcesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.videoSourcesComboBox.FormattingEnabled = true;
            this.videoSourcesComboBox.Location = new System.Drawing.Point(6, 72);
            this.videoSourcesComboBox.Name = "videoSourcesComboBox";
            this.videoSourcesComboBox.Size = new System.Drawing.Size(390, 24);
            this.videoSourcesComboBox.TabIndex = 2;
            this.videoSourcesComboBox.SelectedValueChanged += new System.EventHandler(this.videoSourcesComboBox_SelectedValueChanged);
            // 
            // startButton
            // 
            this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.startButton.Location = new System.Drawing.Point(3, 7);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(115, 27);
            this.startButton.TabIndex = 17;
            this.startButton.Text = "_Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.stopButton.Location = new System.Drawing.Point(124, 7);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(76, 27);
            this.stopButton.TabIndex = 18;
            this.stopButton.Text = "_Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // statInfoCheckBox
            // 
            this.statInfoCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.statInfoCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.statInfoCheckBox.Checked = true;
            this.statInfoCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.statInfoCheckBox.Location = new System.Drawing.Point(363, 7);
            this.statInfoCheckBox.Name = "statInfoCheckBox";
            this.statInfoCheckBox.Size = new System.Drawing.Size(100, 27);
            this.statInfoCheckBox.TabIndex = 55;
            this.statInfoCheckBox.Text = "_DebugInfo";
            this.statInfoCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.statInfoCheckBox.UseVisualStyleBackColor = true;
            this.statInfoCheckBox.CheckedChanged += new System.EventHandler(this.statInfoCheckBox_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.startButton);
            this.panel1.Controls.Add(this.statInfoCheckBox);
            this.panel1.Controls.Add(this.stopButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 375);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(466, 37);
            this.panel1.TabIndex = 56;
            // 
            // ScreenStreamerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.settingPanel);
            this.Name = "ScreenStreamerControl";
            this.Size = new System.Drawing.Size(466, 412);
            this.settingPanel.ResumeLayout(false);
            this.settingPanel.PerformLayout();
            this.networkPanel.ResumeLayout(false);
            this.networkPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel settingPanel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox videoSourcesComboBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button videoPreviewButton;
        private System.Windows.Forms.CheckBox statInfoCheckBox;
        private System.Windows.Forms.Button videoSettingsButton;
        private System.Windows.Forms.Button audioSettingButton;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox audioSourcesComboBox;
        private System.Windows.Forms.TextBox multicastAddressTextBox;
        private System.Windows.Forms.RadioButton unicastRadioButton;
        private System.Windows.Forms.ComboBox transportComboBox;
        private System.Windows.Forms.RadioButton multicastRadioButton;
        private System.Windows.Forms.Button audioUpdateButton;
        private System.Windows.Forms.Button videoUpdateButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox videoEnabledCheckBox;
        private System.Windows.Forms.CheckBox audioEnabledCheckBox;
        private System.Windows.Forms.Button audioPreviewButton;
        private System.Windows.Forms.Panel networkPanel;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
    }
}
