namespace TestStreamer.Controls
{
    partial class MainForm2
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
            this.settingPanel = new System.Windows.Forms.Panel();
            this.networkPanel = new System.Windows.Forms.Panel();
            this.unicastRadioButton = new System.Windows.Forms.RadioButton();
            this.multicastRadioButton = new System.Windows.Forms.RadioButton();
            this.transportComboBox = new System.Windows.Forms.ComboBox();
            this.multicastAddressTextBox = new System.Windows.Forms.TextBox();
            this.videoEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.audioEnabledCheckBox = new System.Windows.Forms.CheckBox();
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label23 = new System.Windows.Forms.Label();
            this.communicationPortNumeric = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.updateNetworksButton = new System.Windows.Forms.Button();
            this.networkComboBox = new System.Windows.Forms.ComboBox();
            this.exitButton = new System.Windows.Forms.Button();
            this.settingPanel.SuspendLayout();
            this.networkPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.communicationPortNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // settingPanel
            // 
            this.settingPanel.Controls.Add(this.videoEnabledCheckBox);
            this.settingPanel.Controls.Add(this.audioEnabledCheckBox);
            this.settingPanel.Controls.Add(this.audioUpdateButton);
            this.settingPanel.Controls.Add(this.videoUpdateButton);
            this.settingPanel.Controls.Add(this.audioSettingButton);
            this.settingPanel.Controls.Add(this.label16);
            this.settingPanel.Controls.Add(this.audioSourcesComboBox);
            this.settingPanel.Controls.Add(this.videoSettingsButton);
            this.settingPanel.Controls.Add(this.label4);
            this.settingPanel.Controls.Add(this.videoSourcesComboBox);
            this.settingPanel.Location = new System.Drawing.Point(6, 156);
            this.settingPanel.Name = "settingPanel";
            this.settingPanel.Size = new System.Drawing.Size(489, 242);
            this.settingPanel.TabIndex = 21;
            // 
            // networkPanel
            // 
            this.networkPanel.Controls.Add(this.unicastRadioButton);
            this.networkPanel.Controls.Add(this.multicastRadioButton);
            this.networkPanel.Controls.Add(this.transportComboBox);
            this.networkPanel.Controls.Add(this.multicastAddressTextBox);
            this.networkPanel.Location = new System.Drawing.Point(6, 77);
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
            // 
            // multicastAddressTextBox
            // 
            this.multicastAddressTextBox.Location = new System.Drawing.Point(356, 16);
            this.multicastAddressTextBox.Name = "multicastAddressTextBox";
            this.multicastAddressTextBox.Size = new System.Drawing.Size(96, 22);
            this.multicastAddressTextBox.TabIndex = 70;
            this.multicastAddressTextBox.Text = "239.0.0.1";
            // 
            // videoEnabledCheckBox
            // 
            this.videoEnabledCheckBox.AutoSize = true;
            this.videoEnabledCheckBox.Checked = true;
            this.videoEnabledCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.videoEnabledCheckBox.Location = new System.Drawing.Point(11, 67);
            this.videoEnabledCheckBox.Name = "videoEnabledCheckBox";
            this.videoEnabledCheckBox.Size = new System.Drawing.Size(82, 21);
            this.videoEnabledCheckBox.TabIndex = 74;
            this.videoEnabledCheckBox.Text = "Enabled";
            this.videoEnabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // audioEnabledCheckBox
            // 
            this.audioEnabledCheckBox.AutoSize = true;
            this.audioEnabledCheckBox.Checked = true;
            this.audioEnabledCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.audioEnabledCheckBox.Location = new System.Drawing.Point(11, 176);
            this.audioEnabledCheckBox.Name = "audioEnabledCheckBox";
            this.audioEnabledCheckBox.Size = new System.Drawing.Size(82, 21);
            this.audioEnabledCheckBox.TabIndex = 73;
            this.audioEnabledCheckBox.Text = "Enabled";
            this.audioEnabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // audioUpdateButton
            // 
            this.audioUpdateButton.AutoSize = true;
            this.audioUpdateButton.Location = new System.Drawing.Point(407, 140);
            this.audioUpdateButton.Name = "audioUpdateButton";
            this.audioUpdateButton.Size = new System.Drawing.Size(30, 27);
            this.audioUpdateButton.TabIndex = 72;
            this.audioUpdateButton.Text = "...";
            this.audioUpdateButton.UseVisualStyleBackColor = true;
            // 
            // videoUpdateButton
            // 
            this.videoUpdateButton.AutoSize = true;
            this.videoUpdateButton.Location = new System.Drawing.Point(407, 30);
            this.videoUpdateButton.Name = "videoUpdateButton";
            this.videoUpdateButton.Size = new System.Drawing.Size(30, 27);
            this.videoUpdateButton.TabIndex = 71;
            this.videoUpdateButton.Text = "...";
            this.videoUpdateButton.UseVisualStyleBackColor = true;
            // 
            // audioSettingButton
            // 
            this.audioSettingButton.Location = new System.Drawing.Point(335, 172);
            this.audioSettingButton.Name = "audioSettingButton";
            this.audioSettingButton.Size = new System.Drawing.Size(102, 27);
            this.audioSettingButton.TabIndex = 59;
            this.audioSettingButton.Text = "_Settings";
            this.audioSettingButton.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(8, 122);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(97, 17);
            this.label16.TabIndex = 58;
            this.label16.Text = "Audio Source:";
            // 
            // audioSourcesComboBox
            // 
            this.audioSourcesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.audioSourcesComboBox.FormattingEnabled = true;
            this.audioSourcesComboBox.Location = new System.Drawing.Point(11, 142);
            this.audioSourcesComboBox.Name = "audioSourcesComboBox";
            this.audioSourcesComboBox.Size = new System.Drawing.Size(390, 24);
            this.audioSourcesComboBox.TabIndex = 57;
            // 
            // videoSettingsButton
            // 
            this.videoSettingsButton.AutoSize = true;
            this.videoSettingsButton.Location = new System.Drawing.Point(335, 63);
            this.videoSettingsButton.Name = "videoSettingsButton";
            this.videoSettingsButton.Size = new System.Drawing.Size(102, 27);
            this.videoSettingsButton.TabIndex = 55;
            this.videoSettingsButton.Text = "_Settings";
            this.videoSettingsButton.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 17);
            this.label4.TabIndex = 14;
            this.label4.Text = "Video Source:";
            // 
            // videoSourcesComboBox
            // 
            this.videoSourcesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.videoSourcesComboBox.FormattingEnabled = true;
            this.videoSourcesComboBox.Location = new System.Drawing.Point(11, 32);
            this.videoSourcesComboBox.Name = "videoSourcesComboBox";
            this.videoSourcesComboBox.Size = new System.Drawing.Size(390, 24);
            this.videoSourcesComboBox.TabIndex = 2;
            // 
            // startButton
            // 
            this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.startButton.Location = new System.Drawing.Point(17, 457);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(115, 27);
            this.startButton.TabIndex = 77;
            this.startButton.Text = "_Start";
            this.startButton.UseVisualStyleBackColor = true;
            // 
            // stopButton
            // 
            this.stopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.stopButton.Location = new System.Drawing.Point(138, 457);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(76, 27);
            this.stopButton.TabIndex = 78;
            this.stopButton.Text = "_Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label23);
            this.panel1.Controls.Add(this.communicationPortNumeric);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.networkPanel);
            this.panel1.Controls.Add(this.updateNetworksButton);
            this.panel1.Controls.Add(this.networkComboBox);
            this.panel1.Location = new System.Drawing.Point(6, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(489, 138);
            this.panel1.TabIndex = 79;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(3, 41);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(46, 17);
            this.label23.TabIndex = 20;
            this.label23.Text = "_Port:";
            // 
            // communicationPortNumeric
            // 
            this.communicationPortNumeric.Location = new System.Drawing.Point(91, 39);
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
            this.communicationPortNumeric.Size = new System.Drawing.Size(78, 22);
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
            this.label5.Location = new System.Drawing.Point(3, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 17);
            this.label5.TabIndex = 17;
            this.label5.Text = "_Networks:";
            // 
            // updateNetworksButton
            // 
            this.updateNetworksButton.Location = new System.Drawing.Point(366, 9);
            this.updateNetworksButton.Name = "updateNetworksButton";
            this.updateNetworksButton.Size = new System.Drawing.Size(75, 23);
            this.updateNetworksButton.TabIndex = 18;
            this.updateNetworksButton.Text = "_Update";
            this.updateNetworksButton.UseVisualStyleBackColor = true;
            // 
            // networkComboBox
            // 
            this.networkComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.networkComboBox.FormattingEnabled = true;
            this.networkComboBox.Location = new System.Drawing.Point(91, 9);
            this.networkComboBox.Name = "networkComboBox";
            this.networkComboBox.Size = new System.Drawing.Size(269, 24);
            this.networkComboBox.TabIndex = 16;
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Location = new System.Drawing.Point(327, 469);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(157, 33);
            this.exitButton.TabIndex = 80;
            this.exitButton.Text = "_Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            // 
            // MainForm2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 514);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.settingPanel);
            this.Name = "MainForm2";
            this.Text = "MainForm2";
            this.settingPanel.ResumeLayout(false);
            this.settingPanel.PerformLayout();
            this.networkPanel.ResumeLayout(false);
            this.networkPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.communicationPortNumeric)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel settingPanel;
        private System.Windows.Forms.CheckBox videoEnabledCheckBox;
        private System.Windows.Forms.CheckBox audioEnabledCheckBox;
        private System.Windows.Forms.Button audioUpdateButton;
        private System.Windows.Forms.Button videoUpdateButton;
        private System.Windows.Forms.Button audioSettingButton;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox audioSourcesComboBox;
        private System.Windows.Forms.Button videoSettingsButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox videoSourcesComboBox;
        private System.Windows.Forms.Panel networkPanel;
        private System.Windows.Forms.RadioButton unicastRadioButton;
        private System.Windows.Forms.RadioButton multicastRadioButton;
        private System.Windows.Forms.ComboBox transportComboBox;
        private System.Windows.Forms.TextBox multicastAddressTextBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.NumericUpDown communicationPortNumeric;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button updateNetworksButton;
        private System.Windows.Forms.ComboBox networkComboBox;
        private System.Windows.Forms.Button exitButton;
    }
}