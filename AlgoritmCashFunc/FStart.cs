using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Globalization;
using AlgoritmCashFunc.Com;
using AlgoritmCashFunc.BLL;
using AlgoritmCashFunc.Lib;
using WordDotx;

namespace AlgoritmCashFunc
{
    public partial class FStart : Form
    {
        private Color DefBaskCoclortSSLabel;
        private object LockObj = new object();

        private BLL.Document CurDoc;

        private DataTable dtDocKassBook = new DataTable("dtDocKassBook");
        private DataView dvDocKassBook;
        private List<Document> TagDocKassBook = new List<Document>();  // Для хранения документов для того чтобы кнопка могла потом выбранный документ передать на редактирование

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
                                
                // Наполняем таблицу данными и подключаем к гриду
                if (dtDocKassBook.Rows.Count==0)
                {
                    this.dtDocKassBook.Columns.Add(new DataColumn("Id", typeof(string)));
                    this.dtDocKassBook.Columns.Add(new DataColumn("NoDoc", typeof(string)));
                    this.dtDocKassBook.Columns.Add(new DataColumn("FromTo", typeof(string)));
                    this.dtDocKassBook.Columns.Add(new DataColumn("KorShet", typeof(string)));
                    this.dtDocKassBook.Columns.Add(new DataColumn("Prihod", typeof(string)));
                    this.dtDocKassBook.Columns.Add(new DataColumn("Rashod", typeof(string)));
                }
                this.dvDocKassBook = new DataView(dtDocKassBook);
                this.dtGridKassBook.DataSource = this.dvDocKassBook;

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

                // Делаем активной вкладку кассовая книга
                this.tabCntOperation.SelectTab(2);

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
                                this.txtBoxPrihNumDoc.ReadOnly = true;
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
                                this.txtBoxRashNumDoc.ReadOnly = true;
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
                            this.cmbBoxRashPoDoc.Enabled = true;                // Список по документу

                            this.txtBoxRashDebitKodDivision.ReadOnly = false;
                            this.txtBoxRashDebitKodAnalUch.ReadOnly = false;
                            this.txtBoxRashSumma.ReadOnly = false;
                            this.txtBoxRashKodNazn.ReadOnly = false;
                            this.txtBoxRashPrilozenie.ReadOnly = false;

                            break;
                        // Кассовая книга
                        case 2:
                            this.txtBoxKasBookOKPO.ReadOnly = false;
                            this.txtBoxKasBookOKUD.ReadOnly = false;
                            this.txtBoxKasBookOrganization.ReadOnly = false;
                            this.txtBoxKasBookStructPodr.ReadOnly = false;
                            this.txtBoxKasBookDateDoc.ReadOnly = false;

                            // разрешено править всем
                            this.cmbBoxKassBookBuh.Enabled = true;
                            this.cmbBoxKassBookKasir.Enabled = true;

                            this.txtBoxKasBookDolRukOrg.ReadOnly = false;
                            this.txtBoxKasBookRukFio.ReadOnly = false;
                            this.txtBoxKasBookGlavBuh.ReadOnly = false;
                            
                            break;
                        case 3:
                            // Делаем поля читаемыми только админу и менеджеру
                            if (Com.UserFarm.CurrentUser.Role == RoleEn.Admin ||
                                Com.UserFarm.CurrentUser.Role == RoleEn.Manager)
                            {
                                this.txtBoxInventOrganization.ReadOnly = false;
                                this.txtBoxInventStructPodr.ReadOnly = false;
                                this.txtBoxInventOKPO.ReadOnly = false;
                                this.txtBoxInventOKUD.ReadOnly = false;
                                this.txtBoxInventDateDoc.ReadOnly = false;
                                this.txtBoxInventNumDoc.ReadOnly = true;
                            }                         
                            
                            // Левая сторона
                            this.txtBoxInventFactStr1.ReadOnly = true;
                            this.txtBoxInventFactStr2.ReadOnly = false;
                            this.txtBoxInventFactStr3.ReadOnly = false;
                            this.txtBoxInventFactStr4.ReadOnly = false;
                            this.txtBoxInventFactStr5.ReadOnly = false;
                            this.txtBoxInventFactVal1.ReadOnly = false;
                            this.txtBoxInventFactVal2.ReadOnly = false;
                            this.txtBoxInventFactVal3.ReadOnly = false;
                            this.txtBoxInventFactVal4.ReadOnly = false;
                            this.txtBoxInventFactVal5.ReadOnly = false;
                            this.txtBoxInventItogPoUchDan.ReadOnly = false;
                            this.txtBoxInventLastPrihodNum.ReadOnly = false;
                            this.txtBoxInventLastRashodNum.ReadOnly = false;

                            // Правая сторона центр
                            this.txtBoxInventPrikazTypAndDocNum.ReadOnly = false;
                            this.txtBoxInventPrikazUreDate.ReadOnly = false;
                            this.txtBoxInventPrikazDolMatOtv.ReadOnly = false;
                            this.txtBoxInventPrikazDecodeMatOtv.ReadOnly = false;

                            // Правая торона низ
                            this.txtBoxInventKomissionStr1.ReadOnly = false;
                            this.txtBoxInventKomissionStr2.ReadOnly = false;
                            this.txtBoxInventKomissionStr3.ReadOnly = false;
                            this.txtBoxInventKomissionStr4.ReadOnly = false;
                            this.txtBoxInventTitleKomissionDecode1.ReadOnly = false;
                            this.txtBoxInventTitleKomissionDecode2.ReadOnly = false;
                            this.txtBoxInventTitleKomissionDecode3.ReadOnly = false;
                            this.txtBoxInventTitleKomissionDecode4.ReadOnly = false;
                            
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
                            this.cmbBoxRashPoDoc.Enabled = false;                   // Список по документу

                            this.txtBoxRashDebitKodDivision.ReadOnly = true;
                            this.txtBoxRashDebitKodAnalUch.ReadOnly = true;
                            this.txtBoxRashSumma.ReadOnly = true;
                            this.txtBoxRashKodNazn.ReadOnly = true;
                            this.txtBoxRashPrilozenie.ReadOnly = true;

                            break;
                        // Кассовая книга
                        case 2:
                            this.txtBoxKasBookOKPO.ReadOnly = true;
                            this.txtBoxKasBookOKUD.ReadOnly = true;
                            this.txtBoxKasBookOrganization.ReadOnly = true;
                            this.txtBoxKasBookStructPodr.ReadOnly = true;
                            this.txtBoxKasBookDateDoc.ReadOnly = false;

                            // разрешено править всем
                            this.cmbBoxKassBookBuh.Enabled = false;
                            this.cmbBoxKassBookKasir.Enabled = false;

                            this.txtBoxKasBookDolRukOrg.ReadOnly = true;
                            this.txtBoxKasBookRukFio.ReadOnly = true;
                            this.txtBoxKasBookGlavBuh.ReadOnly = true;


                            break;
                        // Инвентаризация средств
                        case 3:                          
                            this.txtBoxInventOrganization.ReadOnly = true;
                            this.txtBoxInventStructPodr.ReadOnly = true;
                            this.txtBoxInventOKPO.ReadOnly = true;
                            this.txtBoxInventOKUD.ReadOnly = true;
                            this.txtBoxInventDateDoc.ReadOnly = true;
                            this.txtBoxInventNumDoc.ReadOnly = true;
                          
                            // Левая сторона
                            this.txtBoxInventFactStr1.ReadOnly = true;
                            this.txtBoxInventFactStr2.ReadOnly = true;
                            this.txtBoxInventFactStr3.ReadOnly = true;
                            this.txtBoxInventFactStr4.ReadOnly = true;
                            this.txtBoxInventFactStr5.ReadOnly = true;
                            this.txtBoxInventFactVal1.ReadOnly = true;
                            this.txtBoxInventFactVal2.ReadOnly = true;
                            this.txtBoxInventFactVal3.ReadOnly = true;
                            this.txtBoxInventFactVal4.ReadOnly = true;
                            this.txtBoxInventFactVal5.ReadOnly = true;
                            this.txtBoxInventItogPoUchDan.ReadOnly = true;
                            this.txtBoxInventLastPrihodNum.ReadOnly = true;
                            this.txtBoxInventLastRashodNum.ReadOnly = true;

                            // Правая сторона центр
                            this.txtBoxInventPrikazTypAndDocNum.ReadOnly = true;
                            this.txtBoxInventPrikazUreDate.ReadOnly = true;
                            this.txtBoxInventPrikazDolMatOtv.ReadOnly = true;
                            this.txtBoxInventPrikazDecodeMatOtv.ReadOnly = true;

