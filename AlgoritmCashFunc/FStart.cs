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
using AlgoritmCashFunc.BLL;
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
                if (((ButtonTagStatus)this.btnNew.Tag).Stat == ButtonStatusEn.Active) this.btnNew.ForeColor = Color.Black;
                else this.btnNew.ForeColor = Color.Silver;
                //
                if (((ButtonTagStatus)this.btnSave.Tag).Stat == ButtonStatusEn.Active)
                {
                    this.btnSave.ForeColor = Color.Black;
                    // На какой вкладке активность
                    switch (this.tabCntOperation.SelectedIndex)
                    {
                        // Приходный ордер
                        case 0:
                            // Делаем поля читаемыми только админу и менеджеру
                            if (Com.UserFarm.CurrentUser.Role == RoleEn.Admin ||
                                Com.UserFarm.CurrentUser.Role == RoleEn.Manager)
                            {
                                this.txtBoxPrihOKPO.ReadOnly = false;
                                this.txtBoxPrihOKUD.ReadOnly = false;
                                this.txtBoxPrihOrganization.ReadOnly = false;
                                this.txtBoxPrichStructPodr.ReadOnly = false;
                                this.txtBoxPrihDateDoc.ReadOnly = false;
                                this.txtBoxPrihNumDoc.ReadOnly = false;
                                this.txtBoxPrichDebetNomerSchet.ReadOnly = false;
                                this.txtBoxPrihKreditKorSchet.ReadOnly = false;
                                this.txtBoxPrihOsnovanie.ReadOnly = false;
                                this.txtBoxPrihGlavBuh.ReadOnly = false;
                            }

                            // разрешено править всем
                            this.cmbBoxPrihKreditor.Enabled = true;         // Принято от 
                            this.cmbBoxPrihDebitor.Enabled = true;          // Получил кассир
                            this.cmbBoxPrihPaidInReasons.Enabled = true;    // Основание

                            this.txtBoxPrihKreditKodDivision.ReadOnly = false;
                            this.txtBoxPrihKredikKodAnalUch.ReadOnly = false;
                            this.txtBoxPrihSumma.ReadOnly = false;
                            this.txtBoxPrihKodNazn.ReadOnly = false;
                            this.txtBoxPrihVtomChisle.ReadOnly = false;
                            this.grBoxNdsPrih.Enabled = true;
                            this.txtBoxPrihPrilozenie.ReadOnly = false;

                            break;
                        // Расходный ордер
                        case 1:
                            // Делаем поля читаемыми только админу и менеджеру
                            if (Com.UserFarm.CurrentUser.Role == RoleEn.Admin ||
                                Com.UserFarm.CurrentUser.Role == RoleEn.Manager)
                            {
                                this.txtBoxRashOKPO.ReadOnly = false;
                                this.txtBoxRashOKUD.ReadOnly = false;
                                this.txtBoxRashOrganization.ReadOnly = false;
                                this.txtBoxRashStructPodr.ReadOnly = false;
                                this.txtBoxRashDateDoc.ReadOnly = false;
                                this.txtBoxRashNumDoc.ReadOnly = false;
                                this.txtBoxRashDebitKorSchet.ReadOnly = false;
                                this.txtBoxRashKreditNomerSchet.ReadOnly = false;
                                this.txtBoxRashDolRukOrg.ReadOnly = false;
                                this.txtBoxRashRukFio.ReadOnly = false;
                                this.txtBoxRashGlavBuh.ReadOnly = false;
                            }

                            // разрешено править всем
                            this.cmbBoxRashKreditor.Enabled = true;             // Выдалкассир 
                            this.cmbBoxRashDebitor.Enabled = true;              // Выдать
                            this.cmbBoxRashPaidRashReasons.Enabled = true;      // Основание

                            this.txtBoxRashDebitKodDivision.ReadOnly = false;
                            this.txtBoxRashDebitKodAnalUch.ReadOnly = false;
                            this.txtBoxRashSumma.ReadOnly = false;
                            this.txtBoxRashKodNazn.ReadOnly = false;
                            this.txtBoxRashPoDoc.ReadOnly = false;
                            this.txtBoxRashPrilozenie.ReadOnly = false;

                            break;
                        // Кассовая книга
                        case 2:
                            this.txtBoxKasBookOKPO.ReadOnly = false;
                            this.txtBoxKasBookOKUD.ReadOnly = false;
                            this.txtBoxKasBookOrganization.ReadOnly = false;
                            this.txtBoxKasBookStructPodr.ReadOnly = false;
                            break;
                        // Акт о возврате денег
                        case 3:
                            this.txtBoxActVozvOKPO.ReadOnly = false;
                            this.txtBoxActVozvOKUD.ReadOnly = false;
                            this.txtBoxActVozvOrganization.ReadOnly = false;
                            this.txtBoxActVozvStructPodr.ReadOnly = false;
                            break;
                        // Отчёт кассира
                        case 4:
                            this.txtBoxReportKasOKPO.ReadOnly = false;
                            this.txtBoxReportKasOKUD.ReadOnly = false;
                            this.txtBoxReportKasOrganization.ReadOnly = false;
                            this.txtBoxReportKasStructPodr.ReadOnly = false;
                            break;
                        // Счётчики ККМ
                        case 5:
                            this.txtBoxScetKkmOKPO.ReadOnly = false;
                            this.txtBoxScetKkmOKUD.ReadOnly = false;
                            this.txtBoxScetKkmOrganization.ReadOnly = false;
                            this.txtBoxScetKkmStructPodr.ReadOnly = false;
                            break;
                        // Проверка наличных
                        case 6:
                            this.txtBoxVerifNalOKPO.ReadOnly = false;
                            this.txtBoxVerifNalOKUD.ReadOnly = false;
                            this.txtBoxVerifNalOrganization.ReadOnly = false;
                            this.txtBoxVerifNalStructPodr.ReadOnly = false;
                            break;
                        // Инвентаризация средств
                        case 7:
                            this.txtBoxInventOKPO.ReadOnly = false;
                            this.txtBoxInventOKUD.ReadOnly = false;
                            this.txtBoxInventOrganization.ReadOnly = false;
                            this.txtBoxInventStructPodr.ReadOnly = false;
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
                        // Приходный ордер
                        case 0:
                            this.txtBoxPrihOKPO.ReadOnly = true;
                            this.txtBoxPrihOKUD.ReadOnly = true;
                            this.txtBoxPrihOrganization.ReadOnly = true;
                            this.txtBoxPrichStructPodr.ReadOnly = true;
                            this.txtBoxPrihDateDoc.ReadOnly = true;
                            this.txtBoxPrihNumDoc.ReadOnly = true;
                            this.txtBoxPrichDebetNomerSchet.ReadOnly = true;
                            this.txtBoxPrihKreditKorSchet.ReadOnly = true;
                            this.txtBoxPrihOsnovanie.ReadOnly = true;
                            this.txtBoxPrihGlavBuh.ReadOnly = true;

                            // разрешено править всем
                            this.cmbBoxPrihKreditor.Enabled = false; // Принято от 
                            this.cmbBoxPrihDebitor.Enabled = false;  // Получил кассир
                            this.cmbBoxPrihPaidInReasons.Enabled = false;    // Основание

                            this.txtBoxPrihKreditKodDivision.ReadOnly = true;
                            this.txtBoxPrihKredikKodAnalUch.ReadOnly = true;
                            this.txtBoxPrihSumma.ReadOnly = true;
                            this.txtBoxPrihKodNazn.ReadOnly = true;
                            this.txtBoxPrihVtomChisle.ReadOnly = true;
                            this.grBoxNdsPrih.Enabled = false;
                            this.txtBoxPrihPrilozenie.ReadOnly = true;

                            break;
                        // Расходный ордер
                        case 1:
                            this.txtBoxRashOKPO.ReadOnly = true;
                            this.txtBoxRashOKUD.ReadOnly = true;
                            this.txtBoxRashOrganization.ReadOnly = true;
                            this.txtBoxRashStructPodr.ReadOnly = true;
                            this.txtBoxRashDateDoc.ReadOnly = true;
                            this.txtBoxRashNumDoc.ReadOnly = true;
                            this.txtBoxRashDebitKorSchet.ReadOnly = true;
                            this.txtBoxRashKreditNomerSchet.ReadOnly = true;
                            this.txtBoxRashDolRukOrg.ReadOnly = true;
                            this.txtBoxRashRukFio.ReadOnly = true;
                            this.txtBoxRashGlavBuh.ReadOnly = true;
                            
                            // разрешено править всем
                            this.cmbBoxRashKreditor.Enabled = false;                // Выдалкассир 
                            this.cmbBoxRashDebitor.Enabled = false;                 // Выдать
                            this.cmbBoxRashPaidRashReasons.Enabled = false;         // Основание

                            this.txtBoxRashDebitKodDivision.ReadOnly = true;
                            this.txtBoxRashDebitKodAnalUch.ReadOnly = true;
                            this.txtBoxRashSumma.ReadOnly = true;
                            this.txtBoxRashKodNazn.ReadOnly = true;
                            this.txtBoxRashPoDoc.ReadOnly = true;
                            this.txtBoxRashPrilozenie.ReadOnly = true;

                            break;
                        // Кассовая книга
                        case 2:
                            this.txtBoxKasBookOKPO.ReadOnly = true;
                            this.txtBoxKasBookOKUD.ReadOnly = true;
                            this.txtBoxKasBookOrganization.ReadOnly = true;
                            this.txtBoxKasBookStructPodr.ReadOnly = true;
                            break;
                        // Акт о возврате денег
                        case 3:
                            this.txtBoxActVozvOKPO.ReadOnly = true;
                            this.txtBoxActVozvOKUD.ReadOnly = true;
                            this.txtBoxActVozvOrganization.ReadOnly = true;
                            this.txtBoxActVozvStructPodr.ReadOnly = true;
                            break;
                        // Отчёт кассира
                        case 4:
                            this.txtBoxReportKasOKPO.ReadOnly = true;
                            this.txtBoxReportKasOKUD.ReadOnly = true;
                            this.txtBoxReportKasOrganization.ReadOnly = true;
                            this.txtBoxReportKasStructPodr.ReadOnly = true;
                            break;
                        // Счётчики ККМ
                        case 5:
                            this.txtBoxScetKkmOKPO.ReadOnly = true;
                            this.txtBoxScetKkmOKUD.ReadOnly = true;
                            this.txtBoxScetKkmOrganization.ReadOnly = true;
                            this.txtBoxScetKkmStructPodr.ReadOnly = true;
                            break;
                        // Проверка наличных
                        case 6:
                            this.txtBoxVerifNalOKPO.ReadOnly = true;
                            this.txtBoxVerifNalOKUD.ReadOnly = true;
                            this.txtBoxVerifNalOrganization.ReadOnly = true;
                            this.txtBoxVerifNalStructPodr.ReadOnly = true;
                            break;
                        // Инвентаризация средств
                        case 7:
                            this.txtBoxInventOKPO.ReadOnly = true;
                            this.txtBoxInventOKUD.ReadOnly = true;
                            this.txtBoxInventOrganization.ReadOnly = true;
                            this.txtBoxInventStructPodr.ReadOnly = true;
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
                // Сброс текущего документа
                this.CurDoc = null;

                // Получаем текущее подразделение
                BLL.LocalPlg.LocalKassa Kassa = Com.LocalFarm.CurLocalDepartament;

                // На какой вкладке активность
                switch (this.tabCntOperation.SelectedIndex)
                {
                    // Приходный ордер
                    case 0:
                        // Запоминаем инфу по организации
                        this.txtBoxPrihOrganization.Text = Kassa.Organization;
                        this.txtBoxPrichStructPodr.Text = Kassa.StructPodrazdelenie;
                        this.txtBoxPrihOKPO.Text = Kassa.OKPO;
                        this.txtBoxPrihGlavBuh.Text = Kassa.GlavBuhFio;

                        // Заполняем инфу по операции
                        BLL.OperationPlg.OperationPrihod OperPrihod = (BLL.OperationPlg.OperationPrihod)OperationFarm.CurOperationList["OperationPrihod"];
                        this.txtBoxPrihOKUD.Text = (OperPrihod!=null && !string.IsNullOrWhiteSpace(OperPrihod.OKUD) ? OperPrihod.OKUD:"");

                        // Приход заполняем список принято от
                        if (this.cmbBoxPrihKreditor.Items.Count==0)
                        {
                            foreach (Local item in LocalFarm.CurLocalEmployees)
                            {
                                this.cmbBoxPrihKreditor.Items.Add(item.LocalName);
                            }
                        }

                        // Заполняем получил кассир
                        if (this.cmbBoxPrihDebitor.Items.Count == 0)
                        {
                            foreach (Local item in LocalFarm.CurLocalChiefCashiers)
                            {
                                this.cmbBoxPrihDebitor.Items.Add(item.LocalName);
                            }
                        }

                        // Основание
                        if (this.cmbBoxPrihPaidInReasons.Items.Count == 0)
                        {
                            foreach (Local item in LocalFarm.CurLocalPaidInReasons)
                            {
                                this.cmbBoxPrihPaidInReasons.Items.Add(item.LocalName);
                            }
                        }

                        // Заполняем поле основание значенеие по умолчанию и зависимые поля
                        this.cmbBoxPrihPaidInReasons_SelectedIndexChanged(null, null);
                        this.cmbBoxPrihDebitor.SelectedIndex = -1;
                        this.cmbBoxPrihKreditor.SelectedIndex = -1;

                        this.txtBoxPrihKreditKodDivision.Text = string.Empty;
                        this.txtBoxPrihKredikKodAnalUch.Text = string.Empty;
                        this.txtBoxPrihSumma.Text = string.Empty;
                        this.txtBoxPrihKodNazn.Text = string.Empty;
                        this.txtBoxPrihVtomChisle.Text = string.Empty;
                        this.rBtnPrihNds0.Checked = true;
                        this.txtBoxPrihPrilozenie.Text = string.Empty;

                        break;
                    // Расходный ордер
                    case 1:
                        this.txtBoxRashOrganization.Text = Kassa.Organization;
                        this.txtBoxRashStructPodr.Text = Kassa.StructPodrazdelenie;
                        this.txtBoxRashOKPO.Text = Kassa.OKPO;
                        this.txtBoxRashDolRukOrg.Text = Kassa.DolRukOrg;
                        this.txtBoxRashRukFio.Text = Kassa.RukFio;
                        this.txtBoxRashGlavBuh.Text = Kassa.GlavBuhFio;


                        // Заполняем инфу по операции
                        BLL.OperationPlg.OperationRashod OperRashod = (BLL.OperationPlg.OperationRashod)OperationFarm.CurOperationList["OperationRashod"];
                        this.txtBoxRashOKUD.Text = (OperRashod != null && !string.IsNullOrWhiteSpace(OperRashod.OKUD) ? OperRashod.OKUD : "");

                        // Приход заполняем список выдать
                        if (this.cmbBoxRashDebitor.Items.Count == 0)
                        {
                            foreach (Local item in LocalFarm.CurLocalEmployees)
                            {
                                this.cmbBoxRashDebitor.Items.Add(item.LocalName);
                            }
                        }

                        // Заполняем выдал кассир
                        if (this.cmbBoxRashKreditor.Items.Count == 0)
                        {
                            foreach (Local item in LocalFarm.CurLocalChiefCashiers)
                            {
                                this.cmbBoxRashKreditor.Items.Add(item.LocalName);
                            }
                        }

                        // Основание
                        if (this.cmbBoxRashPaidRashReasons.Items.Count == 0)
                        {
                            foreach (Local item in LocalFarm.CurLocalPaidRashReasons)
                            {
                                this.cmbBoxRashPaidRashReasons.Items.Add(item.LocalName);
                            }
                        }

                        // Заполняем поле основание значенеие по умолчанию и зависимые поля
                        this.cmbBoxRashPaidRashReasons_SelectedIndexChanged(null, null);
                        this.cmbBoxRashDebitor.SelectedIndex = -1;
                        this.cmbBoxRashKreditor.SelectedIndex = -1;

                        this.txtBoxRashDebitKodDivision.Text = string.Empty;
                        this.txtBoxRashDebitKodAnalUch.Text = string.Empty;
                        this.txtBoxRashSumma.Text = string.Empty;
                        this.txtBoxRashKodNazn.Text = string.Empty;
                        this.txtBoxRashPoDoc.Text = string.Empty;
                        this.txtBoxRashPrilozenie.Text = string.Empty;
                        
                        break;
                    // Кассовая книга
                    case 2:
                        this.txtBoxKasBookOrganization.Text = Kassa.Organization;
                        this.txtBoxKasBookStructPodr.Text = Kassa.StructPodrazdelenie;
                        this.txtBoxKasBookOKPO.Text = Kassa.OKPO;
                        break;
                    // Акт о возврате денег
                    case 3:
                        this.txtBoxActVozvOrganization.Text = Kassa.Organization;
                        this.txtBoxActVozvStructPodr.Text = Kassa.StructPodrazdelenie;
                        this.txtBoxActVozvOKPO.Text = Kassa.OKPO;
                        break;
                    // Отчёт кассира
                    case 4:
                        this.txtBoxReportKasOrganization.Text = Kassa.Organization;
                        this.txtBoxReportKasStructPodr.Text = Kassa.StructPodrazdelenie;
                        this.txtBoxReportKasOKPO.Text = Kassa.OKPO;
                        break;
                    // Счётчики ККМ
                    case 5:
                        this.txtBoxScetKkmOrganization.Text = Kassa.Organization;
                        this.txtBoxScetKkmStructPodr.Text = Kassa.StructPodrazdelenie;
                        this.txtBoxScetKkmOKPO.Text = Kassa.OKPO;
                        break;
                    // Проверка наличных
                    case 6:
                        this.txtBoxVerifNalOrganization.Text = Kassa.Organization;
                        this.txtBoxVerifNalStructPodr.Text = Kassa.StructPodrazdelenie;
                        this.txtBoxVerifNalOKPO.Text = Kassa.OKPO;
                        break;
                    // Инвентаризация средств
                    case 7:
                        this.txtBoxInventOrganization.Text = Kassa.Organization;
                        this.txtBoxInventStructPodr.Text = Kassa.StructPodrazdelenie;
                        this.txtBoxInventOKPO.Text = Kassa.OKPO;
                        break;
                    default:
                        break;
                }

                // Правим стиль кнопок
                ((ButtonTagStatus)this.btnNew.Tag).Stat = ButtonStatusEn.Active;
                ((ButtonTagStatus)this.btnSave.Tag).Stat = ButtonStatusEn.Passive;
                this.RenderButtonStyle();
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
                    // Приходный ордер
                    case 0:
                        // Запоминаем инфу по организации
                        Kassa.Organization = this.txtBoxPrihOrganization.Text;
                        Kassa.StructPodrazdelenie = this.txtBoxPrichStructPodr.Text;
                        Kassa.OKPO = this.txtBoxPrihOKPO.Text;
                        Kassa.GlavBuhFio = this.txtBoxPrihGlavBuh.Text;

                        // Валидация заполненных данных по подразделению и сохранение в базе
                        ValidateKassa(Kassa);

                        // Валидация введённой даты
                        if (string.IsNullOrWhiteSpace(this.txtBoxPrihNumDoc.Text)) new ApplicationException("Поле номер документа не может быть пустым.");
                        else
                        {
                            try { this.CurDoc.DocNum = int.Parse(this.txtBoxPrihNumDoc.Text); }
                            catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение номера документа {0} к числу.", this.txtBoxPrihNumDoc.Text)); }
                        }

                        // Валидация введённой даты
                        try { this.CurDoc.UreDate = DateTime.Parse(this.txtBoxPrihDateDoc.Text);}
                        catch (Exception){ throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} к дате.", this.txtBoxPrihDateDoc.Text));}

                        // Валидация кредитора
                        if (this.cmbBoxPrihKreditor.SelectedIndex == -1) throw new ApplicationException("Не указано от кого принято.");
                        else
                        {
                            this.CurDoc.LocalCreditor = LocalFarm.CurLocalEmployees[this.cmbBoxPrihKreditor.SelectedIndex];
                        }

                        // Валидация Дебитора
                        if (this.cmbBoxPrihDebitor.SelectedIndex == -1) throw new ApplicationException("Не указано кто получил.");
                        else
                        {
                            this.CurDoc.LocalDebitor = LocalFarm.CurLocalChiefCashiers[this.cmbBoxPrihDebitor.SelectedIndex];
                        }

                        ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).KreditKodDivision = this.txtBoxPrihKreditKodDivision.Text;
                        ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).KredikKodAnalUch = this.txtBoxPrihKredikKodAnalUch.Text;
                        //
                        // Валидация суммы
                        if (!string.IsNullOrWhiteSpace(this.txtBoxPrihSumma.Text))
                        {
                            try
                            {
                                ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).Summa = decimal.Round(decimal.Parse(this.txtBoxPrihSumma.Text),2);
                                ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).SummaStr = this.lblPrihSummaString.Text.Replace("Сумма:", "").Trim();
                            }
                            catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение к сумме {0}.", this.txtBoxPrihSumma.Text)); }
                        }
                        else ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).Summa = 0;
                        //
                        ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).KodNazn = this.txtBoxPrihKodNazn.Text;

                        // Валидация основание
                        if (this.cmbBoxPrihPaidInReasons.SelectedIndex == -1) throw new ApplicationException("Не указано основание.");
                        else
                        {
                            BLL.LocalPlg.LocalPaidInReasons LoclPaidInReasons = LocalFarm.CurLocalPaidInReasons[this.cmbBoxPrihPaidInReasons.SelectedIndex];
                            //
                            LoclPaidInReasons.DebetNomerSchet = this.txtBoxPrichDebetNomerSchet.Text;
                            ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).DebetNomerSchet = this.txtBoxPrichDebetNomerSchet.Text;
                            //
                            LoclPaidInReasons.KredikKorSchet = this.txtBoxPrihKreditKorSchet.Text;
                            ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).KredikKorSchet = this.txtBoxPrihKreditKorSchet.Text;
                            //
                            LoclPaidInReasons.Osnovanie = this.txtBoxPrihOsnovanie.Text;
                            ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).Osnovanie = this.txtBoxPrihOsnovanie.Text;
                            //
                            ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).VtomChisle = this.txtBoxPrihVtomChisle.Text;
                            //
                            if (this.rBtnPrihNds10.Checked) ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).NDS = 10;
                            else if (this.rBtnPrihNds20.Checked) ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).NDS = 20;
                            else ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).NDS = 0;
                            //
                            ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).Prilozenie = this.txtBoxPrihPrilozenie.Text;
                            //
                            ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).GlavBuh = this.txtBoxPrihGlavBuh.Text;
                            //
                            ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).PaidInReasons = LoclPaidInReasons;
                            LoclPaidInReasons.Save();
                        }

                        // Сохранение инфы в базе
                        Kassa.LastDocNumPrih = int.Parse(this.txtBoxPrihNumDoc.Text);
                        Kassa.Save();

                        // Заполняем инфу по операции
                        BLL.OperationPlg.OperationPrihod OperPrihod = (BLL.OperationPlg.OperationPrihod)this.CurDoc.CurOperation;
                        OperPrihod.OKUD = txtBoxPrihOKUD.Text;
                        OperPrihod.Save();
                        

                        // Сохранение документа
                        this.CurDoc.Save();

                        break;
                    // Расходный ордер
                    case 1:
                        // Запоминаем инфу по организации
                        Kassa.Organization = this.txtBoxRashOrganization.Text;
                        Kassa.StructPodrazdelenie = this.txtBoxRashStructPodr.Text;
                        Kassa.OKPO = this.txtBoxRashOKPO.Text;
                        Kassa.DolRukOrg = this.txtBoxRashDolRukOrg.Text;
                        Kassa.RukFio = this.txtBoxRashRukFio.Text;
                        Kassa.GlavBuhFio = this.txtBoxRashGlavBuh.Text;

                        // Валидация заполненных данных по подразделению и сохранение в базе
                        ValidateKassa(Kassa);

                        // Валидация введённой даты
                        if (string.IsNullOrWhiteSpace(this.txtBoxRashNumDoc.Text)) new ApplicationException("Поле номер документа не может быть пустым.");
                        else
                        {
                            try { this.CurDoc.DocNum = int.Parse(this.txtBoxRashNumDoc.Text); }
                            catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение номера документа {0} к числу.", this.txtBoxRashNumDoc.Text)); }
                        }

                        // Валидация введённой даты
                        try { this.CurDoc.UreDate = DateTime.Parse(this.txtBoxRashDateDoc.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} к дате.", this.txtBoxRashDateDoc.Text)); }

                        // Валидация Дебитора
                        if (this.cmbBoxRashDebitor.SelectedIndex == -1) throw new ApplicationException("Не указано кому выдать.");
                        else
                        {
                            this.CurDoc.LocalDebitor = LocalFarm.CurLocalEmployees[this.cmbBoxRashDebitor.SelectedIndex];
                        }

                        // Валидация кредитора
                        if (this.cmbBoxRashKreditor.SelectedIndex == -1) throw new ApplicationException("Не указано кто выдал.");
                        else
                        {
                            this.CurDoc.LocalCreditor = LocalFarm.CurLocalChiefCashiers[this.cmbBoxRashKreditor.SelectedIndex];
                        }

                        ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).DebetKodDivision = this.txtBoxRashDebitKodDivision.Text;
                        ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).DebetKodAnalUch = this.txtBoxRashDebitKodAnalUch.Text;
                        //
                        // Валидация суммы
                        if (!string.IsNullOrWhiteSpace(this.txtBoxRashSumma.Text))
                        {
                            try
                            {
                                ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).Summa = decimal.Round(decimal.Parse(this.txtBoxRashSumma.Text), 2);
                                ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).SummaStr = this.lblRashSummaString.Text.Replace("Сумма:", "").Trim();
                            }
                            catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение к сумме {0}.", this.txtBoxRashSumma.Text)); }
                        }
                        else ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).Summa = 0;
                        //
                        ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).KodNazn = this.txtBoxRashKodNazn.Text;


                        // Валидация основание
                        if (this.cmbBoxRashPaidRashReasons.SelectedIndex == -1) throw new ApplicationException("Не указано основание.");
                        else
                        {
                            BLL.LocalPlg.LocalPaidRashReasons LoclPaidRashReasons = LocalFarm.CurLocalPaidRashReasons[this.cmbBoxRashPaidRashReasons.SelectedIndex];
                            //
                            LoclPaidRashReasons.DebetKorSchet = this.txtBoxRashDebitKorSchet.Text;
                            ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).DebetKorSchet = this.txtBoxRashDebitKorSchet.Text;
                            //
                            LoclPaidRashReasons.KreditNomerSchet = this.txtBoxRashKreditNomerSchet.Text;
                            ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).KreditNomerSchet = this.txtBoxRashKreditNomerSchet.Text;
                            //
                            LoclPaidRashReasons.Osnovanie = this.txtBoxRashOsnovanie.Text;
                            ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).Osnovanie = this.txtBoxRashOsnovanie.Text;
                            //
                            ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).PoDoc = this.txtBoxRashPoDoc.Text;
                            //
                            ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).Prilozenie = this.txtBoxRashPrilozenie.Text;
                            //
                            ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).GlavBuh = this.txtBoxRashGlavBuh.Text;
                            //
                            ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).PaidRashReasons = LoclPaidRashReasons;
                            LoclPaidRashReasons.Save();
                        }
                        

                        // Сохранение инфы в базе
                        Kassa.LastDocNumRash = int.Parse(this.txtBoxRashNumDoc.Text);
                        Kassa.Save();
                                                
                        // Заполняем инфу по операции
                        BLL.OperationPlg.OperationRashod OperRashod = (BLL.OperationPlg.OperationRashod)this.CurDoc.CurOperation;
                        OperRashod.OKUD = txtBoxRashOKUD.Text;
                        OperRashod.Save();
                        
                        // Сохранение документа
                        this.CurDoc.Save();
                        
                        break;
                    // Кассовая книга
                    case 2:
                        // Запоминаем инфу по организации
                        Kassa.Organization = this.txtBoxKasBookOrganization.Text;
                        Kassa.StructPodrazdelenie = this.txtBoxKasBookStructPodr.Text;
                        Kassa.OKPO = this.txtBoxKasBookOKPO.Text;

                        // Валидация заполненных данных по подразделению и сохранение в базе
                        ValidateKassa(Kassa);

                        // Валидация введённой даты
                        try { this.CurDoc.UreDate = DateTime.Parse(this.txtBoxKasBookDateDoc.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} к дате.", this.txtBoxKasBookDateDoc.Text)); }

                        // Сохранение инфы в базе
                        Kassa.Save();

                        break;
                    // Акт о возврате денег
                    case 3:
                        // Запоминаем инфу по организации
                        Kassa.Organization = this.txtBoxActVozvOrganization.Text;
                        Kassa.StructPodrazdelenie = this.txtBoxActVozvStructPodr.Text;
                        Kassa.OKPO = this.txtBoxActVozvOKPO.Text;

                        // Валидация заполненных данных по подразделению и сохранение в базе
                        ValidateKassa(Kassa);

                        // Валидация введённой даты
                        try { this.CurDoc.UreDate = DateTime.Parse(this.txtBoxActVozvDateDoc.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} к дате.", this.txtBoxActVozvDateDoc.Text)); }

                        // Сохранение инфы в базе
                        Kassa.LastDocNumActVozv = int.Parse(this.txtBoxActVozvNumDoc.Text);
                        Kassa.Save();

                        break;
                    // Отчёт кассира
                    case 4:
                        // Запоминаем инфу по организации
                        Kassa.Organization = this.txtBoxReportKasOrganization.Text;
                        Kassa.StructPodrazdelenie = this.txtBoxReportKasStructPodr.Text;
                        Kassa.OKPO = this.txtBoxReportKasOKPO.Text;

                        // Валидация заполненных данных по подразделению и сохранение в базе
                        ValidateKassa(Kassa);

                        // Валидация введённой даты
                        try { this.CurDoc.UreDate = DateTime.Parse(this.txtBoxReportKasDateDoc.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} к дате.", this.txtBoxReportKasDateDoc.Text)); }

                        // Сохранение инфы в базе
                        Kassa.LastDocNumReportKas = int.Parse(this.txtBoxReportKasNumDoc.Text);
                        Kassa.Save();

                        break;
                    // Счётчики ККМ
                    case 5:
                        // Запоминаем инфу по организации
                        Kassa.Organization = this.txtBoxScetKkmOrganization.Text;
                        Kassa.StructPodrazdelenie = this.txtBoxScetKkmStructPodr.Text;
                        Kassa.OKPO = this.txtBoxScetKkmOKPO.Text;

                        // Валидация заполненных данных по подразделению и сохранение в базе
                        ValidateKassa(Kassa);

                        // Валидация введённой даты
                        try { this.CurDoc.UreDate = DateTime.Parse(this.txtBoxScetKkmDateDoc.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} к дате.", this.txtBoxScetKkmDateDoc.Text)); }

                        // Сохранение инфы в базе
                        Kassa.LastDocNumScetKkm = int.Parse(this.txtBoxScetKkmNumDoc.Text);
                        Kassa.Save();

                        break;
                    // Проверка наличных
                    case 6:
                        // Запоминаем инфу по организации
                        Kassa.Organization = this.txtBoxVerifNalOrganization.Text;
                        Kassa.StructPodrazdelenie = this.txtBoxVerifNalStructPodr.Text;
                        Kassa.OKPO = this.txtBoxVerifNalOKPO.Text;

                        // Валидация заполненных данных по подразделению и сохранение в базе
                        ValidateKassa(Kassa);

                        // Валидация введённой даты
                        try { this.CurDoc.UreDate = DateTime.Parse(this.txtBoxVerifNalDateDoc.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} к дате.", this.txtBoxVerifNalDateDoc.Text)); }

                        // Сохранение инфы в базе
                        Kassa.LastDocNumVerifNal = int.Parse(this.txtBoxVerifNalNumDoc.Text);
                        Kassa.Save();

                        break;
                    // Инвентаризация средств
                    case 7:
                        // Запоминаем инфу по организации
                        Kassa.Organization = this.txtBoxInventOrganization.Text;
                        Kassa.StructPodrazdelenie = this.txtBoxInventStructPodr.Text;
                        Kassa.OKPO = this.txtBoxInventOKPO.Text;

                        // Валидация заполненных данных по подразделению и сохранение в базе
                        ValidateKassa(Kassa);

                        // Валидация введённой даты
                        try { this.CurDoc.UreDate = DateTime.Parse(this.txtBoxInventDateDoc.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} к дате.", this.txtBoxInventDateDoc.Text)); }

                        // Сохранение инфы в базе
                        Kassa.LastDocNumInvent = int.Parse(this.txtBoxInventNumDoc.Text);
                        Kassa.Save();

                        break;
                    default:
                        break;
                }
                
                // Пишем успех но не в лог просто чтобы пользователь увидел что у него всё ок
                Log.EventSave(string.Format("Документ №{0} успешно сохранён ({1}).", this.CurDoc.DocNum, this.CurDoc.DocFullName), string.Format("{0}.btnSave_Click", GetType().Name), EventEn.Message, false, false);

                // Правим стиль кнопок
                ((ButtonTagStatus)this.btnNew.Tag).Stat = ButtonStatusEn.Active;
                ((ButtonTagStatus)this.btnSave.Tag).Stat = ButtonStatusEn.Passive;
                this.RenderButtonStyle();
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
                // Список ошибок
                ApplicationException ErOrganization= new ApplicationException("Не заполнено поле Организация");
                ApplicationException ErStructPodrazdelenie = new ApplicationException("Не заполнено поле Структурное подразделение");
                ApplicationException ErOKPO = new ApplicationException("Не заполнено поле ОКПО");

                // Проверка
                if (string.IsNullOrWhiteSpace(Kassa.Organization)) throw ErOrganization;
                if (string.IsNullOrWhiteSpace(Kassa.StructPodrazdelenie)) throw ErStructPodrazdelenie;
                if (string.IsNullOrWhiteSpace(Kassa.OKPO)) throw ErOKPO;
                 
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
                // Получаем текущее подразделение
                BLL.LocalPlg.LocalKassa Kassa = Com.LocalFarm.CurLocalDepartament;

                // На какой вкладке активность
                switch (this.tabCntOperation.SelectedIndex)
                {
                    // Приходный ордер
                    case 0:
                        // Создаём пустой документ
                        this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentPrihod");
                        this.txtBoxPrihNumDoc.Text = (Kassa.LastDocNumPrih + 1).ToString();
                        this.txtBoxPrihGlavBuh.Text = Kassa.GlavBuhFio;
                        // Проверка на наличие ошибок при создании пустого документа
                        if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", "DocumentPrihod"));
                        //
                        this.txtBoxPrihDateDoc.Text = DateTime.Now.Date.ToShortDateString();
                        
                        // Заполняем инфу по операции
                        BLL.OperationPlg.OperationPrihod OperPrihod = (BLL.OperationPlg.OperationPrihod)this.CurDoc.CurOperation;
                        this.txtBoxPrihOKUD.Text = OperPrihod.OKUD;
                        

                        // Заполняем поле основание значенеие по умолчанию и зависимые поля
                        this.cmbBoxPrihPaidInReasons_SelectedIndexChanged(null, null);

                        this.cmbBoxPrihDebitor.SelectedIndex = -1;
                        this.cmbBoxPrihKreditor.SelectedIndex = -1;

                        this.txtBoxPrihKreditKodDivision.Text = string.Empty;
                        this.txtBoxPrihKredikKodAnalUch.Text = string.Empty;
                        this.txtBoxPrihSumma.Text = string.Empty;
                        this.txtBoxPrihKodNazn.Text = string.Empty;
                        this.txtBoxPrihVtomChisle.Text = string.Empty;
                        this.txtBoxPrihPrilozenie.Text = string.Empty;

                        break;
                    // Расходный ордер
                    case 1:
                        // Создаём пустой документ
                        this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentRashod");
                        this.txtBoxRashNumDoc.Text = (Kassa.LastDocNumRash + 1).ToString();
                        this.txtBoxRashDolRukOrg.Text = Kassa.DolRukOrg;
                        this.txtBoxRashRukFio.Text = Kassa.RukFio;
                        this.txtBoxRashGlavBuh.Text = Kassa.GlavBuhFio;
                        // Проверка на наличие ошибок при создании пустого документа
                        if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", ""));
                        //
                        this.txtBoxRashDateDoc.Text = DateTime.Now.Date.ToShortDateString();

                        // Заполняем инфу по операции
                        BLL.OperationPlg.OperationRashod OperRashod = (BLL.OperationPlg.OperationRashod)this.CurDoc.CurOperation;
                        this.txtBoxRashOKUD.Text = OperRashod.OKUD;


                        // Заполняем поле основание значенеие по умолчанию и зависимые поля
                        this.cmbBoxRashPaidRashReasons_SelectedIndexChanged(null, null);

                        this.cmbBoxRashDebitor.SelectedIndex = -1;
                        this.cmbBoxRashKreditor.SelectedIndex = -1;

                        this.txtBoxRashDebitKodDivision.Text = string.Empty;
                        this.txtBoxRashDebitKodAnalUch.Text = string.Empty;
                        this.txtBoxRashSumma.Text = string.Empty;
                        this.txtBoxRashKodNazn.Text = string.Empty;
                        this.txtBoxRashPoDoc.Text = string.Empty;
                        this.txtBoxRashPrilozenie.Text = string.Empty;

                        break;
                    // Кассовая книга
                    case 2:
                        // Создаём пустой документ
                        //this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentPrihod");
                        // Проверка на наличие ошибок при создании пустого документа
                        //if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", ""));
                        //
                        this.txtBoxKasBookDateDoc.Text = DateTime.Now.Date.ToShortDateString();
                        break;
                    // Акт о возврате денег
                    case 3:
                        // Создаём пустой документ
                        //this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentPrihod");
                        this.txtBoxActVozvNumDoc.Text = (Kassa.LastDocNumActVozv + 1).ToString();
                        // Проверка на наличие ошибок при создании пустого документа
                        //if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", ""));
                        //
                        this.txtBoxActVozvDateDoc.Text = DateTime.Now.Date.ToShortDateString();
                        break;
                    // Отчёт кассира
                    case 4:
                        // Создаём пустой документ
                        //this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentPrihod");
                        this.txtBoxReportKasNumDoc.Text = (Kassa.LastDocNumReportKas + 1).ToString();
                        // Проверка на наличие ошибок при создании пустого документа
                        //if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", ""));
                        //
                        break;
                    // Счётчики ККМ
                    case 5:
                        // Создаём пустой документ
                        //this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentPrihod");
                        this.txtBoxScetKkmNumDoc.Text = (Kassa.LastDocNumScetKkm + 1).ToString();
                        // Проверка на наличие ошибок при создании пустого документа
                        //if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", ""));
                        //
                        this.txtBoxScetKkmDateDoc.Text = DateTime.Now.Date.ToShortDateString();
                        this.txtBoxScetKkmTimeDoc.Text = DateTime.Now.ToShortTimeString().ToString();
                        break;
                    // Проверка наличных
                    case 6:
                        // Создаём пустой документ
                        //this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentPrihod");
                        this.txtBoxVerifNalNumDoc.Text = (Kassa.LastDocNumVerifNal + 1).ToString();
                        // Проверка на наличие ошибок при создании пустого документа
                        //if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", ""));
                        //
                        this.txtBoxVerifNalDateDoc.Text = DateTime.Now.Date.ToShortDateString();
                        break;
                    // Инвентаризация средств
                    case 7:
                        // Создаём пустой документ
                        //this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentPrihod");
                        this.txtBoxInventNumDoc.Text = (Kassa.LastDocNumInvent + 1).ToString();
                        // Проверка на наличие ошибок при создании пустого документа
                        //if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", ""));
                        //
                        this.txtBoxInventDateDoc.Text = DateTime.Now.Date.ToShortDateString();
                        break;
                    default:
                        break;
                }

                // Отрисовываем кнопки
                ((ButtonTagStatus)this.btnNew.Tag).Stat = ButtonStatusEn.Passive;
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


        #region События связанные с выбором внутри элементов котороые к основной логике не имеют отношения

        /// <summary>
        /// Конвертация текстового значения суммы в строку для разных типов документов чтобы не дублировать код
        /// </summary>
        /// <param name="sumtxt">Сумма в виде строки которую переводим в рубли с копейками прописью</param>
        /// <returns>Текст который подставляем во вкладках</returns>
        private string KonvertSummToString(string sumtxt)
        {
            try
            {
                string rez = "Сумма:";

                try
                {
                    // Получакем значение и преобразовываем к строке
                    decimal summ = decimal.Parse(sumtxt);
                    string tmp = decimal.Round(summ, 2).ToString().Replace(".", ",");

                    // Создаём переменные для рублей и копеек
                    int? sumR = null;
                    int? sumK = null;

                    // Проверка на наличие копеек
                    if (tmp.IndexOf(',') > -1)
                    {
                        sumR = int.Parse(tmp.Substring(0, tmp.IndexOf(',')));
                        sumK = int.Parse(tmp.Substring(tmp.IndexOf(',') + 1));
                    }
                    else sumR = (int)summ;

                    // Получаей часть которая представляет из себя рубли
                    string rub = string.Empty;
                    if (sumR != null) rub = Utils.GetStringForInt((int)sumR, "рубль", "рубля", "рублей", false);

                    // получаем часть которая представляет из себя копейки
                    string kop = string.Empty;
                    if (sumK != null) kop = Utils.GetStringForInt((int)sumK, "копейка", "копейки", "копеек", true).ToLower();

                    rez = string.Format("Сумма: {0} {1}", rub, kop).Trim();
                }
                catch (Exception)
                {
                    rez = "Сумма:";
                }

                return rez;
            }
            catch (Exception)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при попытки превратить число в строку с ошибкой: ({0})", txtBoxPrihSumma, sumtxt));
                Log.EventSave(ae.Message, string.Format("{0}.KonvertSummToString", GetType().Name), EventEn.Error, true, true);
                throw ae;
            }

        }

        // В приходе пользователь выбирает основание, необходимо заполнить зависимые поля значениями по умолчанию
        private void cmbBoxPrihPaidInReasons_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Сбрасываем все значения на пустоту
                if (sender==null || this.cmbBoxPrihPaidInReasons.SelectedIndex==-1)
                {
                    this.txtBoxPrichDebetNomerSchet.Text = string.Empty;
                    this.txtBoxPrihKreditKorSchet.Text = string.Empty;
                    this.txtBoxPrihOsnovanie.Text = string.Empty;
                    this.cmbBoxPrihPaidInReasons.SelectedIndex = -1;
                }
                else
                {
                    // Получаем текущее основание
                    BLL.LocalPlg.LocalPaidInReasons LoclPaidInReasons = LocalFarm.CurLocalPaidInReasons[this.cmbBoxPrihPaidInReasons.SelectedIndex];
                    this.txtBoxPrichDebetNomerSchet.Text = LoclPaidInReasons.DebetNomerSchet;
                    this.txtBoxPrihKreditKorSchet.Text = LoclPaidInReasons.KredikKorSchet;
                    this.txtBoxPrihOsnovanie.Text = LoclPaidInReasons.Osnovanie;
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.btnNew_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }
       
        // Пользователь вбил сумму нужно её перевести в текст
        private void txtBoxPrihSumma_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.lblPrihSummaString.Text = this.KonvertSummToString(this.txtBoxPrihSumma.Text);
            }
            catch (Exception)
            {
                //ApplicationException ae = new ApplicationException(string.Format("Упали при попытки превратить число в строку с ошибкой: ({0})", txtBoxPrihSumma, ex.Message));
                //Log.EventSave(ae.Message, string.Format("{0}.txtBoxPrihSumma_TextChanged", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }
        
        // В приходе пользователь выбирает основание, необходимо заполнить зависимые поля значениями по умолчанию
        private void cmbBoxRashPaidRashReasons_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Сбрасываем все значения на пустоту
                if (sender == null || this.cmbBoxRashPaidRashReasons.SelectedIndex == -1)
                {
                    this.txtBoxRashDebitKorSchet.Text = string.Empty;
                    this.txtBoxRashKreditNomerSchet.Text = string.Empty;
                    this.txtBoxRashOsnovanie.Text = string.Empty;
                    this.cmbBoxRashPaidRashReasons.SelectedIndex = -1;
                }
                else
                {
                    // Получаем текущее основание
                    BLL.LocalPlg.LocalPaidRashReasons LoclPaidRashReasons = LocalFarm.CurLocalPaidRashReasons[this.cmbBoxRashPaidRashReasons.SelectedIndex];
                    this.txtBoxRashDebitKorSchet.Text = LoclPaidRashReasons.DebetKorSchet;
                    this.txtBoxRashKreditNomerSchet.Text = LoclPaidRashReasons.KreditNomerSchet;
                    this.txtBoxRashOsnovanie.Text = LoclPaidRashReasons.Osnovanie;
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.btnNew_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Пользователь вбил сумму нужно её перевести в текст
        private void txtBoxRashSumma_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.lblRashSummaString.Text = this.KonvertSummToString(this.txtBoxRashSumma.Text);
            }
            catch (Exception)
            {
                //ApplicationException ae = new ApplicationException(string.Format("Упали при попытки превратить число в строку с ошибкой: ({0})", txtBoxPrihSumma, ex.Message));
                //Log.EventSave(ae.Message, string.Format("{0}.txtBoxPrihSumma_TextChanged", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }
        
        #endregion
    }
}
