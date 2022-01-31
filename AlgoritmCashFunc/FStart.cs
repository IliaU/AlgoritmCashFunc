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

        private BLL.Document CurDoc;

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
                else
                {
                    this.TSMItemConfigPrv.Visible = false;
                    this.TSMItemAboutRep.Visible = false;
                }
                if (UserFarm.CurrentUser.Role == RoleEn.Operator)
                {
                    this.TSMItemConfigUsers.Text = "Сменить пароль";
                    this.TSMItemLic.Visible = false;
                }
                else this.TSMItemLic.Visible = true;


                // Подписываемся на события
                Com.Log.onEventLog += Log_onEventLog;
                Log.EventSave(string.Format("Вход под пользователем {0} ({1})", Com.UserFarm.CurrentUser.Logon, Com.UserFarm.CurrentUser.Role.ToString()), GetType().Name, EventEn.Message);
                this.tabCntOperation.Selected += TabCntOperation_Selected;

                // Настраиваем наши кнопки
                this.btnNew.Tag = new ButtonTagStatus(ButtonStatusEn.Active);
                this.btnSave.Tag = new ButtonTagStatus(ButtonStatusEn.Passive);
                this.btnPrint.Tag = new ButtonTagStatus(ButtonStatusEn.Passive);
                this.btnOperator.Tag = new ButtonTagStatus(ButtonStatusEn.Active);
                this.btnExit.Tag = new ButtonTagStatus(ButtonStatusEn.Active);
                this.RenderButtonStyle();

                // При загрузке не срабатывает событие выбора вкладки по этой причине загрузка основных полей вызывается при старте в ручную потом поля будут заполняться автоматом
                this.TabCntOperation_Selected(null, null);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при загрузке формы FConfig с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }
        
        #region Системные методы для поддержания формы
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
        private void Log_onEventLog(object sender, Lib.EventLog e)
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

        /// <summary>
        /// Отрисовка кнопок в верхнем меню
        /// </summary>
        private void RenderButtonStyle()
        {
            try
            {
                // Настройка стиля кнопок
                if (((ButtonTagStatus)this.btnNew.Tag).Stat== ButtonStatusEn.Active) this.btnNew.ForeColor = Color.Black;
                else this.btnNew.ForeColor = Color.Silver;
                //
                if (((ButtonTagStatus)this.btnSave.Tag).Stat == ButtonStatusEn.Active)
                {
                    this.btnSave.ForeColor = Color.Black;
                    // На какой вкладке активность
                    switch (this.tabCntOperation.SelectedIndex)
                    {
                        case 0:
                            this.txtBoxOKPO.ReadOnly = false;
                            this.txtBoxOKUD.ReadOnly = false;
                            this.txtBoxPrihOrganization.ReadOnly = false;
                            this.txtBoxStructPodr.ReadOnly = false;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    this.btnSave.ForeColor = Color.Silver;
                    // На какой вкладке активность
                    switch (this.tabCntOperation.SelectedIndex)
                    {
                        case 0:
                            this.txtBoxOKPO.ReadOnly = true;
                            this.txtBoxOKUD.ReadOnly = true;
                            this.txtBoxPrihOrganization.ReadOnly = true;
                            this.txtBoxStructPodr.ReadOnly = true;
                            break;
                        default:
                            break;
                    }
                }
                //
                if (((ButtonTagStatus)this.btnPrint.Tag).Stat == ButtonStatusEn.Active) this.btnPrint.ForeColor = Color.Black;
                else this.btnPrint.ForeColor = Color.Silver;
                //
                if (((ButtonTagStatus)this.btnOperator.Tag).Stat == ButtonStatusEn.Active) this.btnOperator.ForeColor = Color.Black;
                else this.btnOperator.ForeColor = Color.Silver;
                //
                if (((ButtonTagStatus)this.btnExit.Tag).Stat == ButtonStatusEn.Active) this.btnExit.ForeColor = Color.Black;
                else this.btnExit.ForeColor = Color.Silver;
                
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при применении стиля к кнопкам: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.RenderButtonStyle", GetType().Name), EventEn.Error);
                //throw ae;
            }
        }

        // Выбор вкладки необходимо построить содержимое документов в зависимости от того какой сейчас активный
        private void TabCntOperation_Selected(object sender, TabControlEventArgs e)
        {
            try
            {
                // Получаем текущее подразделение
                BLL.LocalPlg.LocalKassa Kassa = Com.LocalFarm.CurLocalDepartament;

                // На какой вкладке активность
                switch (this.tabCntOperation.SelectedIndex)
                {
                    case 0:
                        // Запоминаем инфу по организации
                        txtBoxPrihOrganization.Text = Kassa.Organization;
                        txtBoxStructPodr.Text = Kassa.StructPodrazdelenie;
                        txtBoxOKPO.Text = Kassa.OKPO;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.TabCntOperation_Selected", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        #endregion

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
                this.btnOperator_Click(null, null);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выходе пользователя с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.UserFarm_onEventLogOFF", GetType().Name), EventEn.Error);
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

        #region События вызванные выбором в настроечном меню
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

        // Пользователь вызывает форму с лицензиями
        private void TSMItemLic_Click(object sender, EventArgs e)
        {
            try
            {
                FLic Frm = new FLic();
                Frm.ShowDialog();
            }
            catch (Exception ex)
            {
                Com.Log.EventSave(string.Format(@"Ошибка в методе {0}:""{1}""", "TSMItemLic_Click", ex.Message), this.GetType().FullName, EventEn.Error, true, true);
            }
        }
        #endregion

        #region События вызванные выбором в верхнем меню

        // Пользователь закрывает форму
        private void FStart_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                UserFarm.onEventLogOFF -= UserFarm_onEventLogOFF;

                if (Com.UserFarm.CurrentUser != null) this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при чтении конфигурации с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.FStart_FormClosing", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        // Пользователь решил поменять логин под кем зашли
        private void btnOperator_Click(object sender, EventArgs e)
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

        // Пользователь сохряняет документ
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (((ButtonTagStatus)this.btnSave.Tag).Stat != ButtonStatusEn.Active) return;
                if (this.CurDoc == null) throw new ApplicationException("Документ не создан по этой причине сохранять нечего");

                // Получаем текущее подразделение
                BLL.LocalPlg.LocalKassa Kassa = Com.LocalFarm.CurLocalDepartament;

                // На какой вкладке активность
                switch (this.tabCntOperation.SelectedIndex)
                {
                    case 0:
                        // Запоминаем инфу по организации
                        Kassa.Organization = txtBoxPrihOrganization.Text;
                        Kassa.StructPodrazdelenie = txtBoxStructPodr.Text;
                        Kassa.OKPO = txtBoxOKPO.Text;

                        //Валидация заполненных данных по подразделению и сохранение в базе
                        ValidateKassa(Kassa);
                        Kassa.Save();

                        break;
                    default:
                        break;
                }



                
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при сохранении документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.btnSave_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        /// <summary>
        /// Проверка валидных параметров которые пользователь завёл в парамерах кассы
        /// </summary>
        /// <param name="Kassa"></param>
        private void ValidateKassa(BLL.LocalPlg.LocalKassa Kassa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Kassa.Organization)) throw new ApplicationException("Не заполнено поле Организация");
                if (string.IsNullOrWhiteSpace(Kassa.StructPodrazdelenie)) throw new ApplicationException("Не заполнено поле Структурное подразделение");
                if (string.IsNullOrWhiteSpace(Kassa.OKPO)) throw new ApplicationException("Не заполнено поле ОКПО");
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при валидации параметров кассы с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.ValidateKassa", GetType().Name), EventEn.Error);
                throw ae;
            }
        }
        

        // Создаём новый документ
        private void btnNew_Click(object sender, EventArgs e)
        {
            try
            {
                // На какой вкладке активность
                switch (this.tabCntOperation.SelectedIndex)
                {
                    case 0:
                        // Создаём пустой документ прихода
                        this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentPrihod");
                        break;
                    default:
                        break;
                }

                ((ButtonTagStatus)this.btnSave.Tag).Stat = ButtonStatusEn.Active;
                this.RenderButtonStyle();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.btnNew_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }
        #endregion
        
    }
}
