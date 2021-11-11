namespace AlgoritmCashFunc
{
    partial class FUsers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FUsers));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.TSMItemAddUser = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.dGViewLogon = new System.Windows.Forms.DataGridView();
            this.Logon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlLeftTop = new System.Windows.Forms.Panel();
            this.lblLogon = new System.Windows.Forms.Label();
            this.pnlBttm = new System.Windows.Forms.Panel();
            this.pnlFill = new System.Windows.Forms.Panel();
            this.pnlFillFill = new System.Windows.Forms.Panel();
            this.txtBoxLogonEdit = new System.Windows.Forms.TextBox();
            this.cmbBoxRoleEdit = new System.Windows.Forms.ComboBox();
            this.txtBoxDescriptionEdit = new System.Windows.Forms.TextBox();
            this.txtBoxPasswordEdit = new System.Windows.Forms.TextBox();
            this.pnlFillLeft = new System.Windows.Forms.Panel();
            this.lblDescriptionEdit = new System.Windows.Forms.Label();
            this.lblPasswordEditPnlTop = new System.Windows.Forms.Label();
            this.lblRoleEdit = new System.Windows.Forms.Label();
            this.lblPasswordEdit = new System.Windows.Forms.Label();
            this.pnlFillBtm = new System.Windows.Forms.Panel();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblLogonEdit = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGViewLogon)).BeginInit();
            this.pnlLeftTop.SuspendLayout();
            this.pnlFill.SuspendLayout();
            this.pnlFillFill.SuspendLayout();
            this.pnlFillLeft.SuspendLayout();
            this.pnlFillBtm.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSMItemAddUser});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(632, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // TSMItemAddUser
            // 
            this.TSMItemAddUser.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.TSMItemAddUser.ForeColor = System.Drawing.Color.MidnightBlue;
            this.TSMItemAddUser.Name = "TSMItemAddUser";
            this.TSMItemAddUser.Size = new System.Drawing.Size(158, 20);
            this.TSMItemAddUser.Text = "Добавить пользователя";
            this.TSMItemAddUser.Click += new System.EventHandler(this.TSMItemAddUser_Click);
            this.TSMItemAddUser.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.dGViewLogon);
            this.pnlLeft.Controls.Add(this.pnlLeftTop);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 24);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(215, 275);
            this.pnlLeft.TabIndex = 1;
            // 
            // dGViewLogon
            // 
            this.dGViewLogon.AllowUserToAddRows = false;
            this.dGViewLogon.AllowUserToDeleteRows = false;
            this.dGViewLogon.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGViewLogon.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Logon});
            this.dGViewLogon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dGViewLogon.Location = new System.Drawing.Point(0, 22);
            this.dGViewLogon.Name = "dGViewLogon";
            this.dGViewLogon.ReadOnly = true;
            this.dGViewLogon.Size = new System.Drawing.Size(215, 253);
            this.dGViewLogon.TabIndex = 1;
            this.dGViewLogon.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dGViewLogon_CellMouseClick);
            this.dGViewLogon.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // Logon
            // 
            this.Logon.DataPropertyName = "Logon";
            this.Logon.HeaderText = "Логин";
            this.Logon.Name = "Logon";
            this.Logon.ReadOnly = true;
            this.Logon.Width = 150;
            // 
            // pnlLeftTop
            // 
            this.pnlLeftTop.Controls.Add(this.lblLogon);
            this.pnlLeftTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftTop.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftTop.Name = "pnlLeftTop";
            this.pnlLeftTop.Size = new System.Drawing.Size(215, 22);
            this.pnlLeftTop.TabIndex = 0;
            // 
            // lblLogon
            // 
            this.lblLogon.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLogon.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblLogon.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblLogon.Location = new System.Drawing.Point(0, 0);
            this.lblLogon.Name = "lblLogon";
            this.lblLogon.Size = new System.Drawing.Size(215, 21);
            this.lblLogon.TabIndex = 0;
            this.lblLogon.Text = "Пользователи";
            this.lblLogon.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblLogon.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // pnlBttm
            // 
            this.pnlBttm.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBttm.Location = new System.Drawing.Point(0, 299);
            this.pnlBttm.Name = "pnlBttm";
            this.pnlBttm.Size = new System.Drawing.Size(632, 33);
            this.pnlBttm.TabIndex = 2;
            this.pnlBttm.Visible = false;
            this.pnlBttm.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // pnlFill
            // 
            this.pnlFill.Controls.Add(this.pnlFillFill);
            this.pnlFill.Controls.Add(this.pnlFillLeft);
            this.pnlFill.Controls.Add(this.pnlFillBtm);
            this.pnlFill.Controls.Add(this.lblLogonEdit);
            this.pnlFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFill.Location = new System.Drawing.Point(215, 24);
            this.pnlFill.Name = "pnlFill";
            this.pnlFill.Size = new System.Drawing.Size(417, 275);
            this.pnlFill.TabIndex = 3;
            // 
            // pnlFillFill
            // 
            this.pnlFillFill.Controls.Add(this.txtBoxLogonEdit);
            this.pnlFillFill.Controls.Add(this.cmbBoxRoleEdit);
            this.pnlFillFill.Controls.Add(this.txtBoxDescriptionEdit);
            this.pnlFillFill.Controls.Add(this.txtBoxPasswordEdit);
            this.pnlFillFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFillFill.Location = new System.Drawing.Point(136, 22);
            this.pnlFillFill.Name = "pnlFillFill";
            this.pnlFillFill.Size = new System.Drawing.Size(281, 215);
            this.pnlFillFill.TabIndex = 4;
            // 
            // txtBoxLogonEdit
            // 
            this.txtBoxLogonEdit.Location = new System.Drawing.Point(6, 4);
            this.txtBoxLogonEdit.Name = "txtBoxLogonEdit";
            this.txtBoxLogonEdit.ReadOnly = true;
            this.txtBoxLogonEdit.Size = new System.Drawing.Size(261, 20);
            this.txtBoxLogonEdit.TabIndex = 0;
            this.txtBoxLogonEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // cmbBoxRoleEdit
            // 
            this.cmbBoxRoleEdit.Enabled = false;
            this.cmbBoxRoleEdit.FormattingEnabled = true;
            this.cmbBoxRoleEdit.Location = new System.Drawing.Point(6, 56);
            this.cmbBoxRoleEdit.Name = "cmbBoxRoleEdit";
            this.cmbBoxRoleEdit.Size = new System.Drawing.Size(261, 21);
            this.cmbBoxRoleEdit.TabIndex = 2;
            this.cmbBoxRoleEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // txtBoxDescriptionEdit
            // 
            this.txtBoxDescriptionEdit.Location = new System.Drawing.Point(6, 83);
            this.txtBoxDescriptionEdit.Multiline = true;
            this.txtBoxDescriptionEdit.Name = "txtBoxDescriptionEdit";
            this.txtBoxDescriptionEdit.Size = new System.Drawing.Size(261, 116);
            this.txtBoxDescriptionEdit.TabIndex = 3;
            this.txtBoxDescriptionEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // txtBoxPasswordEdit
            // 
            this.txtBoxPasswordEdit.Location = new System.Drawing.Point(6, 30);
            this.txtBoxPasswordEdit.Name = "txtBoxPasswordEdit";
            this.txtBoxPasswordEdit.ReadOnly = true;
            this.txtBoxPasswordEdit.Size = new System.Drawing.Size(261, 20);
            this.txtBoxPasswordEdit.TabIndex = 1;
            this.txtBoxPasswordEdit.UseSystemPasswordChar = true;
            this.txtBoxPasswordEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // pnlFillLeft
            // 
            this.pnlFillLeft.Controls.Add(this.lblDescriptionEdit);
            this.pnlFillLeft.Controls.Add(this.lblPasswordEditPnlTop);
            this.pnlFillLeft.Controls.Add(this.lblRoleEdit);
            this.pnlFillLeft.Controls.Add(this.lblPasswordEdit);
            this.pnlFillLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlFillLeft.Location = new System.Drawing.Point(0, 22);
            this.pnlFillLeft.Name = "pnlFillLeft";
            this.pnlFillLeft.Size = new System.Drawing.Size(136, 215);
            this.pnlFillLeft.TabIndex = 2;
            this.pnlFillLeft.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // lblDescriptionEdit
            // 
            this.lblDescriptionEdit.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblDescriptionEdit.Location = new System.Drawing.Point(3, 83);
            this.lblDescriptionEdit.Name = "lblDescriptionEdit";
            this.lblDescriptionEdit.Size = new System.Drawing.Size(127, 20);
            this.lblDescriptionEdit.TabIndex = 4;
            this.lblDescriptionEdit.Text = "Описание";
            this.lblDescriptionEdit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblDescriptionEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // lblPasswordEditPnlTop
            // 
            this.lblPasswordEditPnlTop.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblPasswordEditPnlTop.Location = new System.Drawing.Point(3, 4);
            this.lblPasswordEditPnlTop.Name = "lblPasswordEditPnlTop";
            this.lblPasswordEditPnlTop.Size = new System.Drawing.Size(127, 20);
            this.lblPasswordEditPnlTop.TabIndex = 3;
            this.lblPasswordEditPnlTop.Text = "Логин";
            this.lblPasswordEditPnlTop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPasswordEditPnlTop.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // lblRoleEdit
            // 
            this.lblRoleEdit.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblRoleEdit.Location = new System.Drawing.Point(3, 57);
            this.lblRoleEdit.Name = "lblRoleEdit";
            this.lblRoleEdit.Size = new System.Drawing.Size(127, 20);
            this.lblRoleEdit.TabIndex = 3;
            this.lblRoleEdit.Text = "Роль";
            this.lblRoleEdit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRoleEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // lblPasswordEdit
            // 
            this.lblPasswordEdit.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblPasswordEdit.Location = new System.Drawing.Point(3, 30);
            this.lblPasswordEdit.Name = "lblPasswordEdit";
            this.lblPasswordEdit.Size = new System.Drawing.Size(127, 20);
            this.lblPasswordEdit.TabIndex = 2;
            this.lblPasswordEdit.Text = "Пароль";
            this.lblPasswordEdit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPasswordEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // pnlFillBtm
            // 
            this.pnlFillBtm.Controls.Add(this.btnDelete);
            this.pnlFillBtm.Controls.Add(this.btnSave);
            this.pnlFillBtm.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFillBtm.Location = new System.Drawing.Point(0, 237);
            this.pnlFillBtm.Name = "pnlFillBtm";
            this.pnlFillBtm.Size = new System.Drawing.Size(417, 38);
            this.pnlFillBtm.TabIndex = 3;
            this.pnlFillBtm.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(43, 7);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(93, 23);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "Удалить";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Visible = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            this.btnDelete.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(310, 7);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(93, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnSave.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // lblLogonEdit
            // 
            this.lblLogonEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLogonEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblLogonEdit.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblLogonEdit.Location = new System.Drawing.Point(0, 0);
            this.lblLogonEdit.Name = "lblLogonEdit";
            this.lblLogonEdit.Size = new System.Drawing.Size(417, 22);
            this.lblLogonEdit.TabIndex = 1;
            this.lblLogonEdit.Text = "Создание / Редактирование";
            this.lblLogonEdit.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblLogonEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // FUsers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 332);
            this.Controls.Add(this.pnlFill);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlBttm);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FUsers";
            this.Text = "Управление пользователями";
            this.Load += new System.EventHandler(this.FUsers_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dGViewLogon)).EndInit();
            this.pnlLeftTop.ResumeLayout(false);
            this.pnlFill.ResumeLayout(false);
            this.pnlFillFill.ResumeLayout(false);
            this.pnlFillFill.PerformLayout();
            this.pnlFillLeft.ResumeLayout(false);
            this.pnlFillBtm.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.DataGridView dGViewLogon;
        private System.Windows.Forms.Panel pnlLeftTop;
        private System.Windows.Forms.Label lblLogon;
        private System.Windows.Forms.Panel pnlBttm;
        private System.Windows.Forms.Panel pnlFill;
        private System.Windows.Forms.DataGridViewTextBoxColumn Logon;
        private System.Windows.Forms.Label lblLogonEdit;
        private System.Windows.Forms.Panel pnlFillFill;
        private System.Windows.Forms.TextBox txtBoxPasswordEdit;
        private System.Windows.Forms.Panel pnlFillLeft;
        private System.Windows.Forms.Label lblPasswordEdit;
        private System.Windows.Forms.Panel pnlFillBtm;
        private System.Windows.Forms.ComboBox cmbBoxRoleEdit;
        private System.Windows.Forms.TextBox txtBoxDescriptionEdit;
        private System.Windows.Forms.Label lblDescriptionEdit;
        private System.Windows.Forms.Label lblRoleEdit;
        private System.Windows.Forms.Label lblPasswordEditPnlTop;
        private System.Windows.Forms.TextBox txtBoxLogonEdit;
        private System.Windows.Forms.ToolStripMenuItem TSMItemAddUser;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
    }
}