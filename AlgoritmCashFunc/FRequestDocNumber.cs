using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AlgoritmCashFunc.BLL.DocumentPlg;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc
{
    public partial class FRequestDocNumber : Form
    {
        private DocumentRashod Doc;
        public int DocNumber = 0;

        public FRequestDocNumber(DocumentRashod Doc)
        {
            try
            {
                this.Doc = Doc;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации конструктора с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.btnCancel_Click", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    this.DocNumber = int.Parse(this.txtBoxDocNumber.Text);
                }
                catch (Exception)
                {
                    throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} в целое число.", this.txtBoxDocNumber));
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.btnSelect_Click", GetType().Name), EventEn.Error, true, true);
            }
        }
    }
}
