namespace AlgoritmCashFunc
{
    partial class FProviderSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FProviderSetup));
            this.lblRepTyp = new System.Windows.Forms.Label();
            this.cmbBoxRepTyp = new System.Windows.Forms.ComboBox();
            this.btnConfig = new System.Windows.Forms.Button();
            this.txtBoxConnectionString = new System.Windows.Forms.TextBox();
            this.lblConnectionString = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblRepTyp
            // 
            this.lblRepTyp.AutoSize = true;
            this.lblRepTyp.Location = new System.Drawing.Point(12, 9);
            this.lblRepTyp.Name = "lblRepTyp";
            this.lblRepTyp.Size = new System.Drawing.Size(92, 13);
            this.lblRepTyp.TabIndex = 0;
            this.lblRepTyp.Text = "Тип провайдера:";
            this.lblRepTyp.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // cmbBoxRepTyp
            // 
            this.cmbBoxRepTyp.FormattingEnabled = true;
            this.cmbBoxRepTyp.Location = new System.Drawing.Point(129, 6);
            this.cmbBoxRepTyp.Name = "cmbBoxRepTyp";
            this.cmbBoxRepTyp.Size = new System.Drawing.Size(261, 21);
            this.cmbBoxRepTyp.TabIndex = 1;
            this.cmbBoxRepTyp.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // btnConfig
            // 
            this.btnConfig.Location = new System.Drawing.Point(315, 89);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(75, 23);
            this.btnConfig.TabIndex = 2;
            this.btnConfig.Text = "Изменить";
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.btnConfig_Click);
            this.btnConfig.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // txtBoxConnectionString
            // 
            this.txtBoxConnectionString.Location = new System.Drawing.Point(129, 33);
            this.txtBoxConnectionString.Multiline = true;
            this.txtBoxConnectionString.Name = "txtBoxConnectionString";
            this.txtBoxConnectionString.ReadOnly = true;
            this.txtBoxConnectionString.Size = new System.Drawing.Size(261, 50);
            this.txtBoxConnectionString.TabIndex = 3;
            this.txtBoxConnectionString.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // lblConnectionString
            // 
            this.lblConnectionString.AutoSize = true;
            this.lblConnectionString.Location = new System.Drawing.Point(12, 36);
            this.lblConnectionString.Name = "lblConnectionString";
            this.lblConnectionString.Size = new System.Drawing.Size(116, 13);
            this.lblConnectionString.TabIndex = 4;
            this.lblConnectionString.Text = "Строка подключения:";
            this.lblConnectionString.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // FProviderSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 124);
            this.Controls.Add(this.lblConnectionString);
            this.Controls.Add(this.txtBoxConnectionString);
            this.Controls.Add(this.btnConfig);
            this.Controls.Add(this.cmbBoxRepTyp);
            this.Controls.Add(this.lblRepTyp);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FProviderSetup";
            this.Text = "Настройка подключения к базе";
            this.Load += new System.EventHandler(this.FRepositorySetup_Load);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRepTyp;
        private System.Windows.Forms.ComboBox cmbBoxRepTyp;
        private System.Windows.Forms.Button btnConfig;
        private System.Windows.Forms.TextBox txtBoxConnectionString;
        private System.Windows.Forms.Label lblConnectionString;
    }
}