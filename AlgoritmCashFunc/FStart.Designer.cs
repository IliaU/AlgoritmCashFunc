namespace AlgoritmCashFunc
{
    partial class FStart
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FStart));
            this.pnlTop = new System.Windows.Forms.Panel();
            this.pnlTopFill = new System.Windows.Forms.Panel();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.pnlTopRight = new System.Windows.Forms.Panel();
            this.btnExit = new System.Windows.Forms.Button();
            this.butnOperator = new System.Windows.Forms.Button();
            this.picBoxAKS = new System.Windows.Forms.PictureBox();
            this.picBoxPrizm = new System.Windows.Forms.PictureBox();
            this.pnlFill = new System.Windows.Forms.Panel();
            this.tabCntOperation = new System.Windows.Forms.TabControl();
            this.tabPagePrihod = new System.Windows.Forms.TabPage();
            this.tabPageRashod = new System.Windows.Forms.TabPage();
            this.tabPageCashBook = new System.Windows.Forms.TabPage();
            this.tabPageVozvrat = new System.Windows.Forms.TabPage();
            this.tabPageReportCash = new System.Windows.Forms.TabPage();
            this.tabPageSequensKKM = new System.Windows.Forms.TabPage();
            this.tabPageCheck = new System.Windows.Forms.TabPage();
            this.tabPageInvent = new System.Windows.Forms.TabPage();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tSSLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.TSMItemSetup = new System.Windows.Forms.ToolStripMenuItem();
            this.TSMItemAboutRep = new System.Windows.Forms.ToolStripMenuItem();
            this.TSMItemConfigPrv = new System.Windows.Forms.ToolStripMenuItem();
            this.TSMItemConfigUsers = new System.Windows.Forms.ToolStripMenuItem();
            this.TSMItemLic = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlTop.SuspendLayout();
            this.pnlTopFill.SuspendLayout();
            this.pnlTopRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxAKS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxPrizm)).BeginInit();
            this.pnlFill.SuspendLayout();
            this.tabCntOperation.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.pnlTopFill);
            this.pnlTop.Controls.Add(this.pnlTopRight);
            this.pnlTop.Controls.Add(this.picBoxAKS);
            this.pnlTop.Controls.Add(this.picBoxPrizm);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 24);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1034, 42);
            this.pnlTop.TabIndex = 0;
            // 
            // pnlTopFill
            // 
            this.pnlTopFill.Controls.Add(this.btnPrint);
            this.pnlTopFill.Controls.Add(this.btnSave);
            this.pnlTopFill.Controls.Add(this.btnNew);
            this.pnlTopFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTopFill.Location = new System.Drawing.Point(304, 0);
            this.pnlTopFill.Name = "pnlTopFill";
            this.pnlTopFill.Size = new System.Drawing.Size(406, 42);
            this.pnlTopFill.TabIndex = 3;
            // 
            // btnPrint
            // 
            this.btnPrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnPrint.Image = ((System.Drawing.Image)(resources.GetObject("btnPrint.Image")));
            this.btnPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPrint.Location = new System.Drawing.Point(260, 3);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(114, 36);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "Печать  ";
            this.btnPrint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnPrint.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSave.Location = new System.Drawing.Point(119, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(135, 36);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Сохранить";
            this.btnSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // btnNew
            // 
            this.btnNew.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnNew.Image = ((System.Drawing.Image)(resources.GetObject("btnNew.Image")));
            this.btnNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNew.Location = new System.Drawing.Point(6, 3);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(107, 36);
            this.btnNew.TabIndex = 0;
            this.btnNew.Text = "Новый  ";
            this.btnNew.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // pnlTopRight
            // 
            this.pnlTopRight.Controls.Add(this.btnExit);
            this.pnlTopRight.Controls.Add(this.butnOperator);
            this.pnlTopRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlTopRight.Location = new System.Drawing.Point(710, 0);
            this.pnlTopRight.Name = "pnlTopRight";
            this.pnlTopRight.Size = new System.Drawing.Size(324, 42);
            this.pnlTopRight.TabIndex = 2;
            // 
            // btnExit
            // 
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnExit.Image = ((System.Drawing.Image)(resources.GetObject("btnExit.Image")));
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExit.Location = new System.Drawing.Point(204, 3);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(108, 36);
            this.btnExit.TabIndex = 2;
            this.btnExit.Text = "Выход  ";
            this.btnExit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            this.btnExit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // butnOperator
            // 
            this.butnOperator.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butnOperator.Image = ((System.Drawing.Image)(resources.GetObject("butnOperator.Image")));
            this.butnOperator.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.butnOperator.Location = new System.Drawing.Point(6, 3);
            this.butnOperator.Name = "butnOperator";
            this.butnOperator.Size = new System.Drawing.Size(192, 36);
            this.butnOperator.TabIndex = 1;
            this.butnOperator.Text = "Смена оператора  ";
            this.butnOperator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.butnOperator.UseVisualStyleBackColor = true;
            this.butnOperator.Click += new System.EventHandler(this.butnOperator_Click);
            this.butnOperator.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // picBoxAKS
            // 
            this.picBoxAKS.Dock = System.Windows.Forms.DockStyle.Left;
            this.picBoxAKS.Image = ((System.Drawing.Image)(resources.GetObject("picBoxAKS.Image")));
            this.picBoxAKS.Location = new System.Drawing.Point(152, 0);
            this.picBoxAKS.Name = "picBoxAKS";
            this.picBoxAKS.Size = new System.Drawing.Size(152, 42);
            this.picBoxAKS.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picBoxAKS.TabIndex = 1;
            this.picBoxAKS.TabStop = false;
            this.picBoxAKS.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // picBoxPrizm
            // 
            this.picBoxPrizm.Dock = System.Windows.Forms.DockStyle.Left;
            this.picBoxPrizm.Image = ((System.Drawing.Image)(resources.GetObject("picBoxPrizm.Image")));
            this.picBoxPrizm.Location = new System.Drawing.Point(0, 0);
            this.picBoxPrizm.Name = "picBoxPrizm";
            this.picBoxPrizm.Size = new System.Drawing.Size(152, 42);
            this.picBoxPrizm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picBoxPrizm.TabIndex = 0;
            this.picBoxPrizm.TabStop = false;
            this.picBoxPrizm.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // pnlFill
            // 
            this.pnlFill.Controls.Add(this.tabCntOperation);
            this.pnlFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFill.Location = new System.Drawing.Point(0, 66);
            this.pnlFill.Name = "pnlFill";
            this.pnlFill.Size = new System.Drawing.Size(1034, 411);
            this.pnlFill.TabIndex = 1;
            // 
            // tabCntOperation
            // 
            this.tabCntOperation.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabCntOperation.Controls.Add(this.tabPagePrihod);
            this.tabCntOperation.Controls.Add(this.tabPageRashod);
            this.tabCntOperation.Controls.Add(this.tabPageCashBook);
            this.tabCntOperation.Controls.Add(this.tabPageVozvrat);
            this.tabCntOperation.Controls.Add(this.tabPageReportCash);
            this.tabCntOperation.Controls.Add(this.tabPageSequensKKM);
            this.tabCntOperation.Controls.Add(this.tabPageCheck);
            this.tabCntOperation.Controls.Add(this.tabPageInvent);
            this.tabCntOperation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCntOperation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabCntOperation.Location = new System.Drawing.Point(0, 0);
            this.tabCntOperation.Name = "tabCntOperation";
            this.tabCntOperation.SelectedIndex = 0;
            this.tabCntOperation.Size = new System.Drawing.Size(1034, 411);
            this.tabCntOperation.TabIndex = 0;
            this.tabCntOperation.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // tabPagePrihod
            // 
            this.tabPagePrihod.BackColor = System.Drawing.SystemColors.Control;
            this.tabPagePrihod.Location = new System.Drawing.Point(4, 4);
            this.tabPagePrihod.Name = "tabPagePrihod";
            this.tabPagePrihod.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePrihod.Size = new System.Drawing.Size(1026, 383);
            this.tabPagePrihod.TabIndex = 0;
            this.tabPagePrihod.Text = "Приходный ордер";
            // 
            // tabPageRashod
            // 
            this.tabPageRashod.Location = new System.Drawing.Point(4, 4);
            this.tabPageRashod.Name = "tabPageRashod";
            this.tabPageRashod.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRashod.Size = new System.Drawing.Size(1026, 383);
            this.tabPageRashod.TabIndex = 1;
            this.tabPageRashod.Text = "Расходный ордер";
            this.tabPageRashod.UseVisualStyleBackColor = true;
            // 
            // tabPageCashBook
            // 
            this.tabPageCashBook.Location = new System.Drawing.Point(4, 4);
            this.tabPageCashBook.Name = "tabPageCashBook";
            this.tabPageCashBook.Size = new System.Drawing.Size(1026, 383);
            this.tabPageCashBook.TabIndex = 2;
            this.tabPageCashBook.Text = "Кассовая книга";
            this.tabPageCashBook.UseVisualStyleBackColor = true;
            // 
            // tabPageVozvrat
            // 
            this.tabPageVozvrat.Location = new System.Drawing.Point(4, 4);
            this.tabPageVozvrat.Name = "tabPageVozvrat";
            this.tabPageVozvrat.Size = new System.Drawing.Size(1026, 383);
            this.tabPageVozvrat.TabIndex = 3;
            this.tabPageVozvrat.Text = "Акт о возврате денег";
            this.tabPageVozvrat.UseVisualStyleBackColor = true;
            // 
            // tabPageReportCash
            // 
            this.tabPageReportCash.Location = new System.Drawing.Point(4, 4);
            this.tabPageReportCash.Name = "tabPageReportCash";
            this.tabPageReportCash.Size = new System.Drawing.Size(1026, 383);
            this.tabPageReportCash.TabIndex = 4;
            this.tabPageReportCash.Text = "Отчёт кассира";
            this.tabPageReportCash.UseVisualStyleBackColor = true;
            // 
            // tabPageSequensKKM
            // 
            this.tabPageSequensKKM.Location = new System.Drawing.Point(4, 4);
            this.tabPageSequensKKM.Name = "tabPageSequensKKM";
            this.tabPageSequensKKM.Size = new System.Drawing.Size(1026, 383);
            this.tabPageSequensKKM.TabIndex = 5;
            this.tabPageSequensKKM.Text = "Счётчики ККМ";
            this.tabPageSequensKKM.UseVisualStyleBackColor = true;
            // 
            // tabPageCheck
            // 
            this.tabPageCheck.Location = new System.Drawing.Point(4, 4);
            this.tabPageCheck.Name = "tabPageCheck";
            this.tabPageCheck.Size = new System.Drawing.Size(1026, 383);
            this.tabPageCheck.TabIndex = 6;
            this.tabPageCheck.Text = "Проверка наличных";
            this.tabPageCheck.UseVisualStyleBackColor = true;
            // 
            // tabPageInvent
            // 
            this.tabPageInvent.Location = new System.Drawing.Point(4, 4);
            this.tabPageInvent.Name = "tabPageInvent";
            this.tabPageInvent.Size = new System.Drawing.Size(1026, 383);
            this.tabPageInvent.TabIndex = 7;
            this.tabPageInvent.Text = "Инвентаризация средств";
            this.tabPageInvent.UseVisualStyleBackColor = true;
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.statusStrip1);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 477);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(1034, 20);
            this.pnlBottom.TabIndex = 2;
            this.pnlBottom.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tSSLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, -2);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1034, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tSSLabel
            // 
            this.tSSLabel.Name = "tSSLabel";
            this.tSSLabel.Size = new System.Drawing.Size(27, 17);
            this.tSSLabel.Text = "Лог";
            this.tSSLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSMItemSetup});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1034, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ActiveStatusLogon_MouseMove);
            // 
            // TSMItemSetup
            // 
            this.TSMItemSetup.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSMItemAboutRep,
            this.TSMItemConfigPrv,
            this.TSMItemConfigUsers,
            this.TSMItemLic});
            this.TSMItemSetup.Name = "TSMItemSetup";
            this.TSMItemSetup.Size = new System.Drawing.Size(78, 20);
            this.TSMItemSetup.Text = "Настройка";
            // 
            // TSMItemAboutRep
            // 
            this.TSMItemAboutRep.Name = "TSMItemAboutRep";
            this.TSMItemAboutRep.Size = new System.Drawing.Size(291, 22);
            this.TSMItemAboutRep.Text = "Список доступных провайдеров";
            // 
            // TSMItemConfigPrv
            // 
            this.TSMItemConfigPrv.Name = "TSMItemConfigPrv";
            this.TSMItemConfigPrv.Size = new System.Drawing.Size(291, 22);
            this.TSMItemConfigPrv.Text = "Hастройка подключения к базе данных";
            this.TSMItemConfigPrv.Click += new System.EventHandler(this.TSMItemConfigPrv_Click);
            // 
            // TSMItemConfigUsers
            // 
            this.TSMItemConfigUsers.Name = "TSMItemConfigUsers";
            this.TSMItemConfigUsers.Size = new System.Drawing.Size(291, 22);
            this.TSMItemConfigUsers.Text = "Настройка списка пользователей";
            this.TSMItemConfigUsers.Click += new System.EventHandler(this.TSMItemConfigUsers_Click);
            // 
            // TSMItemLic
            // 
            this.TSMItemLic.Name = "TSMItemLic";
            this.TSMItemLic.Size = new System.Drawing.Size(291, 22);
            this.TSMItemLic.Text = "Информация по лицензии";
            this.TSMItemLic.Click += new System.EventHandler(this.TSMItemLic_Click);
            // 
            // FStart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1034, 497);
            this.Controls.Add(this.pnlFill);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FStart";
            this.Text = "Кассовые функции RetailPro Prism";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FStart_FormClosing);
            this.pnlTop.ResumeLayout(false);
            this.pnlTopFill.ResumeLayout(false);
            this.pnlTopRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picBoxAKS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxPrizm)).EndInit();
            this.pnlFill.ResumeLayout(false);
            this.tabCntOperation.ResumeLayout(false);
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.PictureBox picBoxAKS;
        private System.Windows.Forms.PictureBox picBoxPrizm;
        private System.Windows.Forms.Panel pnlFill;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Panel pnlTopFill;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Panel pnlTopRight;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button butnOperator;
        private System.Windows.Forms.TabControl tabCntOperation;
        private System.Windows.Forms.TabPage tabPagePrihod;
        private System.Windows.Forms.TabPage tabPageRashod;
        private System.Windows.Forms.TabPage tabPageCashBook;
        private System.Windows.Forms.TabPage tabPageVozvrat;
        private System.Windows.Forms.TabPage tabPageReportCash;
        private System.Windows.Forms.TabPage tabPageSequensKKM;
        private System.Windows.Forms.TabPage tabPageCheck;
        private System.Windows.Forms.TabPage tabPageInvent;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem TSMItemSetup;
        private System.Windows.Forms.ToolStripMenuItem TSMItemAboutRep;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tSSLabel;
        private System.Windows.Forms.ToolStripMenuItem TSMItemConfigPrv;
        private System.Windows.Forms.ToolStripMenuItem TSMItemConfigUsers;
        private System.Windows.Forms.ToolStripMenuItem TSMItemLic;
    }
}

