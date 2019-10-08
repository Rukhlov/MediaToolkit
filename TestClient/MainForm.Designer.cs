namespace TestClient
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.portNumeric = new System.Windows.Forms.NumericUpDown();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.findServiceButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.remoteDesktopTextBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.hostsComboBox = new System.Windows.Forms.ComboBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button8 = new System.Windows.Forms.Button();
            this.srcWidthNumeric = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.srcHeightNumeric = new System.Windows.Forms.NumericUpDown();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.destWidthNumeric = new System.Windows.Forms.NumericUpDown();
            this.destHeightNumeric = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.portNumeric)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.panel2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.srcWidthNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.srcHeightNumeric)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.destWidthNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.destHeightNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(7, 40);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 1;
            this.button1.Text = "Play";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(125, 40);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 28);
            this.button2.TabIndex = 2;
            this.button2.Text = "Stop";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(629, 653);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(188, 28);
            this.button3.TabIndex = 3;
            this.button3.Text = "ScreenStreamer.exe";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(7, 30);
            this.button4.Margin = new System.Windows.Forms.Padding(4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(151, 28);
            this.button4.TabIndex = 4;
            this.button4.Text = "Play";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(205, 30);
            this.button5.Margin = new System.Windows.Forms.Padding(4);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(100, 28);
            this.button5.TabIndex = 5;
            this.button5.Text = "Stop";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.button5);
            this.groupBox1.Location = new System.Drawing.Point(74, 167);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(385, 80);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "TestClient";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 9;
            this.label2.Text = "Port:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "Address:";
            // 
            // portNumeric
            // 
            this.portNumeric.Location = new System.Drawing.Point(76, 56);
            this.portNumeric.Maximum = new decimal(new int[] {
            100500,
            0,
            0,
            0});
            this.portNumeric.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.portNumeric.Name = "portNumeric";
            this.portNumeric.Size = new System.Drawing.Size(123, 22);
            this.portNumeric.TabIndex = 7;
            this.portNumeric.Value = new decimal(new int[] {
            1234,
            0,
            0,
            0});
            // 
            // addressTextBox
            // 
            this.addressTextBox.Location = new System.Drawing.Point(76, 27);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(325, 22);
            this.addressTextBox.TabIndex = 6;
            this.addressTextBox.Text = "239.0.0.1";
            // 
            // findServiceButton
            // 
            this.findServiceButton.Location = new System.Drawing.Point(264, 271);
            this.findServiceButton.Margin = new System.Windows.Forms.Padding(4);
            this.findServiceButton.Name = "findServiceButton";
            this.findServiceButton.Size = new System.Drawing.Size(100, 28);
            this.findServiceButton.TabIndex = 11;
            this.findServiceButton.Text = "Find";
            this.findServiceButton.UseVisualStyleBackColor = true;
            this.findServiceButton.Click += new System.EventHandler(this.findServiceButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Location = new System.Drawing.Point(581, 53);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(274, 94);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "VLC";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.numericUpDown1);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.button6);
            this.groupBox3.Controls.Add(this.button7);
            this.groupBox3.Location = new System.Drawing.Point(55, 448);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(309, 233);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "InputSimulator";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Port:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "Address:";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(76, 57);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            100500,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(137, 22);
            this.numericUpDown1.TabIndex = 7;
            this.numericUpDown1.Value = new decimal(new int[] {
            8888,
            0,
            0,
            0});
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(76, 28);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(137, 22);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "172.16.44.10";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(7, 122);
            this.button6.Margin = new System.Windows.Forms.Padding(4);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(100, 28);
            this.button6.TabIndex = 4;
            this.button6.Text = "Start";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(16, 174);
            this.button7.Margin = new System.Windows.Forms.Padding(4);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(100, 28);
            this.button7.TabIndex = 5;
            this.button7.Text = "Stop";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.disconnectButton);
            this.panel2.Controls.Add(this.remoteDesktopTextBox);
            this.panel2.Controls.Add(this.connectButton);
            this.panel2.Location = new System.Drawing.Point(42, 328);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(344, 83);
            this.panel2.TabIndex = 22;
            // 
            // disconnectButton
            // 
            this.disconnectButton.Location = new System.Drawing.Point(231, 39);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(105, 23);
            this.disconnectButton.TabIndex = 19;
            this.disconnectButton.Text = "_Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // remoteDesktopTextBox
            // 
            this.remoteDesktopTextBox.Location = new System.Drawing.Point(14, 11);
            this.remoteDesktopTextBox.Name = "remoteDesktopTextBox";
            this.remoteDesktopTextBox.Size = new System.Drawing.Size(327, 22);
            this.remoteDesktopTextBox.TabIndex = 17;
            this.remoteDesktopTextBox.Text = "192.168.1.135";
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(123, 39);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(102, 23);
            this.connectButton.TabIndex = 18;
            this.connectButton.Text = "_Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // hostsComboBox
            // 
            this.hostsComboBox.FormattingEnabled = true;
            this.hostsComboBox.Location = new System.Drawing.Point(42, 274);
            this.hostsComboBox.Name = "hostsComboBox";
            this.hostsComboBox.Size = new System.Drawing.Size(215, 24);
            this.hostsComboBox.TabIndex = 23;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.addressTextBox);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.portNumeric);
            this.groupBox4.Location = new System.Drawing.Point(33, 21);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(451, 100);
            this.groupBox4.TabIndex = 24;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Receive From";
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(742, 479);
            this.button8.Margin = new System.Windows.Forms.Padding(4);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(100, 28);
            this.button8.TabIndex = 25;
            this.button8.Text = "_Statistics";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // srcWidthNumeric
            // 
            this.srcWidthNumeric.Location = new System.Drawing.Point(72, 31);
            this.srcWidthNumeric.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.srcWidthNumeric.Name = "srcWidthNumeric";
            this.srcWidthNumeric.Size = new System.Drawing.Size(137, 22);
            this.srcWidthNumeric.TabIndex = 26;
            this.srcWidthNumeric.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 17);
            this.label5.TabIndex = 28;
            this.label5.Text = "Width:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 61);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 17);
            this.label6.TabIndex = 30;
            this.label6.Text = "Height:";
            // 
            // srcHeightNumeric
            // 
            this.srcHeightNumeric.Location = new System.Drawing.Point(72, 59);
            this.srcHeightNumeric.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.srcHeightNumeric.Name = "srcHeightNumeric";
            this.srcHeightNumeric.Size = new System.Drawing.Size(137, 22);
            this.srcHeightNumeric.TabIndex = 29;
            this.srcHeightNumeric.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.srcWidthNumeric);
            this.groupBox5.Controls.Add(this.srcHeightNumeric);
            this.groupBox5.Location = new System.Drawing.Point(536, 167);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(239, 116);
            this.groupBox5.TabIndex = 31;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "SrcSize";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label7);
            this.groupBox6.Controls.Add(this.label8);
            this.groupBox6.Controls.Add(this.destWidthNumeric);
            this.groupBox6.Controls.Add(this.destHeightNumeric);
            this.groupBox6.Location = new System.Drawing.Point(536, 309);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(239, 116);
            this.groupBox6.TabIndex = 32;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "DestSize";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(18, 33);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 17);
            this.label7.TabIndex = 28;
            this.label7.Text = "Width:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(18, 61);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 17);
            this.label8.TabIndex = 30;
            this.label8.Text = "Height:";
            // 
            // destWidthNumeric
            // 
            this.destWidthNumeric.Location = new System.Drawing.Point(72, 31);
            this.destWidthNumeric.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.destWidthNumeric.Name = "destWidthNumeric";
            this.destWidthNumeric.Size = new System.Drawing.Size(137, 22);
            this.destWidthNumeric.TabIndex = 26;
            this.destWidthNumeric.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // destHeightNumeric
            // 
            this.destHeightNumeric.Location = new System.Drawing.Point(72, 59);
            this.destHeightNumeric.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.destHeightNumeric.Name = "destHeightNumeric";
            this.destHeightNumeric.Size = new System.Drawing.Size(137, 22);
            this.destHeightNumeric.TabIndex = 29;
            this.destHeightNumeric.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(889, 762);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.hostsComboBox);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.findServiceButton);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.portNumeric)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.srcWidthNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.srcHeightNumeric)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.destWidthNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.destHeightNumeric)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox addressTextBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown portNumeric;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button findServiceButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.TextBox remoteDesktopTextBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.ComboBox hostsComboBox;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.NumericUpDown srcWidthNumeric;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown srcHeightNumeric;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown destWidthNumeric;
        private System.Windows.Forms.NumericUpDown destHeightNumeric;
    }
}

