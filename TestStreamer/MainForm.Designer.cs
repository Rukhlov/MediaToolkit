namespace TestStreamer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitButton = new System.Windows.Forms.Button();
            this.screensUpdateButton = new System.Windows.Forms.Button();
            this.screensComboBox = new System.Windows.Forms.ComboBox();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.fpsNumeric = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.showMouseCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.portNumeric = new System.Windows.Forms.NumericUpDown();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.previewButton = new System.Windows.Forms.Button();
            this.settingPanel = new System.Windows.Forms.Panel();
            this.updateNetworksButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.networkComboBox = new System.Windows.Forms.ComboBox();
            this.inputSimulatorCheckBox = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.stopRemoteServButton = new System.Windows.Forms.Button();
            this.startRemoteServButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.contextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.portNumeric)).BeginInit();
            this.settingPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenu;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "ScreenStreamer";
            this.notifyIcon.Visible = true;
            // 
            // contextMenu
            // 
            this.contextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripSeparator1,
            this.exitMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(125, 82);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(124, 24);
            this.toolStripMenuItem1.Text = "_TEST1";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(124, 24);
            this.toolStripMenuItem2.Text = "_TEST2";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(121, 6);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(124, 24);
            this.exitMenuItem.Text = "_Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // exitButton
            // 
            this.exitButton.Location = new System.Drawing.Point(692, 405);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(157, 33);
            this.exitButton.TabIndex = 0;
            this.exitButton.Text = "_Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // screensUpdateButton
            // 
            this.screensUpdateButton.Location = new System.Drawing.Point(294, 78);
            this.screensUpdateButton.Name = "screensUpdateButton";
            this.screensUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.screensUpdateButton.TabIndex = 1;
            this.screensUpdateButton.Text = "_Update";
            this.screensUpdateButton.UseVisualStyleBackColor = true;
            this.screensUpdateButton.Click += new System.EventHandler(this.screensUpdateButton_Click);
            // 
            // screensComboBox
            // 
            this.screensComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.screensComboBox.FormattingEnabled = true;
            this.screensComboBox.Location = new System.Drawing.Point(87, 78);
            this.screensComboBox.Name = "screensComboBox";
            this.screensComboBox.Size = new System.Drawing.Size(201, 24);
            this.screensComboBox.TabIndex = 2;
            this.screensComboBox.SelectedValueChanged += new System.EventHandler(this.screensComboBox_SelectedValueChanged);
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(28, 351);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(177, 35);
            this.startButton.TabIndex = 3;
            this.startButton.Text = "_Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(220, 351);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(107, 35);
            this.stopButton.TabIndex = 4;
            this.stopButton.Text = "_Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // fpsNumeric
            // 
            this.fpsNumeric.Location = new System.Drawing.Point(67, 184);
            this.fpsNumeric.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.fpsNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.fpsNumeric.Name = "fpsNumeric";
            this.fpsNumeric.Size = new System.Drawing.Size(47, 22);
            this.fpsNumeric.TabIndex = 5;
            this.fpsNumeric.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 186);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "FPS:";
            // 
            // showMouseCheckBox
            // 
            this.showMouseCheckBox.AutoSize = true;
            this.showMouseCheckBox.Checked = true;
            this.showMouseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showMouseCheckBox.Location = new System.Drawing.Point(26, 237);
            this.showMouseCheckBox.Name = "showMouseCheckBox";
            this.showMouseCheckBox.Size = new System.Drawing.Size(118, 21);
            this.showMouseCheckBox.TabIndex = 7;
            this.showMouseCheckBox.Text = "_Show Mouse";
            this.showMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 152);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 13;
            this.label2.Text = "Port:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 17);
            this.label3.TabIndex = 12;
            this.label3.Text = "Address:";
            // 
            // portNumeric
            // 
            this.portNumeric.Location = new System.Drawing.Point(93, 147);
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
            this.portNumeric.Size = new System.Drawing.Size(146, 22);
            this.portNumeric.TabIndex = 11;
            this.portNumeric.Value = new decimal(new int[] {
            1234,
            0,
            0,
            0});
            // 
            // addressTextBox
            // 
            this.addressTextBox.Location = new System.Drawing.Point(93, 118);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(146, 22);
            this.addressTextBox.TabIndex = 10;
            this.addressTextBox.Text = "239.0.0.1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 17);
            this.label4.TabIndex = 14;
            this.label4.Text = "Display:";
            // 
            // previewButton
            // 
            this.previewButton.Location = new System.Drawing.Point(405, 351);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(97, 35);
            this.previewButton.TabIndex = 15;
            this.previewButton.Text = "_Preview";
            this.previewButton.UseVisualStyleBackColor = true;
            this.previewButton.Click += new System.EventHandler(this.previewButton_Click);
            // 
            // settingPanel
            // 
            this.settingPanel.Controls.Add(this.updateNetworksButton);
            this.settingPanel.Controls.Add(this.label5);
            this.settingPanel.Controls.Add(this.networkComboBox);
            this.settingPanel.Controls.Add(this.inputSimulatorCheckBox);
            this.settingPanel.Controls.Add(this.label4);
            this.settingPanel.Controls.Add(this.screensUpdateButton);
            this.settingPanel.Controls.Add(this.screensComboBox);
            this.settingPanel.Controls.Add(this.fpsNumeric);
            this.settingPanel.Controls.Add(this.label2);
            this.settingPanel.Controls.Add(this.label1);
            this.settingPanel.Controls.Add(this.label3);
            this.settingPanel.Controls.Add(this.showMouseCheckBox);
            this.settingPanel.Controls.Add(this.portNumeric);
            this.settingPanel.Controls.Add(this.addressTextBox);
            this.settingPanel.Location = new System.Drawing.Point(12, 12);
            this.settingPanel.Name = "settingPanel";
            this.settingPanel.Size = new System.Drawing.Size(461, 333);
            this.settingPanel.TabIndex = 16;
            // 
            // updateNetworksButton
            // 
            this.updateNetworksButton.Location = new System.Drawing.Point(339, 3);
            this.updateNetworksButton.Name = "updateNetworksButton";
            this.updateNetworksButton.Size = new System.Drawing.Size(75, 23);
            this.updateNetworksButton.TabIndex = 18;
            this.updateNetworksButton.Text = "_Update";
            this.updateNetworksButton.UseVisualStyleBackColor = true;
            this.updateNetworksButton.Click += new System.EventHandler(this.updateNetworksButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 17);
            this.label5.TabIndex = 17;
            this.label5.Text = "_Networks:";
            // 
            // networkComboBox
            // 
            this.networkComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.networkComboBox.FormattingEnabled = true;
            this.networkComboBox.Location = new System.Drawing.Point(87, 3);
            this.networkComboBox.Name = "networkComboBox";
            this.networkComboBox.Size = new System.Drawing.Size(246, 24);
            this.networkComboBox.TabIndex = 16;
            this.networkComboBox.SelectedValueChanged += new System.EventHandler(this.networkComboBox_SelectedValueChanged);
            // 
            // inputSimulatorCheckBox
            // 
            this.inputSimulatorCheckBox.AutoSize = true;
            this.inputSimulatorCheckBox.Checked = true;
            this.inputSimulatorCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.inputSimulatorCheckBox.Location = new System.Drawing.Point(26, 273);
            this.inputSimulatorCheckBox.Name = "inputSimulatorCheckBox";
            this.inputSimulatorCheckBox.Size = new System.Drawing.Size(128, 21);
            this.inputSimulatorCheckBox.TabIndex = 15;
            this.inputSimulatorCheckBox.Text = "_InputSimulator";
            this.inputSimulatorCheckBox.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(14, 11);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(269, 22);
            this.textBox1.TabIndex = 17;
            this.textBox1.Text = "net.tcp://localhost/RemoteDesktop";
            // 
            // stopRemoteServButton
            // 
            this.stopRemoteServButton.Location = new System.Drawing.Point(208, 39);
            this.stopRemoteServButton.Name = "stopRemoteServButton";
            this.stopRemoteServButton.Size = new System.Drawing.Size(75, 23);
            this.stopRemoteServButton.TabIndex = 19;
            this.stopRemoteServButton.Text = "_Stop";
            this.stopRemoteServButton.UseVisualStyleBackColor = true;
            this.stopRemoteServButton.Click += new System.EventHandler(this.stopRemoteServButton_Click);
            // 
            // startRemoteServButton
            // 
            this.startRemoteServButton.Location = new System.Drawing.Point(127, 39);
            this.startRemoteServButton.Name = "startRemoteServButton";
            this.startRemoteServButton.Size = new System.Drawing.Size(75, 23);
            this.startRemoteServButton.TabIndex = 18;
            this.startRemoteServButton.Text = "_Start";
            this.startRemoteServButton.UseVisualStyleBackColor = true;
            this.startRemoteServButton.Click += new System.EventHandler(this.startRemoteServButton_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 405);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(172, 33);
            this.button1.TabIndex = 20;
            this.button1.Text = "_TestClient";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.stopRemoteServButton);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.startRemoteServButton);
            this.panel1.Location = new System.Drawing.Point(545, 147);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(295, 83);
            this.panel1.TabIndex = 21;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(874, 450);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.settingPanel);
            this.Controls.Add(this.previewButton);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.exitButton);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.contextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.portNumeric)).EndInit();
            this.settingPanel.ResumeLayout(false);
            this.settingPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.Button screensUpdateButton;
        private System.Windows.Forms.ComboBox screensComboBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.NumericUpDown fpsNumeric;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox showMouseCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown portNumeric;
        private System.Windows.Forms.TextBox addressTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button previewButton;
        private System.Windows.Forms.Panel settingPanel;
        private System.Windows.Forms.CheckBox inputSimulatorCheckBox;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button stopRemoteServButton;
        private System.Windows.Forms.Button startRemoteServButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox networkComboBox;
        private System.Windows.Forms.Button updateNetworksButton;
    }
}

