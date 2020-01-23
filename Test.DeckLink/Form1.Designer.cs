namespace Test.DeckLink
{
    partial class Form1
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
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.comboBoxDevices = new System.Windows.Forms.ComboBox();
            this.buttonFind = new System.Windows.Forms.Button();
            this.fitToVideoCheckBox = new System.Windows.Forms.CheckBox();
            this.comboBoxDisplayModes = new System.Windows.Forms.ComboBox();
            this.devicesPanel = new System.Windows.Forms.Panel();
            this.buttonDiscoveryStart = new System.Windows.Forms.Button();
            this.buttonDiscoveryStop = new System.Windows.Forms.Button();
            this.devicesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(96, 109);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 7;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(15, 109);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 6;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // comboBoxDevices
            // 
            this.comboBoxDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDevices.FormattingEnabled = true;
            this.comboBoxDevices.Location = new System.Drawing.Point(3, 3);
            this.comboBoxDevices.Name = "comboBoxDevices";
            this.comboBoxDevices.Size = new System.Drawing.Size(324, 21);
            this.comboBoxDevices.TabIndex = 5;
            this.comboBoxDevices.SelectedValueChanged += new System.EventHandler(this.comboBoxDevices_SelectedValueChanged);
            // 
            // buttonFind
            // 
            this.buttonFind.AutoSize = true;
            this.buttonFind.Location = new System.Drawing.Point(332, 1);
            this.buttonFind.Margin = new System.Windows.Forms.Padding(2);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(65, 23);
            this.buttonFind.TabIndex = 4;
            this.buttonFind.Text = "_Find";
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
            // 
            // fitToVideoCheckBox
            // 
            this.fitToVideoCheckBox.AutoSize = true;
            this.fitToVideoCheckBox.Location = new System.Drawing.Point(14, 154);
            this.fitToVideoCheckBox.Name = "fitToVideoCheckBox";
            this.fitToVideoCheckBox.Size = new System.Drawing.Size(142, 17);
            this.fitToVideoCheckBox.TabIndex = 8;
            this.fitToVideoCheckBox.Text = "_FitWindowToVideoSize";
            this.fitToVideoCheckBox.UseVisualStyleBackColor = true;
            this.fitToVideoCheckBox.CheckedChanged += new System.EventHandler(this.fitToVideoCheckBox_CheckedChanged);
            // 
            // comboBoxDisplayModes
            // 
            this.comboBoxDisplayModes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDisplayModes.FormattingEnabled = true;
            this.comboBoxDisplayModes.Location = new System.Drawing.Point(3, 30);
            this.comboBoxDisplayModes.Name = "comboBoxDisplayModes";
            this.comboBoxDisplayModes.Size = new System.Drawing.Size(324, 21);
            this.comboBoxDisplayModes.TabIndex = 9;
            this.comboBoxDisplayModes.SelectedValueChanged += new System.EventHandler(this.comboBoxDisplayIds_SelectedValueChanged);
            // 
            // devicesPanel
            // 
            this.devicesPanel.Controls.Add(this.comboBoxDevices);
            this.devicesPanel.Controls.Add(this.comboBoxDisplayModes);
            this.devicesPanel.Controls.Add(this.buttonFind);
            this.devicesPanel.Location = new System.Drawing.Point(12, 12);
            this.devicesPanel.Name = "devicesPanel";
            this.devicesPanel.Size = new System.Drawing.Size(411, 75);
            this.devicesPanel.TabIndex = 10;
            // 
            // buttonDiscoveryStart
            // 
            this.buttonDiscoveryStart.Location = new System.Drawing.Point(273, 163);
            this.buttonDiscoveryStart.Name = "buttonDiscoveryStart";
            this.buttonDiscoveryStart.Size = new System.Drawing.Size(75, 23);
            this.buttonDiscoveryStart.TabIndex = 11;
            this.buttonDiscoveryStart.Text = "Start";
            this.buttonDiscoveryStart.UseVisualStyleBackColor = true;
            this.buttonDiscoveryStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonDiscoveryStop
            // 
            this.buttonDiscoveryStop.Location = new System.Drawing.Point(354, 163);
            this.buttonDiscoveryStop.Name = "buttonDiscoveryStop";
            this.buttonDiscoveryStop.Size = new System.Drawing.Size(75, 23);
            this.buttonDiscoveryStop.TabIndex = 12;
            this.buttonDiscoveryStop.Text = "Stop";
            this.buttonDiscoveryStop.UseVisualStyleBackColor = true;
            this.buttonDiscoveryStop.Click += new System.EventHandler(this.buttonDiscoveryStop_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 198);
            this.Controls.Add(this.buttonDiscoveryStop);
            this.Controls.Add(this.buttonDiscoveryStart);
            this.Controls.Add(this.devicesPanel);
            this.Controls.Add(this.fitToVideoCheckBox);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Name = "Form1";
            this.Text = "Form1";
            this.devicesPanel.ResumeLayout(false);
            this.devicesPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.ComboBox comboBoxDevices;
        private System.Windows.Forms.Button buttonFind;
        private System.Windows.Forms.CheckBox fitToVideoCheckBox;
        private System.Windows.Forms.ComboBox comboBoxDisplayModes;
        private System.Windows.Forms.Panel devicesPanel;
        private System.Windows.Forms.Button buttonDiscoveryStart;
        private System.Windows.Forms.Button buttonDiscoveryStop;
    }
}

