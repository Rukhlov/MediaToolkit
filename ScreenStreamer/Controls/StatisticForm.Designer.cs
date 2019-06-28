namespace ScreenStreamer.Controls
{
    partial class StatisticForm
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.labelTime = new System.Windows.Forms.Label();
            this.labelCpuUsage = new System.Windows.Forms.Label();
            this.labelFps = new System.Windows.Forms.Label();
            this.labelFramesCount = new System.Windows.Forms.Label();
            this.labelBytesPerSec = new System.Windows.Forms.Label();
            this.labelTotalBytes = new System.Windows.Forms.Label();
            this.labelRtpTotal = new System.Windows.Forms.Label();
            this.labelRtpCount = new System.Windows.Forms.Label();
            this.labelRtpBytesPerSec = new System.Windows.Forms.Label();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanel1.Controls.Add(this.labelTime);
            this.flowLayoutPanel1.Controls.Add(this.labelCpuUsage);
            this.flowLayoutPanel1.Controls.Add(this.labelFps);
            this.flowLayoutPanel1.Controls.Add(this.labelFramesCount);
            this.flowLayoutPanel1.Controls.Add(this.labelBytesPerSec);
            this.flowLayoutPanel1.Controls.Add(this.labelTotalBytes);
            this.flowLayoutPanel1.Controls.Add(this.labelRtpCount);
            this.flowLayoutPanel1.Controls.Add(this.labelRtpBytesPerSec);
            this.flowLayoutPanel1.Controls.Add(this.labelRtpTotal);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(263, 262);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // labelTime
            // 
            this.labelTime.AutoSize = true;
            this.labelTime.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelTime.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelTime.ForeColor = System.Drawing.Color.Red;
            this.labelTime.Location = new System.Drawing.Point(3, 0);
            this.labelTime.Margin = new System.Windows.Forms.Padding(3, 0, 3, 1);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(31, 23);
            this.labelTime.TabIndex = 6;
            this.labelTime.Text = "---";
            this.labelTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCpuUsage
            // 
            this.labelCpuUsage.AutoSize = true;
            this.labelCpuUsage.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelCpuUsage.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelCpuUsage.ForeColor = System.Drawing.Color.Red;
            this.labelCpuUsage.Location = new System.Drawing.Point(3, 24);
            this.labelCpuUsage.Margin = new System.Windows.Forms.Padding(3, 0, 3, 1);
            this.labelCpuUsage.Name = "labelCpuUsage";
            this.labelCpuUsage.Size = new System.Drawing.Size(31, 23);
            this.labelCpuUsage.TabIndex = 8;
            this.labelCpuUsage.Text = "---";
            this.labelCpuUsage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelFps
            // 
            this.labelFps.AutoSize = true;
            this.labelFps.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelFps.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelFps.ForeColor = System.Drawing.Color.Red;
            this.labelFps.Location = new System.Drawing.Point(3, 48);
            this.labelFps.Margin = new System.Windows.Forms.Padding(3, 0, 3, 1);
            this.labelFps.Name = "labelFps";
            this.labelFps.Size = new System.Drawing.Size(65, 23);
            this.labelFps.TabIndex = 7;
            this.labelFps.Text = "--- FPS";
            this.labelFps.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelFramesCount
            // 
            this.labelFramesCount.AutoSize = true;
            this.labelFramesCount.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelFramesCount.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelFramesCount.ForeColor = System.Drawing.Color.Red;
            this.labelFramesCount.Location = new System.Drawing.Point(3, 77);
            this.labelFramesCount.Margin = new System.Windows.Forms.Padding(3, 5, 3, 1);
            this.labelFramesCount.Name = "labelFramesCount";
            this.labelFramesCount.Size = new System.Drawing.Size(93, 23);
            this.labelFramesCount.TabIndex = 3;
            this.labelFramesCount.Text = "--- Frames";
            this.labelFramesCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelBytesPerSec
            // 
            this.labelBytesPerSec.AutoSize = true;
            this.labelBytesPerSec.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelBytesPerSec.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelBytesPerSec.ForeColor = System.Drawing.Color.Red;
            this.labelBytesPerSec.Location = new System.Drawing.Point(3, 101);
            this.labelBytesPerSec.Margin = new System.Windows.Forms.Padding(3, 0, 3, 1);
            this.labelBytesPerSec.Name = "labelBytesPerSec";
            this.labelBytesPerSec.Size = new System.Drawing.Size(98, 23);
            this.labelBytesPerSec.TabIndex = 4;
            this.labelBytesPerSec.Text = "--- KByte/s";
            this.labelBytesPerSec.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTotalBytes
            // 
            this.labelTotalBytes.AutoSize = true;
            this.labelTotalBytes.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelTotalBytes.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelTotalBytes.ForeColor = System.Drawing.Color.Red;
            this.labelTotalBytes.Location = new System.Drawing.Point(3, 125);
            this.labelTotalBytes.Margin = new System.Windows.Forms.Padding(3, 0, 3, 1);
            this.labelTotalBytes.Name = "labelTotalBytes";
            this.labelTotalBytes.Size = new System.Drawing.Size(88, 23);
            this.labelTotalBytes.TabIndex = 5;
            this.labelTotalBytes.Text = "--- MByte";
            this.labelTotalBytes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelRtpTotal
            // 
            this.labelRtpTotal.AutoSize = true;
            this.labelRtpTotal.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelRtpTotal.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelRtpTotal.ForeColor = System.Drawing.Color.Red;
            this.labelRtpTotal.Location = new System.Drawing.Point(3, 202);
            this.labelRtpTotal.Margin = new System.Windows.Forms.Padding(3, 0, 3, 1);
            this.labelRtpTotal.Name = "labelRtpTotal";
            this.labelRtpTotal.Size = new System.Drawing.Size(88, 23);
            this.labelRtpTotal.TabIndex = 9;
            this.labelRtpTotal.Text = "--- MByte";
            this.labelRtpTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelRtpCount
            // 
            this.labelRtpCount.AutoSize = true;
            this.labelRtpCount.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelRtpCount.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelRtpCount.ForeColor = System.Drawing.Color.Red;
            this.labelRtpCount.Location = new System.Drawing.Point(3, 154);
            this.labelRtpCount.Margin = new System.Windows.Forms.Padding(3, 5, 3, 1);
            this.labelRtpCount.Name = "labelRtpCount";
            this.labelRtpCount.Size = new System.Drawing.Size(96, 23);
            this.labelRtpCount.TabIndex = 10;
            this.labelRtpCount.Text = "--- Packets";
            this.labelRtpCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelRtpBytesPerSec
            // 
            this.labelRtpBytesPerSec.AutoSize = true;
            this.labelRtpBytesPerSec.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelRtpBytesPerSec.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelRtpBytesPerSec.ForeColor = System.Drawing.Color.Red;
            this.labelRtpBytesPerSec.Location = new System.Drawing.Point(3, 178);
            this.labelRtpBytesPerSec.Margin = new System.Windows.Forms.Padding(3, 0, 3, 1);
            this.labelRtpBytesPerSec.Name = "labelRtpBytesPerSec";
            this.labelRtpBytesPerSec.Size = new System.Drawing.Size(98, 23);
            this.labelRtpBytesPerSec.TabIndex = 11;
            this.labelRtpBytesPerSec.Text = "--- KByte/s";
            this.labelRtpBytesPerSec.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StatisticForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(263, 262);
            this.Controls.Add(this.flowLayoutPanel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "StatisticForm";
            this.Opacity = 0.7D;
            this.Text = "Form1";
            this.TransparencyKey = System.Drawing.Color.Transparent;
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label labelFramesCount;
        private System.Windows.Forms.Label labelBytesPerSec;
        private System.Windows.Forms.Label labelTotalBytes;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.Label labelFps;
        private System.Windows.Forms.Label labelCpuUsage;
        private System.Windows.Forms.Label labelRtpCount;
        private System.Windows.Forms.Label labelRtpBytesPerSec;
        private System.Windows.Forms.Label labelRtpTotal;
    }
}