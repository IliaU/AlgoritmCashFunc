namespace AlgoritmCashFunc
{
    partial class FListLocalPaidRashReasons
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
            this.KreditNomerSchet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DebetKorSchet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FlagFormReturn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
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
            this.pnlBottom.Size = new System.Drawing.Size(885, 32);
            this.pnlBottom.TabIndex = 7;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(798, 6);
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
            this.KreditNomerSchet,
            this.DebetKorSchet,
            this.FlagFormReturn});
            this.dgData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgData.Location = new System.Drawing.Point(0, 0);
            this.dgData.Name = "dgData";
            this.dgData.Size = new System.Drawing.Size(885, 493);
            this.dgData.TabIndex = 0;
            // 
            // pnlFill
            // 
            this.pnlFill.Controls.Add(this.dgData);
            this.pnlFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFill.Location = new System.Drawing.Point(0, 0);
            this.pnlFill.Name = "pnlFill";
            this.pnlFill.Size = new System.Drawing.Size(885, 493);
            this.pnlFill.TabIndex = 6;
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
            // KreditNomerSchet
            // 
            this.KreditNomerSchet.DataPropertyName = "KreditNomerSchet";
            this.KreditNomerSchet.HeaderText = "Кред № сч";
            this.KreditNomerSchet.Name = "KreditNomerSchet";
            // 
            // DebetKorSchet
            // 
            this.DebetKorSchet.DataPropertyName = "DebetKorSchet";
            this.DebetKorSchet.HeaderText = "Деб кор сч";
            this.DebetKorSchet.Name = "DebetKorSchet";
            // 
            // FlagFormReturn
            // 
            this.FlagFormReturn.DataPropertyName = "FlagFormReturn";
            this.FlagFormReturn.HeaderText = "Печ заяв на возв";
            this.FlagFormReturn.Name = "FlagFormReturn";
            this.FlagFormReturn.Width = 120;
            // 
            // FListLocalPaidRashReasons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(885, 493);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlFill);
            this.Name = "FListLocalPaidRashReasons";
            this.Text = "Список оснований для расхода";
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
        private System.Windows.Forms.DataGridViewTextBoxColumn KreditNomerSchet;
        private System.Windows.Forms.DataGridViewTextBoxColumn DebetKorSchet;
        private System.Windows.Forms.DataGridViewCheckBoxColumn FlagFormReturn;
    }
}