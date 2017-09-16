namespace UniverGraphics
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
            this.nextButton = new System.Windows.Forms.Button();
            this.glControl1 = new OpenTK.GLControl();
            this.label1 = new System.Windows.Forms.Label();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.serverButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // nextButton
            // 
            this.nextButton.Enabled = false;
            this.nextButton.Location = new System.Drawing.Point(558, 254);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(114, 23);
            this.nextButton.TabIndex = 0;
            this.nextButton.Text = "Следующий цвет";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(12, 12);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(533, 432);
            this.glControl1.TabIndex = 1;
            this.glControl1.VSync = false;
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl1.Resize += new System.EventHandler(this.glControl1_Resize);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(555, 183);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Адрес сервера:";
            // 
            // addressTextBox
            // 
            this.addressTextBox.Location = new System.Drawing.Point(558, 199);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(114, 20);
            this.addressTextBox.TabIndex = 3;
            this.addressTextBox.Text = "127.0.0.1";
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(558, 225);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(114, 23);
            this.connectButton.TabIndex = 6;
            this.connectButton.Text = "Подключиться";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // serverButton
            // 
            this.serverButton.Location = new System.Drawing.Point(558, 157);
            this.serverButton.Name = "serverButton";
            this.serverButton.Size = new System.Drawing.Size(114, 23);
            this.serverButton.TabIndex = 7;
            this.serverButton.Text = "Стать сервером";
            this.serverButton.UseVisualStyleBackColor = true;
            this.serverButton.Click += new System.EventHandler(this.serverButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 457);
            this.Controls.Add(this.serverButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.addressTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.nextButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Лабораторная работа №1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button nextButton;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox addressTextBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button serverButton;
    }
}

