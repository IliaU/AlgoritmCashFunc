namespace AlgoritmCashFunc.Com.Provider.ODBC
{
    partial class FSetupConnectDB
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FSetupConnectDB));
            this.txtBox_Passvord_ODBC = new System.Windows.Forms.TextBox();
            this.lbl_Passvord_ODBC = new System.Windows.Forms.Label();
            this.txtBox_Login_ODBC = new System.Windows.Forms.TextBox();
            this.lbl_Login_ODBC = new System.Windows.Forms.Label();
            this.txtBox_DSN_ODBC = new System.Windows.Forms.TextBox();
            this.lbl_DSN_ODBC = new System.Windows.Forms.Label();
            this.btnTestODBC = new System.Windows.Forms.Button();
            this.btnSaveODBC = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtBox_Passvord_ODBC
            // 
            this.txtBox_Passvord_ODBC.Location = new System.Drawing.Point(117, 64);
            this.txtBox_Passvord_ODBC.Name = "txtBox_Passvord_ODBC";
            this.txtBox_Passvord_ODBC.Size = new System.Drawing.Size(196, 20);
            this.txtBox_Passvord_ODBC.TabIndex = 26;
            this.txtBox_Passvord_ODBC.UseSystemPasswordChar = true;
            // 
            // lbl_Passvord_ODBC
            // 
            this.lbl_Passvord_ODBC.AutoSize = true;
            this.lbl_Passvord_ODBC.Location = new System.Drawing.Point(12, 67);
            this.lbl_Passvord_ODBC.Name = "lbl_Passvord_ODBC";
            this.lbl_Passvord_ODBC.Size = new System.Drawing.Size(45, 13);
            this.lbl_Passvord_ODBC.TabIndex = 25;
            this.lbl_Passvord_ODBC.Text = "Пароль";
            // 
            // txtBox_Login_ODBC
            // 
            this.txtBox_Login_ODBC.Location = new System.Drawing.Point(117, 38);
            this.txtBox_Login_ODBC.Name = "txtBox_Login_ODBC";
            this.txtBox_Login_ODBC.Size = new System.Drawing.Size(196, 20);
            this.txtBox_Login_ODBC.TabIndex = 24;
            // 
            // lbl_Login_ODBC
            // 
            this.lbl_Login_ODBC.AutoSize = true;
            this.lbl_Login_ODBC.Location = new System.Drawing.Point(12, 38);
            this.lbl_Login_ODBC.Name = "lbl_Login_ODBC";
            this.lbl_Login_ODBC.Size = new System.Drawing.Size(38, 13);
            this.lbl_Login_ODBC.TabIndex = 23;
            this.lbl_Login_ODBC.Text = "Логин";
            // 
            // txtBox_DSN_ODBC
            // 
            this.txtBox_DSN_ODBC.Location = new System.Drawing.Point(117, 12);
            this.txtBox_DSN_ODBC.Name = "txtBox_DSN_ODBC";
            this.txtBox_DSN_ODBC.Size = new System.Drawing.Size(196, 20);
            this.txtBox_DSN_ODBC.TabIndex = 22;
            // 
            // lbl_DSN_ODBC
            // 
            this.lbl_DSN_ODBC.AutoSize = true;
            this.lbl_DSN_ODBC.Location = new System.Drawing.Point(12, 15);
            this.lbl_DSN_ODBC.Name = "lbl_DSN_ODBC";
            this.lbl_DSN_ODBC.Size = new System.Drawing.Size(55, 13);
            this.lbl_DSN_ODBC.TabIndex = 21;
            this.lbl_DSN_ODBC.Text = "Имя DSN";
            // 
            // btnTestODBC
            // 
            this.btnTestODBC.Location = new System.Drawing.Point(15, 110);
            this.btnTestODBC.Name = "btnTestODBC";
            this.btnTestODBC.Size = new System.Drawing.Size(179, 23);
            this.btnTestODBC.TabIndex = 27;
            this.btnTestODBC.Text = "Протестировать подключение";
            this.btnTestODBC.UseVisualStyleBackColor = true;
            this.btnTestODBC.Click += new System.EventHandler(this.btnTestODBC_Click);
            // 
            // btnSaveODBC
            // 
            this.btnSaveODBC.Location = new System.Drawing.Point(223, 110);
            this.btnSaveODBC.Name = "btnSaveODBC";
            this.btnSaveODBC.Size = new System.Drawing.Size(75, 23);
            this.btnSaveODBC.TabIndex = 28;
            this.btnSaveODBC.Text = "Сохранить";
            this.btnSaveODBC.UseVisualStyleBackColor = true;
            this.btnSaveODBC.Click += new System.EventHandler(this.btnSaveODBC_Click);
            // 
            // FSetupConnectDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 140);
            this.Controls.Add(this.btnSaveODBC);
            this.Controls.Add(this.btnTestODBC);
            this.Controls.Add(this.txtBox_Passvord_ODBC);
            this.Controls.Add(this.lbl_Passvord_ODBC);
            this.Controls.Add(this.txtBox_Login_ODBC);
            this.Controls.Add(this.lbl_Login_ODBC);
            this.Controls.Add(this.txtBox_DSN_ODBC);
            this.Controls.Add(this.lbl_DSN_ODBC);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FSetupConnectDB";
            this.Text = "Настройка подключения по ODBC";
            this.Load += new System.EventHandler(this.FSetupConnectDB_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtBox_Passvord_ODBC;
        private System.Windows.Forms.Label lbl_Passvord_ODBC;
        private System.Windows.Forms.TextBox txtBox_Login_ODBC;
        private System.Windows.Forms.Label lbl_Login_ODBC;
        private System.Windows.Forms.TextBox txtBox_DSN_ODBC;
        private System.Windows.Forms.Label lbl_DSN_ODBC;
        private System.Windows.Forms.Button btnTestODBC;
        private System.Windows.Forms.Button btnSaveODBC;
    }
}