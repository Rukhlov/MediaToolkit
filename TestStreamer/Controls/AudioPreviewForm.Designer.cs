namespace TestStreamer.Controls
{
    partial class AudioPreviewForm
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
            this.waveformPainter1 = new NAudio.Gui.WaveformPainter();
            this.SuspendLayout();
            // 
            // waveformPainter1
            // 
            this.waveformPainter1.BackColor = System.Drawing.SystemColors.Info;
            this.waveformPainter1.Location = new System.Drawing.Point(12, 12);
            this.waveformPainter1.Name = "waveformPainter1";
            this.waveformPainter1.Size = new System.Drawing.Size(705, 102);
            this.waveformPainter1.TabIndex = 0;
            this.waveformPainter1.Text = "waveformPainter1";
            // 
            // AudioPreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(729, 433);
            this.Controls.Add(this.waveformPainter1);
            this.Name = "AudioPreviewForm";
            this.Text = "AudioPreviewForm";
            this.ResumeLayout(false);

        }

        #endregion

        private NAudio.Gui.WaveformPainter waveformPainter1;
    }
}