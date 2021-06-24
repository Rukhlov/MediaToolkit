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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelHeight = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.labelWidth = new System.Windows.Forms.Label();
            this.labelFps = new System.Windows.Forms.Label();
            this.label = new System.Windows.Forms.Label();
            this.comboBoxVideoFiles = new System.Windows.Forms.ComboBox();
            this.comboBoxDriverType = new System.Windows.Forms.ComboBox();
            this.buttonSourceStop = new System.Windows.Forms.Button();
            this.buttonSourceStart = new System.Windows.Forms.Button();
            this.buttonDecoderStop = new System.Windows.Forms.Button();
            this.buttonDecoderStart = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
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
            this.panel2.Controls.Add(this.tableLayoutPanel1);
            this.panel2.Controls.Add(this.comboBoxVideoFiles);
            this.panel2.Controls.Add(this.comboBoxDriverType);
            this.panel2.Controls.Add(this.buttonSourceStop);
            this.panel2.Controls.Add(this.buttonSourceStart);
            this.panel2.Controls.Add(this.buttonDecoderStop);
            this.panel2.Controls.Add(this.buttonDecoderStart);
            this.panel2.Location = new System.Drawing.Point(650, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(280, 565);
            this.panel2.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.labelHeight, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelWidth, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelFps, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 67);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(138, 61);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // labelHeight
            // 
            this.labelHeight.AutoSize = true;
            this.labelHeight.Location = new System.Drawing.Point(62, 17);
            this.labelHeight.Name = "labelHeight";
            this.labelHeight.Size = new System.Drawing.Size(13, 17);
            this.labelHeight.TabIndex = 3;
            this.labelHeight.Text = "-";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Height:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Width:";
            // 
            // labelWidth
            // 
            this.labelWidth.AutoSize = true;
            this.labelWidth.Location = new System.Drawing.Point(62, 0);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new System.Drawing.Size(13, 17);
            this.labelWidth.TabIndex = 1;
            this.labelWidth.Text = "-";
            // 
            // labelFps
            // 
            this.labelFps.AutoSize = true;
            this.labelFps.Location = new System.Drawing.Point(62, 34);
            this.labelFps.Name = "labelFps";
            this.labelFps.Size = new System.Drawing.Size(13, 17);
            this.labelFps.TabIndex = 5;
            this.labelFps.Text = "-";
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.Location = new System.Drawing.Point(3, 34);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(38, 17);
            this.label.TabIndex = 4;
            this.label.Text = "FPS:";
            // 
            // comboBoxVideoFiles
            // 
            this.comboBoxVideoFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVideoFiles.FormattingEnabled = true;
            this.comboBoxVideoFiles.Location = new System.Drawing.Point(3, 25);
            this.comboBoxVideoFiles.Name = "comboBoxVideoFiles";
            this.comboBoxVideoFiles.Size = new System.Drawing.Size(274, 24);
            this.comboBoxVideoFiles.TabIndex = 5;
            this.comboBoxVideoFiles.SelectedValueChanged += new System.EventHandler(this.comboBoxVideoFiles_SelectedValueChanged);
            // 
            // comboBoxDriverType
            // 
            this.comboBoxDriverType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDriverType.FormattingEnabled = true;
            this.comboBoxDriverType.Location = new System.Drawing.Point(123, 457);
            this.comboBoxDriverType.Name = "comboBoxDriverType";
            this.comboBoxDriverType.Size = new System.Drawing.Size(121, 24);
            this.comboBoxDriverType.TabIndex = 4;
            // 
            // buttonSourceStop
            // 
            this.buttonSourceStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSourceStop.Location = new System.Drawing.Point(140, 156);
            this.buttonSourceStop.Name = "buttonSourceStop";
            this.buttonSourceStop.Size = new System.Drawing.Size(81, 25);
            this.buttonSourceStop.TabIndex = 3;
            this.buttonSourceStop.Text = "Stop";
            this.buttonSourceStop.UseVisualStyleBackColor = true;
            this.buttonSourceStop.Click += new System.EventHandler(this.buttonSourceStop_Click);
            // 
            // buttonSourceStart
            // 
            this.buttonSourceStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSourceStart.Location = new System.Drawing.Point(26, 156);
            this.buttonSourceStart.Name = "buttonSourceStart";
            this.buttonSourceStart.Size = new System.Drawing.Size(81, 25);
            this.buttonSourceStart.TabIndex = 2;
            this.buttonSourceStart.Text = "Start";
            this.buttonSourceStart.UseVisualStyleBackColor = true;
            this.buttonSourceStart.Click += new System.EventHandler(this.buttonSourceStart_Click);
            // 
            // buttonDecoderStop
            // 
            this.buttonDecoderStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDecoderStop.Location = new System.Drawing.Point(151, 522);
            this.buttonDecoderStop.Name = "buttonDecoderStop";
            this.buttonDecoderStop.Size = new System.Drawing.Size(81, 25);
            this.buttonDecoderStop.TabIndex = 1;
            this.buttonDecoderStop.Text = "Stop";
            this.buttonDecoderStop.UseVisualStyleBackColor = true;
            this.buttonDecoderStop.Click += new System.EventHandler(this.buttonDecoderStop_Click);
            // 
            // buttonDecoderStart
            // 
            this.buttonDecoderStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDecoderStart.Location = new System.Drawing.Point(25, 522);
            this.buttonDecoderStart.Name = "buttonDecoderStart";
            this.buttonDecoderStart.Size = new System.Drawing.Size(81, 25);
            this.buttonDecoderStart.TabIndex = 0;
            this.buttonDecoderStart.Text = "Run";
            this.buttonDecoderStart.UseVisualStyleBackColor = true;
            this.buttonDecoderStart.Click += new System.EventHandler(this.buttonDecoderStart_Click);
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
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

		#endregion

		private System.Windows.Forms.Panel videoPanel;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button buttonDecoderStart;
		private System.Windows.Forms.Button buttonDecoderStop;
		private System.Windows.Forms.Button buttonSourceStop;
		private System.Windows.Forms.Button buttonSourceStart;
		private System.Windows.Forms.ComboBox comboBoxDriverType;
		private System.Windows.Forms.ComboBox comboBoxVideoFiles;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label labelHeight;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelWidth;
		private System.Windows.Forms.Label labelFps;
		private System.Windows.Forms.Label label;
	}
}

