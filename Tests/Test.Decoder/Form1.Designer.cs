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
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.comboBoxVideoAdapters = new System.Windows.Forms.ComboBox();
			this.checkBoxAspectRatio = new System.Windows.Forms.CheckBox();
			this.checkBoxDebugInfo = new System.Windows.Forms.CheckBox();
			this.comboBoxDecoderTypes = new System.Windows.Forms.ComboBox();
			this.buttonDecoderStart = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.buttonSourceStart = new System.Windows.Forms.Button();
			this.comboBoxVideoFiles = new System.Windows.Forms.ComboBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.numericFps = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.labelHeight = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.labelWidth = new System.Windows.Forms.Label();
			this.labelFps = new System.Windows.Forms.Label();
			this.label = new System.Windows.Forms.Label();
			this.labelProfile = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.labelLevel = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.numericUpDownBufferSize = new System.Windows.Forms.NumericUpDown();
			this.panel2.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericFps)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferSize)).BeginInit();
			this.SuspendLayout();
			// 
			// videoPanel
			// 
			this.videoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.videoPanel.BackColor = System.Drawing.Color.Black;
			this.videoPanel.Location = new System.Drawing.Point(11, 12);
			this.videoPanel.Name = "videoPanel";
			this.videoPanel.Size = new System.Drawing.Size(822, 692);
			this.videoPanel.TabIndex = 0;
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.Controls.Add(this.groupBox2);
			this.panel2.Controls.Add(this.groupBox1);
			this.panel2.Location = new System.Drawing.Point(840, 12);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(280, 692);
			this.panel2.TabIndex = 1;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.comboBoxVideoAdapters);
			this.groupBox2.Controls.Add(this.checkBoxAspectRatio);
			this.groupBox2.Controls.Add(this.checkBoxDebugInfo);
			this.groupBox2.Controls.Add(this.comboBoxDecoderTypes);
			this.groupBox2.Controls.Add(this.buttonDecoderStart);
			this.groupBox2.Location = new System.Drawing.Point(3, 307);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(271, 248);
			this.groupBox2.TabIndex = 8;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Decoder";
			// 
			// comboBoxVideoAdapters
			// 
			this.comboBoxVideoAdapters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxVideoAdapters.FormattingEnabled = true;
			this.comboBoxVideoAdapters.Location = new System.Drawing.Point(6, 51);
			this.comboBoxVideoAdapters.Name = "comboBoxVideoAdapters";
			this.comboBoxVideoAdapters.Size = new System.Drawing.Size(259, 24);
			this.comboBoxVideoAdapters.TabIndex = 7;
			// 
			// checkBoxAspectRatio
			// 
			this.checkBoxAspectRatio.AutoSize = true;
			this.checkBoxAspectRatio.Location = new System.Drawing.Point(6, 102);
			this.checkBoxAspectRatio.Name = "checkBoxAspectRatio";
			this.checkBoxAspectRatio.Size = new System.Drawing.Size(106, 21);
			this.checkBoxAspectRatio.TabIndex = 6;
			this.checkBoxAspectRatio.Text = "AspectRatio";
			this.checkBoxAspectRatio.UseVisualStyleBackColor = true;
			this.checkBoxAspectRatio.CheckedChanged += new System.EventHandler(this.checkBoxAspectRatio_CheckedChanged);
			// 
			// checkBoxDebugInfo
			// 
			this.checkBoxDebugInfo.AutoSize = true;
			this.checkBoxDebugInfo.Location = new System.Drawing.Point(6, 129);
			this.checkBoxDebugInfo.Name = "checkBoxDebugInfo";
			this.checkBoxDebugInfo.Size = new System.Drawing.Size(136, 21);
			this.checkBoxDebugInfo.TabIndex = 5;
			this.checkBoxDebugInfo.Text = "ShowDecodeInfo";
			this.checkBoxDebugInfo.UseVisualStyleBackColor = true;
			this.checkBoxDebugInfo.CheckedChanged += new System.EventHandler(this.checkBoxDebugInfo_CheckedChanged);
			// 
			// comboBoxDecoderTypes
			// 
			this.comboBoxDecoderTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxDecoderTypes.FormattingEnabled = true;
			this.comboBoxDecoderTypes.Location = new System.Drawing.Point(6, 21);
			this.comboBoxDecoderTypes.Name = "comboBoxDecoderTypes";
			this.comboBoxDecoderTypes.Size = new System.Drawing.Size(259, 24);
			this.comboBoxDecoderTypes.TabIndex = 4;
			// 
			// buttonDecoderStart
			// 
			this.buttonDecoderStart.Location = new System.Drawing.Point(123, 205);
			this.buttonDecoderStart.Name = "buttonDecoderStart";
			this.buttonDecoderStart.Size = new System.Drawing.Size(133, 25);
			this.buttonDecoderStart.TabIndex = 0;
			this.buttonDecoderStart.Text = "Start Decoder";
			this.buttonDecoderStart.UseVisualStyleBackColor = true;
			this.buttonDecoderStart.Click += new System.EventHandler(this.buttonDecoderStart_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.buttonSourceStart);
			this.groupBox1.Controls.Add(this.comboBoxVideoFiles);
			this.groupBox1.Controls.Add(this.tableLayoutPanel1);
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(274, 301);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "H264 Source:";
			// 
			// buttonSourceStart
			// 
			this.buttonSourceStart.Location = new System.Drawing.Point(135, 261);
			this.buttonSourceStart.Name = "buttonSourceStart";
			this.buttonSourceStart.Size = new System.Drawing.Size(133, 25);
			this.buttonSourceStart.TabIndex = 2;
			this.buttonSourceStart.Text = "Start Source";
			this.buttonSourceStart.UseVisualStyleBackColor = true;
			this.buttonSourceStart.Click += new System.EventHandler(this.buttonSourceStart_Click);
			// 
			// comboBoxVideoFiles
			// 
			this.comboBoxVideoFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxVideoFiles.FormattingEnabled = true;
			this.comboBoxVideoFiles.Location = new System.Drawing.Point(6, 21);
			this.comboBoxVideoFiles.Name = "comboBoxVideoFiles";
			this.comboBoxVideoFiles.Size = new System.Drawing.Size(262, 24);
			this.comboBoxVideoFiles.TabIndex = 5;
			this.comboBoxVideoFiles.SelectedValueChanged += new System.EventHandler(this.comboBoxVideoFiles_SelectedValueChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.numericFps, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.label5, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.labelHeight, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelWidth, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelFps, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.label, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.labelProfile, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.labelLevel, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.label4, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.label6, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.numericUpDownBufferSize, 1, 6);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 51);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 7;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(164, 171);
			this.tableLayoutPanel1.TabIndex = 6;
			// 
			// numericFps
			// 
			this.numericFps.Location = new System.Drawing.Point(86, 118);
			this.numericFps.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
			this.numericFps.Name = "numericFps";
			this.numericFps.Size = new System.Drawing.Size(75, 22);
			this.numericFps.TabIndex = 7;
			this.numericFps.ValueChanged += new System.EventHandler(this.numericFps_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Location = new System.Drawing.Point(3, 115);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(77, 28);
			this.label5.TabIndex = 8;
			this.label5.Text = "FPS:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelHeight
			// 
			this.labelHeight.AutoSize = true;
			this.labelHeight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelHeight.Location = new System.Drawing.Point(86, 26);
			this.labelHeight.Margin = new System.Windows.Forms.Padding(3);
			this.labelHeight.Name = "labelHeight";
			this.labelHeight.Size = new System.Drawing.Size(75, 17);
			this.labelHeight.TabIndex = 3;
			this.labelHeight.Text = "-";
			this.labelHeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 26);
			this.label3.Margin = new System.Windows.Forms.Padding(3);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(77, 17);
			this.label3.TabIndex = 2;
			this.label3.Text = "Height:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 3);
			this.label1.Margin = new System.Windows.Forms.Padding(3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Width:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelWidth
			// 
			this.labelWidth.AutoSize = true;
			this.labelWidth.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelWidth.Location = new System.Drawing.Point(86, 3);
			this.labelWidth.Margin = new System.Windows.Forms.Padding(3);
			this.labelWidth.Name = "labelWidth";
			this.labelWidth.Size = new System.Drawing.Size(75, 17);
			this.labelWidth.TabIndex = 1;
			this.labelWidth.Text = "-";
			this.labelWidth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelFps
			// 
			this.labelFps.AutoSize = true;
			this.labelFps.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelFps.Location = new System.Drawing.Point(86, 49);
			this.labelFps.Margin = new System.Windows.Forms.Padding(3);
			this.labelFps.Name = "labelFps";
			this.labelFps.Size = new System.Drawing.Size(75, 17);
			this.labelFps.TabIndex = 5;
			this.labelFps.Text = "-";
			this.labelFps.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label.Location = new System.Drawing.Point(3, 49);
			this.label.Margin = new System.Windows.Forms.Padding(3);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(77, 17);
			this.label.TabIndex = 4;
			this.label.Text = "MaxFPS:";
			this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProfile
			// 
			this.labelProfile.AutoSize = true;
			this.labelProfile.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelProfile.Location = new System.Drawing.Point(86, 72);
			this.labelProfile.Margin = new System.Windows.Forms.Padding(3);
			this.labelProfile.Name = "labelProfile";
			this.labelProfile.Size = new System.Drawing.Size(75, 17);
			this.labelProfile.TabIndex = 7;
			this.labelProfile.Text = "-";
			this.labelProfile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(3, 72);
			this.label2.Margin = new System.Windows.Forms.Padding(3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(77, 17);
			this.label2.TabIndex = 6;
			this.label2.Text = "Profile:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelLevel
			// 
			this.labelLevel.AutoSize = true;
			this.labelLevel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelLevel.Location = new System.Drawing.Point(86, 95);
			this.labelLevel.Margin = new System.Windows.Forms.Padding(3);
			this.labelLevel.Name = "labelLevel";
			this.labelLevel.Size = new System.Drawing.Size(75, 17);
			this.labelLevel.TabIndex = 9;
			this.labelLevel.Text = "-";
			this.labelLevel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(3, 95);
			this.label4.Margin = new System.Windows.Forms.Padding(3);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(77, 17);
			this.label4.TabIndex = 8;
			this.label4.Text = "Level:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label6.Location = new System.Drawing.Point(3, 146);
			this.label6.Margin = new System.Windows.Forms.Padding(3);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(77, 22);
			this.label6.TabIndex = 10;
			this.label6.Text = "BufferSize:";
			// 
			// numericUpDownBufferSize
			// 
			this.numericUpDownBufferSize.Location = new System.Drawing.Point(86, 146);
			this.numericUpDownBufferSize.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.numericUpDownBufferSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDownBufferSize.Name = "numericUpDownBufferSize";
			this.numericUpDownBufferSize.Size = new System.Drawing.Size(75, 22);
			this.numericUpDownBufferSize.TabIndex = 11;
			this.numericUpDownBufferSize.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1131, 718);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.videoPanel);
			this.Name = "Form1";
			this.Text = "Form1";
			this.panel2.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericFps)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferSize)).EndInit();
			this.ResumeLayout(false);

        }

		#endregion

		private System.Windows.Forms.Panel videoPanel;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button buttonDecoderStart;
		private System.Windows.Forms.Button buttonSourceStart;
		private System.Windows.Forms.ComboBox comboBoxDecoderTypes;
		private System.Windows.Forms.ComboBox comboBoxVideoFiles;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label labelHeight;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelWidth;
		private System.Windows.Forms.Label labelFps;
		private System.Windows.Forms.Label label;
		private System.Windows.Forms.Label labelProfile;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label labelLevel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox checkBoxDebugInfo;
		private System.Windows.Forms.CheckBox checkBoxAspectRatio;
        private System.Windows.Forms.ComboBox comboBoxVideoAdapters;
        private System.Windows.Forms.NumericUpDown numericFps;
        private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown numericUpDownBufferSize;
	}
}

