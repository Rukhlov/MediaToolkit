namespace Test.Encoder.DDATest
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
            this.buttonInit = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.videoPanel = new System.Windows.Forms.Panel();
            this.buttonLoadTexture = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonPresent = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonInit
            // 
            this.buttonInit.Location = new System.Drawing.Point(22, 24);
            this.buttonInit.Name = "buttonInit";
            this.buttonInit.Size = new System.Drawing.Size(105, 36);
            this.buttonInit.TabIndex = 1;
            this.buttonInit.Text = "Init";
            this.buttonInit.UseVisualStyleBackColor = true;
            this.buttonInit.Click += new System.EventHandler(this.buttonInit_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(22, 508);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(105, 36);
            this.button6.TabIndex = 12;
            this.button6.Text = "StartClient";
            this.button6.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(22, 421);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(105, 36);
            this.button5.TabIndex = 11;
            this.button5.Text = "LogDxInfo";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // videoPanel
            // 
            this.videoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoPanel.BackColor = System.Drawing.Color.Lime;
            this.videoPanel.Location = new System.Drawing.Point(167, 12);
            this.videoPanel.Name = "videoPanel";
            this.videoPanel.Size = new System.Drawing.Size(728, 572);
            this.videoPanel.TabIndex = 10;
            // 
            // buttonLoadTexture
            // 
            this.buttonLoadTexture.Location = new System.Drawing.Point(22, 83);
            this.buttonLoadTexture.Name = "buttonLoadTexture";
            this.buttonLoadTexture.Size = new System.Drawing.Size(105, 36);
            this.buttonLoadTexture.TabIndex = 9;
            this.buttonLoadTexture.Text = "LoadTexture";
            this.buttonLoadTexture.UseVisualStyleBackColor = true;
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(22, 253);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(105, 36);
            this.buttonClose.TabIndex = 8;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonPresent
            // 
            this.buttonPresent.Location = new System.Drawing.Point(22, 176);
            this.buttonPresent.Name = "buttonPresent";
            this.buttonPresent.Size = new System.Drawing.Size(105, 36);
            this.buttonPresent.TabIndex = 7;
            this.buttonPresent.Text = "Present";
            this.buttonPresent.UseVisualStyleBackColor = true;
            this.buttonPresent.Click += new System.EventHandler(this.buttonPresent_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(907, 596);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.videoPanel);
            this.Controls.Add(this.buttonLoadTexture);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonPresent);
            this.Controls.Add(this.buttonInit);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonInit;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Panel videoPanel;
        private System.Windows.Forms.Button buttonLoadTexture;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonPresent;
    }
}