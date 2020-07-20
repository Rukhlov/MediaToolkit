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
            this.statusLabel = new System.Windows.Forms.Label();
            this.debugPanel = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.controlPanel = new System.Windows.Forms.TableLayoutPanel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.findServiceButton = new System.Windows.Forms.Button();
            this.hostsComboBox = new System.Windows.Forms.ComboBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.hostAddressTextBox = new System.Windows.Forms.TextBox();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.settingsButton = new System.Windows.Forms.Button();
            this.showDetailsButton = new System.Windows.Forms.Button();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.d3dImageControl = new MediaToolkit.UI.D3DImageControl();
            this.debugPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.controlPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.BackColor = System.Drawing.Color.White;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.statusLabel.ForeColor = System.Drawing.Color.Black;
            this.statusLabel.Location = new System.Drawing.Point(0, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(18, 19);
            this.statusLabel.TabIndex = 2;
            this.statusLabel.Text = "...";
            // 
            // debugPanel
            // 
            this.debugPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.debugPanel.AutoSize = true;
            this.debugPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.debugPanel.BackColor = System.Drawing.Color.PeachPuff;
            this.debugPanel.Controls.Add(this.flowLayoutPanel1);
            this.debugPanel.Location = new System.Drawing.Point(1, 399);
            this.debugPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.debugPanel.Name = "debugPanel";
            this.debugPanel.Size = new System.Drawing.Size(367, 137);
            this.debugPanel.TabIndex = 3;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.controlPanel);
            this.flowLayoutPanel1.Controls.Add(this.settingsPanel);
            this.flowLayoutPanel1.Controls.Add(this.panel2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(367, 137);
            this.flowLayoutPanel1.TabIndex = 38;
            // 
            // controlPanel
            // 
            this.controlPanel.AutoSize = true;
            this.controlPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.controlPanel.ColumnCount = 2;
            this.controlPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.controlPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.controlPanel.Controls.Add(this.labelStatus, 0, 2);
            this.controlPanel.Controls.Add(this.findServiceButton, 1, 0);
            this.controlPanel.Controls.Add(this.hostsComboBox, 0, 0);
            this.controlPanel.Controls.Add(this.connectButton, 1, 1);
            this.controlPanel.Controls.Add(this.hostAddressTextBox, 0, 1);
            this.controlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlPanel.Location = new System.Drawing.Point(3, 3);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.RowCount = 4;
            this.controlPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.controlPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.controlPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.controlPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.controlPanel.Size = new System.Drawing.Size(361, 94);
            this.controlPanel.TabIndex = 39;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoEllipsis = true;
            this.labelStatus.BackColor = System.Drawing.Color.LightSalmon;
            this.labelStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.controlPanel.SetColumnSpan(this.labelStatus, 2);
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelStatus.Location = new System.Drawing.Point(3, 69);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(3);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(355, 22);
            this.labelStatus.TabIndex = 37;
            this.labelStatus.Text = "__STATUS___";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // findServiceButton
            // 
            this.findServiceButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.findServiceButton.Location = new System.Drawing.Point(258, 3);
            this.findServiceButton.Name = "findServiceButton";
            this.findServiceButton.Size = new System.Drawing.Size(100, 27);
            this.findServiceButton.TabIndex = 30;
            this.findServiceButton.Text = "_Find";
            this.findServiceButton.UseVisualStyleBackColor = true;
            this.findServiceButton.Click += new System.EventHandler(this.findServiceButton_Click);
            // 
            // hostsComboBox
            // 
            this.hostsComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hostsComboBox.FormattingEnabled = true;
            this.hostsComboBox.Location = new System.Drawing.Point(3, 3);
            this.hostsComboBox.Name = "hostsComboBox";
            this.hostsComboBox.Size = new System.Drawing.Size(249, 24);
            this.hostsComboBox.TabIndex = 31;
            // 
            // connectButton
            // 
            this.connectButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.connectButton.Location = new System.Drawing.Point(258, 36);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(100, 27);
            this.connectButton.TabIndex = 28;
            this.connectButton.Text = "_Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // hostAddressTextBox
            // 
            this.hostAddressTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hostAddressTextBox.Location = new System.Drawing.Point(3, 36);
            this.hostAddressTextBox.Name = "hostAddressTextBox";
            this.hostAddressTextBox.Size = new System.Drawing.Size(249, 22);
            this.hostAddressTextBox.TabIndex = 27;
            this.hostAddressTextBox.Text = "127.0.0.1:808";
            // 
            // settingsPanel
            // 
            this.settingsPanel.AutoSize = true;
            this.settingsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPanel.Location = new System.Drawing.Point(3, 103);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(361, 0);
            this.settingsPanel.TabIndex = 32;
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.settingsButton);
            this.panel2.Controls.Add(this.showDetailsButton);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 108);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel2.MinimumSize = new System.Drawing.Size(29, 27);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(361, 27);
            this.panel2.TabIndex = 38;
            // 
            // settingsButton
            // 
            this.settingsButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.settingsButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.settingsButton.Location = new System.Drawing.Point(0, 0);
            this.settingsButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(100, 27);
            this.settingsButton.TabIndex = 32;
            this.settingsButton.Text = "Settings";
            this.settingsButton.UseVisualStyleBackColor = true;
            this.settingsButton.Click += new System.EventHandler(this.settingsButton_Click);
            // 
            // showDetailsButton
            // 
            this.showDetailsButton.AutoSize = true;
            this.showDetailsButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.showDetailsButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.showDetailsButton.Location = new System.Drawing.Point(327, 0);
            this.showDetailsButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.showDetailsButton.Name = "showDetailsButton";
            this.showDetailsButton.Size = new System.Drawing.Size(34, 27);
            this.showDetailsButton.TabIndex = 36;
            this.showDetailsButton.Text = ">>";
            this.showDetailsButton.UseVisualStyleBackColor = true;
            this.showDetailsButton.Click += new System.EventHandler(this.showDetailsButton_Click);
            // 
            // elementHost1
            // 
            this.elementHost1.BackColor = System.Drawing.Color.Cyan;
            this.elementHost1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 0);
            this.elementHost1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(749, 539);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.d3dImageControl;
            // 
            // ScreenCastControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.debugPanel);
            this.Controls.Add(this.elementHost1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ScreenCastControl";
            this.Size = new System.Drawing.Size(749, 539);
            this.debugPanel.ResumeLayout(false);
            this.debugPanel.PerformLayout();
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
        public D3DImageControl d3dImageControl;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Panel debugPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel controlPanel;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button findServiceButton;
        private System.Windows.Forms.ComboBox hostsComboBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox hostAddressTextBox;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button settingsButton;
        private System.Windows.Forms.Button showDetailsButton;
    }
}
