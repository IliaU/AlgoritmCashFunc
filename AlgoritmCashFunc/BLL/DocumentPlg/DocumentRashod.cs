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
using Wrd = WordDotx;

namespace AlgoritmCashFunc.BLL.DocumentPlg
{
    /// <summary>
    /// Класс который представляет из себя докимент расхода
    /// </summary>
    public sealed class DocumentRashod : Document
    {
        /// <summary>
        /// Код подразделения
        /// </summary>
        public string DebetKodDivision;

        /// <summary>
        /// Кор счёт
        /// </summary>
        public string DebetKorSchet;

        /// <summary>
        /// Код аналитического учёта
        /// </summary>
        public string DebetKodAnalUch;

        /// <summary>
        /// Номер счёта
        /// </summary>
        public string KreditNomerSchet;

        /// <summary>
        /// Сумма докмента
        /// </summary>
        public decimal Summa;

        /// <summary>
        /// Сумма в виде строки для отчётов
        /// </summary>
        public string SummaStr;

        /// <summary>
        /// Код назначения
        /// </summary>
        public string KodNazn;

        /// <summary>
        /// По документу
        /// </summary>
        public string PoDoc;

        /// <summary>
        /// Основание
        /// </summary>
        public string Osnovanie;

        /// <summary>
        /// Основание прихода
        /// </summary>
        public BLL.LocalPlg.LocalPaidRashReasons PaidRashReasons;
        
        /// <summary>
        /// Приложение
        /// </summary>
        public string Prilozenie;

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
        public DocumentRashod(int? Id, DateTime? UreDate, DateTime CreateDate, DateTime ModifyDate, string ModifyUser, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, Local Departament, string OtherDebitor, string OtherKreditor, int DocNum, bool IsDraft, bool IsProcessed) : base("DocumentRashod", CurOperation, LocalDebitor, LocalCreditor, Departament, OtherDebitor, OtherKreditor, DocNum, IsProcessed)
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

