namespace AlgoritmCashFunc
{
    partial class FListLocalPaidInReasons
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
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.dgData = new System.Windows.Forms.DataGridView();
            this.pnlFill = new System.Windows.Forms.Panel();
            this.CId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Osnovanie = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DebetNomerSchet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.KredikKorSchet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgData)).BeginInit();
            this.pnlFill.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.btnSave);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 461);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(767, 32);
            this.pnlBottom.TabIndex = 5;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(689, 6);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // dgData
            // 
            this.dgData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CId,
            this.ColData,
            this.Osnovanie,
            this.DebetNomerSchet,
            this.KredikKorSchet});
            this.dgData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgData.Location = new System.Drawing.Point(0, 0);
            this.dgData.Name = "dgData";
            this.dgData.Size = new System.Drawing.Size(767, 493);
            this.dgData.TabIndex = 0;
            // 
            // pnlFill
            // 
            this.pnlFill.Controls.Add(this.dgData);
            this.pnlFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFill.Location = new System.Drawing.Point(0, 0);
            this.pnlFill.Name = "pnlFill";
            this.pnlFill.Size = new System.Drawing.Size(767, 493);
            this.pnlFill.TabIndex = 4;
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
            this.ColData.DataPropertyName = "LocalName";
            this.ColData.HeaderText = "Текст основания";
            this.ColData.Name = "ColData";
            this.ColData.Width = 400;
            // 
            // Osnovanie
            // 
            this.Osnovanie.DataPropertyName = "Osnovanie";
            this.Osnovanie.HeaderText = "Основание";
            this.Osnovanie.Name = "Osnovanie";
            // 
            // DebetNomerSchet
            // 
            this.DebetNomerSchet.DataPropertyName = "DebetNomerSchet";
            this.DebetNomerSchet.HeaderText = "Деб № сч";
            this.DebetNomerSchet.Name = "DebetNomerSchet";
            // 
            // KredikKorSchet
            // 
            this.KredikKorSchet.DataPropertyName = "KredikKorSchet";
            this.KredikKorSchet.HeaderText = "Кред № сч";
            this.KredikKorSchet.Name = "KredikKorSchet";
            // 
            // FListLocalPaidInReasons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(767, 493);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlFill);
            this.Name = "FListLocalPaidInReasons";
            this.Text = "Список оснований для прихода";
            this.pnlBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgData)).EndInit();
            this.pnlFill.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridView dgData;
        private System.Windows.Forms.Panel pnlFill;
        private System.Windows.Forms.DataGridViewTextBoxColumn CId;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColData;
        private System.Windows.Forms.DataGridViewTextBoxColumn Osnovanie;
        private System.Windows.Forms.DataGridViewTextBoxColumn DebetNomerSchet;
        private System.Windows.Forms.DataGridViewTextBoxColumn KredikKorSchet;
    }
}