                            // Правая торона низ
                            this.txtBoxInventKomissionStr1.ReadOnly = true;
                            this.txtBoxInventKomissionStr2.ReadOnly = true;
                            this.txtBoxInventKomissionStr3.ReadOnly = true;
                            this.txtBoxInventKomissionStr4.ReadOnly = true;
                            this.txtBoxInventTitleKomissionDecode1.ReadOnly = true;
                            this.txtBoxInventTitleKomissionDecode2.ReadOnly = true;
                            this.txtBoxInventTitleKomissionDecode3.ReadOnly = true;
                            this.txtBoxInventTitleKomissionDecode4.ReadOnly = true;

                            break;
                        default:
                            break;
                    }
                }
                //
                if (((ButtonTagStatus)this.btnPrint.Tag).Stat == ButtonStatusEn.Active
                    || (this.CurDoc != null && this.CurDoc.Id != null))
                {
                    this.btnPrint.ForeColor = Color.Black;
                    this.btnPrint.Enabled = true;
                }
                else
                {
                    this.btnPrint.ForeColor = Color.Silver;
                    this.btnPrint.Enabled = false;
                }
                //
                if (((ButtonTagStatus)this.btnOperator.Tag).Stat == ButtonStatusEn.Active)
                {
                    this.btnOperator.ForeColor = Color.Black;
                    this.btnOperator.Enabled = true;
                }
                else
                {
                    this.btnOperator.ForeColor = Color.Silver;
                    this.btnOperator.Enabled = false;
                }
                //
                if (((ButtonTagStatus)this.btnExit.Tag).Stat == ButtonStatusEn.Active)
                {
                    this.btnExit.ForeColor = Color.Black;
                    this.btnExit.Enabled = true;
                }
                else
                {
                    this.btnExit.ForeColor = Color.Silver;
                    this.btnExit.Enabled = false;
                }
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

                // Если нет подключенияк базе
                if (Com.ProviderFarm.CurrentPrv == null || !Com.ProviderFarm.CurrentPrv.HashConnect)
                {
                    Com.Log.EventSave("Нет подключения к базе данных", string.Format("{0}.btnNew_Click", GetType().Name), EventEn.Error, true, true);
                    return;
                }

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
                        this.txtBoxPrihOKUD.Text = (OperPrihod!=null && !string.IsNullOrWhiteSpace(OperPrihod.OKUD) ? OperPrihod.OKUD: "0310001");

                        // Приход заполняем список принято от
                        this.cmbBoxPrihKreditor.Items.Clear();
                        foreach (Local item in LocalFarm.CurLocalEmployees)
                        {
                            this.cmbBoxPrihKreditor.Items.Add(item.LocalName);
                        }


                        // Заполняем получил кассир
                        this.cmbBoxPrihDebitor.Items.Clear();
                        foreach (Local item in LocalFarm.CurLocalChiefCashiers)
                        {
                            this.cmbBoxPrihDebitor.Items.Add(item.LocalName);
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
                        this.txtBoxRashOKUD.Text = (OperRashod != null && !string.IsNullOrWhiteSpace(OperRashod.OKUD) ? OperRashod.OKUD : "0310002");

                        // Приход заполняем список выдать
                        this.cmbBoxRashDebitor.Items.Clear();
                        foreach (Local item in LocalFarm.CurLocalEmployees)
                        {
                            this.cmbBoxRashDebitor.Items.Add(item.LocalName);
                        }


                        // Заполняем выдал кассир
                        this.cmbBoxRashKreditor.Items.Clear();
                        foreach (Local item in LocalFarm.CurLocalChiefCashiers)
                        {
                            this.cmbBoxRashKreditor.Items.Add(item.LocalName);
                        }

                        // Основание
                        if (this.cmbBoxRashPaidRashReasons.Items.Count == 0)
                        {
                            foreach (Local item in LocalFarm.CurLocalPaidRashReasons)
                            {
                                this.cmbBoxRashPaidRashReasons.Items.Add(item.LocalName);
                            }
                        }

                        // Список по документу
                        if (this.cmbBoxRashPaidRashReasons.Items.Count == 0)
                        {
                            foreach (Local item in LocalFarm.CurLocalRashPoDocum)
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
                        this.txtBoxRashPrilozenie.Text = string.Empty;
                        
                        break;
                    // Кассовая книга
                    case 2:
                        this.txtBoxKasBookOrganization.Text = Kassa.Organization;
                        this.txtBoxKasBookStructPodr.Text = Kassa.StructPodrazdelenie;
                        this.txtBoxKasBookOKPO.Text = Kassa.OKPO;

                        
                        this.txtBoxKasBookDolRukOrg.Text = Kassa.DolRukOrg;
                        this.txtBoxKasBookRukFio.Text = Kassa.RukFio;
                        this.txtBoxKasBookGlavBuh.Text = Kassa.GlavBuhFio;

                        // Заполняем инфу по операции
                        BLL.OperationPlg.OperationKasBook OperKasBook = (BLL.OperationPlg.OperationKasBook)OperationFarm.CurOperationList["OperationKasBook"];
                        this.txtBoxKasBookOKUD.Text = (OperKasBook != null && !string.IsNullOrWhiteSpace(OperKasBook.OKUD) ? OperKasBook.OKUD : "0310004");

                        // Заполняем Бухгалтер
                        this.cmbBoxKassBookBuh.Items.Clear();
                        foreach (Local item in LocalFarm.CurLocalAccounters)
                        {
                            this.cmbBoxKassBookBuh.Items.Add(item.LocalName);
                        }

                        // Заполняем получил кассир
                        this.cmbBoxKassBookKasir.Items.Clear();
                        foreach (Local item in LocalFarm.CurLocalChiefCashiers)
                        {
                            this.cmbBoxKassBookKasir.Items.Add(item.LocalName);
                        }

                        // Заполняем список по документу
                        this.cmbBoxRashPoDoc.Items.Clear();
                        foreach (Local item in LocalFarm.CurLocalRashPoDocum)
                        {
                            this.cmbBoxRashPoDoc.Items.Add(item.LocalName);
                        }

                        // Заполняем поле основание значенеие по умолчанию и зависимые поля
                        this.cmbBoxKassBookBuh.SelectedIndex = -1;
                        this.cmbBoxKassBookKasir.SelectedIndex = -1;

                        // Если даты в строке небыло то не нужно чтобы сработала кнопка как при смене даты так как мы тут поменяем дату она и так сработает
                        if (string.IsNullOrWhiteSpace(this.txtBoxKasBookDateDoc.Text))
                        {
                           
                            this.txtBoxKasBookDateDoc.Text = DateTime.Now.Date.ToShortDateString();
                        }
                        else this.txtBoxKasBookDateDoc_TextChanged(null, null);

                        break;
                    // Инвентаризация средств
                    case 3:
                        this.txtBoxInventOrganization.Text = Kassa.Organization;
                        this.txtBoxInventStructPodr.Text = Kassa.StructPodrazdelenie;
                        this.txtBoxInventOKPO.Text = Kassa.OKPO;
                        this.txtBoxInventDateDoc.Text = DateTime.Now.ToShortDateString();
                        this.txtBoxInventNumDoc.Text = string.Empty;

                        // Заполняем инфу по операции
                        BLL.OperationPlg.OperationInvent OperInvent = (BLL.OperationPlg.OperationInvent)OperationFarm.CurOperationList["OperationInvent"];
                        this.txtBoxInventOKUD.Text = (OperInvent != null && !string.IsNullOrWhiteSpace(OperInvent.OKUD) ? OperInvent.OKUD : "0317013");

                        // Левая сторона
                        this.txtBoxInventFactStr1.Text = "наличных денег";
                        this.txtBoxInventFactStr2.Text = string.Empty;
                        this.txtBoxInventFactStr3.Text = string.Empty;
                        this.txtBoxInventFactStr4.Text = string.Empty;
                        this.txtBoxInventFactStr5.Text = string.Empty;
                        this.txtBoxInventFactVal1.Text = "0";
                        this.txtBoxInventFactVal2.Text = "0";
                        this.txtBoxInventFactVal3.Text = "0";
                        this.txtBoxInventFactVal4.Text = "0";
                        this.txtBoxInventFactVal5.Text = "0";
                        this.txtBoxInventItogPoUchDan.Text = "0";
                        this.txtBoxInventLastPrihodNum.Text = "0";
                        this.txtBoxInventLastRashodNum.Text = "0";

                        // Правая сторона центр
                        this.txtBoxInventPrikazTypAndDocNum.Text = string.Empty;
                        this.txtBoxInventPrikazUreDate.Text = string.Empty;
                        this.txtBoxInventPrikazDolMatOtv.Text = string.Empty;
                        this.txtBoxInventPrikazDecodeMatOtv.Text = string.Empty;

                        // Правая торона низ
                        this.txtBoxInventKomissionStr1.Text = string.Empty;
                        this.txtBoxInventKomissionStr2.Text = string.Empty;
                        this.txtBoxInventKomissionStr3.Text = string.Empty;
                        this.txtBoxInventKomissionStr4.Text = string.Empty;
                        this.txtBoxInventTitleKomissionDecode1.Text = string.Empty;
                        this.txtBoxInventTitleKomissionDecode2.Text = string.Empty;
                        this.txtBoxInventTitleKomissionDecode3.Text = string.Empty;
                        this.txtBoxInventTitleKomissionDecode4.Text = string.Empty;

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
        
        // Всовываем на всех эледементах чтобы смотерть активность пользователя на движение мышки
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

        // Пользователь вызвал список старших кассиров для правки
        private void TSMItemLocalChiefCashiers_Click(object sender, EventArgs e)
        {
            try
            {
                using (FListLocalChiefCashiers Frm = new FListLocalChiefCashiers())
                {
                    Frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при работе со списком старших кассиров с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.TSMItemLocalChiefCashiers", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Пользователь вызвал список кассиров
        private void TSMItemLocalEmployees_Click(object sender, EventArgs e)
        {
            try
            {
                using (FListLocalEmployees Frm = new FListLocalEmployees())
                {
                    Frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при работе со списком старших кассиров с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.TSMItemLocalEmployees_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Пользователь вызвал список бухгалтеров
        private void TSMItemLocalAccounters_Click(object sender, EventArgs e)
        {
            try
            {
                using (FListLocalAccounters Frm = new FListLocalAccounters())
                {
                    Frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при работе со списком старших кассиров с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.TSMItemLocalAccounters_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Настройка кассы
        private void TSMItemLocalKassa_Click(object sender, EventArgs e)
        {
            try
            {
                using (FListLocalKassa Frm = new FListLocalKassa())
                {
                    Frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при работе со списком старших кассиров с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.TSMItemLocalKassa_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Список орснований для пихода
        private void TSMItemLocalPaidInReasons_Click(object sender, EventArgs e)
        {
            try
            {
                using (FListLocalPaidInReasons Frm = new FListLocalPaidInReasons())
                {
                    Frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при работе со списком старших кассиров с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.TSMItemLocalPaidInReasons_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Список орснований для расхода
        private void TSMItemLocalPaidRashReasons_Click(object sender, EventArgs e)
        {
            try
            {
                using (FListLocalPaidRashReasons Frm = new FListLocalPaidRashReasons())
                {
                    Frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при работе со списком старших кассиров с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.TSMItemLocalPaidRashReasons_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Список по документу в расходе
        private void TSMItemLocalRashPoDocum_Click(object sender, EventArgs e)
        {
            try
            {
                using (FListLocalRashPoDocum Frm = new FListLocalRashPoDocum())
                {
                    Frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при работе со списком старших кассиров с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.TSMItemLocalRashPoDocum_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
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
                        // Запоминаем инфу по организации только если документ в текущей дете если нет то это правка старого документа запоминать тогда не нужно
                        if (DateTime.Parse(this.txtBoxPrihDateDoc.Text).Date == DateTime.Now.Date)
                        {
                            Kassa.Organization = this.txtBoxPrihOrganization.Text;
                            Kassa.StructPodrazdelenie = this.txtBoxPrichStructPodr.Text;
                            Kassa.OKPO = this.txtBoxPrihOKPO.Text;
                            Kassa.GlavBuhFio = this.txtBoxPrihGlavBuh.Text;

                            // Валидация заполненных данных по подразделению и сохранение в базе
                            ValidateKassa(Kassa);
                        }

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
                        
                        // Валидация Дебитора
                        if (this.cmbBoxPrihDebitor.SelectedIndex == -1)
                        {
                            if (string.IsNullOrWhiteSpace(this.cmbBoxPrihDebitor.Text)) throw new ApplicationException("Не указано кто получил.");
                            else
                            {
                                this.CurDoc.LocalDebitor = null;
                                this.CurDoc.OtherDebitor= this.cmbBoxPrihDebitor.Text.Trim();
                            }
                        }
                        else
                        {
                            this.CurDoc.LocalDebitor = LocalFarm.CurLocalChiefCashiers[this.cmbBoxPrihDebitor.SelectedIndex];
                        }

                        // Валидация кредитора
                        if (this.cmbBoxPrihKreditor.SelectedIndex == -1)
                        {
                            if (string.IsNullOrWhiteSpace(this.cmbBoxPrihKreditor.Text)) throw new ApplicationException("Не указано от кого принято.");
                            else
                            {
                                this.CurDoc.LocalCreditor = null;
                                CurDoc.OtherKreditor = this.cmbBoxPrihKreditor.Text.Trim();
                            }
                        }
                        else
                        {
                            this.CurDoc.LocalCreditor = LocalFarm.CurLocalEmployees[this.cmbBoxPrihKreditor.SelectedIndex];
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
                        if (((DateTime)this.CurDoc.UreDate).Date == DateTime.Now.Date)
                        {
                            Kassa.LastDocNumPrih = int.Parse(this.txtBoxPrihNumDoc.Text);
                        }
                        else
                        {
                            // Меняем нумерацию документов в текущем году
                            Com.ProviderFarm.CurrentPrv.UpdateNumDocForAdd(this.CurDoc);

                            // Если это текущий годж то менять номер нужно если не тот год но вставка в предыдущий год не меняет номер документа в текущем году
                            if (((DateTime)this.CurDoc.UreDate).Year== DateTime.Now.Year) Kassa.LastDocNumPrih++;
                        }

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
                        // Запоминаем инфу по организации только если документ в текущей дете если нет то это правка старого документа запоминать тогда не нужно
                        if (DateTime.Parse(this.txtBoxRashDateDoc.Text).Date==DateTime.Now.Date)
                        { 
                            Kassa.Organization = this.txtBoxRashOrganization.Text;
                            Kassa.StructPodrazdelenie = this.txtBoxRashStructPodr.Text;
                            Kassa.OKPO = this.txtBoxRashOKPO.Text;
                            Kassa.DolRukOrg = this.txtBoxRashDolRukOrg.Text;
                            Kassa.RukFio = this.txtBoxRashRukFio.Text;
                            Kassa.GlavBuhFio = this.txtBoxRashGlavBuh.Text;

                            // Валидация заполненных данных по подразделению и сохранение в базе
                            ValidateKassa(Kassa);
                        }
                        

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
                        if (this.cmbBoxRashDebitor.SelectedIndex == -1)
                        {
                            if(string.IsNullOrWhiteSpace(this.cmbBoxRashDebitor.Text)) throw new ApplicationException("Не указано кому выдать.");
                            else
                            {
                                this.CurDoc.LocalDebitor = null;
                                this.CurDoc.OtherDebitor = this.cmbBoxRashDebitor.Text.Trim();
                            }
                        }
                        else
                        {
                            this.CurDoc.LocalDebitor = LocalFarm.CurLocalEmployees[this.cmbBoxRashDebitor.SelectedIndex];
                        }

                        // Валидация кредитора
                        if (this.cmbBoxRashKreditor.SelectedIndex == -1)
                        {
                            if (string.IsNullOrWhiteSpace(this.cmbBoxRashKreditor.Text)) throw new ApplicationException("Не указано кто выдал.");
                            else
                            {
                                this.CurDoc.LocalCreditor = null;
                                this.CurDoc.OtherKreditor = this.cmbBoxRashKreditor.Text.Trim();
                            }
                        }
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
                            ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).PaidRashReasons = LoclPaidRashReasons;
                            LoclPaidRashReasons.Save();

                        }

                        // Сохраняем осталдьные параметры документа
                        ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).PoDoc = this.cmbBoxRashPoDoc.Text;
                        ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).Prilozenie = this.txtBoxRashPrilozenie.Text;
                        ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).DolRukFio = this.txtBoxRashDolRukOrg.Text;
                        ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).RukFio = this.txtBoxRashRukFio.Text;
                        ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).GlavBuh = this.txtBoxRashGlavBuh.Text;
                        
                        // Сохранение инфы в базе
                        if (((DateTime)this.CurDoc.UreDate).Date == DateTime.Now.Date)
                        {
                            Kassa.LastDocNumPrih = int.Parse(this.txtBoxRashNumDoc.Text);
                        }
                        else
                        {
                            // Меняем нумерацию документов в текущем году
                            Com.ProviderFarm.CurrentPrv.UpdateNumDocForAdd(this.CurDoc);

                            // Если это текущий годж то менять номер нужно если не тот год но вставка в предыдущий год не меняет номер документа в текущем году
                            if (((DateTime)this.CurDoc.UreDate).Year == DateTime.Now.Year) Kassa.LastDocNumPrih++;
                        }
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
                        // Запоминаем инфу по организации только если документ в текущей дете если нет то это правка старого документа запоминать тогда не нужно
                        if (DateTime.Parse(this.txtBoxKasBookDateDoc.Text).Date == DateTime.Now.Date)
                        {
                            Kassa.Organization = this.txtBoxKasBookOrganization.Text;
                            Kassa.StructPodrazdelenie = this.txtBoxKasBookStructPodr.Text;
                            Kassa.OKPO = this.txtBoxKasBookOKPO.Text;
                            Kassa.DolRukOrg = this.txtBoxKasBookDolRukOrg.Text;
                            Kassa.RukFio = this.txtBoxKasBookRukFio.Text;
                            Kassa.GlavBuhFio = this.txtBoxRashGlavBuh.Text;

                            // Валидация заполненных данных по подразделению и сохранение в базе
                            ValidateKassa(Kassa);
                        }               
                        
                        // Валидация Дебитора
                        if (this.cmbBoxKassBookBuh.SelectedIndex == -1)
                        {
                            if (string.IsNullOrWhiteSpace(this.cmbBoxKassBookBuh.Text)) throw new ApplicationException("Не указан бухгалтер.");
                            else
                            {
                                this.CurDoc.LocalDebitor = null;
                                this.CurDoc.OtherDebitor = this.cmbBoxKassBookBuh.Text.Trim();
                            }
                        }
                        else
                        {
                            this.CurDoc.LocalDebitor = LocalFarm.CurLocalAccounters[this.cmbBoxKassBookBuh.SelectedIndex];
                        }

                        // Валидация кредитора - кассира
                        if (this.cmbBoxKassBookKasir.SelectedIndex == -1)
                        {
                            if (string.IsNullOrWhiteSpace(this.cmbBoxKassBookKasir.Text)) throw new ApplicationException("Не указано кто выдал.");
                            else
                            {
                                this.CurDoc.LocalCreditor = null;
                                this.CurDoc.OtherKreditor = this.cmbBoxKassBookKasir.Text.Trim();
                            }
                        }
                        else
                        {
                            this.CurDoc.LocalCreditor = LocalFarm.CurLocalChiefCashiers[this.cmbBoxKassBookKasir.SelectedIndex];
                        }


                        // Валидация суммы на начало дня
                        if (!string.IsNullOrWhiteSpace(this.txtBoxKassBookStartDay.Text))
                        {
                            try
                            {
                                ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).SummaStartDay = decimal.Round(decimal.Parse(this.txtBoxKassBookStartDay.Text), 2);
                            }
                            catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение к сумме на начало дня {0}.", this.txtBoxKassBookStartDay.Text)); }
                        }
                        else ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).SummaStartDay = 0;

                        // Валидация суммы на конец дня
                        if (!string.IsNullOrWhiteSpace(this.txtBoxKassBookEndDay.Text))
                        {
                            try
                            {
                                ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).SummaEndDay = decimal.Round(decimal.Parse(this.txtBoxKassBookEndDay.Text), 2);
                            }
                            catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение к сумме на конец дня {0}.", this.txtBoxKassBookEndDay.Text)); }
                        }
                        else ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).SummaEndDay = 0;

                        ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).DolRukFio = this.txtBoxKasBookDolRukOrg.Text;
                        ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).RukFio = this.txtBoxKasBookRukFio.Text;
                        ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).GlavBuh = this.txtBoxKasBookGlavBuh.Text;

                        // Сохранение инфы в базе
                        Kassa.LastDocNumKasBook = this.CurDoc.DocNum;
                        Kassa.Save();

                        // Валидация введённой даты
                        try { this.CurDoc.UreDate = DateTime.Parse(this.txtBoxKasBookDateDoc.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} к дате.", this.txtBoxKasBookDateDoc.Text)); }
                        
                        // Заполняем инфу по операции
                        BLL.OperationPlg.OperationKasBook OperKasBook = (BLL.OperationPlg.OperationKasBook)this.CurDoc.CurOperation;
                        OperKasBook.OKUD = txtBoxKasBookOKUD.Text;
                        OperKasBook.Save();

                        // Сохранение документа
                        this.CurDoc.Save();

                        break;
                    // Инвентаризация средств
                    case 3:
                        // Запоминаем инфу по организации только если документ в текущей дете если нет то это правка старого документа запоминать тогда не нужно
                        if (DateTime.Parse(this.txtBoxInventDateDoc.Text).Date == DateTime.Now.Date)
                        {
                            Kassa.Organization = this.txtBoxInventOrganization.Text;
                            Kassa.StructPodrazdelenie = this.txtBoxInventStructPodr.Text;
                            Kassa.OKPO = this.txtBoxInventOKPO.Text;
                            
                            // Валидация заполненных данных по подразделению и сохранение в базе
                            ValidateKassa(Kassa);
                        }

                        // Валидация введённой даты
                        try { this.CurDoc.UreDate = DateTime.Parse(this.txtBoxInventDateDoc.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} к дате.", this.txtBoxInventDateDoc.Text)); }
                        
                        // Валидация введённой даты
                        if (string.IsNullOrWhiteSpace(this.txtBoxInventDateDoc.Text)) new ApplicationException("Поле номер документа не может быть пустым.");
                        else
                        {
                            try { this.CurDoc.DocNum = int.Parse(this.txtBoxInventNumDoc.Text); }
                            catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение номера документа {0} к числу.", this.txtBoxRashNumDoc.Text)); }
                        }
                        
                        // Валидация Дебитора
                        this.CurDoc.LocalDebitor = Kassa;

                        // Валидация кредитора
                        this.CurDoc.LocalCreditor = Kassa;

                        // Левая сторона
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactStr1 = this.txtBoxInventFactStr1.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactStr2 = this.txtBoxInventFactStr2.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactStr3 = this.txtBoxInventFactStr3.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactStr4 = this.txtBoxInventFactStr4.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactStr5 = this.txtBoxInventFactStr5.Text;
                        try { ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactVal1 = Decimal.Parse(this.txtBoxInventFactVal1.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventFactVal1.Text)); }
                        try { ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactVal2 = Decimal.Parse(this.txtBoxInventFactVal2.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventFactVal2.Text)); }
                        try { ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactVal3 = Decimal.Parse(this.txtBoxInventFactVal3.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventFactVal3.Text)); }
                        try { ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactVal4 = Decimal.Parse(this.txtBoxInventFactVal4.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventFactVal4.Text)); }
                        try { ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactVal5 = Decimal.Parse(this.txtBoxInventFactVal5.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventFactVal5.Text)); }
                        try { ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).ItogPoUchDan = Decimal.Parse(this.txtBoxInventItogPoUchDan.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventItogPoUchDan.Text)); }
                        try { ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).LastPrihodNum = int.Parse(this.txtBoxInventLastPrihodNum.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventLastPrihodNum.Text)); }
                        try { ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).LastRashodNum = int.Parse(this.txtBoxInventLastRashodNum.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventLastRashodNum.Text)); }

                        // Правая сторона центр
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).PrikazTypAndDocNum = this.txtBoxInventPrikazTypAndDocNum.Text;
                        try { ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).PrikazUreDate = DateTime.Parse(this.txtBoxInventPrikazUreDate.Text); }
                        catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать значение {0} к дате.", this.txtBoxInventPrikazUreDate.Text)); }
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).PrikazDolMatOtv = this.txtBoxInventPrikazDolMatOtv.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).PrikazDecodeMatOtv = this.txtBoxInventPrikazDecodeMatOtv.Text;

                        // Правая торона низ
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionStr1 = this.txtBoxInventKomissionStr1.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionStr2 = this.txtBoxInventKomissionStr2.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionStr3 = this.txtBoxInventKomissionStr3.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionStr4 = this.txtBoxInventKomissionStr4.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionDecode1 = this.txtBoxInventTitleKomissionDecode1.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionDecode2 = this.txtBoxInventTitleKomissionDecode2.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionDecode3 = this.txtBoxInventTitleKomissionDecode3.Text;
                        ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionDecode4 = this.txtBoxInventTitleKomissionDecode4.Text;
                        
                        // Сохранение инфы в базе
                        if (((DateTime)this.CurDoc.UreDate).Date == DateTime.Now.Date)
                        {
                            Kassa.LastDocNumPrih = int.Parse(this.txtBoxInventNumDoc.Text);
                        }
                        else
                        {
                            // Меняем нумерацию документов в текущем году
                            Com.ProviderFarm.CurrentPrv.UpdateNumDocForAdd(this.CurDoc);

                            // Если это текущий годж то менять номер нужно если не тот год но вставка в предыдущий год не меняет номер документа в текущем году
                            if (((DateTime)this.CurDoc.UreDate).Year == DateTime.Now.Year) Kassa.LastDocNumPrih++;
                        }
                        Kassa.Save();

                        // Заполняем инфу по операции
                        BLL.OperationPlg.OperationInvent OperInvent = (BLL.OperationPlg.OperationInvent)this.CurDoc.CurOperation;
                        OperInvent.OKUD = this.txtBoxInventOKUD.Text;
                        OperInvent.Save();

                        // Сохранение документа
                        this.CurDoc.Save();

                        // Печать документа
                        this.CurDoc.PrintDefault();

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

                if (this.tabCntOperation.SelectedIndex==2 && sender!=null && e != null)
                {
                    txtBoxKasBookDateDoc_TextChanged(null, null);
                    return;
                }

                // Если нет подключенияк базе
                if (Com.ProviderFarm.CurrentPrv == null || !Com.ProviderFarm.CurrentPrv.HashConnect)
                {
                    Com.Log.EventSave("Нет подключения к базе данных", string.Format("{0}.btnNew_Click", GetType().Name), EventEn.Error, true, true);
                    return;
                }

                // Получаем текущее подразделение
                BLL.LocalPlg.LocalKassa Kassa = Com.LocalFarm.CurLocalDepartament;

                // На какой вкладке активность
                switch (this.tabCntOperation.SelectedIndex)
                {
                    // Приходный ордер
                    case 0:
                        // Если был передан конкретный документ который пользователь правит то заполняем полями из документа
                        if (sender != null && e == null)
                        {
                            this.CurDoc = (BLL.DocumentPlg.DocumentPrihod)sender;
                            this.txtBoxPrihNumDoc.Text = this.CurDoc.DocNum.ToString();
                            this.txtBoxPrihGlavBuh.Text = ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).GlavBuh;
                            this.txtBoxPrihDateDoc.Text = ((DateTime)this.CurDoc.UreDate).ToShortDateString();

                            // Заполняем инфу по операции
                            BLL.OperationPlg.OperationPrihod OperPrihod = (BLL.OperationPlg.OperationPrihod)this.CurDoc.CurOperation;
                            this.txtBoxPrihOKUD.Text = (OperPrihod != null && !string.IsNullOrWhiteSpace(OperPrihod.OKUD) ? OperPrihod.OKUD : "0310001");

                            // Заполняем поле основание значенеие по умолчанию и зависимые поля
                            this.cmbBoxPrihPaidInReasons.SelectedIndexChanged -= cmbBoxPrihPaidInReasons_SelectedIndexChanged;
                            //
                            int selectIndexPaidInReasons = -1;
                            for (int i = 0; i < LocalFarm.CurLocalPaidInReasons.Count; i++)
                            {
                                if (LocalFarm.CurLocalPaidInReasons[i].LocalName == ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).PaidInReasons.LocalName)
                                {
                                    selectIndexPaidInReasons = i;
                                    break;
                                }
                            }
                            this.cmbBoxPrihPaidInReasons.SelectedIndex = selectIndexPaidInReasons;
                            //
                            this.txtBoxPrichDebetNomerSchet.Text = ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).DebetNomerSchet;
                            this.txtBoxPrihKreditKorSchet.Text = ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).KredikKorSchet;
                            this.txtBoxPrihOsnovanie.Text = ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).Osnovanie;
                            this.cmbBoxPrihPaidInReasons.SelectedIndexChanged += cmbBoxPrihPaidInReasons_SelectedIndexChanged;


                            // Дебитор
                            int selectIndexPrihDebitor= -1;
                            if (this.CurDoc.LocalDebitor != null)
                            {
                                for (int i = 0; i < LocalFarm.CurLocalChiefCashiers.Count; i++)
                                {
                                    if (LocalFarm.CurLocalChiefCashiers[i].LocalName == (this.CurDoc.LocalDebitor.LocalName))
                                    {
                                        selectIndexPrihDebitor = i;
                                        break;
                                    }
                                }
                            }
                            if (selectIndexPrihDebitor > -1) this.cmbBoxPrihDebitor.SelectedIndex = selectIndexPrihDebitor;
                            else this.cmbBoxPrihDebitor.Text = this.CurDoc.OtherDebitor;

                            // Кредитор
                            int selectIndexPrihKreditor = -1;
                            if (this.CurDoc.LocalCreditor != null)
                            {
                                for (int i = 0; i < LocalFarm.CurLocalEmployees.Count; i++)
                                {
                                    if (LocalFarm.CurLocalEmployees[i].LocalName == (this.CurDoc.LocalCreditor.LocalName))
                                    {
                                        selectIndexPrihKreditor = i;
                                        break;
                                    }
                                }
                            }
                            if (selectIndexPrihKreditor > -1) this.cmbBoxPrihKreditor.SelectedIndex = selectIndexPrihKreditor;
                            else this.cmbBoxPrihKreditor.Text = this.CurDoc.OtherKreditor;
                            
                            this.txtBoxPrihKreditKodDivision.Text = ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).KreditKodDivision;
                            this.txtBoxPrihKredikKodAnalUch.Text = ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).KredikKodAnalUch;
                            this.txtBoxPrihSumma.Text = ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                            this.txtBoxPrihKodNazn.Text = ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).KodNazn;
                            this.txtBoxPrihVtomChisle.Text = ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).VtomChisle;
                            this.txtBoxPrihPrilozenie.Text = ((BLL.DocumentPlg.DocumentPrihod)this.CurDoc).Prilozenie;
                        }
                        else
                        {
                            // Создаём пустой документ
                            this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentPrihod");
                            this.txtBoxPrihNumDoc.Text = (/*Kassa.LastDocNumPrih*/ Com.ProviderFarm.CurrentPrv.MaxDocNumForYaer(this.CurDoc) + 1).ToString();
                            this.txtBoxPrihGlavBuh.Text = Kassa.GlavBuhFio;
                            // Проверка на наличие ошибок при создании пустого документа
                            if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", "DocumentPrihod"));
                            //
                            this.txtBoxPrihDateDoc.Text = DateTime.Now.Date.ToShortDateString();

                            // Заполняем инфу по операции
                            BLL.OperationPlg.OperationPrihod OperPrihod = (BLL.OperationPlg.OperationPrihod)this.CurDoc.CurOperation;
                            this.txtBoxPrihOKUD.Text = (OperPrihod != null && !string.IsNullOrWhiteSpace(OperPrihod.OKUD) ? OperPrihod.OKUD : "0310001");

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

                            // Если указано значение по умолчению то выставляем его
                            if (Com.UserFarm.CurrentUser.EmploeeId!=null)
                            {
                                for (int i = 0; i < Com.LocalFarm.CurLocalEmployees.Count; i++)
                                {
                                    if (Com.LocalFarm.CurLocalEmployees[i].Id== Com.UserFarm.CurrentUser.EmploeeId)
                                    {
                                        this.cmbBoxPrihKreditor.SelectedIndex = i;
                                    }
                                }
                            }
                        }

                        break;
                    // Расходный ордер
                    case 1:

                        // Если был передан конкретный документ который пользователь правит то заполняем полями из документа
                        if (sender != null && e == null)
                        {
                            this.CurDoc = (BLL.DocumentPlg.DocumentRashod)sender;
                            this.txtBoxRashNumDoc.Text = this.CurDoc.DocNum.ToString();
                            this.txtBoxRashDolRukOrg.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).DolRukFio;
                            this.txtBoxRashRukFio.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).RukFio;
                            this.txtBoxRashGlavBuh.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).GlavBuh;
                            this.txtBoxRashDateDoc.Text = ((DateTime)this.CurDoc.UreDate).ToShortDateString();

                            // Заполняем инфу по операции
                            BLL.OperationPlg.OperationRashod OperRashod = (BLL.OperationPlg.OperationRashod)this.CurDoc.CurOperation;
                            this.txtBoxRashOKUD.Text = (OperRashod != null && !string.IsNullOrWhiteSpace(OperRashod.OKUD) ? OperRashod.OKUD : "0310002");

                            // Заполняем поле основание значенеие по умолчанию и зависимые поля
                            this.cmbBoxRashPaidRashReasons.SelectedIndexChanged -= cmbBoxRashPaidRashReasons_SelectedIndexChanged;
                            //
                            int selectIndexPaidRashReasons = -1;
                            for (int i = 0; i < LocalFarm.CurLocalPaidRashReasons.Count; i++)
                            {
                                if (LocalFarm.CurLocalPaidRashReasons[i].LocalName == ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).PaidRashReasons.LocalName)
                                {
                                    selectIndexPaidRashReasons = i;
                                    break;
                                }
                            }
                            this.cmbBoxRashPaidRashReasons.SelectedIndex = selectIndexPaidRashReasons;
                            //
                            this.txtBoxRashDebitKorSchet.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).DebetKorSchet;
                            this.txtBoxRashKreditNomerSchet.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).KreditNomerSchet;
                            this.txtBoxRashOsnovanie.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).Osnovanie;
                            this.cmbBoxRashPaidRashReasons.SelectedIndexChanged += cmbBoxRashPaidRashReasons_SelectedIndexChanged;


                            // Дебитор
                            int selectIndexRashDebitor = -1;
                            if (this.CurDoc.LocalDebitor != null)
                            {
                                for (int i = 0; i < LocalFarm.CurLocalEmployees.Count; i++)
                                {
                                    if (LocalFarm.CurLocalEmployees[i].LocalName == (this.CurDoc.LocalDebitor.LocalName))
                                    {
                                        selectIndexRashDebitor = i;
                                        break;
                                    }
                                }
                            }
                            if (selectIndexRashDebitor > -1) this.cmbBoxRashDebitor.SelectedIndex = selectIndexRashDebitor;
                            else this.cmbBoxRashDebitor.Text = this.CurDoc.OtherDebitor;
                            // Кредитор
                            int selectIndexRashKreditor = -1;
                            if (this.CurDoc.LocalCreditor != null)
                            {
                                for (int i = 0; i < LocalFarm.CurLocalChiefCashiers.Count; i++)
                                {
                                    if (LocalFarm.CurLocalChiefCashiers[i].LocalName == (this.CurDoc.LocalCreditor.LocalName))
                                    {
                                        selectIndexRashKreditor = i;
                                        break;
                                    }
                                }
                            }
                            if (selectIndexRashKreditor > -1) this.cmbBoxRashKreditor.SelectedIndex = selectIndexRashKreditor;
                            else this.cmbBoxRashKreditor.Text = this.CurDoc.OtherKreditor;

                            // Кредитор
                            int selectIndexRashPoDoc = -1;
                            if (((BLL.DocumentPlg.DocumentRashod)this.CurDoc).PoDoc != null)
                            {
                                for (int i = 0; i < LocalFarm.CurLocalRashPoDocum.Count; i++)
                                {
                                    if (LocalFarm.CurLocalRashPoDocum[i].LocalName == ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).PoDoc)
                                    {
                                        selectIndexRashPoDoc = i;
                                        break;
                                    }
                                }
                            }
                            if (selectIndexRashPoDoc > -1) this.cmbBoxRashPoDoc.SelectedIndex = selectIndexRashPoDoc;
                            else this.cmbBoxRashPoDoc.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).PoDoc;


                            this.txtBoxRashDebitKodDivision.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).DebetKodDivision;
                            this.txtBoxRashDebitKodAnalUch.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).DebetKodAnalUch;
                            this.txtBoxRashSumma.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                            this.txtBoxRashKodNazn.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).KodNazn;
                            this.txtBoxRashPrilozenie.Text = ((BLL.DocumentPlg.DocumentRashod)this.CurDoc).Prilozenie;
                        }
                        else
                        {
                            // Создаём пустой документ
                            this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentRashod");
                            this.txtBoxRashNumDoc.Text = (/*Kassa.LastDocNumRash*/ Com.ProviderFarm.CurrentPrv.MaxDocNumForYaer(this.CurDoc) + 1).ToString();
                            this.txtBoxRashDolRukOrg.Text = Kassa.DolRukOrg;
                            this.txtBoxRashRukFio.Text = Kassa.RukFio;
                            this.txtBoxRashGlavBuh.Text = Kassa.GlavBuhFio;
                            // Проверка на наличие ошибок при создании пустого документа
                            if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", ""));
                            //
                            this.txtBoxRashDateDoc.Text = DateTime.Now.Date.ToShortDateString();

                            // Заполняем инфу по операции
                            BLL.OperationPlg.OperationRashod OperRashod = (BLL.OperationPlg.OperationRashod)this.CurDoc.CurOperation;
                            this.txtBoxRashOKUD.Text = (OperRashod != null && !string.IsNullOrWhiteSpace(OperRashod.OKUD) ? OperRashod.OKUD : "0310002");


                            // Заполняем поле основание значенеие по умолчанию и зависимые поля
                            this.cmbBoxRashPaidRashReasons_SelectedIndexChanged(null, null);

                            this.cmbBoxRashDebitor.SelectedIndex = -1;
                            this.cmbBoxRashKreditor.SelectedIndex = -1;
                            this.cmbBoxRashPoDoc.SelectedIndex = -1;

                            this.txtBoxRashDebitKodDivision.Text = string.Empty;
                            this.txtBoxRashDebitKodAnalUch.Text = string.Empty;
                            this.txtBoxRashSumma.Text = string.Empty;
                            this.txtBoxRashKodNazn.Text = string.Empty;
                            this.txtBoxRashPrilozenie.Text = string.Empty;

                            // Если указано значение по умолчению то выставляем его
                            if (Com.UserFarm.CurrentUser.EmploeeId != null)
                            {
                                for (int i = 0; i < Com.LocalFarm.CurLocalEmployees.Count; i++)
                                {
                                    if (Com.LocalFarm.CurLocalEmployees[i].Id == Com.UserFarm.CurrentUser.EmploeeId)
                                    {
                                        this.cmbBoxRashDebitor.SelectedIndex = i;
                                    }
                                }
                            }
                        }

                        break;
                    // Кассовая книга
                    case 2:

                        // Если был передан конкретный документ который пользователь правит то заполняем полями из документа
                        if (sender != null && e == null)
                        {
                            // Был найден документ в базе по этой причине нам надо не создаввать новый а править найденый документ
                            this.CurDoc = (BLL.DocumentPlg.DocumentKasBook)sender;

                            // !!!!!! Тут не факт что нужно что-то сохранять так как документ подтягивался сюда как раз с использованием этой даты в фильтре скорее всего дата будет всегда одна и таже
                            this.txtBoxKasBookDateDoc.TextChanged -= txtBoxKasBookDateDoc_TextChanged;
                            this.txtBoxKasBookDateDoc.Text = ((DateTime)this.CurDoc.UreDate).ToShortDateString();
                            this.txtBoxKasBookDateDoc.TextChanged += txtBoxKasBookDateDoc_TextChanged;

                            this.txtBoxKasBookDolRukOrg.Text = ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).DolRukFio;
                            this.txtBoxKasBookRukFio.Text = ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).RukFio;
                            this.txtBoxKasBookGlavBuh.Text = ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).GlavBuh;
                            
                            // Дебитор - Бухгалтер
                            int selectIndexKassBookBuh = -1;
                            if (this.CurDoc.LocalDebitor != null)
                            {
                                for (int i = 0; i < LocalFarm.CurLocalAccounters.Count; i++)
                                {
                                    if (LocalFarm.CurLocalAccounters[i].LocalName == (this.CurDoc.LocalDebitor.LocalName))
                                    {
                                        selectIndexKassBookBuh = i;
                                        break;
                                    }
                                }
                            }
                            if (selectIndexKassBookBuh > -1) this.cmbBoxKassBookBuh.SelectedIndex = selectIndexKassBookBuh;
                            else this.cmbBoxKassBookBuh.Text = this.CurDoc.OtherDebitor;
                            // Кредитор - Кассир
                            int selectIndexKassBookKasir = -1;
                            if (this.CurDoc.LocalCreditor != null)
                            {
                                for (int i = 0; i < LocalFarm.CurLocalChiefCashiers.Count; i++)
                                {
                                    if (LocalFarm.CurLocalChiefCashiers[i].LocalName == (this.CurDoc.LocalCreditor.LocalName))
                                    {
                                        selectIndexKassBookKasir = i;
                                        break;
                                    }
                                }
                            }
                            if (selectIndexKassBookKasir > -1) this.cmbBoxKassBookKasir.SelectedIndex = selectIndexKassBookKasir;
                            else this.cmbBoxKassBookKasir.Text = this.CurDoc.OtherKreditor;
                        }
                        else
                        {
                            // Валидация введённой даты
                            DateTime UreDt = DateTime.Now.Date;
                            try { UreDt = DateTime.Parse(this.txtBoxKasBookDateDoc.Text); }
                            catch (Exception) { }

                            // Создаём пустой документ так как за эту дату документ не найден
                            this.CurDoc=Com.DocumentFarm.CreateNewDocument("DocumentKasBook");
                            this.CurDoc = Com.DocumentFarm.CreateNewDocument(null, "DocumentKasBook", UreDt, DateTime.Now, DateTime.Now, Com.UserFarm.CurrentUser.Logon, Com.OperationFarm.CurOperationList["OperationKasBook"], null, null, null, null, /*Com.LocalFarm.CurLocalDepartament.LastDocNumKasBook*/ Com.ProviderFarm.CurrentPrv.MaxDocNumForYaer(this.CurDoc) + 1, true, false);  // тут надо получить документ и список на день который указан если документа нет то создаём его и получаем список документов в этом дне с остатками на начало и конец для того чтобы можно было мостроить суммы на начало дня и конец выбранного дня
                            //this.CurDoc.DocNum = (Kassa.LastDocNumKasBook + 1);  // Номер документа получили при создании документа

                            this.txtBoxKasBookDolRukOrg.Text = Kassa.DolRukOrg;
                            this.txtBoxKasBookRukFio.Text = Kassa.RukFio;
                            this.txtBoxKasBookGlavBuh.Text = Kassa.GlavBuhFio;
                        }
                        
                        // Заполняем список документов
                        DocumentList DocListKassBock = ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).DocList;
                        //
                        this.dtDocKassBook.Rows.Clear();
                        this.TagDocKassBook = new List<Document>();
                        //
                        foreach (Document itemDocKassBook in DocListKassBock)
                        {
                            if (itemDocKassBook.DocFullName == "DocumentPrihod"
                                || itemDocKassBook.DocFullName == "DocumentRashod")
                            {
                                DataRow nrow = this.dtDocKassBook.NewRow();
                                nrow["Id"] = itemDocKassBook.Id;

                                switch (itemDocKassBook.DocFullName)
                                {
                                    case "DocumentPrihod":

                                        if (itemDocKassBook.LocalCreditor != null) nrow["FromTo"] = itemDocKassBook.LocalCreditor.LocalName;
                                        else nrow["FromTo"] = ((BLL.DocumentPlg.DocumentPrihod)itemDocKassBook).OtherKreditor;
                                           
                                        break;
                                    case "DocumentRashod":

                                        if (itemDocKassBook.LocalDebitor != null) nrow["FromTo"] = itemDocKassBook.LocalDebitor.LocalName;
                                        else nrow["FromTo"] = ((BLL.DocumentPlg.DocumentRashod)itemDocKassBook).OtherDebitor;

                                        break;
                                    default:
                                        break;
                                }

                                switch (itemDocKassBook.DocFullName)
                                {
                                    case "DocumentPrihod":
                                        nrow["NoDoc"] = string.Format("no{0}",itemDocKassBook.DocNum);
                                        nrow["KorShet"] = ((BLL.DocumentPlg.DocumentPrihod)itemDocKassBook).KredikKorSchet;
                                        nrow["Prihod"] = ((BLL.DocumentPlg.DocumentPrihod)itemDocKassBook).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                                        break;
                                    case "DocumentRashod":
                                        nrow["NoDoc"] = string.Format("po{0}", itemDocKassBook.DocNum);
                                        nrow["KorShet"] = ((BLL.DocumentPlg.DocumentRashod)itemDocKassBook).DebetKorSchet;
                                        nrow["Rashod"] = ((BLL.DocumentPlg.DocumentRashod)itemDocKassBook).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                                        break;
                                    default:
                                        break;
                                }

                                this.dtDocKassBook.Rows.Add(nrow);
                                this.TagDocKassBook.Add(itemDocKassBook);   // По этому списку будем искать тот документ который выбрал пользователь и именно выбранный передадим на редактирование
                            }
                        }
 
                        // Заполняем на начало и на конец дня исходя из значения в документе эта часть общая 
                        this.txtBoxKassBookStartDay.Text = ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).SummaStartDay.ToString("#.00", CultureInfo.CurrentCulture);
                        this.txtBoxKassBookEndDay.Text = ((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).SummaEndDay.ToString("#.00", CultureInfo.CurrentCulture);

                        // Тут похоже надо сообщить пользаку что документ надо бы сохранить иначе сумма в базе не будет совпадать с той что мы посчитали
                        // Может цвет поменять в ячейке надо подумать
                        if (((BLL.DocumentPlg.DocumentKasBook)this.CurDoc).SaveValueNotValid) { }
                        
                        break;
                    // Инвентаризация средств
                    case 3:

                        // Если был передан конкретный документ который пользователь правит то заполняем полями из документа
                        if (sender != null && e == null)
                        {
                            // Получаем документ
                            this.CurDoc = (BLL.DocumentPlg.DocumentInvent)sender;
                                                        
                            // Проверка на наличие ошибок при создании пустого документа
                            if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", ""));
                            
                            this.txtBoxInventOrganization.Text = Kassa.Organization;
                            this.txtBoxInventStructPodr.Text = Kassa.StructPodrazdelenie;
                            this.txtBoxInventOKPO.Text = Kassa.OKPO;
                            this.txtBoxInventDateDoc.Text = ((DateTime)this.CurDoc.UreDate).ToShortDateString();
                            this.txtBoxInventNumDoc.Text = this.CurDoc.DocNum.ToString();

                            // Заполняем инфу по операции
                            BLL.OperationPlg.OperationInvent OperInvent = (BLL.OperationPlg.OperationInvent)OperationFarm.CurOperationList["OperationInvent"];
                            this.txtBoxInventOKUD.Text = (OperInvent != null && !string.IsNullOrWhiteSpace(OperInvent.OKUD) ? OperInvent.OKUD : "0317013");

                            // Левая сторона
                            this.txtBoxInventFactStr1.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactStr1;
                            this.txtBoxInventFactStr2.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactStr2;
                            this.txtBoxInventFactStr3.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactStr3;
                            this.txtBoxInventFactStr4.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactStr4;
                            this.txtBoxInventFactStr5.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactStr5;
                            this.txtBoxInventFactVal1.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactVal1.ToString();
                            this.txtBoxInventFactVal2.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactVal2.ToString();
                            this.txtBoxInventFactVal3.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactVal3.ToString();
                            this.txtBoxInventFactVal4.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactVal4.ToString();
                            this.txtBoxInventFactVal5.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).FactVal5.ToString();
                            this.txtBoxInventItogPoUchDan.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).ItogPoUchDan.ToString();
                            this.txtBoxInventLastPrihodNum.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).LastPrihodNum.ToString();
                            this.txtBoxInventLastRashodNum.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).LastRashodNum.ToString();

                            // Правая сторона центр
                            this.txtBoxInventPrikazTypAndDocNum.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).PrikazTypAndDocNum;
                            this.txtBoxInventPrikazUreDate.Text = ((DateTime)((BLL.DocumentPlg.DocumentInvent)this.CurDoc).PrikazUreDate).ToShortDateString();
                            this.txtBoxInventPrikazDolMatOtv.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).PrikazDolMatOtv;
                            this.txtBoxInventPrikazDecodeMatOtv.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).PrikazDecodeMatOtv;

                            // Правая торона низ
                            this.txtBoxInventKomissionStr1.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionStr1;
                            this.txtBoxInventKomissionStr2.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionStr2;
                            this.txtBoxInventKomissionStr3.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionStr3;
                            this.txtBoxInventKomissionStr4.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionStr4;
                            this.txtBoxInventTitleKomissionDecode1.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionDecode1;
                            this.txtBoxInventTitleKomissionDecode2.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionDecode2;
                            this.txtBoxInventTitleKomissionDecode3.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionDecode3;
                            this.txtBoxInventTitleKomissionDecode4.Text = ((BLL.DocumentPlg.DocumentInvent)this.CurDoc).KomissionDecode4;
                        }
                        else
                        {
                            // Создаём пустой документ
                            this.CurDoc = Com.DocumentFarm.CreateNewDocument("DocumentInvent");

                            // Проверка на наличие ошибок при создании пустого документа
                            if (this.CurDoc == null) throw new ApplicationException(string.Format("Не удалось создать документ разбирайся с плагином для документа: {0}", ""));

                            this.txtBoxInventOrganization.Text = Kassa.Organization;
                            this.txtBoxInventStructPodr.Text = Kassa.StructPodrazdelenie;
                            this.txtBoxInventOKPO.Text = Kassa.OKPO;
                            this.txtBoxInventDateDoc.Text = DateTime.Now.Date.ToShortDateString();
                            this.txtBoxInventNumDoc.Text = (/*Kassa.LastDocNumInvent*/ Com.ProviderFarm.CurrentPrv.MaxDocNumForYaer(this.CurDoc) + 1).ToString();

                            // Заполняем инфу по операции
                            BLL.OperationPlg.OperationInvent OperInvent = (BLL.OperationPlg.OperationInvent)OperationFarm.CurOperationList["OperationInvent"];
                            this.txtBoxInventOKUD.Text = (OperInvent != null && !string.IsNullOrWhiteSpace(OperInvent.OKUD) ? OperInvent.OKUD : "0317013");

                            // Левая сторона
                            this.txtBoxInventFactStr1.Text = "наличных денег";
                            this.txtBoxInventFactStr2.Text = string.Empty;
                            this.txtBoxInventFactStr3.Text = string.Empty;
                            this.txtBoxInventFactStr4.Text = string.Empty;
                            this.txtBoxInventFactStr5.Text = string.Empty;
                            this.txtBoxInventFactVal1.Text = "0";
                            this.txtBoxInventFactVal2.Text = "0";
                            this.txtBoxInventFactVal3.Text = "0";
                            this.txtBoxInventFactVal4.Text = "0";
                            this.txtBoxInventFactVal5.Text = "0";
                            this.txtBoxInventItogPoUchDan.Text = "0";
                            this.txtBoxInventLastPrihodNum.Text = "0";
                            this.txtBoxInventLastRashodNum.Text = "0";

                            // Правая сторона центр
                            this.txtBoxInventPrikazTypAndDocNum.Text = string.Empty;
                            this.txtBoxInventPrikazUreDate.Text = string.Empty;
                            this.txtBoxInventPrikazDolMatOtv.Text = string.Empty;
                            this.txtBoxInventPrikazDecodeMatOtv.Text = string.Empty;

                            // Правая торона низ
                            this.txtBoxInventKomissionStr1.Text = string.Empty;
                            this.txtBoxInventKomissionStr2.Text = string.Empty;
                            this.txtBoxInventKomissionStr3.Text = string.Empty;
                            this.txtBoxInventKomissionStr4.Text = string.Empty;
                            this.txtBoxInventTitleKomissionDecode1.Text = string.Empty;
                            this.txtBoxInventTitleKomissionDecode2.Text = string.Empty;
                            this.txtBoxInventTitleKomissionDecode3.Text = string.Empty;
                            this.txtBoxInventTitleKomissionDecode4.Text = string.Empty;
                        }

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
                    //this.cmbBoxPrihPaidInReasons.SelectedIndex = -1;
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
                //this.cmbBoxRashPaidRashReasons.SelectedIndexChanged -= cmbBoxRashPaidRashReasons_SelectedIndexChanged;

                // Сбрасываем все значения на пустоту
                if (sender == null || this.cmbBoxRashPaidRashReasons.SelectedIndex == -1)
                {
                    this.txtBoxRashDebitKorSchet.Text = string.Empty;
                    this.txtBoxRashKreditNomerSchet.Text = string.Empty;
                    this.txtBoxRashOsnovanie.Text = string.Empty;
                    //this.cmbBoxRashPaidRashReasons.SelectedIndex = -1;
                }
                else
                {
                    // Получаем текущее основание
                    BLL.LocalPlg.LocalPaidRashReasons LoclPaidRashReasons = LocalFarm.CurLocalPaidRashReasons[this.cmbBoxRashPaidRashReasons.SelectedIndex];
                    this.txtBoxRashDebitKorSchet.Text = LoclPaidRashReasons.DebetKorSchet;
                    this.txtBoxRashKreditNomerSchet.Text = LoclPaidRashReasons.KreditNomerSchet;
                    this.txtBoxRashOsnovanie.Text = LoclPaidRashReasons.Osnovanie;
                }

                //this.cmbBoxRashPaidRashReasons.SelectedIndexChanged += cmbBoxRashPaidRashReasons_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.cmbBoxRashPaidRashReasons_SelectedIndexChanged", GetType().Name), EventEn.Error, true, true);
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
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.txtBoxRashSumma_TextChanged", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // В кассовой книге произошло изменение даты
        private void txtBoxKasBookDateDoc_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Если текущая вкладка кассовая книга
                if (this.tabCntOperation.SelectedIndex==2)
                {
                    // Валидация введённой даты
                    DateTime KasBookDate;
                    try { KasBookDate = DateTime.Parse(this.txtBoxKasBookDateDoc.Text); }
                    catch (Exception) { return; }

                    DocumentList DocL = Com.ProviderFarm.CurrentPrv.GetDocumentListFromDB(KasBookDate, Com.OperationFarm.CurOperationList["OperationKasBook"].Id, false);
                    // Если текущего документа за данную дату нет то создаём его но пока без сохранения
                    if (DocL.Count==0)
                    {
                        this.btnNew_Click(null, null);
                    }
                    else this.btnNew_Click(DocL[0], null);
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.txtBoxKasBookDateDoc_TextChanged", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }
        
        // Пользователь выбрал документ в кассовой книге для редактирования
        private void btnKassBookEdit_Click(object sender, EventArgs e)
        {
            try
            {
                // Если текущая вкладка кассовая книга
                if (this.tabCntOperation.SelectedIndex == 2 && this.TagDocKassBook.Count>0)
                {
                    // Если у пользователя выделана ячейка
                    if (dtGridKassBook.SelectedCells.Count > 0)
                    {
                        int tIndex = dtGridKassBook.SelectedCells[0].RowIndex;

                        //this.tabCntOperation.Selected -= TabCntOperation_Selected;
                        switch (this.TagDocKassBook[tIndex].DocFullName)
                        {
                            case "DocumentPrihod":
                                this.tabCntOperation.SelectedIndex = 0;
                                break;
                            case "DocumentRashod":
                                this.tabCntOperation.SelectedIndex = 1;
                                break;
                            default:
                                break;
                        }
                        //this.tabCntOperation.Selected += TabCntOperation_Selected;
                        this.btnNew_Click(this.TagDocKassBook[tIndex], null);
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.btnKassBookEdit_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Пользователь решил напечатать документ
        private void btnKassBookTitle_Click(object sender, EventArgs e)
        {
            try
            {
                Document SelectDocument = null;

                // Если текущая вкладка кассовая книга
                if (this.tabCntOperation.SelectedIndex == 2 && this.TagDocKassBook.Count > 0)
                {
                    // Если у пользователя выделана ячейка
                    if (dtGridKassBook.SelectedCells.Count > 0)
                    {
                        int tIndex = dtGridKassBook.SelectedCells[0].RowIndex;

                        foreach (Document item in this.TagDocKassBook)
                        {
                            if (item.Id != null && item.Id.ToString()== dtGridKassBook.Rows[tIndex].Cells["Id"].Value.ToString())
                            {
                                SelectDocument = item;
                                break;
                            }
                        }
                    }
                }

                // Если документ найден
                if(SelectDocument!=null)
                {
                    SelectDocument.PrintTitle();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.btnKassBookTitle_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Пользователь стукнул на конкретном документе и решил выгрузить документ в 1C
        private void btnKassBookExport_Click(object sender, EventArgs e)
        {
            try
            {
                Document SelectDocument = null;

                // Если текущая вкладка кассовая книга
                if (this.tabCntOperation.SelectedIndex == 2 && this.TagDocKassBook.Count > 0)
                {
                    // Если у пользователя выделана ячейка
                    if (dtGridKassBook.SelectedCells.Count > 0)
                    {
                        int tIndex = dtGridKassBook.SelectedCells[0].RowIndex;

                        foreach (Document item in this.TagDocKassBook)
                        {
                            if (item.Id != null && item.Id.ToString() == dtGridKassBook.Rows[tIndex].Cells["Id"].Value.ToString())
                            {
                                SelectDocument = item;
                                break;
                            }
                        }
                    }
                }

                // Если документ найден
                if (SelectDocument != null)
                {
                    SelectDocument.ExportTo1C();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.btnKassBookExport_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Печать документа
        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                Document SelectDocument = this.CurDoc;

                // Если текущая вкладка кассовая книга
                if (this.tabCntOperation.SelectedIndex == 2 && this.TagDocKassBook.Count > 0)
                {
                    if (this.CurDoc != null) SelectDocument = this.CurDoc;
                }

                // Если документ найден
                if (SelectDocument != null)
                {
                    SelectDocument.PrintDefault();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.btnPrint_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Печать бланка возврата документа
        private void btnRashReportReturnBlank_Click(object sender, EventArgs e)
        {
            try
            {
                BLL.DocumentPlg.DocumentRashod SelectDocument = null;

                // Если текущая вкладка возврат
                if (this.tabCntOperation.SelectedIndex == 1)
                {
                    if (this.CurDoc != null) SelectDocument = (BLL.DocumentPlg.DocumentRashod)this.CurDoc;
                }

                // Если документ найден
                if (SelectDocument != null)
                {
                    if (SelectDocument.PaidRashReasons == null) throw new ApplicationException("Не выбрано основание возврата.");
                    if (!SelectDocument.PaidRashReasons.FlagFormReturn) throw new ApplicationException("Для этого обоснования не рассчитана печатная форма.");

                    int DocNumber = 0;
                    using (FRequestDocNumber Frm = new FRequestDocNumber(SelectDocument))
                    {
                        Frm.ShowDialog();

                        if(Frm.DialogResult == DialogResult.OK)
                        {
                            DocNumber = Frm.DocNumber;
                        }
                    }

                    if (DocNumber != 0)
                    {
                        SelectDocument.PrintReportReturnBlankWrd(DocNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при создании документа с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.btnPrint_Click", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
        }

        // Подсчёт итога в поле txtBoxInventFactValTotal
        private void txtBoxInventFactVal_TextChanged(object sender, EventArgs e)
        {
            decimal sum = 0;
            try
            {
                try { sum += Decimal.Parse(this.txtBoxInventFactVal1.Text);}
                catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventFactVal1.Text)); }
                try { sum += Decimal.Parse(this.txtBoxInventFactVal2.Text); }
                catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventFactVal2.Text)); }
                try { sum += Decimal.Parse(this.txtBoxInventFactVal3.Text); }
                catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventFactVal3.Text)); }
                try { sum += Decimal.Parse(this.txtBoxInventFactVal4.Text); }
                catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventFactVal4.Text)); }
                try { sum += Decimal.Parse(this.txtBoxInventFactVal5.Text); }
                catch (Exception) { throw new ApplicationException(string.Format("Не смогли преобразовать {0} в число.", this.txtBoxInventFactVal5.Text)); }
                
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при подсчёте суммы в поле txtBoxInventFactValTotal с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.txtBoxInventFactVal_TextChanged", GetType().Name), EventEn.Error, true, true);
                //throw ae;
            }
            finally
            {
                this.txtBoxInventFactValTotal.Text = sum.ToString();
            }
        }
        #endregion
    }
}
