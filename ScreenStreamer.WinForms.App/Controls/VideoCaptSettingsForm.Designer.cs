namespace ScreenStreamer.WinForms
{
    partial class VideoCaptSettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.screenCaptureDetailsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.captureMouseCheckBox = new System.Windows.Forms.CheckBox();
            this.showDebugInfoCheckBox = new System.Windows.Forms.CheckBox();
            this.showCaptureBorderCheckBox = new System.Windows.Forms.CheckBox();
            this.fpsNumeric = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.screenCaptureDetailsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(11, 11);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 10, 11);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(249, 192);
            this.tableLayoutPanel1.TabIndex = 80;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(2, 158);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 4, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.panel1.Size = new System.Drawing.Size(245, 32);
            this.panel1.TabIndex = 60;
            // 
            // cancelButton
            // 
            this.cancelButton.AutoSize = true;
            this.cancelButton.Location = new System.Drawing.Point(134, 2);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(109, 28);
            this.cancelButton.TabIndex = 210;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.AutoSize = true;
            this.applyButton.Location = new System.Drawing.Point(16, 2);
            this.applyButton.Margin = new System.Windows.Forms.Padding(2);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(112, 28);
            this.applyButton.TabIndex = 200;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.screenCaptureDetailsPanel);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 15, 6, 15);
            this.groupBox1.Size = new System.Drawing.Size(243, 148);
            this.groupBox1.TabIndex = 61;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Video Capture Settings";
            // 
            // screenCaptureDetailsPanel
            // 
            this.screenCaptureDetailsPanel.AutoSize = true;
            this.screenCaptureDetailsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.screenCaptureDetailsPanel.ColumnCount = 3;
            this.screenCaptureDetailsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.screenCaptureDetailsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.screenCaptureDetailsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.screenCaptureDetailsPanel.Controls.Add(this.captureMouseCheckBox, 0, 1);
            this.screenCaptureDetailsPanel.Controls.Add(this.fpsNumeric, 1, 0);
            this.screenCaptureDetailsPanel.Controls.Add(this.label1, 0, 0);
            this.screenCaptureDetailsPanel.Controls.Add(this.showDebugInfoCheckBox, 0, 2);
            this.screenCaptureDetailsPanel.Controls.Add(this.showCaptureBorderCheckBox, 0, 3);
            this.screenCaptureDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.screenCaptureDetailsPanel.Location = new System.Drawing.Point(6, 31);
            this.screenCaptureDetailsPanel.Margin = new System.Windows.Forms.Padding(2, 10, 0, 2);
            this.screenCaptureDetailsPanel.Name = "screenCaptureDetailsPanel";
            this.screenCaptureDetailsPanel.RowCount = 5;
            this.screenCaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.screenCaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.screenCaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.screenCaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.screenCaptureDetailsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.screenCaptureDetailsPanel.Size = new System.Drawing.Size(231, 102);
            this.screenCaptureDetailsPanel.TabIndex = 109;
            // 
            // captureMouseCheckBox
            // 
            this.captureMouseCheckBox.AutoSize = true;
            this.captureMouseCheckBox.Checked = true;
            this.captureMouseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.screenCaptureDetailsPanel.SetColumnSpan(this.captureMouseCheckBox, 2);
            this.captureMouseCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.captureMouseCheckBox.Location = new System.Drawing.Point(5, 29);
            this.captureMouseCheckBox.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.captureMouseCheckBox.Name = "captureMouseCheckBox";
            this.captureMouseCheckBox.Size = new System.Drawing.Size(224, 21);
            this.captureMouseCheckBox.TabIndex = 80;
            this.captureMouseCheckBox.Text = "Capture Mouse";
            this.captureMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // showDebugInfoCheckBox
            // 
            this.showDebugInfoCheckBox.AutoSize = true;
            this.showDebugInfoCheckBox.Checked = true;
            this.showDebugInfoCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.screenCaptureDetailsPanel.SetColumnSpan(this.showDebugInfoCheckBox, 2);
            this.showDebugInfoCheckBox.Location = new System.Drawing.Point(5, 54);
            this.showDebugInfoCheckBox.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.showDebugInfoCheckBox.Name = "showDebugInfoCheckBox";
            this.showDebugInfoCheckBox.Size = new System.Drawing.Size(137, 21);
            this.showDebugInfoCheckBox.TabIndex = 82;
            this.showDebugInfoCheckBox.Text = "Show Debug Info";
            this.showDebugInfoCheckBox.UseVisualStyleBackColor = true;
            // 
            // showCaptureBorderCheckBox
            // 
            this.showCaptureBorderCheckBox.AutoSize = true;
            this.showCaptureBorderCheckBox.Checked = true;
            this.showCaptureBorderCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.screenCaptureDetailsPanel.SetColumnSpan(this.showCaptureBorderCheckBox, 2);
            this.showCaptureBorderCheckBox.Location = new System.Drawing.Point(5, 79);
            this.showCaptureBorderCheckBox.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.showCaptureBorderCheckBox.Name = "showCaptureBorderCheckBox";
            this.showCaptureBorderCheckBox.Size = new System.Drawing.Size(162, 21);
            this.showCaptureBorderCheckBox.TabIndex = 84;
            this.showCaptureBorderCheckBox.Text = "Show Capture Frame";
            this.showCaptureBorderCheckBox.UseVisualStyleBackColor = true;
            // 
            // fpsNumeric
            // 
            this.fpsNumeric.Dock = System.Windows.Forms.DockStyle.Right;
            this.fpsNumeric.Location = new System.Drawing.Point(158, 2);
            this.fpsNumeric.Margin = new System.Windows.Forms.Padding(2);
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
            this.fpsNumeric.Size = new System.Drawing.Size(71, 23);
            this.fpsNumeric.TabIndex = 108;
            this.fpsNumeric.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(2, 2);
            this.label1.Margin = new System.Windows.Forms.Padding(2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 23);
            this.label1.TabIndex = 6;
            this.label1.Text = "FPS:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // VideoCaptSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(337, 265);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VideoCaptSettingsForm";
            this.ShowIcon = false;
            this.Text = "Video Capture Settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.screenCaptureDetailsPanel.ResumeLayout(false);
            this.screenCaptureDetailsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fpsNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown fpsNumeric;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel screenCaptureDetailsPanel;
        private System.Windows.Forms.CheckBox captureMouseCheckBox;
        private System.Windows.Forms.CheckBox showDebugInfoCheckBox;
        private System.Windows.Forms.CheckBox showCaptureBorderCheckBox;
    }
}