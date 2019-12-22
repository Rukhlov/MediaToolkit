namespace MediaToolkit.UI
{
    partial class ScreenCastControl
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
            this.mainPanel = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.controlPanel = new System.Windows.Forms.Panel();
            this.hostsComboBox = new System.Windows.Forms.ComboBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.hostAddressTextBox = new System.Windows.Forms.TextBox();
            this.findServiceButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.showDetailsButton = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.wpfRemoteControl = new MediaToolkit.UI.D3DImageControl();
            this.mainPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.controlPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.AutoSize = true;
            this.mainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.mainPanel.BackColor = System.Drawing.Color.PeachPuff;
            this.mainPanel.Controls.Add(this.flowLayoutPanel1);
            this.mainPanel.Location = new System.Drawing.Point(3, 3);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(378, 118);
            this.mainPanel.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.controlPanel);
            this.flowLayoutPanel1.Controls.Add(this.panel2);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(372, 112);
            this.flowLayoutPanel1.TabIndex = 38;
            // 
            // controlPanel
            // 
            this.controlPanel.AutoSize = true;
            this.controlPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.controlPanel.Controls.Add(this.hostsComboBox);
            this.controlPanel.Controls.Add(this.connectButton);
            this.controlPanel.Controls.Add(this.hostAddressTextBox);
            this.controlPanel.Controls.Add(this.findServiceButton);
            this.controlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlPanel.Location = new System.Drawing.Point(3, 3);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(366, 73);
            this.controlPanel.TabIndex = 35;
            // 
            // hostsComboBox
            // 
            this.hostsComboBox.FormattingEnabled = true;
            this.hostsComboBox.Location = new System.Drawing.Point(6, 7);
            this.hostsComboBox.Name = "hostsComboBox";
            this.hostsComboBox.Size = new System.Drawing.Size(249, 24);
            this.hostsComboBox.TabIndex = 31;
            this.hostsComboBox.SelectedIndexChanged += new System.EventHandler(this.hostsComboBox_SelectedIndexChanged);
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(261, 43);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(100, 27);
            this.connectButton.TabIndex = 28;
            this.connectButton.Text = "_Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // hostAddressTextBox
            // 
            this.hostAddressTextBox.Location = new System.Drawing.Point(7, 43);
            this.hostAddressTextBox.Name = "hostAddressTextBox";
            this.hostAddressTextBox.Size = new System.Drawing.Size(248, 22);
            this.hostAddressTextBox.TabIndex = 27;
            this.hostAddressTextBox.Text = "192.168.1.135";
            // 
            // findServiceButton
            // 
            this.findServiceButton.Location = new System.Drawing.Point(262, 7);
            this.findServiceButton.Margin = new System.Windows.Forms.Padding(4);
            this.findServiceButton.Name = "findServiceButton";
            this.findServiceButton.Size = new System.Drawing.Size(100, 27);
            this.findServiceButton.TabIndex = 30;
            this.findServiceButton.Text = "_Find";
            this.findServiceButton.UseVisualStyleBackColor = true;
            this.findServiceButton.Click += new System.EventHandler(this.findServiceButton_Click);
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.showDetailsButton);
            this.panel2.Controls.Add(this.labelStatus);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 82);
            this.panel2.MinimumSize = new System.Drawing.Size(30, 27);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(366, 27);
            this.panel2.TabIndex = 38;
            // 
            // showDetailsButton
            // 
            this.showDetailsButton.AutoSize = true;
            this.showDetailsButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.showDetailsButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.showDetailsButton.Location = new System.Drawing.Point(332, 0);
            this.showDetailsButton.Name = "showDetailsButton";
            this.showDetailsButton.Size = new System.Drawing.Size(34, 27);
            this.showDetailsButton.TabIndex = 36;
            this.showDetailsButton.Text = ">>";
            this.showDetailsButton.UseVisualStyleBackColor = true;
            this.showDetailsButton.Click += new System.EventHandler(this.showDetailsButton_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoEllipsis = true;
            this.labelStatus.AutoSize = true;
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelStatus.Location = new System.Drawing.Point(0, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.labelStatus.Size = new System.Drawing.Size(103, 21);
            this.labelStatus.TabIndex = 37;
            this.labelStatus.Text = "__STATUS___";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // elementHost1
            // 
            this.elementHost1.BackColor = System.Drawing.Color.Cyan;
            this.elementHost1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(750, 539);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.wpfRemoteControl;
            // 
            // ScreenCastControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.elementHost1);
            this.Name = "ScreenCastControl";
            this.Size = new System.Drawing.Size(750, 539);
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.controlPanel.ResumeLayout(false);
            this.controlPanel.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.Integration.ElementHost elementHost1;
        public D3DImageControl wpfRemoteControl;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel controlPanel;
        private System.Windows.Forms.ComboBox hostsComboBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox hostAddressTextBox;
        private System.Windows.Forms.Button findServiceButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button showDetailsButton;
    }
}
