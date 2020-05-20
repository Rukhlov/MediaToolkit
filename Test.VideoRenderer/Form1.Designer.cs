namespace Test.VideoRenderer
{
    partial class Form1
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
			this.buttonStart = new System.Windows.Forms.Button();
			this.buttonStop = new System.Windows.Forms.Button();
			this.buttonPause = new System.Windows.Forms.Button();
			this.buttonSetup = new System.Windows.Forms.Button();
			this.buttonClose = new System.Windows.Forms.Button();
			this.buttonAudioSetup = new System.Windows.Forms.Button();
			this.buttonAudioStart = new System.Windows.Forms.Button();
			this.buttonAudioStop = new System.Windows.Forms.Button();
			this.buttonCloseAudio = new System.Windows.Forms.Button();
			this.buttonProcessSample = new System.Windows.Forms.Button();
			this.checkBoxMute = new System.Windows.Forms.CheckBox();
			this.trackBarVolume = new System.Windows.Forms.TrackBar();
			this.buttonSetBitmap = new System.Windows.Forms.Button();
			this.buttonClearBitmap = new System.Windows.Forms.Button();
			this.buttonGetBitmap = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
			this.d3DImageControl1 = new MediaToolkit.UI.D3DImageControl();
			this.button4 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonStart
			// 
			this.buttonStart.Location = new System.Drawing.Point(109, 92);
			this.buttonStart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonStart.Name = "buttonStart";
			this.buttonStart.Size = new System.Drawing.Size(101, 30);
			this.buttonStart.TabIndex = 0;
			this.buttonStart.Text = "Start";
			this.buttonStart.UseVisualStyleBackColor = true;
			this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
			// 
			// buttonStop
			// 
			this.buttonStop.Location = new System.Drawing.Point(373, 92);
			this.buttonStop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonStop.Name = "buttonStop";
			this.buttonStop.Size = new System.Drawing.Size(83, 30);
			this.buttonStop.TabIndex = 1;
			this.buttonStop.Text = "Stop";
			this.buttonStop.UseVisualStyleBackColor = true;
			this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
			// 
			// buttonPause
			// 
			this.buttonPause.Location = new System.Drawing.Point(233, 92);
			this.buttonPause.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonPause.Name = "buttonPause";
			this.buttonPause.Size = new System.Drawing.Size(101, 30);
			this.buttonPause.TabIndex = 2;
			this.buttonPause.Text = "Pause";
			this.buttonPause.UseVisualStyleBackColor = true;
			this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
			// 
			// buttonSetup
			// 
			this.buttonSetup.Location = new System.Drawing.Point(109, 25);
			this.buttonSetup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonSetup.Name = "buttonSetup";
			this.buttonSetup.Size = new System.Drawing.Size(101, 30);
			this.buttonSetup.TabIndex = 3;
			this.buttonSetup.Text = "Setup";
			this.buttonSetup.UseVisualStyleBackColor = true;
			this.buttonSetup.Click += new System.EventHandler(this.buttonSetup_Click);
			// 
			// buttonClose
			// 
			this.buttonClose.Location = new System.Drawing.Point(373, 198);
			this.buttonClose.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size(101, 30);
			this.buttonClose.TabIndex = 4;
			this.buttonClose.Text = "Close";
			this.buttonClose.UseVisualStyleBackColor = true;
			this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
			// 
			// buttonAudioSetup
			// 
			this.buttonAudioSetup.Location = new System.Drawing.Point(29, 261);
			this.buttonAudioSetup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonAudioSetup.Name = "buttonAudioSetup";
			this.buttonAudioSetup.Size = new System.Drawing.Size(117, 30);
			this.buttonAudioSetup.TabIndex = 5;
			this.buttonAudioSetup.Text = "SetupAudio";
			this.buttonAudioSetup.UseVisualStyleBackColor = true;
			this.buttonAudioSetup.Click += new System.EventHandler(this.buttonAudioSetup_Click);
			// 
			// buttonAudioStart
			// 
			this.buttonAudioStart.Location = new System.Drawing.Point(30, 308);
			this.buttonAudioStart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonAudioStart.Name = "buttonAudioStart";
			this.buttonAudioStart.Size = new System.Drawing.Size(116, 30);
			this.buttonAudioStart.TabIndex = 6;
			this.buttonAudioStart.Text = "AudioStart";
			this.buttonAudioStart.UseVisualStyleBackColor = true;
			this.buttonAudioStart.Click += new System.EventHandler(this.buttonAudioStart_Click);
			// 
			// buttonAudioStop
			// 
			this.buttonAudioStop.Location = new System.Drawing.Point(325, 308);
			this.buttonAudioStop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonAudioStop.Name = "buttonAudioStop";
			this.buttonAudioStop.Size = new System.Drawing.Size(117, 30);
			this.buttonAudioStop.TabIndex = 7;
			this.buttonAudioStop.Text = "AudioStop";
			this.buttonAudioStop.UseVisualStyleBackColor = true;
			this.buttonAudioStop.Click += new System.EventHandler(this.buttonAudioStop_Click);
			// 
			// buttonCloseAudio
			// 
			this.buttonCloseAudio.Location = new System.Drawing.Point(325, 368);
			this.buttonCloseAudio.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonCloseAudio.Name = "buttonCloseAudio";
			this.buttonCloseAudio.Size = new System.Drawing.Size(117, 25);
			this.buttonCloseAudio.TabIndex = 8;
			this.buttonCloseAudio.Text = "CloseAudio";
			this.buttonCloseAudio.UseVisualStyleBackColor = true;
			this.buttonCloseAudio.Click += new System.EventHandler(this.buttonCloseAudio_Click);
			// 
			// buttonProcessSample
			// 
			this.buttonProcessSample.Location = new System.Drawing.Point(165, 308);
			this.buttonProcessSample.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonProcessSample.Name = "buttonProcessSample";
			this.buttonProcessSample.Size = new System.Drawing.Size(133, 30);
			this.buttonProcessSample.TabIndex = 9;
			this.buttonProcessSample.Text = "ProcessSample";
			this.buttonProcessSample.UseVisualStyleBackColor = true;
			this.buttonProcessSample.Click += new System.EventHandler(this.buttonProcessSample_Click);
			// 
			// checkBoxMute
			// 
			this.checkBoxMute.AutoSize = true;
			this.checkBoxMute.Location = new System.Drawing.Point(165, 383);
			this.checkBoxMute.Name = "checkBoxMute";
			this.checkBoxMute.Size = new System.Drawing.Size(61, 21);
			this.checkBoxMute.TabIndex = 10;
			this.checkBoxMute.Text = "Mute";
			this.checkBoxMute.UseVisualStyleBackColor = true;
			this.checkBoxMute.CheckedChanged += new System.EventHandler(this.checkBoxMute_CheckedChanged);
			// 
			// trackBarVolume
			// 
			this.trackBarVolume.AutoSize = false;
			this.trackBarVolume.Location = new System.Drawing.Point(165, 359);
			this.trackBarVolume.Maximum = 100;
			this.trackBarVolume.Name = "trackBarVolume";
			this.trackBarVolume.Size = new System.Drawing.Size(104, 18);
			this.trackBarVolume.TabIndex = 11;
			this.trackBarVolume.TickFrequency = 10;
			this.trackBarVolume.Scroll += new System.EventHandler(this.trackBarVolume_Scroll);
			// 
			// buttonSetBitmap
			// 
			this.buttonSetBitmap.Location = new System.Drawing.Point(641, 25);
			this.buttonSetBitmap.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonSetBitmap.Name = "buttonSetBitmap";
			this.buttonSetBitmap.Size = new System.Drawing.Size(101, 30);
			this.buttonSetBitmap.TabIndex = 12;
			this.buttonSetBitmap.Text = "SetBitmap";
			this.buttonSetBitmap.UseVisualStyleBackColor = true;
			this.buttonSetBitmap.Click += new System.EventHandler(this.buttonTest_Click);
			// 
			// buttonClearBitmap
			// 
			this.buttonClearBitmap.Location = new System.Drawing.Point(641, 83);
			this.buttonClearBitmap.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonClearBitmap.Name = "buttonClearBitmap";
			this.buttonClearBitmap.Size = new System.Drawing.Size(101, 30);
			this.buttonClearBitmap.TabIndex = 13;
			this.buttonClearBitmap.Text = "ClearBitmap";
			this.buttonClearBitmap.UseVisualStyleBackColor = true;
			this.buttonClearBitmap.Click += new System.EventHandler(this.buttonClearBitmap_Click);
			// 
			// buttonGetBitmap
			// 
			this.buttonGetBitmap.Location = new System.Drawing.Point(650, 154);
			this.buttonGetBitmap.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonGetBitmap.Name = "buttonGetBitmap";
			this.buttonGetBitmap.Size = new System.Drawing.Size(101, 30);
			this.buttonGetBitmap.TabIndex = 14;
			this.buttonGetBitmap.Text = "GetBitmap";
			this.buttonGetBitmap.UseVisualStyleBackColor = true;
			this.buttonGetBitmap.Click += new System.EventHandler(this.buttonGetBitmap_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(579, 231);
			this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(101, 30);
			this.button1.TabIndex = 15;
			this.button1.Text = "GC.Collect";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(75, 163);
			this.button2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(205, 30);
			this.button2.TabIndex = 16;
			this.button2.Text = "ProcessSurface";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(534, 318);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 17;
			this.button3.Text = "button3";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// elementHost1
			// 
			this.elementHost1.Location = new System.Drawing.Point(480, 359);
			this.elementHost1.Name = "elementHost1";
			this.elementHost1.Size = new System.Drawing.Size(232, 129);
			this.elementHost1.TabIndex = 18;
			this.elementHost1.Text = "elementHost1";
			this.elementHost1.Child = this.d3DImageControl1;
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(637, 318);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(75, 23);
			this.button4.TabIndex = 19;
			this.button4.Text = "button4";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(974, 552);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.elementHost1);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.buttonGetBitmap);
			this.Controls.Add(this.buttonClearBitmap);
			this.Controls.Add(this.buttonSetBitmap);
			this.Controls.Add(this.trackBarVolume);
			this.Controls.Add(this.checkBoxMute);
			this.Controls.Add(this.buttonProcessSample);
			this.Controls.Add(this.buttonCloseAudio);
			this.Controls.Add(this.buttonAudioStop);
			this.Controls.Add(this.buttonAudioStart);
			this.Controls.Add(this.buttonAudioSetup);
			this.Controls.Add(this.buttonClose);
			this.Controls.Add(this.buttonSetup);
			this.Controls.Add(this.buttonPause);
			this.Controls.Add(this.buttonStop);
			this.Controls.Add(this.buttonStart);
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Name = "Form1";
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.Button buttonSetup;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonAudioSetup;
        private System.Windows.Forms.Button buttonAudioStart;
        private System.Windows.Forms.Button buttonAudioStop;
        private System.Windows.Forms.Button buttonCloseAudio;
        private System.Windows.Forms.Button buttonProcessSample;
        private System.Windows.Forms.CheckBox checkBoxMute;
        private System.Windows.Forms.TrackBar trackBarVolume;
        private System.Windows.Forms.Button buttonSetBitmap;
        private System.Windows.Forms.Button buttonClearBitmap;
        private System.Windows.Forms.Button buttonGetBitmap;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Integration.ElementHost elementHost1;
		private MediaToolkit.UI.D3DImageControl d3DImageControl1;
		private System.Windows.Forms.Button button4;
	}
}

