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
            this.labelCpuUsage = new System.Windows.Forms.Label();
            this.labelStats = new System.Windows.Forms.Label();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanel1.Controls.Add(this.labelCpuUsage);
            this.flowLayoutPanel1.Controls.Add(this.labelStats);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(263, 262);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // labelCpuUsage
            // 
            this.labelCpuUsage.AutoSize = true;
            this.labelCpuUsage.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelCpuUsage.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelCpuUsage.ForeColor = System.Drawing.Color.Red;
            this.labelCpuUsage.Location = new System.Drawing.Point(3, 0);
            this.labelCpuUsage.Name = "labelCpuUsage";
            this.labelCpuUsage.Size = new System.Drawing.Size(36, 28);
            this.labelCpuUsage.TabIndex = 8;
            this.labelCpuUsage.Text = "---";
            this.labelCpuUsage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelStats
            // 
            this.labelStats.AutoSize = true;
            this.labelStats.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelStats.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelStats.ForeColor = System.Drawing.Color.Red;
            this.labelStats.Location = new System.Drawing.Point(3, 31);
            this.labelStats.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelStats.Name = "labelStats";
            this.labelStats.Size = new System.Drawing.Size(31, 23);
            this.labelStats.TabIndex = 5;
            this.labelStats.Text = "---";
            this.labelStats.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
        private System.Windows.Forms.Label labelStats;
        private System.Windows.Forms.Label labelCpuUsage;
    }
}