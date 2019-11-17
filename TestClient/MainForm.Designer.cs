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
            this.disconnectButton = new System.Windows.Forms.Button();
            this.remoteDesktopTextBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.hostsComboBox = new System.Windows.Forms.ComboBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.ScreenCasterPage = new System.Windows.Forms.TabPage();
            this.screenCastControl1 = new TestClient.Controls.ScreenCastReceiverControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.simpleReceiverControl1 = new TestClient.Controls.SimpleReceiverControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.audioReceiverControl1 = new TestClient.Controls.AudioReceiverControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.ScreenCasterPage.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 478);
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
            this.panel2.Controls.Add(this.disconnectButton);
            this.panel2.Controls.Add(this.remoteDesktopTextBox);
            this.panel2.Controls.Add(this.connectButton);
            this.panel2.Location = new System.Drawing.Point(25, 73);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(563, 475);
            this.panel2.TabIndex = 22;
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
            // remoteDesktopTextBox
            // 
            this.remoteDesktopTextBox.Location = new System.Drawing.Point(14, 11);
            this.remoteDesktopTextBox.Name = "remoteDesktopTextBox";
            this.remoteDesktopTextBox.Size = new System.Drawing.Size(455, 22);
            this.remoteDesktopTextBox.TabIndex = 17;
            this.remoteDesktopTextBox.Text = "192.168.1.135";
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
            // hostsComboBox
            // 
            this.hostsComboBox.FormattingEnabled = true;
            this.hostsComboBox.Location = new System.Drawing.Point(25, 28);
            this.hostsComboBox.Name = "hostsComboBox";
            this.hostsComboBox.Size = new System.Drawing.Size(371, 24);
            this.hostsComboBox.TabIndex = 23;
            this.hostsComboBox.SelectedIndexChanged += new System.EventHandler(this.hostsComboBox_SelectedIndexChanged);
            this.hostsComboBox.SelectedValueChanged += new System.EventHandler(this.hostsComboBox_SelectedValueChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.ScreenCasterPage);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(606, 442);
            this.tabControl1.TabIndex = 33;
            // 
            // ScreenCasterPage
            // 
            this.ScreenCasterPage.Controls.Add(this.screenCastControl1);
            this.ScreenCasterPage.Location = new System.Drawing.Point(4, 25);
            this.ScreenCasterPage.Name = "ScreenCasterPage";
            this.ScreenCasterPage.Padding = new System.Windows.Forms.Padding(3);
            this.ScreenCasterPage.Size = new System.Drawing.Size(598, 413);
            this.ScreenCasterPage.TabIndex = 3;
            this.ScreenCasterPage.Text = "ScreenCaster";
            this.ScreenCasterPage.UseVisualStyleBackColor = true;
            // 
            // screenCastControl1
            // 
            this.screenCastControl1.BackColor = System.Drawing.SystemColors.Info;
            this.screenCastControl1.Location = new System.Drawing.Point(6, 6);
            this.screenCastControl1.Name = "screenCastControl1";
            this.screenCastControl1.Size = new System.Drawing.Size(467, 156);
            this.screenCastControl1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.simpleReceiverControl1);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(598, 413);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "SimpleReceiver";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // simpleReceiverControl1
            // 
            this.simpleReceiverControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleReceiverControl1.Location = new System.Drawing.Point(3, 3);
            this.simpleReceiverControl1.Name = "simpleReceiverControl1";
            this.simpleReceiverControl1.Size = new System.Drawing.Size(592, 407);
            this.simpleReceiverControl1.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.audioReceiverControl1);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(598, 413);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "AudioReceiver";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // audioReceiverControl1
            // 
            this.audioReceiverControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.audioReceiverControl1.Location = new System.Drawing.Point(0, 0);
            this.audioReceiverControl1.Name = "audioReceiverControl1";
            this.audioReceiverControl1.Size = new System.Drawing.Size(598, 413);
            this.audioReceiverControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel2);
            this.tabPage1.Controls.Add(this.hostsComboBox);
            this.tabPage1.Controls.Add(this.findServiceButton);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(598, 413);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "ClientMode";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 539);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button3);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "TestClient";
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ScreenCasterPage.ResumeLayout(false);
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
        private System.Windows.Forms.TabPage tabPage3;
        private Controls.SimpleReceiverControl simpleReceiverControl1;
        private Controls.AudioReceiverControl audioReceiverControl1;
        private System.Windows.Forms.TabPage ScreenCasterPage;
        private Controls.ScreenCastReceiverControl screenCastControl1;
    }
}

