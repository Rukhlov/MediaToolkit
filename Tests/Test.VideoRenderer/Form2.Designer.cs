namespace Test.VideoRenderer
{
    partial class Form2
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
			this.buttonSetBitmap = new System.Windows.Forms.Button();
			this.buttonClearBitmap = new System.Windows.Forms.Button();
			this.buttonGetBitmap = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonStart
			// 
			this.buttonStart.Location = new System.Drawing.Point(90, 130);
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
			this.buttonStop.Location = new System.Drawing.Point(377, 130);
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
			this.buttonPause.Location = new System.Drawing.Point(221, 130);
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
			this.buttonSetup.Location = new System.Drawing.Point(47, 72);
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
			this.buttonClose.Location = new System.Drawing.Point(359, 210);
			this.buttonClose.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size(101, 30);
			this.buttonClose.TabIndex = 4;
			this.buttonClose.Text = "Close";
			this.buttonClose.UseVisualStyleBackColor = true;
			this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
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
			this.buttonGetBitmap.Location = new System.Drawing.Point(641, 138);
			this.buttonGetBitmap.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.buttonGetBitmap.Name = "buttonGetBitmap";
			this.buttonGetBitmap.Size = new System.Drawing.Size(101, 30);
			this.buttonGetBitmap.TabIndex = 14;
			this.buttonGetBitmap.Text = "GetBitmap";
			this.buttonGetBitmap.UseVisualStyleBackColor = true;
			this.buttonGetBitmap.Click += new System.EventHandler(this.buttonGetBitmap_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(58, 201);
			this.button2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(205, 30);
			this.button2.TabIndex = 16;
			this.button2.Text = "ProcessSurface";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(58, 13);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(252, 22);
			this.textBox1.TabIndex = 17;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(316, 12);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(34, 23);
			this.button1.TabIndex = 18;
			this.button1.Text = "...";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// Form2
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 315);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.buttonGetBitmap);
			this.Controls.Add(this.buttonClearBitmap);
			this.Controls.Add(this.buttonSetBitmap);
			this.Controls.Add(this.buttonClose);
			this.Controls.Add(this.buttonSetup);
			this.Controls.Add(this.buttonPause);
			this.Controls.Add(this.buttonStop);
			this.Controls.Add(this.buttonStart);
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Name = "Form2";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.Button buttonSetup;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonSetBitmap;
        private System.Windows.Forms.Button buttonClearBitmap;
        private System.Windows.Forms.Button buttonGetBitmap;
        private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button1;
	}
}

