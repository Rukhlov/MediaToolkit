namespace TestStreamer
{
    partial class StreamingForm
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
            this.statInfoCheckBox = new System.Windows.Forms.CheckBox();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.previewButton = new System.Windows.Forms.Button();
            this.settingButton = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.audioSrcComboBox = new System.Windows.Forms.ComboBox();
            this.videoSettingsButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.screensComboBox = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.multicastAddressTextBox = new System.Windows.Forms.TextBox();
            this.updateNetworksButton = new System.Windows.Forms.Button();
            this.unicastRadioButton = new System.Windows.Forms.RadioButton();
            this.transportComboBox = new System.Windows.Forms.ComboBox();
            this.networkComboBox = new System.Windows.Forms.ComboBox();
            this.multicastRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statInfoCheckBox
            // 
            this.statInfoCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.statInfoCheckBox.Checked = true;
            this.statInfoCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.statInfoCheckBox.Location = new System.Drawing.Point(340, 317);
            this.statInfoCheckBox.Name = "statInfoCheckBox";
            this.statInfoCheckBox.Size = new System.Drawing.Size(105, 27);
            this.statInfoCheckBox.TabIndex = 88;
            this.statInfoCheckBox.Text = "_DebugInfo";
            this.statInfoCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.statInfoCheckBox.UseVisualStyleBackColor = true;
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(18, 317);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(131, 27);
            this.startButton.TabIndex = 85;
            this.startButton.Text = "_Start";
            this.startButton.UseVisualStyleBackColor = true;
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(155, 317);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(85, 27);
            this.stopButton.TabIndex = 86;
            this.stopButton.Text = "_Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            // 
            // previewButton
            // 
            this.previewButton.Location = new System.Drawing.Point(451, 317);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(97, 27);
            this.previewButton.TabIndex = 87;
            this.previewButton.Text = "_Preview";
            this.previewButton.UseVisualStyleBackColor = true;
            // 
            // settingButton
            // 
            this.settingButton.Location = new System.Drawing.Point(401, 57);
            this.settingButton.Name = "settingButton";
            this.settingButton.Size = new System.Drawing.Size(75, 23);
            this.settingButton.TabIndex = 84;
            this.settingButton.Text = "_Settings";
            this.settingButton.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(9, 60);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(93, 17);
            this.label16.TabIndex = 83;
            this.label16.Text = "AudioSource:";
            // 
            // audioSrcComboBox
            // 
            this.audioSrcComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.audioSrcComboBox.FormattingEnabled = true;
            this.audioSrcComboBox.Location = new System.Drawing.Point(108, 57);
            this.audioSrcComboBox.Name = "audioSrcComboBox";
            this.audioSrcComboBox.Size = new System.Drawing.Size(287, 24);
            this.audioSrcComboBox.TabIndex = 82;
            // 
            // videoSettingsButton
            // 
            this.videoSettingsButton.Location = new System.Drawing.Point(401, 27);
            this.videoSettingsButton.Name = "videoSettingsButton";
            this.videoSettingsButton.Size = new System.Drawing.Size(75, 23);
            this.videoSettingsButton.TabIndex = 81;
            this.videoSettingsButton.Text = "_Settings";
            this.videoSettingsButton.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 17);
            this.label4.TabIndex = 80;
            this.label4.Text = "VideoSource:";
            // 
            // screensComboBox
            // 
            this.screensComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.screensComboBox.FormattingEnabled = true;
            this.screensComboBox.Location = new System.Drawing.Point(108, 26);
            this.screensComboBox.Name = "screensComboBox";
            this.screensComboBox.Size = new System.Drawing.Size(287, 24);
            this.screensComboBox.TabIndex = 79;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.multicastAddressTextBox);
            this.panel1.Controls.Add(this.updateNetworksButton);
            this.panel1.Controls.Add(this.unicastRadioButton);
            this.panel1.Controls.Add(this.transportComboBox);
            this.panel1.Controls.Add(this.networkComboBox);
            this.panel1.Controls.Add(this.multicastRadioButton);
            this.panel1.Location = new System.Drawing.Point(6, 21);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(510, 96);
            this.panel1.TabIndex = 78;
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
            // multicastAddressTextBox
            // 
            this.multicastAddressTextBox.Location = new System.Drawing.Point(322, 50);
            this.multicastAddressTextBox.Name = "multicastAddressTextBox";
            this.multicastAddressTextBox.Size = new System.Drawing.Size(92, 22);
            this.multicastAddressTextBox.TabIndex = 66;
            this.multicastAddressTextBox.Text = "239.0.0.1";
            // 
            // updateNetworksButton
            // 
            this.updateNetworksButton.Location = new System.Drawing.Point(422, 9);
            this.updateNetworksButton.Name = "updateNetworksButton";
            this.updateNetworksButton.Size = new System.Drawing.Size(75, 23);
            this.updateNetworksButton.TabIndex = 18;
            this.updateNetworksButton.Text = "_Update";
            this.updateNetworksButton.UseVisualStyleBackColor = true;
            // 
            // unicastRadioButton
            // 
            this.unicastRadioButton.AutoSize = true;
            this.unicastRadioButton.Location = new System.Drawing.Point(4, 51);
            this.unicastRadioButton.Name = "unicastRadioButton";
            this.unicastRadioButton.Size = new System.Drawing.Size(76, 21);
            this.unicastRadioButton.TabIndex = 19;
            this.unicastRadioButton.Text = "Unicast";
            this.unicastRadioButton.UseVisualStyleBackColor = true;
            // 
            // transportComboBox
            // 
            this.transportComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.transportComboBox.FormattingEnabled = true;
            this.transportComboBox.Location = new System.Drawing.Point(92, 50);
            this.transportComboBox.Name = "transportComboBox";
            this.transportComboBox.Size = new System.Drawing.Size(96, 24);
            this.transportComboBox.TabIndex = 65;
            // 
            // networkComboBox
            // 
            this.networkComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.networkComboBox.FormattingEnabled = true;
            this.networkComboBox.Location = new System.Drawing.Point(91, 9);
            this.networkComboBox.Name = "networkComboBox";
            this.networkComboBox.Size = new System.Drawing.Size(325, 24);
            this.networkComboBox.TabIndex = 16;
            // 
            // multicastRadioButton
            // 
            this.multicastRadioButton.AutoSize = true;
            this.multicastRadioButton.Checked = true;
            this.multicastRadioButton.Location = new System.Drawing.Point(232, 51);
            this.multicastRadioButton.Name = "multicastRadioButton";
            this.multicastRadioButton.Size = new System.Drawing.Size(84, 21);
            this.multicastRadioButton.TabIndex = 20;
            this.multicastRadioButton.TabStop = true;
            this.multicastRadioButton.Text = "Multicast";
            this.multicastRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(530, 127);
            this.groupBox1.TabIndex = 89;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "NetworkSettings";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.screensComboBox);
            this.groupBox2.Controls.Add(this.videoSettingsButton);
            this.groupBox2.Controls.Add(this.audioSrcComboBox);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.settingButton);
            this.groupBox2.Location = new System.Drawing.Point(12, 145);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(530, 110);
            this.groupBox2.TabIndex = 90;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "MediaSettings";
            // 
            // StreamingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 356);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statInfoCheckBox);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.previewButton);
            this.Name = "StreamingForm";
            this.Text = "StreamingForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox statInfoCheckBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button previewButton;
        private System.Windows.Forms.Button settingButton;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox audioSrcComboBox;
        private System.Windows.Forms.Button videoSettingsButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox screensComboBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox multicastAddressTextBox;
        private System.Windows.Forms.Button updateNetworksButton;
        private System.Windows.Forms.RadioButton unicastRadioButton;
        private System.Windows.Forms.ComboBox transportComboBox;
        private System.Windows.Forms.ComboBox networkComboBox;
        private System.Windows.Forms.RadioButton multicastRadioButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}