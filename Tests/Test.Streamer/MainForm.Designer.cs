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
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitButton = new System.Windows.Forms.Button();
            this.updateNetworksButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.networkComboBox = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            //this.screenStreamerControl = new TestStreamer.Controls.ScreenStreamerControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.audioStreamerControl = new TestStreamer.Controls.AudioStreamerControl();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.webCamStreamerControl1 = new TestStreamer.Controls.WebCamStreamerControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.httpStreamerControl1 = new TestStreamer.Controls.HttpStreamerControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            //this.remoteServerControl1 = new TestStreamer.Controls.RemoteServerControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label23 = new System.Windows.Forms.Label();
            this.communicationPortNumeric = new System.Windows.Forms.NumericUpDown();
            this.contextMenu.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.communicationPortNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenu;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "ScreenStreamer";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseClick);
            // 
            // contextMenu
            // 
            this.contextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem,
            this.settingToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(115, 98);
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.startToolStripMenuItem.Text = "_Start";
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.stopToolStripMenuItem.Text = "_Stop";
            // 
            // settingToolStripMenuItem
            // 
            this.settingToolStripMenuItem.Name = "settingToolStripMenuItem";
            this.settingToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.settingToolStripMenuItem.Text = "_Debug";
            this.settingToolStripMenuItem.Click += new System.EventHandler(this.settingToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(111, 6);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(114, 22);
            this.exitMenuItem.Text = "_Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Location = new System.Drawing.Point(271, 395);
            this.exitButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(126, 26);
            this.exitButton.TabIndex = 0;
            this.exitButton.Text = "_Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // updateNetworksButton
            // 
            this.updateNetworksButton.Location = new System.Drawing.Point(293, 7);
            this.updateNetworksButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.updateNetworksButton.Name = "updateNetworksButton";
            this.updateNetworksButton.Size = new System.Drawing.Size(60, 21);
            this.updateNetworksButton.TabIndex = 18;
            this.updateNetworksButton.Text = "_Update";
            this.updateNetworksButton.UseVisualStyleBackColor = true;
            this.updateNetworksButton.Click += new System.EventHandler(this.updateNetworksButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 10);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "_Networks:";
            // 
            // networkComboBox
            // 
            this.networkComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.networkComboBox.FormattingEnabled = true;
            this.networkComboBox.Location = new System.Drawing.Point(73, 7);
            this.networkComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.networkComboBox.Name = "networkComboBox";
            this.networkComboBox.Size = new System.Drawing.Size(216, 21);
            this.networkComboBox.TabIndex = 16;
            this.networkComboBox.SelectedValueChanged += new System.EventHandler(this.networkComboBox_SelectedValueChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(13, 395);
            this.button1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(138, 26);
            this.button1.TabIndex = 20;
            this.button1.Text = "_TestClient";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(10, 82);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(392, 309);
            this.tabControl1.TabIndex = 23;
            // 
            // tabPage2
            // 
            //this.tabPage2.Controls.Add(this.screenStreamerControl);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Size = new System.Drawing.Size(384, 283);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "RtpStreamer";
            this.tabPage2.UseVisualStyleBackColor = true;
            //// 
            //// screenStreamerControl
            //// 
            //this.screenStreamerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.screenStreamerControl.Location = new System.Drawing.Point(2, 2);
            //this.screenStreamerControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            //this.screenStreamerControl.Name = "screenStreamerControl";
            //this.screenStreamerControl.Size = new System.Drawing.Size(380, 279);
            //this.screenStreamerControl.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.audioStreamerControl);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(384, 283);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "AudioStreamer";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // audioStreamerControl
            // 
            this.audioStreamerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.audioStreamerControl.Location = new System.Drawing.Point(0, 0);
            this.audioStreamerControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.audioStreamerControl.Name = "audioStreamerControl";
            this.audioStreamerControl.Size = new System.Drawing.Size(384, 283);
            this.audioStreamerControl.TabIndex = 0;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.webCamStreamerControl1);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage5.Size = new System.Drawing.Size(384, 283);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "WebCamStreamer";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // webCamStreamerControl1
            // 
            this.webCamStreamerControl1.Location = new System.Drawing.Point(7, 5);
            this.webCamStreamerControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.webCamStreamerControl1.Name = "webCamStreamerControl1";
            this.webCamStreamerControl1.Size = new System.Drawing.Size(374, 276);
            this.webCamStreamerControl1.TabIndex = 0;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.httpStreamerControl1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(384, 283);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "HttpStreamer";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // httpStreamerControl1
            // 
            this.httpStreamerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.httpStreamerControl1.Location = new System.Drawing.Point(0, 0);
            this.httpStreamerControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.httpStreamerControl1.Name = "httpStreamerControl1";
            this.httpStreamerControl1.Size = new System.Drawing.Size(384, 283);
            this.httpStreamerControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
           // this.tabPage1.Controls.Add(this.remoteServerControl1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Size = new System.Drawing.Size(384, 283);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // remoteServerControl1
            // 
            //this.remoteServerControl1.Location = new System.Drawing.Point(2, 5);
            //this.remoteServerControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            //this.remoteServerControl1.Name = "remoteServerControl1";
            //this.remoteServerControl1.Size = new System.Drawing.Size(378, 298);
            //this.remoteServerControl1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label23);
            this.panel1.Controls.Add(this.communicationPortNumeric);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.updateNetworksButton);
            this.panel1.Controls.Add(this.networkComboBox);
            this.panel1.Location = new System.Drawing.Point(15, 10);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(365, 67);
            this.panel1.TabIndex = 67;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(2, 33);
            this.label23.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(35, 13);
            this.label23.TabIndex = 20;
            this.label23.Text = "_Port:";
            // 
            // communicationPortNumeric
            // 
            this.communicationPortNumeric.Location = new System.Drawing.Point(73, 31);
            this.communicationPortNumeric.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.communicationPortNumeric.Maximum = new decimal(new int[] {
            100500,
            0,
            0,
            0});
            this.communicationPortNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.communicationPortNumeric.Name = "communicationPortNumeric";
            this.communicationPortNumeric.Size = new System.Drawing.Size(62, 20);
            this.communicationPortNumeric.TabIndex = 19;
            this.communicationPortNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(410, 431);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.exitButton);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MainForm";
            this.Text = "TestServer";
            this.contextMenu.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.communicationPortNumeric)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox networkComboBox;
        private System.Windows.Forms.Button updateNetworksButton;
        private System.Windows.Forms.ToolStripMenuItem settingToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        //private Controls.ScreenStreamerControl screenStreamerControl;
        private Controls.AudioStreamerControl audioStreamerControl;
        private Controls.HttpStreamerControl httpStreamerControl1;
        //private Controls.RemoteServerControl remoteServerControl1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.NumericUpDown communicationPortNumeric;
        private System.Windows.Forms.TabPage tabPage5;
        private Controls.WebCamStreamerControl webCamStreamerControl1;
    }
}

