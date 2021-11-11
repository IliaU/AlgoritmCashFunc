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
        private Color DefBaskCoclortSSLabel;
        private object LockObj = new object();

        /// <summary>
        /// Для того чтобы статус в нижней части работал последлвательно
        /// </summary>
        private object LockEventLog = new object();

        // Конструктор
        public FStart()
        {
            try
            {
                InitializeComponent();
                this.DefBaskCoclortSSLabel = this.tSSLabel.BackColor;

                //Инициируем механизм который проверяет необходимость блокировки пользователя
                UserFarm.ActiveStatusLogon();
                UserFarm.onEventLogOFF += UserFarm_onEventLogOFF;

                // Проверяем роль и если админ то даем править менюшку
                if (Com.UserFarm.CurrentUser.Role == RoleEn.Admin)
                {
                    this.menuStrip1.Visible = true;

                    // Подключаем список доступных репозиториев для подключения ToolStripMenuItem
                    // TSMItemAboutPrv
                    foreach (string item in Com.ProviderFarm.ListProviderName())
                    {
                        try
                        {
                            this.TSMItemAboutRep.DropDownItems.Add((new Lib.UProvider(item)).InfoToolStripMenuItem());
                        }
                        catch (Exception ex)
                        {
                            Com.Log.EventSave(string.Format("Не смогли загрузить провайдер с типом {0} ({1})", item, ex.Message), "FStart", Lib.EventEn.Error, true, false);
                        }
                    }
                }
                else this.menuStrip1.Visible = false;

                // Подписываемся на события
                Com.Log.onEventLog += Log_onEventLog;
                Log.EventSave(string.Format("Вход под пользователем {0} ({1})", Com.UserFarm.CurrentUser.Logon, Com.UserFarm.CurrentUser.Role.ToString()), GetType().Name, EventEn.Message);

            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при загрузке формы FConfig с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        #region Основные методы которые нужны для блокировки пользователя если он не активен
        /// <summary>
        ///  Произошло событие блокировки пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserFarm_onEventLogOFF(object sender, EventLogOFF e)
        {
            try
            {
                // Нажымаем кнопку смены оператора
                this.butnOperator_Click(null, null);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при чтении конфигурации с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.FStart_FormClosing", GetType().Name), EventEn.Error);
                throw ae;
            }
        }
        
        // Всовываем на всех ледементах чтобы смотерть активность пользователя на движение мышки
        private void ActiveStatusLogon_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                UserFarm.ActiveStatusLogon();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при регистрации активности пользователя с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.ActiveStatusLogon_MouseMove", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        #endregion

        /// <summary>
        /// Произошло событие системное правим текущий статус
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        delegate void delig_Log_onEventLog(object sender, Lib.EventLog e);
        /// <summary>
        /// Логирование системного события
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Log_onEventLog(object sender, Lib.EventLog e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    lock (this.LockEventLog)
                    {
                        delig_Log_onEventLog dl = new delig_Log_onEventLog(Log_onEventLog);
                        this.Invoke(dl, new object[] { sender, e });
                    }
                }
                else
                {
                    lock (this.LockObj)
                    {
                        if (e != null)
                        {
                            switch (e.Evn)
                            {
                                case Lib.EventEn.Empty:
                                case Lib.EventEn.Dump:
                                    break;
                                case Lib.EventEn.Warning:
                                    this.tSSLabel.BackColor = Color.Khaki;
                                    this.tSSLabel.Text = e.Message;
                                    break;
                                case Lib.EventEn.Error:
                                case Lib.EventEn.FatalError:
                                    this.tSSLabel.BackColor = Color.Tomato;
                                    this.tSSLabel.Text = e.Message;
                                    break;
                                default:
                                    this.tSSLabel.BackColor = this.DefBaskCoclortSSLabel;
                                    this.tSSLabel.Text = e.Message;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Com.Log.EventSave(string.Format(@"Ошибка в методе {0}:""{1}""", "Log_onEventLog", ex.Message), this.GetType().FullName, EventEn.Error, true, true);
            }
        }

        // Пользователь закрывает форму
        private void FStart_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                UserFarm.onEventLogOFF -= UserFarm_onEventLogOFF;

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
                ApplicationException ae = new ApplicationException(string.Format("Упали при попытке сменить оператора с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.butnOperator_Click", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        // Пользователь закрывает форму кнопкой
        private void btnExit_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при закрытии формы с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.btnExit_Click", GetType().Name), EventEn.Error);
                throw ae;
            }
        }


        #region Собятия вызванные выбором в верхнем меню
        // Пользователь решил настроить список пользователей
        private void TSMItemConfigUsers_Click(object sender, EventArgs e)
        {
            try
            {
                using (FUsers Frm = new FUsers())
                {
                    Frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Com.Log.EventSave(string.Format(@"Ошибка в методе {0}:""{1}""", "TSMItemConfigUsers_Click", ex.Message), this.GetType().FullName, EventEn.Error, true, true);
            }
        }

        // Пользователь решил поправить подключение к репозиторию
        private void TSMItemConfigPrv_Click(object sender, EventArgs e)
        {
            try
            {
                using (FProviderSetup Frm = new FProviderSetup())
                {
                    Frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Com.Log.EventSave(string.Format(@"Ошибка в методе {0}:""{1}""", "TSMItemConfigPrv_Click", ex.Message), this.GetType().FullName, EventEn.Error, true, true);
            }
        }
        #endregion
    }
}
