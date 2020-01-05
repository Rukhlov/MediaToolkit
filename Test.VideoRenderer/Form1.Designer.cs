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
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(82, 75);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(76, 24);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(280, 75);
            this.buttonStop.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(62, 24);
            this.buttonStop.TabIndex = 1;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonPause
            // 
            this.buttonPause.Location = new System.Drawing.Point(175, 75);
            this.buttonPause.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(76, 24);
            this.buttonPause.TabIndex = 2;
            this.buttonPause.Text = "Pause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonSetup
            // 
            this.buttonSetup.Location = new System.Drawing.Point(82, 20);
            this.buttonSetup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonSetup.Name = "buttonSetup";
            this.buttonSetup.Size = new System.Drawing.Size(76, 24);
            this.buttonSetup.TabIndex = 3;
            this.buttonSetup.Text = "Setup";
            this.buttonSetup.UseVisualStyleBackColor = true;
            this.buttonSetup.Click += new System.EventHandler(this.buttonSetup_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(280, 161);
            this.buttonClose.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(76, 24);
            this.buttonClose.TabIndex = 4;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonAudioSetup
            // 
            this.buttonAudioSetup.Location = new System.Drawing.Point(18, 227);
            this.buttonAudioSetup.Margin = new System.Windows.Forms.Padding(2);
            this.buttonAudioSetup.Name = "buttonAudioSetup";
            this.buttonAudioSetup.Size = new System.Drawing.Size(88, 24);
            this.buttonAudioSetup.TabIndex = 5;
            this.buttonAudioSetup.Text = "SetupAudio";
            this.buttonAudioSetup.UseVisualStyleBackColor = true;
            this.buttonAudioSetup.Click += new System.EventHandler(this.buttonAudioSetup_Click);
            // 
            // buttonAudioStart
            // 
            this.buttonAudioStart.Location = new System.Drawing.Point(19, 265);
            this.buttonAudioStart.Margin = new System.Windows.Forms.Padding(2);
            this.buttonAudioStart.Name = "buttonAudioStart";
            this.buttonAudioStart.Size = new System.Drawing.Size(87, 24);
            this.buttonAudioStart.TabIndex = 6;
            this.buttonAudioStart.Text = "AudioStart";
            this.buttonAudioStart.UseVisualStyleBackColor = true;
            this.buttonAudioStart.Click += new System.EventHandler(this.buttonAudioStart_Click);
            // 
            // buttonAudioStop
            // 
            this.buttonAudioStop.Location = new System.Drawing.Point(127, 265);
            this.buttonAudioStop.Margin = new System.Windows.Forms.Padding(2);
            this.buttonAudioStop.Name = "buttonAudioStop";
            this.buttonAudioStop.Size = new System.Drawing.Size(88, 24);
            this.buttonAudioStop.TabIndex = 7;
            this.buttonAudioStop.Text = "AudioStop";
            this.buttonAudioStop.UseVisualStyleBackColor = true;
            this.buttonAudioStop.Click += new System.EventHandler(this.buttonAudioStop_Click);
            // 
            // buttonCloseAudio
            // 
            this.buttonCloseAudio.Location = new System.Drawing.Point(127, 310);
            this.buttonCloseAudio.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCloseAudio.Name = "buttonCloseAudio";
            this.buttonCloseAudio.Size = new System.Drawing.Size(88, 24);
            this.buttonCloseAudio.TabIndex = 8;
            this.buttonCloseAudio.Text = "CloseAudio";
            this.buttonCloseAudio.UseVisualStyleBackColor = true;
            this.buttonCloseAudio.Click += new System.EventHandler(this.buttonCloseAudio_Click);
            // 
            // buttonProcessSample
            // 
            this.buttonProcessSample.Location = new System.Drawing.Point(268, 289);
            this.buttonProcessSample.Margin = new System.Windows.Forms.Padding(2);
            this.buttonProcessSample.Name = "buttonProcessSample";
            this.buttonProcessSample.Size = new System.Drawing.Size(100, 24);
            this.buttonProcessSample.TabIndex = 9;
            this.buttonProcessSample.Text = "ProcessSample";
            this.buttonProcessSample.UseVisualStyleBackColor = true;
            this.buttonProcessSample.Click += new System.EventHandler(this.buttonProcessSample_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 366);
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
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

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
    }
}

