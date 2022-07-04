using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using AlgoritmCashFunc.BLL.DocumentPlg.Lib;
using AlgoritmCashFunc.Lib;
using System.Data;
using WordDotx;

namespace AlgoritmCashFunc.BLL.DocumentPlg
{
    /// <summary>
    /// Класс который представляет из себя докимент расхода
    /// </summary>
    public sealed class DocumentKasBook : Document 
    {
        /// <summary>
        /// Сумма докмента на начало дня
        /// </summary>
        public decimal SummaStartDay;

        /// <summary>
        /// Сумма документа на конец дня
        /// </summary>
        public decimal SummaEndDay;
        
        /// <summary>
        /// Должность руководителя
        /// </summary>
        public string DolRukFio;

        /// <summary>
        /// Руководитель
        /// </summary>
        public string RukFio;

        /// <summary>
        /// Главный бухгалтер
        /// </summary>
        public string GlavBuh;

        /// <summary>
        /// Значение которое говорит что в базе либо значение на даты начала и конца не сохранениы либо не валидны если True значит проверка совпала с тем что сохранено в базе
        /// </summary>
        public bool SaveValueNotValid = false;

        /// <summary>
        /// Список документов за этот день
        /// </summary>
        public DocumentList DocList;

        /// <summary>
        /// Конструктор для загрузки из базы данных
        /// </summary>
        /// <param name="Id">Идентификатор в базе данных</param>
        /// <param name="UreDate">Дата создания документа</param>
        /// <param name="CreateDate">Дата создания документа</param>
        /// <param name="ModifyDate">Дата изменеия документа</param>
        /// <param name="ModifyUser">Пользовтаель который изменил последний раз документ</param>
        /// <param name="CurOperation">Операция к которой относится этот документ</param>
        /// <param name="LocalDebitor">Дебитор</param>
        /// <param name="LocalCreditor">Кредитор</param>
        /// <param name="Departament">Департамент или касса в которой создан документ</param>
        /// <param name="OtherDebitor">Дебитор который ввели вручную не из списка</param>
        /// <param name="OtherKreditor">Кредитор который ввели вручную не из списка</param>
        /// <param name="DocNum"> Черновик</param>
        /// <param name="IsDraft">Черновик</param>
        /// <param name="IsProcessed">Проведённый документ или нет</param>
        public DocumentKasBook(int? Id, DateTime? UreDate, DateTime CreateDate, DateTime ModifyDate, string ModifyUser, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, Local Departament, string OtherDebitor, string OtherKreditor, int DocNum, bool IsDraft, bool IsProcessed) : base("DocumentKasBook", CurOperation, LocalDebitor, LocalCreditor, Departament, OtherDebitor, OtherKreditor, DocNum, IsProcessed)
        {
            try
            {
                base.Id = Id;
                base.UreDate = UreDate;
                base.CreateDate = CreateDate;
                base.ModifyDate = ModifyDate;
                base.ModifyUser = ModifyUser;
                base.IsDraft = IsDraft;
                base.IsProcessed = IsProcessed;

                // Получаем список документов
                this.DocList = Com.ProviderFarm.CurrentPrv.GetDocumentListFromDB((UreDate == null ? DateTime.Now : (DateTime)UreDate), CurOperation.Id, true);

                // На выбранную дату нужно получить остаток на начало даты и оборот на конец даты
                RezultForOstatokAndOborot OborotForDay = Com.ProviderFarm.CurrentPrv.GetOstatokAndOborotForDay((UreDate == null ? DateTime.Now : (DateTime)UreDate));

                // Если документ читается из базы данных то нужно прочитать дополнительные параметры
                if (base.Id != null)
                {
                    DocumentKasBook MyObj = this;
                    bool tt = Com.ProviderFarm.CurrentPrv.GetDocumentKasBook(ref MyObj);
                }

                // Сравниваем если данные в документе не равены данным в базе при пересчёте то нужно выставить влаг валидности в False и поправить сумму в документе чтобы пользователь сохранил изменения в базу и не забыл
                if (this.SummaStartDay != OborotForDay.Ostatok || this.SummaEndDay != (OborotForDay.Ostatok + OborotForDay.Oborot))
                {
                    this.SaveValueNotValid = false;
                    this.SummaStartDay = OborotForDay.Ostatok;
                    this.SummaEndDay = (OborotForDay.Ostatok + OborotForDay.Oborot);
                }
                else this.SaveValueNotValid = true;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации конструктора с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }
        //
        /// <summary>
        /// Конструктор
        /// </summary>
        public DocumentKasBook() : this(null, DateTime.Now.Date, DateTime.Now, DateTime.Now, Com.UserFarm.CurrentUser.Logon, Com.OperationFarm.CurOperationList["OperationKasBook"], null, null, Com.LocalFarm.CurLocalDepartament, null, null, Com.LocalFarm.CurLocalDepartament.LastDocNumKasBook + 1, true, false)
        {
            try
            {
                this.CurOperation = Com.OperationFarm.CurOperationList["OperationKasBook"];

                // Документ ещё не сохранялся в базе он новый по этому ставим флаг что не совсем валидный может потом цветом подкрасим
                this.SaveValueNotValid = false;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации конструктора с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Метод заставляет переписать родительский не новый поверз создаёт а переписывает. Для того чтобы плагин мог реализовать своё специфическое сохранение
        /// </summary>
        protected override void SaveChildron()
        {
            try
            {
                if (Com.ProviderFarm.CurrentPrv.HashDocumentKasBook(this))
                {
                    Com.ProviderFarm.CurrentPrv.UpdateDocumentKasBook(this);
                }
                else  // Если нет то вставляем
                {
                    Com.ProviderFarm.CurrentPrv.SetDocumentKasBook(this);
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.SaveChildron", GetType().Name), EventEn.Error);
                throw ae;
            }
        }
        
        /// <summary>
        /// Печать документа PrintDefault
        /// </summary>
        public override void PrintDefault()
        {
            try
            {
                // Получаем текущее подразделение
                BLL.LocalPlg.LocalKassa Kassa = Com.LocalFarm.CurLocalDepartament;

                // Создаём таблицу с которой потом будем работать
                TableList TabL = new TableList();
                //
                DataTable TabTmp = new DataTable();
                TabTmp.Columns.Add(new DataColumn("A1", typeof(string)));
                TabTmp.Columns.Add(new DataColumn("A2", typeof(string)));
                TabTmp.Columns.Add(new DataColumn("A3", typeof(string)));
                TabTmp.Columns.Add(new DataColumn("A4", typeof(string)));
                TabTmp.Columns.Add(new DataColumn("A5", typeof(string)));
                int CountPrich = 0;
                int CountRash = 0;
                foreach (Document item in this.DocList)
                {
                    DataRow nrow = TabTmp.NewRow();
                    switch (item.DocFullName)
                    {
                        case "DocumentPrihod":
                            nrow["A1"] = string.Format("no{0}", item.DocNum);
                            if (item.LocalDebitor != null) nrow["A2"] = item.LocalDebitor.LocalName;
                            else nrow["A2"] = this.OtherDebitor;
                            nrow["A3"] = ((DocumentPrihod)item).KredikKorSchet;
                            nrow["A4"] = ((DocumentPrihod)item).Summa.ToString("#.00", CultureInfo.CurrentCulture); 
                            CountPrich++;
                            break;
                        case "DocumentRashod":
                            nrow["A1"] = string.Format("po{0}", item.DocNum);
                            if (item.LocalDebitor!=null) nrow["A2"] = item.LocalDebitor.LocalName;
                            else nrow["A2"] = this.OtherDebitor;
                            nrow["A3"] = ((DocumentRashod)item).DebetKorSchet;
                            nrow["A5"] = ((DocumentRashod)item).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                            CountRash++;
                            break;
                        default:
                            break;
                    }

                    TabTmp.Rows.Add(nrow);
                }
                //
                Table Tab = new Table("Tab", TabTmp);
                Tab.TtlList.Add(new Total("Total0", "f"), true); // {@DTab.T0}
                //
                TabL.Add(Tab, true);


                // Создаём список Закладок
                BookmarkList BmL = new BookmarkList();
                //
                Bookmark BmDay = new Bookmark("BmD", ((DateTime)this.UreDate).Day.ToString());
                BmL.Add(BmDay, true);
                //
                Bookmark BmMount = null;
                switch (((DateTime)this.UreDate).Month)
                {
                    case 1:
                        BmMount = new Bookmark("BmM", "Январь");
                        break;
                    case 2:
                        BmMount = new Bookmark("BmM", "Февраль");
                        break;
                    case 3:
                        BmMount = new Bookmark("BmM", "Март");
                        break;
                    case 4:
                        BmMount = new Bookmark("BmM", "Апрель");
                        break;
                    case 5:
                        BmMount = new Bookmark("BmM", "Май");
                        break;
                    case 6:
                        BmMount = new Bookmark("BmM", "Июнь");
                        break;
                    case 7:
                        BmMount = new Bookmark("BmM", "Июль");
                        break;
                    case 8:
                        BmMount = new Bookmark("BmM", "Август");
                        break;
                    case 9:
                        BmMount = new Bookmark("BmM", "Сентябрь");
                        break;
                    case 10:
                        BmMount = new Bookmark("BmM", "Октябрь");
                        break;
                    case 11:
                        BmMount = new Bookmark("BmM", "Ноябрь");
                        break;
                    case 12:
                        BmMount = new Bookmark("BmM", "Декабрь");
                        break;
                    default:
                        break;
                }
                BmL.Add(BmMount, true);
                //
                Bookmark BmY = new Bookmark("BmY", ((DateTime)this.UreDate).Year.ToString());
                BmL.Add(BmY, true);
                //
                Bookmark BmStart = new Bookmark("BmStart", this.SummaStartDay.ToString("#.00", CultureInfo.CurrentCulture));
                BmL.Add(BmStart, true);
                //
                Bookmark BmEnd = new Bookmark("BmEnd", this.SummaEndDay.ToString("#.00", CultureInfo.CurrentCulture));
                BmL.Add(BmEnd, true);
                //
                Bookmark BmCPrih = new Bookmark("BmCPrih", Com.Utils.GetStringForInt(CountPrich, "", "", "", false).ToLower());
                BmL.Add(BmCPrih, true);
                //
                Bookmark BmCRash = new Bookmark("BmCRash", Com.Utils.GetStringForInt(CountRash, "", "", "", false).ToLower());
                BmL.Add(BmCRash, true);
                //
                Bookmark BmKassir= new Bookmark("BmKassir", (this.LocalCreditor!=null?this.LocalCreditor.LocalName:this.OtherKreditor));
                BmL.Add(BmKassir, true);
                //
                Bookmark BmBuhg = new Bookmark("BmBuhg", this.GlavBuh);
                BmL.Add(BmBuhg, true);
                

                //////////////////////////////////////

                // Создаём задание
                TaskWord Tsk = new TaskWord(@"KasBookDefault.dotx", null, BmL, TabL);

                // Можно создать отдельный екземпляр который сможет работать асинхронно со своими параметрами
                WordDotxServer SrvStatic = new WordDotxServer(string.Format(@"{0}\Dotx", Environment.CurrentDirectory), string.Format(@"{0}\Report", Environment.CurrentDirectory));

                // Запускаем формирование отчёта в синхронном режиме
                SrvStatic.StartCreateReport(Tsk);

                // открываем приложение Excel
                //SrvStatic.OlpenReport(Tsk);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.PrintDefault", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Экспорт документа в 1С
        /// </summary>
        public override void ExportTo1C()
        {
            try
            {
                if (this.UreDate == null) throw new ApplicationException("У документа не задана юридическая дата.");

                string FileName = string.Format("{0}_{1}_{2}{3}{4}.txt", Com.LocalFarm.CurLocalDepartament.StoreCode
                    , Com.LocalFarm.CurLocalDepartament.CompanyCode
                    , ((DateTime)this.UreDate).Year
                    , ((DateTime)this.UreDate).Month.ToString("00")
                    , ((DateTime)this.UreDate).Day.ToString("00"));

                string PolKas = null;
                if (this.LocalDebitor != null) PolKas = ((BLL.LocalPlg.LocalChiefCashiers)this.LocalDebitor).LocalName;
                else PolKas = this.OtherDebitor;

                string Sotrudnik = null;
                if (this.LocalCreditor != null) Sotrudnik = ((BLL.LocalPlg.LocalEmployees)this.LocalCreditor).LocalName;
                Sotrudnik = this.OtherKreditor;

                // Пробегаем по списку документов
                foreach (Document item in this.DocList)
                {
                    string FromTo = null;
                    string NoDoc = null;
                    string KorShet = null;
                    string Sum = null;
                    switch (item.DocFullName)
                    {
                        case "DocumentPrihod":

                            if (item.LocalCreditor != null) FromTo = item.LocalCreditor.LocalName;
                            else FromTo = ((DocumentPrihod)item).OtherKreditor;

                            NoDoc = string.Format("no{0}", item.DocNum);
                            KorShet = ((DocumentPrihod)item).KredikKorSchet;
                            Sum = ((DocumentPrihod)item).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                            break;                            
                        case "DocumentRashod":

                            if (item.LocalDebitor != null) FromTo = item.LocalDebitor.LocalName;
                            else FromTo = ((DocumentRashod)item).OtherDebitor;

                            NoDoc = string.Format("po{0}", item.DocNum);
                            KorShet = ((DocumentRashod)item).DebetKorSchet;
                            Sum = ((DocumentRashod)item).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                            break;
                        default:
                            break;
                    }

                    string Row = string.Format("{0}\t{1}" +
                    "\t{2}\t{3}" +
                    "\t{4}\t{5}" +
                    "\t{6}\t{7}" +
                    "\t{8}\t{9}" +
                    "\t{10}\t{11}" +
                    "\t{12}\t{13}" +
                    "\t{14}"
                    , 1, this.DocNum
                    , 20220127181619, 7708790060
                    , Com.LocalFarm.CurLocalDepartament.OKPO, Com.LocalFarm.CurLocalDepartament.Organization
                    , Com.LocalFarm.CurLocalDepartament.CompanyCode, Com.LocalFarm.CurLocalDepartament.StructPodrazdelenie
                    , Sum, 0
                    , this.DolRukFio, this.RukFio
                    , this.GlavBuh, PolKas
                    , Sotrudnik);
                    base.ExportTo1C(FileName, Row);
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.ExportTo1C", GetType().Name), EventEn.Error);
                throw ae;
            }
        }
    }
}
