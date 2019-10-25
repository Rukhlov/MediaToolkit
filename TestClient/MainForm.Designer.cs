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
            this.button3 = new System.Windows.Forms.Button();
            this.findServiceButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.inputSimulatorCheckBox2 = new System.Windows.Forms.CheckBox();
            this.stopButton2 = new System.Windows.Forms.Button();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.playButton2 = new System.Windows.Forms.Button();
            this.remoteDesktopTextBox = new System.Windows.Forms.TextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.srcXNumeric = new System.Windows.Forms.NumericUpDown();
            this.srcYNumeric = new System.Windows.Forms.NumericUpDown();
            this.screensUpdateButton2 = new System.Windows.Forms.Button();
            this.connectButton = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.fpsNumeric2 = new System.Windows.Forms.NumericUpDown();
            this.screensComboBox2 = new System.Windows.Forms.ComboBox();
            this.hostsComboBox = new System.Windows.Forms.ComboBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.simpleReceiverControl1 = new TestClient.Controls.SimpleReceiverControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.audioReceiverControl1 = new TestClient.Controls.AudioReceiverControl();
            this.panel2.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.srcXNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.srcYNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric2)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(16, 705);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(188, 28);
            this.button3.TabIndex = 3;
            this.button3.Text = "TestServer.exe";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // findServiceButton
            // 
            this.findServiceButton.Location = new System.Drawing.Point(426, 24);
            this.findServiceButton.Margin = new System.Windows.Forms.Padding(4);
            this.findServiceButton.Name = "findServiceButton";
            this.findServiceButton.Size = new System.Drawing.Size(100, 28);
            this.findServiceButton.TabIndex = 11;
            this.findServiceButton.Text = "Find";
            this.findServiceButton.UseVisualStyleBackColor = true;
            this.findServiceButton.Click += new System.EventHandler(this.findServiceButton_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.inputSimulatorCheckBox2);
            this.panel2.Controls.Add(this.stopButton2);
            this.panel2.Controls.Add(this.disconnectButton);
            this.panel2.Controls.Add(this.playButton2);
            this.panel2.Controls.Add(this.remoteDesktopTextBox);
            this.panel2.Controls.Add(this.groupBox7);
            this.panel2.Controls.Add(this.label15);
            this.panel2.Controls.Add(this.groupBox5);
            this.panel2.Controls.Add(this.screensUpdateButton2);
            this.panel2.Controls.Add(this.connectButton);
            this.panel2.Controls.Add(this.label16);
            this.panel2.Controls.Add(this.fpsNumeric2);
            this.panel2.Controls.Add(this.screensComboBox2);
            this.panel2.Location = new System.Drawing.Point(25, 73);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(563, 475);
            this.panel2.TabIndex = 22;
            // 
            // inputSimulatorCheckBox2
            // 
            this.inputSimulatorCheckBox2.AutoSize = true;
            this.inputSimulatorCheckBox2.Checked = true;
            this.inputSimulatorCheckBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.inputSimulatorCheckBox2.Enabled = false;
            this.inputSimulatorCheckBox2.Location = new System.Drawing.Point(312, 266);
            this.inputSimulatorCheckBox2.Name = "inputSimulatorCheckBox2";
            this.inputSimulatorCheckBox2.Size = new System.Drawing.Size(128, 21);
            this.inputSimulatorCheckBox2.TabIndex = 44;
            this.inputSimulatorCheckBox2.Text = "_InputSimulator";
            this.inputSimulatorCheckBox2.UseVisualStyleBackColor = true;
            // 
            // stopButton2
            // 
            this.stopButton2.Location = new System.Drawing.Point(153, 348);
            this.stopButton2.Name = "stopButton2";
            this.stopButton2.Size = new System.Drawing.Size(105, 23);
            this.stopButton2.TabIndex = 43;
            this.stopButton2.Text = "_Stop";
            this.stopButton2.UseVisualStyleBackColor = true;
            // 
            // disconnectButton
            // 
            this.disconnectButton.Location = new System.Drawing.Point(364, 39);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(105, 23);
            this.disconnectButton.TabIndex = 19;
            this.disconnectButton.Text = "_Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // playButton2
            // 
            this.playButton2.Location = new System.Drawing.Point(36, 348);
            this.playButton2.Name = "playButton2";
            this.playButton2.Size = new System.Drawing.Size(102, 23);
            this.playButton2.TabIndex = 42;
            this.playButton2.Text = "_Play";
            this.playButton2.UseVisualStyleBackColor = true;
            // 
            // remoteDesktopTextBox
            // 
            this.remoteDesktopTextBox.Location = new System.Drawing.Point(14, 11);
            this.remoteDesktopTextBox.Name = "remoteDesktopTextBox";
            this.remoteDesktopTextBox.Size = new System.Drawing.Size(455, 22);
            this.remoteDesktopTextBox.TabIndex = 17;
            this.remoteDesktopTextBox.Text = "192.168.1.135";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label11);
            this.groupBox7.Controls.Add(this.label12);
            this.groupBox7.Controls.Add(this.numericUpDown2);
            this.groupBox7.Controls.Add(this.numericUpDown3);
            this.groupBox7.Location = new System.Drawing.Point(312, 131);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(239, 116);
            this.groupBox7.TabIndex = 36;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "DestSize";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(18, 33);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(48, 17);
            this.label11.TabIndex = 28;
            this.label11.Text = "Width:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(18, 61);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(53, 17);
            this.label12.TabIndex = 30;
            this.label12.Text = "Height:";
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(72, 31);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(137, 22);
            this.numericUpDown2.TabIndex = 26;
            this.numericUpDown2.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.Location = new System.Drawing.Point(72, 59);
            this.numericUpDown3.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(137, 22);
            this.numericUpDown3.TabIndex = 29;
            this.numericUpDown3.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(21, 88);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(58, 17);
            this.label15.TabIndex = 41;
            this.label15.Text = "Display:";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label13);
            this.groupBox5.Controls.Add(this.label14);
            this.groupBox5.Controls.Add(this.numericUpDown4);
            this.groupBox5.Controls.Add(this.numericUpDown5);
            this.groupBox5.Controls.Add(this.label9);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.srcXNumeric);
            this.groupBox5.Controls.Add(this.srcYNumeric);
            this.groupBox5.Location = new System.Drawing.Point(24, 158);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(239, 154);
            this.groupBox5.TabIndex = 33;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "SrcRect";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(18, 95);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(48, 17);
            this.label13.TabIndex = 41;
            this.label13.Text = "Width:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(18, 123);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(53, 17);
            this.label14.TabIndex = 43;
            this.label14.Text = "Height:";
            // 
            // numericUpDown4
            // 
            this.numericUpDown4.Location = new System.Drawing.Point(72, 93);
            this.numericUpDown4.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.numericUpDown4.Name = "numericUpDown4";
            this.numericUpDown4.Size = new System.Drawing.Size(137, 22);
            this.numericUpDown4.TabIndex = 40;
            this.numericUpDown4.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // numericUpDown5
            // 
            this.numericUpDown5.Location = new System.Drawing.Point(72, 121);
            this.numericUpDown5.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.numericUpDown5.Name = "numericUpDown5";
            this.numericUpDown5.Size = new System.Drawing.Size(137, 22);
            this.numericUpDown5.TabIndex = 42;
            this.numericUpDown5.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(18, 28);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(21, 17);
            this.label9.TabIndex = 32;
            this.label9.Text = "X:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(18, 56);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(21, 17);
            this.label10.TabIndex = 34;
            this.label10.Text = "Y:";
            // 
            // srcXNumeric
            // 
            this.srcXNumeric.Location = new System.Drawing.Point(72, 26);
            this.srcXNumeric.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.srcXNumeric.Minimum = new decimal(new int[] {
            8128,
            0,
            0,
            -2147483648});
            this.srcXNumeric.Name = "srcXNumeric";
            this.srcXNumeric.Size = new System.Drawing.Size(137, 22);
            this.srcXNumeric.TabIndex = 31;
            // 
            // srcYNumeric
            // 
            this.srcYNumeric.Location = new System.Drawing.Point(72, 54);
            this.srcYNumeric.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.srcYNumeric.Minimum = new decimal(new int[] {
            8128,
            0,
            0,
            -2147483648});
            this.srcYNumeric.Name = "srcYNumeric";
            this.srcYNumeric.Size = new System.Drawing.Size(137, 22);
            this.srcYNumeric.TabIndex = 33;
            // 
            // screensUpdateButton2
            // 
            this.screensUpdateButton2.Enabled = false;
            this.screensUpdateButton2.Location = new System.Drawing.Point(292, 85);
            this.screensUpdateButton2.Name = "screensUpdateButton2";
            this.screensUpdateButton2.Size = new System.Drawing.Size(75, 23);
            this.screensUpdateButton2.TabIndex = 39;
            this.screensUpdateButton2.Text = "_Update";
            this.screensUpdateButton2.UseVisualStyleBackColor = true;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(256, 39);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(102, 23);
            this.connectButton.TabIndex = 18;
            this.connectButton.Text = "_Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(21, 117);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(38, 17);
            this.label16.TabIndex = 38;
            this.label16.Text = "FPS:";
            // 
            // fpsNumeric2
            // 
            this.fpsNumeric2.Enabled = false;
            this.fpsNumeric2.Location = new System.Drawing.Point(85, 115);
            this.fpsNumeric2.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.fpsNumeric2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.fpsNumeric2.Name = "fpsNumeric2";
            this.fpsNumeric2.Size = new System.Drawing.Size(201, 22);
            this.fpsNumeric2.TabIndex = 37;
            this.fpsNumeric2.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // screensComboBox2
            // 
            this.screensComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.screensComboBox2.Enabled = false;
            this.screensComboBox2.FormattingEnabled = true;
            this.screensComboBox2.Location = new System.Drawing.Point(85, 85);
            this.screensComboBox2.Name = "screensComboBox2";
            this.screensComboBox2.Size = new System.Drawing.Size(201, 24);
            this.screensComboBox2.TabIndex = 40;
            // 
            // hostsComboBox
            // 
            this.hostsComboBox.FormattingEnabled = true;
            this.hostsComboBox.Location = new System.Drawing.Point(25, 28);
            this.hostsComboBox.Name = "hostsComboBox";
            this.hostsComboBox.Size = new System.Drawing.Size(371, 24);
            this.hostsComboBox.TabIndex = 23;
            this.hostsComboBox.SelectedValueChanged += new System.EventHandler(this.hostsComboBox_SelectedValueChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(654, 613);
            this.tabControl1.TabIndex = 33;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.simpleReceiverControl1);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(646, 584);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "SimpleReceiver";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // simpleReceiverControl1
            // 
            this.simpleReceiverControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleReceiverControl1.Location = new System.Drawing.Point(3, 3);
            this.simpleReceiverControl1.Name = "simpleReceiverControl1";
            this.simpleReceiverControl1.Size = new System.Drawing.Size(640, 578);
            this.simpleReceiverControl1.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.audioReceiverControl1);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(646, 584);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "AudioReceiver";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel2);
            this.tabPage1.Controls.Add(this.hostsComboBox);
            this.tabPage1.Controls.Add(this.findServiceButton);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(646, 584);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "ClientMode";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // audioReceiverControl1
            // 
            this.audioReceiverControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.audioReceiverControl1.Location = new System.Drawing.Point(0, 0);
            this.audioReceiverControl1.Name = "audioReceiverControl1";
            this.audioReceiverControl1.Size = new System.Drawing.Size(646, 584);
            this.audioReceiverControl1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 762);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button3);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "TestClient";
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.srcXNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.srcYNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric2)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button findServiceButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.TextBox remoteDesktopTextBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.ComboBox hostsComboBox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown srcXNumeric;
        private System.Windows.Forms.NumericUpDown srcYNumeric;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown numericUpDown4;
        private System.Windows.Forms.NumericUpDown numericUpDown5;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button screensUpdateButton2;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox screensComboBox2;
        private System.Windows.Forms.NumericUpDown fpsNumeric2;
        private System.Windows.Forms.Button stopButton2;
        private System.Windows.Forms.Button playButton2;
        private System.Windows.Forms.CheckBox inputSimulatorCheckBox2;
        private System.Windows.Forms.TabPage tabPage3;
        private Controls.SimpleReceiverControl simpleReceiverControl1;
        private Controls.AudioReceiverControl audioReceiverControl1;
    }
}

