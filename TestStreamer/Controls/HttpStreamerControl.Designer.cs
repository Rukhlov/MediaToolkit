namespace TestStreamer.Controls
{
    partial class HttpStreamerControl
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.httpDestWidthNumeric = new System.Windows.Forms.NumericUpDown();
            this.httpDestHeightNumeric = new System.Windows.Forms.NumericUpDown();
            this.label22 = new System.Windows.Forms.Label();
            this.httpUpdateButton = new System.Windows.Forms.Button();
            this.httpDisplayComboBox = new System.Windows.Forms.ComboBox();
            this.httpFpsNumeric = new System.Windows.Forms.NumericUpDown();
            this.label23 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.httpPortNumeric = new System.Windows.Forms.NumericUpDown();
            this.httpAddrTextBox = new System.Windows.Forms.TextBox();
            this.httpStartButton = new System.Windows.Forms.Button();
            this.httpStopButton = new System.Windows.Forms.Button();
            this.panel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.httpDestWidthNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.httpDestHeightNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.httpFpsNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.httpPortNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.groupBox1);
            this.panel3.Controls.Add(this.label22);
            this.panel3.Controls.Add(this.httpUpdateButton);
            this.panel3.Controls.Add(this.httpDisplayComboBox);
            this.panel3.Controls.Add(this.httpFpsNumeric);
            this.panel3.Controls.Add(this.label23);
            this.panel3.Controls.Add(this.label24);
            this.panel3.Controls.Add(this.label25);
            this.panel3.Controls.Add(this.httpPortNumeric);
            this.panel3.Controls.Add(this.httpAddrTextBox);
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(461, 399);
            this.panel3.TabIndex = 23;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.httpDestWidthNumeric);
            this.groupBox1.Controls.Add(this.httpDestHeightNumeric);
            this.groupBox1.Location = new System.Drawing.Point(16, 121);
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
            // httpDestWidthNumeric
            // 
            this.httpDestWidthNumeric.Location = new System.Drawing.Point(66, 29);
            this.httpDestWidthNumeric.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.httpDestWidthNumeric.Name = "httpDestWidthNumeric";
            this.httpDestWidthNumeric.Size = new System.Drawing.Size(78, 22);
            this.httpDestWidthNumeric.TabIndex = 26;
            this.httpDestWidthNumeric.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // httpDestHeightNumeric
            // 
            this.httpDestHeightNumeric.Location = new System.Drawing.Point(66, 57);
            this.httpDestHeightNumeric.Maximum = new decimal(new int[] {
            8128,
            0,
            0,
            0});
            this.httpDestHeightNumeric.Name = "httpDestHeightNumeric";
            this.httpDestHeightNumeric.Size = new System.Drawing.Size(78, 22);
            this.httpDestHeightNumeric.TabIndex = 29;
            this.httpDestHeightNumeric.Value = new decimal(new int[] {
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
            this.label22.Size = new System.Drawing.Size(58, 17);
            this.label22.TabIndex = 14;
            this.label22.Text = "Display:";
            // 
            // httpUpdateButton
            // 
            this.httpUpdateButton.Location = new System.Drawing.Point(373, 13);
            this.httpUpdateButton.Name = "httpUpdateButton";
            this.httpUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.httpUpdateButton.TabIndex = 1;
            this.httpUpdateButton.Text = "_Update";
            this.httpUpdateButton.UseVisualStyleBackColor = true;
            this.httpUpdateButton.Click += new System.EventHandler(this.httpUpdateButton_Click);
            // 
            // httpDisplayComboBox
            // 
            this.httpDisplayComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.httpDisplayComboBox.FormattingEnabled = true;
            this.httpDisplayComboBox.Location = new System.Drawing.Point(77, 13);
            this.httpDisplayComboBox.Name = "httpDisplayComboBox";
            this.httpDisplayComboBox.Size = new System.Drawing.Size(290, 24);
            this.httpDisplayComboBox.TabIndex = 2;
            // 
            // httpFpsNumeric
            // 
            this.httpFpsNumeric.Location = new System.Drawing.Point(57, 236);
            this.httpFpsNumeric.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.httpFpsNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.httpFpsNumeric.Name = "httpFpsNumeric";
            this.httpFpsNumeric.Size = new System.Drawing.Size(47, 22);
            this.httpFpsNumeric.TabIndex = 5;
            this.httpFpsNumeric.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(13, 84);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(38, 17);
            this.label23.TabIndex = 13;
            this.label23.Text = "Port:";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(13, 238);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(38, 17);
            this.label24.TabIndex = 6;
            this.label24.Text = "FPS:";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(13, 53);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(64, 17);
            this.label25.TabIndex = 12;
            this.label25.Text = "Address:";
            // 
            // httpPortNumeric
            // 
            this.httpPortNumeric.Location = new System.Drawing.Point(83, 79);
            this.httpPortNumeric.Maximum = new decimal(new int[] {
            100500,
            0,
            0,
            0});
            this.httpPortNumeric.Minimum = new decimal(new int[] {
            8086,
            0,
            0,
            0});
            this.httpPortNumeric.Name = "httpPortNumeric";
            this.httpPortNumeric.Size = new System.Drawing.Size(276, 22);
            this.httpPortNumeric.TabIndex = 11;
            this.httpPortNumeric.Value = new decimal(new int[] {
            8086,
            0,
            0,
            0});
            // 
            // httpAddrTextBox
            // 
            this.httpAddrTextBox.Location = new System.Drawing.Point(83, 50);
            this.httpAddrTextBox.Name = "httpAddrTextBox";
            this.httpAddrTextBox.Size = new System.Drawing.Size(276, 22);
            this.httpAddrTextBox.TabIndex = 10;
            this.httpAddrTextBox.Text = "0.0.0.0";
            // 
            // httpStartButton
            // 
            this.httpStartButton.Location = new System.Drawing.Point(17, 408);
            this.httpStartButton.Name = "httpStartButton";
            this.httpStartButton.Size = new System.Drawing.Size(177, 35);
            this.httpStartButton.TabIndex = 21;
            this.httpStartButton.Text = "_Start";
            this.httpStartButton.UseVisualStyleBackColor = true;
            this.httpStartButton.Click += new System.EventHandler(this.httpStartButton_Click);
            // 
            // httpStopButton
            // 
            this.httpStopButton.Location = new System.Drawing.Point(240, 408);
            this.httpStopButton.Name = "httpStopButton";
            this.httpStopButton.Size = new System.Drawing.Size(107, 35);
            this.httpStopButton.TabIndex = 22;
            this.httpStopButton.Text = "_Stop";
            this.httpStopButton.UseVisualStyleBackColor = true;
            this.httpStopButton.Click += new System.EventHandler(this.httpStopButton_Click);
            // 
            // HttpStreamerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.httpStartButton);
            this.Controls.Add(this.httpStopButton);
            this.Name = "HttpStreamerControl";
            this.Size = new System.Drawing.Size(495, 497);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.httpDestWidthNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.httpDestHeightNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.httpFpsNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.httpPortNumeric)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown httpDestWidthNumeric;
        private System.Windows.Forms.NumericUpDown httpDestHeightNumeric;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Button httpUpdateButton;
        private System.Windows.Forms.ComboBox httpDisplayComboBox;
        private System.Windows.Forms.NumericUpDown httpFpsNumeric;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.NumericUpDown httpPortNumeric;
        private System.Windows.Forms.TextBox httpAddrTextBox;
        private System.Windows.Forms.Button httpStartButton;
        private System.Windows.Forms.Button httpStopButton;
    }
}
