using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AlgoritmCashFunc.Com;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc
{
    public partial class FStart : Form
    {
        public FStart()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при загрузке формы FConfig с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
            
        }

        // Пользователь закрывает форму
        private void FStart_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (Com.UserFarm.CurrentUser!=null) this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при чтении конфигурации с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.FStart_FormClosing", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        // Пользователь решил поменять логин под кем зашли
        private void butnOperator_Click(object sender, EventArgs e)
        {
            try
            {
                Com.UserFarm.LogOff();
                this.DialogResult = DialogResult.No;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при чтении конфигурации с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.FStart_FormClosing", GetType().Name), EventEn.Error);
                throw ae;
            }
        }
    }
}
