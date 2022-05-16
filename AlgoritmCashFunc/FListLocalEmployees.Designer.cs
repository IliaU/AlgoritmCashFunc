namespace AlgoritmCashFunc
{
    partial class FListLocalEmployees
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
            this.components = new System.ComponentModel.Container();
            this.dgData = new System.Windows.Forms.DataGridView();
            this.cntxMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tlStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlFill = new System.Windows.Forms.Panel();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.CId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgData)).BeginInit();
            this.cntxMenuStrip.SuspendLayout();
            this.pnlFill.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgData
            // 
            this.dgData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CId,
            this.ColData});
            this.dgData.ContextMenuStrip = this.cntxMenuStrip;
            this.dgData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgData.Location = new System.Drawing.Point(0, 0);
            this.dgData.Name = "dgData";
            this.dgData.Size = new System.Drawing.Size(655, 461);
            this.dgData.TabIndex = 0;
            this.dgData.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgData_CellMouseEnter);
            // 
            // cntxMenuStrip
            // 
            this.cntxMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlStripMenuItemDelete});
            this.cntxMenuStrip.Name = "cntxMenuStrip";
            this.cntxMenuStrip.Size = new System.Drawing.Size(119, 26);
            // 
            // tlStripMenuItemDelete
            // 
            this.tlStripMenuItemDelete.Name = "tlStripMenuItemDelete";
            this.tlStripMenuItemDelete.Size = new System.Drawing.Size(118, 22);
            this.tlStripMenuItemDelete.Text = "Удалить";
            this.tlStripMenuItemDelete.Click += new System.EventHandler(this.tlStripMenuItemDelete_Click);
            // 
            // pnlFill
            // 
            this.pnlFill.Controls.Add(this.dgData);
            this.pnlFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFill.Location = new System.Drawing.Point(0, 0);
            this.pnlFill.Name = "pnlFill";
            this.pnlFill.Size = new System.Drawing.Size(655, 461);
            this.pnlFill.TabIndex = 2;
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.btnSave);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 461);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(655, 32);
            this.pnlBottom.TabIndex = 3;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(568, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // CId
            // 
            this.CId.DataPropertyName = "CId";
            this.CId.HeaderText = "Id";
            this.CId.Name = "CId";
            this.CId.ReadOnly = true;
            this.CId.Visible = false;
            // 
            // ColData
            // 
            this.ColData.DataPropertyName = "ColData";
            this.ColData.HeaderText = "Сотрудники";
            this.ColData.Name = "ColData";
            this.ColData.Width = 600;
            // 
            // FListLocalEmployees
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 493);
            this.Controls.Add(this.pnlFill);
            this.Controls.Add(this.pnlBottom);
            this.Name = "FListLocalEmployees";
            this.Text = "Сотрудники";
            ((System.ComponentModel.ISupportInitialize)(this.dgData)).EndInit();
            this.cntxMenuStrip.ResumeLayout(false);
            this.pnlFill.ResumeLayout(false);
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgData;
        private System.Windows.Forms.Panel pnlFill;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ContextMenuStrip cntxMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem tlStripMenuItemDelete;
        private System.Windows.Forms.DataGridViewTextBoxColumn CId;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColData;
    }
}