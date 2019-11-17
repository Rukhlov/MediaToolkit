namespace TestClient.Controls
{
    partial class ScreenCastReceiverControl
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel2 = new System.Windows.Forms.Panel();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.hostsComboBox = new System.Windows.Forms.ComboBox();
            this.findServiceButton = new System.Windows.Forms.Button();
            this.remoteDesktopTextBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.disconnectButton);
            this.panel2.Controls.Add(this.hostsComboBox);
            this.panel2.Controls.Add(this.findServiceButton);
            this.panel2.Controls.Add(this.remoteDesktopTextBox);
            this.panel2.Controls.Add(this.connectButton);
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(489, 275);
            this.panel2.TabIndex = 25;
            // 
            // disconnectButton
            // 
            this.disconnectButton.AutoSize = true;
            this.disconnectButton.Location = new System.Drawing.Point(228, 73);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(105, 27);
            this.disconnectButton.TabIndex = 19;
            this.disconnectButton.Text = "_Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // hostsComboBox
            // 
            this.hostsComboBox.FormattingEnabled = true;
            this.hostsComboBox.Location = new System.Drawing.Point(3, 3);
            this.hostsComboBox.Name = "hostsComboBox";
            this.hostsComboBox.Size = new System.Drawing.Size(330, 24);
            this.hostsComboBox.TabIndex = 26;
            this.hostsComboBox.SelectedIndexChanged += new System.EventHandler(this.hostsComboBox_SelectedIndexChanged);
            // 
            // findServiceButton
            // 
            this.findServiceButton.AutoSize = true;
            this.findServiceButton.Location = new System.Drawing.Point(340, 3);
            this.findServiceButton.Margin = new System.Windows.Forms.Padding(4);
            this.findServiceButton.Name = "findServiceButton";
            this.findServiceButton.Size = new System.Drawing.Size(100, 28);
            this.findServiceButton.TabIndex = 24;
            this.findServiceButton.Text = "Find";
            this.findServiceButton.UseVisualStyleBackColor = true;
            this.findServiceButton.Click += new System.EventHandler(this.findServiceButton_Click);
            // 
            // remoteDesktopTextBox
            // 
            this.remoteDesktopTextBox.Location = new System.Drawing.Point(3, 45);
            this.remoteDesktopTextBox.Name = "remoteDesktopTextBox";
            this.remoteDesktopTextBox.Size = new System.Drawing.Size(330, 22);
            this.remoteDesktopTextBox.TabIndex = 17;
            this.remoteDesktopTextBox.Text = "192.168.1.135";
            // 
            // connectButton
            // 
            this.connectButton.AutoSize = true;
            this.connectButton.Location = new System.Drawing.Point(120, 73);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(102, 27);
            this.connectButton.TabIndex = 18;
            this.connectButton.Text = "_Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // ScreenCastReceiverControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Name = "ScreenCastReceiverControl";
            this.Size = new System.Drawing.Size(505, 293);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.ComboBox hostsComboBox;
        private System.Windows.Forms.Button findServiceButton;
        private System.Windows.Forms.TextBox remoteDesktopTextBox;
        private System.Windows.Forms.Button connectButton;
    }
}
