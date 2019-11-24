namespace TestStreamer.Controls
{
    partial class WebCamStreamerControl
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
            this.panel3 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.profilesComboBox = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.destWidthNumeric = new System.Windows.Forms.NumericUpDown();
            this.destHeightNumeric = new System.Windows.Forms.NumericUpDown();
            this.label22 = new System.Windows.Forms.Label();
            this.deviceUpdateButton = new System.Windows.Forms.Button();
            this.devicesComboBox = new System.Windows.Forms.ComboBox();
            this.label23 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.portNumeric = new System.Windows.Forms.NumericUpDown();
            this.addrTextBox = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.panel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.destWidthNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.destHeightNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.portNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.profilesComboBox);
            this.panel3.Controls.Add(this.groupBox1);
            this.panel3.Controls.Add(this.label22);
            this.panel3.Controls.Add(this.deviceUpdateButton);
            this.panel3.Controls.Add(this.devicesComboBox);
            this.panel3.Controls.Add(this.label23);
            this.panel3.Controls.Add(this.label25);
            this.panel3.Controls.Add(this.portNumeric);
            this.panel3.Controls.Add(this.addrTextBox);
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(464, 302);
            this.panel3.TabIndex = 24;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 17);
            this.label1.TabIndex = 40;
            this.label1.Text = "Profile:";
            // 
            // profilesComboBox
            // 
            this.profilesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.profilesComboBox.FormattingEnabled = true;
            this.profilesComboBox.Location = new System.Drawing.Point(77, 43);
            this.profilesComboBox.Name = "profilesComboBox";
            this.profilesComboBox.Size = new System.Drawing.Size(290, 24);
            this.profilesComboBox.TabIndex = 39;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.destWidthNumeric);
            this.groupBox1.Controls.Add(this.destHeightNumeric);
            this.groupBox1.Location = new System.Drawing.Point(18, 191);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(163, 99);
            this.groupBox1.TabIndex = 38;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "DestSize";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 31);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(48, 17);
            this.label14.TabIndex = 28;
            this.label14.Text = "Width:";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 59);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(53, 17);
            this.label15.TabIndex = 30;
            this.label15.Text = "Height:";
            // 
            // destWidthNumeric
            // 
            this.destWidthNumeric.Location = new System.Drawing.Point(66, 29);
            this.destWidthNumeric.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.destWidthNumeric.Name = "destWidthNumeric";
            this.destWidthNumeric.Size = new System.Drawing.Size(78, 22);
            this.destWidthNumeric.TabIndex = 26;
            this.destWidthNumeric.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // destHeightNumeric
            // 
            this.destHeightNumeric.Location = new System.Drawing.Point(66, 57);
            this.destHeightNumeric.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.destHeightNumeric.Name = "destHeightNumeric";
            this.destHeightNumeric.Size = new System.Drawing.Size(78, 22);
            this.destHeightNumeric.TabIndex = 29;
            this.destHeightNumeric.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(13, 16);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(55, 17);
            this.label22.TabIndex = 14;
            this.label22.Text = "Device:";
            // 
            // deviceUpdateButton
            // 
            this.deviceUpdateButton.Location = new System.Drawing.Point(373, 13);
            this.deviceUpdateButton.Name = "deviceUpdateButton";
            this.deviceUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.deviceUpdateButton.TabIndex = 1;
            this.deviceUpdateButton.Text = "_Update";
            this.deviceUpdateButton.UseVisualStyleBackColor = true;
            // 
            // devicesComboBox
            // 
            this.devicesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.devicesComboBox.FormattingEnabled = true;
            this.devicesComboBox.Location = new System.Drawing.Point(77, 13);
            this.devicesComboBox.Name = "devicesComboBox";
            this.devicesComboBox.Size = new System.Drawing.Size(290, 24);
            this.devicesComboBox.TabIndex = 2;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(21, 133);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(38, 17);
            this.label23.TabIndex = 13;
            this.label23.Text = "Port:";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(21, 102);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(64, 17);
            this.label25.TabIndex = 12;
            this.label25.Text = "Address:";
            // 
            // portNumeric
            // 
            this.portNumeric.Location = new System.Drawing.Point(91, 128);
            this.portNumeric.Maximum = new decimal(new int[] {
            100500,
            0,
            0,
            0});
            this.portNumeric.Minimum = new decimal(new int[] {
            8086,
            0,
            0,
            0});
            this.portNumeric.Name = "portNumeric";
            this.portNumeric.Size = new System.Drawing.Size(276, 22);
            this.portNumeric.TabIndex = 11;
            this.portNumeric.Value = new decimal(new int[] {
            8086,
            0,
            0,
            0});
            // 
            // addrTextBox
            // 
            this.addrTextBox.Location = new System.Drawing.Point(91, 99);
            this.addrTextBox.Name = "addrTextBox";
            this.addrTextBox.Size = new System.Drawing.Size(276, 22);
            this.addrTextBox.TabIndex = 10;
            this.addrTextBox.Text = "0.0.0.0";
            // 
            // startButton
            // 
            this.startButton.AutoSize = true;
            this.startButton.Location = new System.Drawing.Point(3, 311);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(121, 27);
            this.startButton.TabIndex = 25;
            this.startButton.Text = "_Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.AutoSize = true;
            this.stopButton.Location = new System.Drawing.Point(130, 311);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(107, 27);
            this.stopButton.TabIndex = 26;
            this.stopButton.Text = "_Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // WebCamStreamerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.panel3);
            this.Name = "WebCamStreamerControl";
            this.Size = new System.Drawing.Size(478, 365);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.destWidthNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.destHeightNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.portNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox profilesComboBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown destWidthNumeric;
        private System.Windows.Forms.NumericUpDown destHeightNumeric;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Button deviceUpdateButton;
        private System.Windows.Forms.ComboBox devicesComboBox;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.NumericUpDown portNumeric;
        private System.Windows.Forms.TextBox addrTextBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
    }
}
