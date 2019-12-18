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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StreamingForm));
            this.stopStreamingButton = new System.Windows.Forms.Button();
            this.videoSourceComboBox = new System.Windows.Forms.ComboBox();
            this.streamNameTextBox = new System.Windows.Forms.TextBox();
            this.exitButton = new System.Windows.Forms.Button();
            this.videoSourceDetailsButton = new System.Windows.Forms.Button();
            this.networkSettingsButton = new System.Windows.Forms.Button();
            this.videoSourceEnableCheckBox = new System.Windows.Forms.CheckBox();
            this.audioSourceEnableCheckBox = new System.Windows.Forms.CheckBox();
            this.audioSourceComboBox = new System.Windows.Forms.ComboBox();
            this.audioSourceDetailsButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.videoSourceUpdateButton = new System.Windows.Forms.Button();
            this.startStreamingButton = new System.Windows.Forms.Button();
            this.audioSourceUpdateButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // stopStreamingButton
            // 
            this.stopStreamingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.stopStreamingButton.Location = new System.Drawing.Point(175, 426);
            this.stopStreamingButton.Name = "stopStreamingButton";
            this.stopStreamingButton.Size = new System.Drawing.Size(85, 32);
            this.stopStreamingButton.TabIndex = 86;
            this.stopStreamingButton.Text = "_Stop";
            this.stopStreamingButton.UseVisualStyleBackColor = true;
            this.stopStreamingButton.Visible = false;
            this.stopStreamingButton.Click += new System.EventHandler(this.stopStreamingButton_Click);
            // 
            // videoSourceComboBox
            // 
            this.videoSourceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoSourceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.videoSourceComboBox.FormattingEnabled = true;
            this.videoSourceComboBox.Location = new System.Drawing.Point(30, 156);
            this.videoSourceComboBox.Margin = new System.Windows.Forms.Padding(3, 3, 1, 3);
            this.videoSourceComboBox.Name = "videoSourceComboBox";
            this.videoSourceComboBox.Size = new System.Drawing.Size(502, 24);
            this.videoSourceComboBox.TabIndex = 82;
            this.videoSourceComboBox.SelectedValueChanged += new System.EventHandler(this.videoSourceComboBox_SelectedValueChanged);
            // 
            // streamNameTextBox
            // 
            this.streamNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.streamNameTextBox.Location = new System.Drawing.Point(131, 51);
            this.streamNameTextBox.Name = "streamNameTextBox";
            this.streamNameTextBox.Size = new System.Drawing.Size(425, 22);
            this.streamNameTextBox.TabIndex = 66;
            this.streamNameTextBox.Text = "RAS-HOME10";
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Location = new System.Drawing.Point(463, 426);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(96, 32);
            this.exitButton.TabIndex = 88;
            this.exitButton.Text = "_Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // videoSourceDetailsButton
            // 
            this.videoSourceDetailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.videoSourceDetailsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.videoSourceDetailsButton.Location = new System.Drawing.Point(464, 184);
            this.videoSourceDetailsButton.Margin = new System.Windows.Forms.Padding(1);
            this.videoSourceDetailsButton.Name = "videoSourceDetailsButton";
            this.videoSourceDetailsButton.Size = new System.Drawing.Size(96, 27);
            this.videoSourceDetailsButton.TabIndex = 90;
            this.videoSourceDetailsButton.Text = "Details...";
            this.videoSourceDetailsButton.UseVisualStyleBackColor = true;
            this.videoSourceDetailsButton.Click += new System.EventHandler(this.videoSourceDetailsButton_Click);
            // 
            // networkSettingsButton
            // 
            this.networkSettingsButton.AutoSize = true;
            this.networkSettingsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.networkSettingsButton.Location = new System.Drawing.Point(30, 79);
            this.networkSettingsButton.Name = "networkSettingsButton";
            this.networkSettingsButton.Size = new System.Drawing.Size(144, 27);
            this.networkSettingsButton.TabIndex = 96;
            this.networkSettingsButton.Text = "More settings...";
            this.networkSettingsButton.UseVisualStyleBackColor = true;
            this.networkSettingsButton.Click += new System.EventHandler(this.networkSettingsButton_Click);
            // 
            // videoSourceEnableCheckBox
            // 
            this.videoSourceEnableCheckBox.AutoSize = true;
            this.videoSourceEnableCheckBox.Checked = true;
            this.videoSourceEnableCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.videoSourceEnableCheckBox.Location = new System.Drawing.Point(30, 188);
            this.videoSourceEnableCheckBox.Name = "videoSourceEnableCheckBox";
            this.videoSourceEnableCheckBox.Size = new System.Drawing.Size(159, 21);
            this.videoSourceEnableCheckBox.TabIndex = 91;
            this.videoSourceEnableCheckBox.Text = "Enable video source";
            this.videoSourceEnableCheckBox.UseVisualStyleBackColor = true;
            // 
            // audioSourceEnableCheckBox
            // 
            this.audioSourceEnableCheckBox.AutoSize = true;
            this.audioSourceEnableCheckBox.Location = new System.Drawing.Point(30, 308);
            this.audioSourceEnableCheckBox.Name = "audioSourceEnableCheckBox";
            this.audioSourceEnableCheckBox.Size = new System.Drawing.Size(160, 21);
            this.audioSourceEnableCheckBox.TabIndex = 91;
            this.audioSourceEnableCheckBox.Text = "Enable audio source";
            this.audioSourceEnableCheckBox.UseVisualStyleBackColor = true;
            // 
            // audioSourceComboBox
            // 
            this.audioSourceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.audioSourceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.audioSourceComboBox.FormattingEnabled = true;
            this.audioSourceComboBox.Location = new System.Drawing.Point(30, 276);
            this.audioSourceComboBox.Margin = new System.Windows.Forms.Padding(3, 3, 1, 3);
            this.audioSourceComboBox.Name = "audioSourceComboBox";
            this.audioSourceComboBox.Size = new System.Drawing.Size(502, 24);
            this.audioSourceComboBox.TabIndex = 82;
            this.audioSourceComboBox.SelectedValueChanged += new System.EventHandler(this.audioSourceComboBox_SelectedValueChanged);
            // 
            // audioSourceDetailsButton
            // 
            this.audioSourceDetailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.audioSourceDetailsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.audioSourceDetailsButton.Location = new System.Drawing.Point(464, 304);
            this.audioSourceDetailsButton.Margin = new System.Windows.Forms.Padding(1);
            this.audioSourceDetailsButton.Name = "audioSourceDetailsButton";
            this.audioSourceDetailsButton.Size = new System.Drawing.Size(96, 27);
            this.audioSourceDetailsButton.TabIndex = 90;
            this.audioSourceDetailsButton.Text = "Details...";
            this.audioSourceDetailsButton.UseVisualStyleBackColor = true;
            this.audioSourceDetailsButton.Click += new System.EventHandler(this.audioSourceDetailsButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 248);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(148, 17);
            this.label2.TabIndex = 92;
            this.label2.Text = "Audio Source Settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 128);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(148, 17);
            this.label3.TabIndex = 93;
            this.label3.Text = "Video Source Settings";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Location = new System.Drawing.Point(11, 407);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(554, 10);
            this.groupBox3.TabIndex = 99;
            this.groupBox3.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 17);
            this.label4.TabIndex = 100;
            this.label4.Text = "Network Settings";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 17);
            this.label1.TabIndex = 102;
            this.label1.Text = "Stream Name:";
            // 
            // videoSourceUpdateButton
            // 
            this.videoSourceUpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.videoSourceUpdateButton.AutoSize = true;
            this.videoSourceUpdateButton.Image = global::Test.Streamer.Properties.Resources.baseline_cached_black_18dp;
            this.videoSourceUpdateButton.Location = new System.Drawing.Point(534, 155);
            this.videoSourceUpdateButton.Margin = new System.Windows.Forms.Padding(1);
            this.videoSourceUpdateButton.Name = "videoSourceUpdateButton";
            this.videoSourceUpdateButton.Size = new System.Drawing.Size(26, 26);
            this.videoSourceUpdateButton.TabIndex = 89;
            this.videoSourceUpdateButton.UseVisualStyleBackColor = true;
            this.videoSourceUpdateButton.Click += new System.EventHandler(this.videoSourceUpdateButton_Click);
            // 
            // startStreamingButton
            // 
            this.startStreamingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.startStreamingButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.startStreamingButton.Location = new System.Drawing.Point(11, 426);
            this.startStreamingButton.Name = "startStreamingButton";
            this.startStreamingButton.Size = new System.Drawing.Size(157, 32);
            this.startStreamingButton.TabIndex = 85;
            this.startStreamingButton.Text = "_Start Streaming";
            this.startStreamingButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.startStreamingButton.UseVisualStyleBackColor = true;
            this.startStreamingButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // audioSourceUpdateButton
            // 
            this.audioSourceUpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.audioSourceUpdateButton.AutoSize = true;
            this.audioSourceUpdateButton.Image = global::Test.Streamer.Properties.Resources.baseline_cached_black_18dp;
            this.audioSourceUpdateButton.Location = new System.Drawing.Point(534, 275);
            this.audioSourceUpdateButton.Margin = new System.Windows.Forms.Padding(1);
            this.audioSourceUpdateButton.Name = "audioSourceUpdateButton";
            this.audioSourceUpdateButton.Size = new System.Drawing.Size(26, 26);
            this.audioSourceUpdateButton.TabIndex = 89;
            this.audioSourceUpdateButton.UseVisualStyleBackColor = true;
            this.audioSourceUpdateButton.Click += new System.EventHandler(this.audioSourceUpdateButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(165, 255);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(398, 10);
            this.groupBox1.TabIndex = 97;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox2.Location = new System.Drawing.Point(171, 135);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(392, 10);
            this.groupBox2.TabIndex = 98;
            this.groupBox2.TabStop = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Location = new System.Drawing.Point(131, 30);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(427, 10);
            this.groupBox4.TabIndex = 101;
            this.groupBox4.TabStop = false;
            // 
            // StreamingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(571, 470);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.videoSourceEnableCheckBox);
            this.Controls.Add(this.networkSettingsButton);
            this.Controls.Add(this.audioSourceEnableCheckBox);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.videoSourceComboBox);
            this.Controls.Add(this.videoSourceUpdateButton);
            this.Controls.Add(this.streamNameTextBox);
            this.Controls.Add(this.audioSourceComboBox);
            this.Controls.Add(this.startStreamingButton);
            this.Controls.Add(this.videoSourceDetailsButton);
            this.Controls.Add(this.stopStreamingButton);
            this.Controls.Add(this.audioSourceUpdateButton);
            this.Controls.Add(this.audioSourceDetailsButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(450, 450);
            this.Name = "StreamingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Polywall Streamer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button startStreamingButton;
        private System.Windows.Forms.Button stopStreamingButton;
        private System.Windows.Forms.ComboBox videoSourceComboBox;
        private System.Windows.Forms.TextBox streamNameTextBox;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button videoSourceDetailsButton;
        private System.Windows.Forms.Button videoSourceUpdateButton;
        private System.Windows.Forms.Button networkSettingsButton;
        private System.Windows.Forms.CheckBox videoSourceEnableCheckBox;
        private System.Windows.Forms.CheckBox audioSourceEnableCheckBox;
        private System.Windows.Forms.ComboBox audioSourceComboBox;
        private System.Windows.Forms.Button audioSourceUpdateButton;
        private System.Windows.Forms.Button audioSourceDetailsButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox4;
    }
}