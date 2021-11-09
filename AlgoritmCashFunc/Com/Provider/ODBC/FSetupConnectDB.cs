using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.Odbc;

namespace AlgoritmCashFunc.Com.Provider.ODBC
{
        public partial class FSetupConnectDB : Form
    {
        private ODBCprv Prv;
        public string ConnectionString;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Prv">Класс который представляет из себя данный тип провайдера</param>
        public FSetupConnectDB(ODBCprv Prv)
        {
            this.Prv = Prv;
            InitializeComponent();

            // Установка значаний в элементы из текущих указанных в базовом классе
            if(Prv.ConnectionString!=null && Prv.ConnectionString!=string.Empty)
            {
                this.Prv.Ocsb = new OdbcConnectionStringBuilder(Prv.ConnectionString);
                this.txtBox_DSN_ODBC.Text = this.Prv.Ocsb.Dsn;
                object U;
                this.Prv.Ocsb.TryGetValue("Uid",out U);
                this.txtBox_Login_ODBC.Text = U.ToString();
                object P;
                this.Prv.Ocsb.TryGetValue("Pwd", out P);
                this.txtBox_Passvord_ODBC.Text = P.ToString();
            }
        }


        private void FSetupConnectDB_Load(object sender, EventArgs e)
        {
            //this.ConnectionString=this.Prv.InstalProvider("DW_MON", "monitoring", "ware22mon");
        }

        // Пользователь решил протестировать соединение
        private void btnTestODBC_Click(object sender, EventArgs e)
        {
            this.ConnectionString = this.Prv.InstalProvider(this.txtBox_DSN_ODBC.Text, this.txtBox_Login_ODBC.Text, this.txtBox_Passvord_ODBC.Text, true, false, false);
            if (this.ConnectionString != null && this.ConnectionString.Trim() != string.Empty) MessageBox.Show("Тестирование подключения завершилось успешно");
        }

        // Пользователь сохраняет подключение
        private void btnSaveODBC_Click(object sender, EventArgs e)
        {
            this.ConnectionString = this.Prv.InstalProvider(this.txtBox_DSN_ODBC.Text, this.txtBox_Login_ODBC.Text, this.txtBox_Passvord_ODBC.Text, true, true, true);
            if (this.ConnectionString != null && this.ConnectionString.Trim() != string.Empty) this.DialogResult = DialogResult.Yes;
            else this.DialogResult = DialogResult.No;

            this.Close();
        }
    }
}
