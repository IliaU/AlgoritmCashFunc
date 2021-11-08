namespace AlgoritmCashFunc
{
    partial class Start
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Start));
            this.pnlTop = new System.Windows.Forms.Panel();
            this.pnlTopFill = new System.Windows.Forms.Panel();
            this.btnNew = new System.Windows.Forms.Button();
            this.pnlTopRight = new System.Windows.Forms.Panel();
            this.picBoxAKS = new System.Windows.Forms.PictureBox();
            this.picBoxPrizm = new System.Windows.Forms.PictureBox();
            this.pnlFill = new System.Windows.Forms.Panel();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.butnOperator = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.tabCntOperation = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.pnlTop.SuspendLayout();
            this.pnlTopFill.SuspendLayout();
            this.pnlTopRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxAKS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxPrizm)).BeginInit();
            this.pnlFill.SuspendLayout();
            this.tabCntOperation.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.pnlTopFill);
            this.pnlTop.Controls.Add(this.pnlTopRight);
            this.pnlTop.Controls.Add(this.picBoxAKS);
            this.pnlTop.Controls.Add(this.picBoxPrizm);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(952, 42);
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
            this.pnlTopFill.Size = new System.Drawing.Size(388, 42);
            this.pnlTopFill.TabIndex = 3;
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
            // 
            // pnlTopRight
            // 
            this.pnlTopRight.Controls.Add(this.btnExit);
            this.pnlTopRight.Controls.Add(this.butnOperator);
            this.pnlTopRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlTopRight.Location = new System.Drawing.Point(692, 0);
            this.pnlTopRight.Name = "pnlTopRight";
            this.pnlTopRight.Size = new System.Drawing.Size(260, 42);
            this.pnlTopRight.TabIndex = 2;
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
            // 
            // pnlFill
            // 
            this.pnlFill.Controls.Add(this.tabCntOperation);
            this.pnlFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFill.Location = new System.Drawing.Point(0, 42);
            this.pnlFill.Name = "pnlFill";
            this.pnlFill.Size = new System.Drawing.Size(952, 435);
            this.pnlFill.TabIndex = 1;
            // 
            // pnlBottom
            // 
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 477);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(952, 20);
            this.pnlBottom.TabIndex = 2;
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
            // butnOperator
            // 
            this.butnOperator.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butnOperator.Image = ((System.Drawing.Image)(resources.GetObject("butnOperator.Image")));
            this.butnOperator.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.butnOperator.Location = new System.Drawing.Point(6, 3);
            this.butnOperator.Name = "butnOperator";
            this.butnOperator.Size = new System.Drawing.Size(133, 36);
            this.butnOperator.TabIndex = 1;
            this.butnOperator.Text = "Оператор  ";
            this.butnOperator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.butnOperator.UseVisualStyleBackColor = true;
            // 
            // btnExit
            // 
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnExit.Image = ((System.Drawing.Image)(resources.GetObject("btnExit.Image")));
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExit.Location = new System.Drawing.Point(145, 3);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(108, 36);
            this.btnExit.TabIndex = 2;
            this.btnExit.Text = "Выход  ";
            this.btnExit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExit.UseVisualStyleBackColor = true;
            // 
            // tabCntOperation
            // 
            this.tabCntOperation.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabCntOperation.Controls.Add(this.tabPage1);
            this.tabCntOperation.Controls.Add(this.tabPage2);
            this.tabCntOperation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCntOperation.Location = new System.Drawing.Point(0, 0);
            this.tabCntOperation.Name = "tabCntOperation";
            this.tabCntOperation.SelectedIndex = 0;
            this.tabCntOperation.Size = new System.Drawing.Size(952, 435);
            this.tabCntOperation.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(944, 409);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(944, 389);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // Start
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 497);
            this.Controls.Add(this.pnlFill);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.Name = "Start";
            this.Text = "Кассовые функции RetailPro Prism";
            this.pnlTop.ResumeLayout(false);
            this.pnlTopFill.ResumeLayout(false);
            this.pnlTopRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picBoxAKS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxPrizm)).EndInit();
            this.pnlFill.ResumeLayout(false);
            this.tabCntOperation.ResumeLayout(false);
            this.ResumeLayout(false);

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
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
    }
}

