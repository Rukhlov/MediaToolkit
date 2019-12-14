namespace TestStreamer.Controls
{
    partial class RemoteServerControl
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
            this.stopRemoteServButton = new System.Windows.Forms.Button();
            this.startRemoteServButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // stopRemoteServButton
            // 
            this.stopRemoteServButton.Location = new System.Drawing.Point(186, 129);
            this.stopRemoteServButton.Name = "stopRemoteServButton";
            this.stopRemoteServButton.Size = new System.Drawing.Size(119, 39);
            this.stopRemoteServButton.TabIndex = 24;
            this.stopRemoteServButton.Text = "_Stop";
            this.stopRemoteServButton.UseVisualStyleBackColor = true;
            this.stopRemoteServButton.Click += new System.EventHandler(this.stopRemoteServButton_Click);
            // 
            // startRemoteServButton
            // 
            this.startRemoteServButton.Location = new System.Drawing.Point(27, 129);
            this.startRemoteServButton.Name = "startRemoteServButton";
            this.startRemoteServButton.Size = new System.Drawing.Size(127, 39);
            this.startRemoteServButton.TabIndex = 23;
            this.startRemoteServButton.Text = "_Start";
            this.startRemoteServButton.UseVisualStyleBackColor = true;
            this.startRemoteServButton.Click += new System.EventHandler(this.startRemoteServButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Location = new System.Drawing.Point(13, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(472, 82);
            this.panel1.TabIndex = 25;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(14, 11);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(439, 22);
            this.textBox1.TabIndex = 17;
            this.textBox1.Text = "0.0.0.0";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(327, 268);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 26;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // RemoteServerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.stopRemoteServButton);
            this.Controls.Add(this.startRemoteServButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.button2);
            this.Name = "RemoteServerControl";
            this.Size = new System.Drawing.Size(487, 462);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button stopRemoteServButton;
        private System.Windows.Forms.Button startRemoteServButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button2;
    }
}
