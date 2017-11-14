namespace Client
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
            this.changeModelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.cullFaceModeButton = new System.Windows.Forms.Button();
            this.lightingButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // changeModelButton
            // 
            this.changeModelButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.changeModelButton.Enabled = false;
            this.changeModelButton.Location = new System.Drawing.Point(615, 192);
            this.changeModelButton.Name = "changeModelButton";
            this.changeModelButton.Size = new System.Drawing.Size(114, 36);
            this.changeModelButton.TabIndex = 0;
            this.changeModelButton.TabStop = false;
            this.changeModelButton.Text = "Сменить модель отображения";
            this.changeModelButton.UseVisualStyleBackColor = true;
            this.changeModelButton.Click += new System.EventHandler(this.changeModelButton_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(612, 121);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Адрес сервера:";
            // 
            // addressTextBox
            // 
            this.addressTextBox.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.addressTextBox.Location = new System.Drawing.Point(615, 137);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(114, 20);
            this.addressTextBox.TabIndex = 3;
            this.addressTextBox.Text = "127.0.0.1";
            // 
            // connectButton
            // 
            this.connectButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.connectButton.Location = new System.Drawing.Point(615, 163);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(114, 23);
            this.connectButton.TabIndex = 6;
            this.connectButton.Text = "Подключиться";
            this.connectButton.UseVisualStyleBackColor = true;
            // 
            // cullFaceModeButton
            // 
            this.cullFaceModeButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cullFaceModeButton.Enabled = false;
            this.cullFaceModeButton.Location = new System.Drawing.Point(615, 234);
            this.cullFaceModeButton.Name = "cullFaceModeButton";
            this.cullFaceModeButton.Size = new System.Drawing.Size(114, 37);
            this.cullFaceModeButton.TabIndex = 8;
            this.cullFaceModeButton.TabStop = false;
            this.cullFaceModeButton.Tag = "0";
            this.cullFaceModeButton.Text = "Переключить режим отсечения граней";
            this.cullFaceModeButton.UseVisualStyleBackColor = true;
            this.cullFaceModeButton.Click += new System.EventHandler(this.cullFaceModeButton_Click);
            // 
            // lightingButton
            // 
            this.lightingButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lightingButton.Enabled = false;
            this.lightingButton.Location = new System.Drawing.Point(615, 277);
            this.lightingButton.Name = "lightingButton";
            this.lightingButton.Size = new System.Drawing.Size(114, 36);
            this.lightingButton.TabIndex = 9;
            this.lightingButton.TabStop = false;
            this.lightingButton.Text = "Изменить режим освещения";
            this.lightingButton.UseVisualStyleBackColor = true;
            this.lightingButton.Click += new System.EventHandler(this.lightingButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(741, 498);
            this.Controls.Add(this.lightingButton);
            this.Controls.Add(this.cullFaceModeButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.addressTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.changeModelButton);
            this.Name = "MainForm";
            this.Text = "Лабораторная работа №7, клиент";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button changeModelButton;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox addressTextBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button cullFaceModeButton;
        private System.Windows.Forms.Button lightingButton;
    }
}