                // Если документ читается из базы данных то нужно прочитать дополнительные параметры
                if (base.Id != null)
                {
                    DocumentRashod MyObj = this;
                    bool tt = Com.ProviderFarm.CurrentPrv.GetDocumentRashod(ref MyObj);
                }
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
        public DocumentRashod() : this(null, DateTime.Now.Date, DateTime.Now, DateTime.Now, Com.UserFarm.CurrentUser.Logon, Com.OperationFarm.CurOperationList["OperationRashod"], null, null, Com.LocalFarm.CurLocalDepartament, null, null, Com.LocalFarm.CurLocalDepartament.LastDocNumRash+1, true, false)
        {
            try
            {
                this.CurOperation = Com.OperationFarm.CurOperationList["OperationRashod"];
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации конструктора с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Установка даты создания для документа чтобы пользователь мог его поправить
        /// </summary>
        /// <param name="dt">Дата время которую выставить в документе</param>
        public void SetCreatedDate(DateTime dt)
        {
            try
            {
                base.CreateDate = dt;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.SetCreatedDate", GetType().Name), EventEn.Error);
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
                if (Com.ProviderFarm.CurrentPrv.HashDocumentRashod(this))
                {
                    Com.ProviderFarm.CurrentPrv.UpdateDocumentRashod(this);
                }
                else  // Если нет то вставляем
                {
                    Com.ProviderFarm.CurrentPrv.SetDocumentRashod(this);
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
        /// Печать документа Title
        /// </summary>
        public override void PrintTitle()
        {
            try
            {
                // Получаем текущее подразделение
                BLL.LocalPlg.LocalKassa Kassa = Com.LocalFarm.CurLocalDepartament;

                // Создаём таблицу с которой потом будем работать
                TableList TabL = new TableList();

                string SourceFile = string.Format(@"{0}\Dotx\Title.xlsx", Environment.CurrentDirectory);
                string TargetFile = string.Format(@"{0}\Report\{1}Title.xlsx", Environment.CurrentDirectory, ((int)this.Id).ToString());

                // Создаём временную таблицу
                DataTable TabTmpPrihod = new DataTable();
                //
                TabTmpPrihod.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowPrihodOKUD = TabTmpPrihod.NewRow();
                nrowPrihodOKUD["A"] = ((BLL.OperationPlg.OperationRashod)base.CurOperation).OKUD;
                TabTmpPrihod.Rows.Add(nrowPrihodOKUD);
                DataRow nrowPrihodOKPO = TabTmpPrihod.NewRow();
                nrowPrihodOKPO["A"] = Kassa.OKPO;
                TabTmpPrihod.Rows.Add(nrowPrihodOKPO);

                // Добавлем эту таблицу в наш класс
                Table Tab0 = new Table("1|BQ6", TabTmpPrihod);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(Tab0, true);

                // Добавлем эту таблицу в наш класс
                Table Tab1 = new Table("1|BQ24", TabTmpPrihod);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(Tab1, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpPrihodGlavBuh = new DataTable();
                //
                TabTmpPrihodGlavBuh.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowPrihodGlavBuh = TabTmpPrihodGlavBuh.NewRow();
                nrowPrihodGlavBuh["A"] = this.GlavBuh;
                TabTmpPrihodGlavBuh.Rows.Add(nrowPrihodGlavBuh);

                // Добавлем эту таблицу в наш класс
                Table TabPrihodGlavBuh = new Table("1|AR46", TabTmpPrihodGlavBuh);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabPrihodGlavBuh, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpOrg = new DataTable();
                //
                TabTmpOrg.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowOrg = TabTmpOrg.NewRow();
                nrowOrg["A"] = Kassa.Organization;
                TabTmpOrg.Rows.Add(nrowOrg);

                // Добавлем эту таблицу в наш класс
                Table TabOrg0 = new Table("1|A7", TabTmpOrg);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabOrg0, true);

                // Добавлем эту таблицу в наш класс
                Table TabOrg1 = new Table("1|A25", TabTmpOrg);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabOrg1, true);
                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpStructPodrazdelenie = new DataTable();
                //
                TabTmpStructPodrazdelenie.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowStructPodrazdelenie = TabTmpStructPodrazdelenie.NewRow();
                nrowStructPodrazdelenie["A"] = Kassa.StructPodrazdelenie;
                TabTmpStructPodrazdelenie.Rows.Add(nrowStructPodrazdelenie);

                // Добавлем эту таблицу в наш класс
                Table TabStructPodrazdelenie0 = new Table("1|A9", TabTmpStructPodrazdelenie);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabStructPodrazdelenie0, true);

                // Добавлем эту таблицу в наш класс
                Table TabStructPodrazdelenie1 = new Table("1|A27", TabTmpStructPodrazdelenie);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabStructPodrazdelenie1, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpDolRukOrg = new DataTable();
                //
                TabTmpDolRukOrg.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDolRukOrg = TabTmpDolRukOrg.NewRow();
                nrowDolRukOrg["A"] = Kassa.DolRukOrg;
                TabTmpDolRukOrg.Rows.Add(nrowDolRukOrg);

                // Добавлем эту таблицу в наш класс
                Table TabDolRukOrg = new Table("1|AI43", TabTmpDolRukOrg);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabDolRukOrg, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpRukFio = new DataTable();
                //
                TabTmpRukFio.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowRukFio = TabTmpRukFio.NewRow();
                nrowRukFio["A"] = Kassa.RukFio;
                TabTmpRukFio.Rows.Add(nrowRukFio);

                // Добавлем эту таблицу в наш класс
                Table TabRukFio = new Table("1|BJ43", TabTmpRukFio);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabRukFio, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpMon = new DataTable();
                //
                TabTmpMon.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowMon = TabTmpMon.NewRow();
                switch (((DateTime)this.UreDate).Month)
                {
                    case 1:
                        nrowMon["A"] = "Январь";
                        break;
                    case 2:
                        nrowMon["A"] = "Февраль";
                        break;
                    case 3:
                        nrowMon["A"] = "Март";
                        break;
                    case 4:
                        nrowMon["A"] = "Апрель";
                        break;
                    case 5:
                        nrowMon["A"] = "Май";
                        break;
                    case 6:
                        nrowMon["A"] = "Июнь";
                        break;
                    case 7:
                        nrowMon["A"] = "Июль";
                        break;
                    case 8:
                        nrowMon["A"] = "Август";
                        break;
                    case 9:
                        nrowMon["A"] = "Сентябрь";
                        break;
                    case 10:
                        nrowMon["A"] = "Октябрь";
                        break;
                    case 11:
                        nrowMon["A"] = "Ноябрь";
                        break;
                    case 12:
                        nrowMon["A"] = "Декабрь";
                        break;
                    default:
                        break;
                }

                TabTmpMon.Rows.Add(nrowMon);

                // Добавлем эту таблицу в наш класс
                Table TabMon0 = new Table("1|AD16", TabTmpMon);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabMon0, true);

                // Добавлем эту таблицу в наш класс
                Table TabMon1 = new Table("1|AD34", TabTmpMon);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabMon1, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpYear = new DataTable();
                //
                TabTmpYear.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowYear = TabTmpYear.NewRow();
                nrowYear["A"] = ((DateTime)this.UreDate).Year;
                TabTmpYear.Rows.Add(nrowYear);

                // Добавлем эту таблицу в наш класс
                Table TabYear0 = new Table("1|AT16", TabTmpYear);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabYear0, true);

                // Добавлем эту таблицу в наш класс
                Table TabYear1 = new Table("1|AT34", TabTmpYear);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabYear1, true);

                //////////////////////////////////////

                // Создаём задание
                TaskExcel Tsk = new TaskExcel(SourceFile, TargetFile, TabL, true);

                // Можно создать отдельный екземпляр который сможет работать асинхронно со своими параметрами
                ExcelServer SrvStatic = new ExcelServer(string.Format(@"{0}\Dotx", Environment.CurrentDirectory), string.Format(@"{0}\Report", Environment.CurrentDirectory));

                // Запускаем формирование отчёта в синхронном режиме
                SrvStatic.StartCreateReport(Tsk, 1);

                // открываем приложение Excel
                //SrvStatic.OlpenReport(Tsk);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.PrintTitle", GetType().Name), EventEn.Error);
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

                string SourceFile = string.Format(@"{0}\Dotx\RashDefault.xlsx", Environment.CurrentDirectory);
                //string TargetFile = string.Format(@"{0}\Report\{1}PrihDefault.xlsx", Environment.CurrentDirectory, ((int)this.Id).ToString());

                // Создаём временную таблицу
                DataTable TabTmpRash = new DataTable();
                //
                TabTmpRash.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowRashOKUD = TabTmpRash.NewRow();
                nrowRashOKUD["A"] = ((BLL.OperationPlg.OperationRashod)base.CurOperation).OKUD;
                TabTmpRash.Rows.Add(nrowRashOKUD);
                DataRow nrowRashOKPO = TabTmpRash.NewRow();
                nrowRashOKPO["A"] = Kassa.OKPO;
                TabTmpRash.Rows.Add(nrowRashOKPO);

                // Добавлем эту таблицу в наш класс
                Table Tab0 = new Table("1|CT5", TabTmpRash);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(Tab0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpDolRukFio = new DataTable();
                //
                TabTmpDolRukFio.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDolRukFio = TabTmpDolRukFio.NewRow();
                nrowDolRukFio["A"] = this.DolRukFio;
                TabTmpDolRukFio.Rows.Add(nrowDolRukFio);

                // Добавлем эту таблицу в наш класс
                Table TabDolRukFio0 = new Table("1|Z25", TabTmpDolRukFio);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabDolRukFio0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpRukFio = new DataTable();
                //
                TabTmpRukFio.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowRukFio = TabTmpRukFio.NewRow();
                nrowRukFio["A"] = this.RukFio;
                TabTmpRukFio.Rows.Add(nrowRukFio);

                // Добавлем эту таблицу в наш класс
                Table TabRukFio0 = new Table("1|BR25", TabTmpRukFio);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabRukFio0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpRashGlavBuh = new DataTable();
                //
                TabTmpRashGlavBuh.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowRashGlavBuh = TabTmpRashGlavBuh.NewRow();
                nrowRashGlavBuh["A"] = this.GlavBuh;
                TabTmpRashGlavBuh.Rows.Add(nrowRashGlavBuh);

                // Добавлем эту таблицу в наш класс
                Table TabRashGlavBuh0 = new Table("1|AK27", TabTmpRashGlavBuh);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabRashGlavBuh0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpPrilozenie = new DataTable();
                //
                TabTmpPrilozenie.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowPrilozenie = TabTmpPrilozenie.NewRow();
                nrowPrilozenie["A"] = this.Prilozenie;
                TabTmpPrilozenie.Rows.Add(nrowPrilozenie);

                // Добавлем эту таблицу в наш класс
                Table TabPrilozenie0 = new Table("1|L23", TabTmpPrilozenie);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabPrilozenie0, true);
                
                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpDocNum = new DataTable();
                //
                TabTmpDocNum.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDocNum = TabTmpDocNum.NewRow();
                nrowDocNum["A"] = this.DocNum.ToString();
                TabTmpDocNum.Rows.Add(nrowDocNum);

                // Добавлем эту таблицу в наш класс
                Table TabDocNum0 = new Table("1|CC11", TabTmpDocNum);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabDocNum0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpOrg = new DataTable();
                //
                TabTmpOrg.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowOrg = TabTmpOrg.NewRow();
                nrowOrg["A"] = Kassa.Organization;
                TabTmpOrg.Rows.Add(nrowOrg);

                // Добавлем эту таблицу в наш класс
                Table TabOrg0 = new Table("1|A6", TabTmpOrg);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabOrg0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpStructPodrazdelenie = new DataTable();
                //
                TabTmpStructPodrazdelenie.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowStructPodrazdelenie = TabTmpStructPodrazdelenie.NewRow();
                nrowStructPodrazdelenie["A"] = Kassa.StructPodrazdelenie;
                TabTmpStructPodrazdelenie.Rows.Add(nrowStructPodrazdelenie);

                // Добавлем эту таблицу в наш класс
                Table TabStructPodrazdelenie0 = new Table("1|A8", TabTmpStructPodrazdelenie);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabStructPodrazdelenie0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKassir = new DataTable();
                //
                TabTmpKassir.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKassir = TabTmpKassir.NewRow();
                if (this.LocalDebitor != null) nrowKassir["A"] = ((BLL.LocalPlg.LocalEmployees)this.LocalDebitor).LocalName;
                else nrowKassir["A"] = this.OtherDebitor;
                TabTmpKassir.Rows.Add(nrowKassir);

                // Добавлем эту таблицу в наш класс
                Table TabKassir0 = new Table("1|H17", TabTmpKassir);
                TabL.Add(TabKassir0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpFrom = new DataTable();
                //
                TabTmpFrom.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowFrom = TabTmpFrom.NewRow();
                if (this.LocalCreditor!=null ) nrowFrom["A"] = ((BLL.LocalPlg.LocalChiefCashiers)this.LocalCreditor).LocalName;
                else nrowFrom["A"] = this.OtherKreditor;
                TabTmpFrom.Rows.Add(nrowFrom);

                // Добавлем эту таблицу в наш класс
                Table TabFrom0 = new Table("1|AG37", TabTmpFrom);
                TabL.Add(TabFrom0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpOsnovanie = new DataTable();
                //
                TabTmpOsnovanie.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowOsnovanie = TabTmpOsnovanie.NewRow();
                nrowOsnovanie["A"] = this.PaidRashReasons.LocalName;
                TabTmpOsnovanie.Rows.Add(nrowOsnovanie);

                // Добавлем эту таблицу в наш класс
                Table TabOsnovanie0 = new Table("1|K19", TabTmpOsnovanie);
                TabL.Add(TabOsnovanie0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKodNazn = new DataTable();
                //
                TabTmpKodNazn.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKodNazn = TabTmpKodNazn.NewRow();
                nrowKodNazn["A"] = this.KodNazn;
                TabTmpKodNazn.Rows.Add(nrowKodNazn);

                // Добавлем эту таблицу в наш класс
                Table TabKodNazn0 = new Table("1|CQ15", TabTmpKodNazn);
                TabL.Add(TabKodNazn0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKredikKodAnalUch = new DataTable();
                //
                TabTmpKredikKodAnalUch.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKredikKodAnalUch = TabTmpKredikKodAnalUch.NewRow();
                nrowKredikKodAnalUch["A"] = this.DebetKodAnalUch;
                TabTmpKredikKodAnalUch.Rows.Add(nrowKredikKodAnalUch);

                // Добавлем эту таблицу в наш класс
                Table TabKredikKodAnalUch0 = new Table("1|AV15", TabTmpKredikKodAnalUch);
                TabL.Add(TabKredikKodAnalUch0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKredikKorSchet = new DataTable();
                //
                TabTmpKredikKorSchet.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKredikKorSchet = TabTmpKredikKorSchet.NewRow();
                nrowKredikKorSchet["A"] = this.DebetKorSchet;
                TabTmpKredikKorSchet.Rows.Add(nrowKredikKorSchet);

                // Добавлем эту таблицу в наш класс
                Table TabKredikKorSchet0 = new Table("1|AA15", TabTmpKredikKorSchet);
                TabL.Add(TabKredikKorSchet0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKodDivision = new DataTable();
                //
                TabTmpKodDivision.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKodDivision = TabTmpKodDivision.NewRow();
                nrowKodDivision["A"] = this.DebetKodDivision;
                TabTmpKodDivision.Rows.Add(nrowKodDivision);

                // Добавлем эту таблицу в наш класс
                Table TabKodDivision0 = new Table("1|H15", TabTmpKodDivision);
                TabL.Add(TabKodDivision0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpNomerSchet = new DataTable();
                //
                TabTmpNomerSchet.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowNomerSchet = TabTmpNomerSchet.NewRow();
                nrowNomerSchet["A"] = this.KreditNomerSchet;
                TabTmpNomerSchet.Rows.Add(nrowNomerSchet);

                // Добавлем эту таблицу в наш класс
                Table TabNomerSchet0 = new Table("1|BT15", TabTmpNomerSchet);
                TabL.Add(TabNomerSchet0, true);

                //////////////////////////////////////

                // Получакем значение и преобразовываем к строке
                string tmp = decimal.Round(this.Summa, 2).ToString().Replace(".", ",");

                // Создаём переменные для рублей и копеек
                int? sumR = null;
                int? sumK = null;

                // Проверка на наличие копеек
                if (tmp.IndexOf(',') > -1)
                {
                    sumR = int.Parse(tmp.Substring(0, tmp.IndexOf(',')));
                    sumK = int.Parse(tmp.Substring(tmp.IndexOf(',') + 1));
                }
                else sumR = (int)this.Summa;

                // Создаём временную таблицу
                DataTable TabTmpSummaRub = new DataTable();
                //
                TabTmpSummaRub.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowSummaRub = TabTmpSummaRub.NewRow();
                nrowSummaRub["A"] = sumR;
                TabTmpSummaRub.Rows.Add(nrowSummaRub);

                // Добавлем эту таблицу в наш класс
                Table TabSummaRub0 = new Table("1|CC19", TabTmpSummaRub);
                //TabL.Add(TabSummaRub0, true);

                // Создаём временную таблицу
                DataTable TabTmpSummaKop = new DataTable();
                //
                TabTmpSummaKop.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowSummaKop = TabTmpSummaKop.NewRow();
                nrowSummaKop["A"] = sumK;
                TabTmpSummaKop.Rows.Add(nrowSummaKop);

                // Добавлем эту таблицу в наш класс
                Table TabSummaKop0 = new Table("1|CY19", TabTmpSummaKop);
                //TabL.Add(TabSummaKop0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpSumma = new DataTable();
                //
                TabTmpSumma.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowSumma = TabTmpSumma.NewRow();
                nrowSumma["A"] = this.Summa.ToString("#.00", CultureInfo.CurrentCulture);
                TabTmpSumma.Rows.Add(nrowSumma);

                // Добавлем эту таблицу в наш класс
                Table TabSumma0 = new Table("1|CC15", TabTmpSumma);
                TabL.Add(TabSumma0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpSummaStr = new DataTable();
                //
                TabTmpSummaStr.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowSummaStr = TabTmpSummaStr.NewRow();
                nrowSummaStr["A"] = this.SummaStr;
                TabTmpSummaStr.Rows.Add(nrowSummaStr);

                // Добавлем эту таблицу в наш класс
                Table TabSummaStr0 = new Table("1|K20", TabTmpSummaStr);
                TabL.Add(TabSummaStr0, true);

                // Добавлем эту таблицу в наш класс
                Table TabSummaStr1 = new Table("1|K29", TabTmpSummaStr);
                TabL.Add(TabSummaStr1, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpDMY = new DataTable();
                //
                TabTmpDMY.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDMY = TabTmpDMY.NewRow();
                nrowDMY["A"] = ((DateTime)this.UreDate).ToShortDateString();
                TabTmpDMY.Rows.Add(nrowDMY);

                // Добавлем эту таблицу в наш класс
                Table TabDMY0 = new Table("1|CT11", TabTmpDMY);
                TabL.Add(TabDMY0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpMon = new DataTable();
                //
                TabTmpMon.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowMon = TabTmpMon.NewRow();
                switch (((DateTime)this.UreDate).Month)
                {
                    case 1:
                        nrowMon["A"] = "Январь";
                        break;
                    case 2:
                        nrowMon["A"] = "Февраль";
                        break;
                    case 3:
                        nrowMon["A"] = "Март";
                        break;
                    case 4:
                        nrowMon["A"] = "Апрель";
                        break;
                    case 5:
                        nrowMon["A"] = "Май";
                        break;
                    case 6:
                        nrowMon["A"] = "Июнь";
                        break;
                    case 7:
                        nrowMon["A"] = "Июль";
                        break;
                    case 8:
                        nrowMon["A"] = "Август";
                        break;
                    case 9:
                        nrowMon["A"] = "Сентябрь";
                        break;
                    case 10:
                        nrowMon["A"] = "Октябрь";
                        break;
                    case 11:
                        nrowMon["A"] = "Ноябрь";
                        break;
                    case 12:
                        nrowMon["A"] = "Декабрь";
                        break;
                    default:
                        break;
                }

                TabTmpMon.Rows.Add(nrowMon);

                // Добавлем эту таблицу в наш класс
                Table TabMon0 = new Table("1|J32", TabTmpMon);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabMon0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpYear = new DataTable();
                //
                TabTmpYear.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowYear = TabTmpYear.NewRow();
                nrowYear["A"] = ((DateTime)this.UreDate).Year;
                TabTmpYear.Rows.Add(nrowYear);

                // Добавлем эту таблицу в наш класс
                Table TabYear0 = new Table("1|AC32", TabTmpYear);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabYear0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpDay = new DataTable();
                //
                TabTmpDay.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDay = TabTmpDay.NewRow();
                nrowDay["A"] = ((DateTime)this.UreDate).Day.ToString("00");
                TabTmpDay.Rows.Add(nrowDay);

                // Добавлем эту таблицу в наш класс
                Table TabDay0 = new Table("1|C32", TabTmpDay);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabDay0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpPoDoc = new DataTable();
                //
                TabTmpPoDoc.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowPoDoc = TabTmpPoDoc.NewRow();
                nrowPoDoc["A"] = this.PoDoc;
                TabTmpPoDoc.Rows.Add(nrowPoDoc);

                // Добавлем эту таблицу в наш класс
                Table TabPoDoc0 = new Table("1|K33", TabTmpPoDoc);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabPoDoc0, true);

                //////////////////////////////////////

                // Проверяем пути прежде чем выложить файл
                string PathDir = ((DateTime)base.UreDate).Year.ToString();
                base.CreateFolder(null, PathDir, "");
                string PathDirTmp = ((DateTime)base.UreDate).Month.ToString("00");
                base.CreateFolder(PathDir, PathDirTmp, "");
                PathDir = string.Format("{0}\\{1}", PathDir, PathDirTmp);
                PathDirTmp = ((DateTime)base.UreDate).Day.ToString("00");

                // Создаём задание
                TaskExcel Tsk = new TaskExcel(SourceFile
                    , base.CreateFolder(PathDir, PathDirTmp
                                            , string.Format("расход_ордер{0}{1}{2}{3}.xlsx"
                                                        , ((DateTime)base.UreDate).Year
                                                        , ((DateTime)base.UreDate).Month.ToString("00")
                                                        , ((DateTime)base.UreDate).Day.ToString("00")
                                                        , this.DocNum.ToString("00000")))
                    , TabL, true);

                // Можно создать отдельный екземпляр который сможет работать асинхронно со своими параметрами
                ExcelServer SrvStatic = new ExcelServer(string.Format(@"{0}\Dotx", Environment.CurrentDirectory), string.Format(@"{0}\Report", Environment.CurrentDirectory));

                // Запускаем формирование отчёта в синхронном режиме
                SrvStatic.StartCreateReport(Tsk, 1);

                // открываем приложение Excel
                SrvStatic.OlpenReport(Tsk);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.PrintDefault", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Заявление на возврат
        /// </summary>
        /// <param name="DocNumber">Номер документа инвентаризации</param>
        public void PrintReportReturnBlankWrd(int DocNumber)
        {
            try
            {
                // Создаём запрос для получения списка закладок
                DataTable TblBkm = Com.ProviderFarm.CurrentPrv.getData(string.Format(@"with r as (Select coalesce(ref_sale_sid, sid) As sid, d.return_subtotal_with_tax, d.doc_no
    From `rpsods`.`document` d
    where d.`doc_no` = {0}
      and d.receipt_type=1
      and d.has_return=1)
select date_format(d.post_date, '%d') as fd,
    case when date_format(d.post_date, '%m') = '01' then 'января'
		when date_format(d.post_date, '%m') = '02' then 'февраля'
        when date_format(d.post_date, '%m') = '03' then 'марта'
        when date_format(d.post_date, '%m') = '04' then 'апреля'
        when date_format(d.post_date, '%m') = '05' then 'мая'
        when date_format(d.post_date, '%m') = '06' then 'июня'
        when date_format(d.post_date, '%m') = '07' then 'июля'
        when date_format(d.post_date, '%m') = '08' then 'августа'
        when date_format(d.post_date, '%m') = '09' then 'сентября'
        when date_format(d.post_date, '%m') = '10' then 'октября'
        when date_format(d.post_date, '%m') = '11' then 'ноября'
        else 'декабря' end as fm, 
	substr(date_format(d.post_date, '%Y'),1,3) as fy, 
    substr(date_format(d.post_date, '%Y'),4,1)  as y,
    coalesce(convert(d.doc_no, char),'--') as fr_no, 
    date_format(d.post_date,'%d.%m.%Y') fr_data, 
    convert(round((Select return_subtotal_with_tax From r),2), char) As fr_summa,
    convert(round((Select return_subtotal_with_tax From r),2), char) As fr_nal,
    convert(round(0,2), char) fr_bnal,
    date_format(sysdate(),'%d.%m.%Y') As curdate
From `rpsods`.`document` d
	inner join `rpsods`.`tender` t on d.sid = t.doc_sid
where d.`sid` = (Select sid From r)
group by d.`sid`, d.post_date", DocNumber));

                if (TblBkm == null) throw new ApplicationException(string.Format("Документ с номером документа {0} не найден.", DocNumber));

                // Создаём список закладок
                Wrd.BookmarkList BkmL = new Wrd.BookmarkList();
                if (TblBkm.Rows.Count == 1)
                {
                    BkmL.Add(new Wrd.Bookmark("fd", TblBkm.Rows[0]["fd"].ToString()), true);
                    BkmL.Add(new Wrd.Bookmark("fm", TblBkm.Rows[0]["fm"].ToString()), true);
                    BkmL.Add(new Wrd.Bookmark("fy", TblBkm.Rows[0]["fy"].ToString()), true);
                    BkmL.Add(new Wrd.Bookmark("y", TblBkm.Rows[0]["y"].ToString()), true);
                    BkmL.Add(new Wrd.Bookmark("fr_no", TblBkm.Rows[0]["fr_no"].ToString()), true);
                    BkmL.Add(new Wrd.Bookmark("fr_data", TblBkm.Rows[0]["fr_data"].ToString()), true);
                    BkmL.Add(new Wrd.Bookmark("fr_summa", TblBkm.Rows[0]["fr_summa"].ToString()), true);
                    BkmL.Add(new Wrd.Bookmark("fr_nal", TblBkm.Rows[0]["fr_nal"].ToString()), true);
                    BkmL.Add(new Wrd.Bookmark("fr_bnal", TblBkm.Rows[0]["fr_bnal"].ToString()), true);
                    BkmL.Add(new Wrd.Bookmark("curdate", TblBkm.Rows[0]["curdate"].ToString()), true);
                }

                // Создаём запрос для получения таблицы
                DataTable TblVal = Com.ProviderFarm.CurrentPrv.getData(string.Format(@"with r as (Select coalesce(ref_sale_sid, sid) As sid, d.`doc_no`
    From `rpsods`.`document` d
    where d.`doc_no` = {0}
      and d.receipt_type=1
      and d.has_return=1)
select p.description2 as C0, 
	Concat(coalesce(p.description1,''), ' ' , coalesce(p.attribute,'')) as C1, 
	p.item_size as C2,
	case when row_number() over() = count(*) over() Then'' else ',' end As C3
From `rpsods`.`document` d
	inner join `rpsods`.`document_item` i on d.sid=i.doc_sid
    inner join `rpsods`.invn_sbs_item p on i.invn_sbs_item_sid=p.sid
where d.`sid` = (Select sid From r)
order by i.item_pos", DocNumber));

                // Создаём список таблиц
                Wrd.TableList TblL = new Wrd.TableList();
                TblL.Add(new Wrd.Table("T1", TblVal), true);

                // Создаём список итогов
                Wrd.TotalList Ttl = new Wrd.TotalList();

                // Создаём задание и получаем объект которым будем смотреть результат
                Wrd.TaskWord Tsk = new Wrd.TaskWord(string.Format(@"{0}\Dotx\Заявления на возврат.docx", Environment.CurrentDirectory), null, BkmL, TblL);

                // Можно создать отдельный екземпляр который сможет работать асинхронно со своими параметрами
                Wrd.WordDotxServer SrvStatic = new WordDotxServer(string.Format(@"{0}\Dotx", Environment.CurrentDirectory), null);
                
                // Запускаем формирование отчёта в синхронном режиме
                SrvStatic.StartCreateReport(Tsk);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при формировании отчёта с ошибкой: {0}", ex.Message));

                Com.Log.EventSave(ae.Message, string.Format("{0}.CreateReportReturnBlankWrd", GetType().Name), EventEn.Error);
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

                string FileName = string.Format("{0}_{1}_{2}{3}{4}.txt", Com.LocalFarm.CurLocalDepartament.CompanyCode
                    , Com.LocalFarm.CurLocalDepartament.StoreCode
                    , ((DateTime)this.UreDate).Year
                    , ((DateTime)this.UreDate).Month.ToString("00")
                    , ((DateTime)this.UreDate).Day.ToString("00"));

                string VidalKas = null;
                if (this.LocalDebitor != null) VidalKas = ((BLL.LocalPlg.LocalEmployees)this.LocalDebitor).LocalName;
                else VidalKas = this.OtherDebitor;

                string Vidat = null;
                if (this.LocalCreditor != null) Vidat = ((BLL.LocalPlg.LocalChiefCashiers)this.LocalCreditor).LocalName;
                Vidat = this.OtherKreditor;

                string Row = string.Format(@"{0}\t{1}" +
                    "\t{2}\t{3}" +
                    "\t{4}\t{5}" +
                    "\t{6}\t{7}" +
                    "\t{8}\t{9}" +
                    "\t{10}\t{11}" +
                    "\t{12}\t{13}" +
                    "\t{14}\t{15}" +
                    "\t{16}\t{17}" +
                    "\t{18}\t{19}"
                    , 2, this.DocNum
                    , 20220127181619, 7708790060
                    , Com.LocalFarm.CurLocalDepartament.OKPO, Com.LocalFarm.CurLocalDepartament.Organization
                    , Com.LocalFarm.CurLocalDepartament.CompanyCode, Com.LocalFarm.CurLocalDepartament.StructPodrazdelenie
                    , this.Summa, 0
                    , this.DolRukFio, this.RukFio
                    , this.GlavBuh, VidalKas
                    , Vidat, this.PoDoc
                    , this.Osnovanie, this.PaidRashReasons.LocalName
                    , this.DebetKorSchet, this.KreditNomerSchet);
                base.ExportTo1C(FileName, Row, true);
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
