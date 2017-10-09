namespace Server
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
            this.portLabel = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.squaresPanel = new System.Windows.Forms.Panel();
            this.defaultMapButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(12, 15);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(35, 13);
            this.portLabel.TabIndex = 0;
            this.portLabel.Text = "Порт:";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(53, 12);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(42, 20);
            this.portTextBox.TabIndex = 1;
            this.portTextBox.Text = "4115";
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(101, 12);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(130, 20);
            this.startButton.TabIndex = 2;
            this.startButton.Text = "Запуск сервера";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // squaresPanel
            // 
            this.squaresPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.squaresPanel.AutoSize = true;
            this.squaresPanel.Location = new System.Drawing.Point(12, 38);
            this.squaresPanel.Name = "squaresPanel";
            this.squaresPanel.Size = new System.Drawing.Size(415, 292);
            this.squaresPanel.TabIndex = 3;
            // 
            // defaultMapButton
            // 
            this.defaultMapButton.Location = new System.Drawing.Point(238, 12);
            this.defaultMapButton.Name = "defaultMapButton";
            this.defaultMapButton.Size = new System.Drawing.Size(119, 20);
            this.defaultMapButton.TabIndex = 4;
            this.defaultMapButton.Text = "Стандартная карта";
            this.defaultMapButton.UseVisualStyleBackColor = true;
            this.defaultMapButton.Click += new System.EventHandler(this.defaultMapButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(439, 342);
            this.Controls.Add(this.defaultMapButton);
            this.Controls.Add(this.squaresPanel);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.portLabel);
            this.Name = "MainForm";
            this.Text = "Лабораторная работа №5, сервер";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Panel squaresPanel;
        private System.Windows.Forms.Button defaultMapButton;
    }
}

