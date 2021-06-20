namespace Test.Decoder
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
			this.videoPanel = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.button1 = new System.Windows.Forms.Button();
			this.buttonRun = new System.Windows.Forms.Button();
			this.buttonSourceStart = new System.Windows.Forms.Button();
			this.buttonSourceStop = new System.Windows.Forms.Button();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// videoPanel
			// 
			this.videoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.videoPanel.BackColor = System.Drawing.Color.Black;
			this.videoPanel.Location = new System.Drawing.Point(12, 12);
			this.videoPanel.Name = "videoPanel";
			this.videoPanel.Size = new System.Drawing.Size(632, 565);
			this.videoPanel.TabIndex = 0;
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.Controls.Add(this.buttonSourceStop);
			this.panel2.Controls.Add(this.buttonSourceStart);
			this.panel2.Controls.Add(this.button1);
			this.panel2.Controls.Add(this.buttonRun);
			this.panel2.Location = new System.Drawing.Point(650, 12);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(280, 565);
			this.panel2.TabIndex = 1;
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point(151, 522);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(81, 25);
			this.button1.TabIndex = 1;
			this.button1.Text = "Stop";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// buttonRun
			// 
			this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonRun.Location = new System.Drawing.Point(25, 522);
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.Size = new System.Drawing.Size(81, 25);
			this.buttonRun.TabIndex = 0;
			this.buttonRun.Text = "Run";
			this.buttonRun.UseVisualStyleBackColor = true;
			this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
			// 
			// buttonSourceStart
			// 
			this.buttonSourceStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSourceStart.Location = new System.Drawing.Point(25, 244);
			this.buttonSourceStart.Name = "buttonSourceStart";
			this.buttonSourceStart.Size = new System.Drawing.Size(81, 25);
			this.buttonSourceStart.TabIndex = 2;
			this.buttonSourceStart.Text = "Start";
			this.buttonSourceStart.UseVisualStyleBackColor = true;
			this.buttonSourceStart.Click += new System.EventHandler(this.buttonSourceStart_Click);
			// 
			// buttonSourceStop
			// 
			this.buttonSourceStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSourceStop.Location = new System.Drawing.Point(139, 244);
			this.buttonSourceStop.Name = "buttonSourceStop";
			this.buttonSourceStop.Size = new System.Drawing.Size(81, 25);
			this.buttonSourceStop.TabIndex = 3;
			this.buttonSourceStop.Text = "Stop";
			this.buttonSourceStop.UseVisualStyleBackColor = true;
			this.buttonSourceStop.Click += new System.EventHandler(this.buttonSourceStop_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(942, 591);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.videoPanel);
			this.Name = "Form1";
			this.Text = "Form1";
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

        }

		#endregion

		private System.Windows.Forms.Panel videoPanel;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button buttonRun;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button buttonSourceStop;
		private System.Windows.Forms.Button buttonSourceStart;
	}
}

