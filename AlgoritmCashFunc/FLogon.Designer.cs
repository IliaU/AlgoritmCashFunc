namespace AlgoritmCashFunc
{
    partial class FLogon
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FLogon));
            this.label1 = new System.Windows.Forms.Label();
            this.cmbBoxLogon = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBoxPassword = new System.Windows.Forms.TextBox();
            this.btnLogon = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Выбирите свой логин:";
            // 
            // cmbBoxLogon
            // 
            this.cmbBoxLogon.FormattingEnabled = true;
            this.cmbBoxLogon.Location = new System.Drawing.Point(151, 50);
            this.cmbBoxLogon.Name = "cmbBoxLogon";
            this.cmbBoxLogon.Size = new System.Drawing.Size(194, 21);
            this.cmbBoxLogon.TabIndex = 1;
            this.cmbBoxLogon.SelectedIndexChanged += new System.EventHandler(this.cmbBoxLogon_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Введите свой пароль:";
            // 
            // txtBoxPassword
            // 
            this.txtBoxPassword.Location = new System.Drawing.Point(151, 77);
            this.txtBoxPassword.Name = "txtBoxPassword";
            this.txtBoxPassword.Size = new System.Drawing.Size(194, 20);
            this.txtBoxPassword.TabIndex = 3;
            this.txtBoxPassword.UseSystemPasswordChar = true;
            this.txtBoxPassword.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtBoxPassword_KeyUp);
            // 
            // btnLogon
            // 
            this.btnLogon.Location = new System.Drawing.Point(242, 114);
            this.btnLogon.Name = "btnLogon";
            this.btnLogon.Size = new System.Drawing.Size(103, 23);
            this.btnLogon.TabIndex = 4;
            this.btnLogon.Text = "Войти в систему";
            this.btnLogon.UseVisualStyleBackColor = true;
            this.btnLogon.Click += new System.EventHandler(this.btnLogon_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(74, 114);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(129, 23);
            this.btnExit.TabIndex = 5;
            this.btnExit.Text = "Закрыть программу";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.ForeColor = System.Drawing.Color.MidnightBlue;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(357, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Выбирите свой логин:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblInfo
            // 
            this.lblInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInfo.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblInfo.Location = new System.Drawing.Point(0, 20);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(357, 20);
            this.lblInfo.TabIndex = 7;
            this.lblInfo.Text = "Придумайте себе парль.";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblInfo.Visible = false;
            // 
            // FLogon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(357, 149);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnLogon);
            this.Controls.Add(this.txtBoxPassword);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbBoxLogon);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FLogon";
            this.Text = "Вход в программу";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbBoxLogon;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBoxPassword;
        private System.Windows.Forms.Button btnLogon;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblInfo;
    }
}