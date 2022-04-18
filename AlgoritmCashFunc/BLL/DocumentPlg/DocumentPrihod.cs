using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.BLL.DocumentPlg.Lib;
using AlgoritmCashFunc.Lib;
using System.Data;
using WordDotx;

namespace AlgoritmCashFunc.BLL.DocumentPlg
{
    /// <summary>
    /// Класс который представляет из себя докимент прихода
    /// </summary>
    public sealed class DocumentPrihod : Document
    {
        /// <summary>
        /// Номер счёта
        /// </summary>
        public string DebetNomerSchet;

        /// <summary>
        /// Код подразделения
        /// </summary>
        public string KreditKodDivision;

        /// <summary>
        /// Кор счёт
        /// </summary>
        public string KredikKorSchet;

        /// <summary>
        /// Код аналитического учёта
        /// </summary>
        public string KredikKodAnalUch;

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
        /// Основание
        /// </summary>
        public string Osnovanie;

        /// <summary>
        /// Основание прихода
        /// </summary>
        public BLL.LocalPlg.LocalPaidInReasons PaidInReasons;

        /// <summary>
        /// В том числе
        /// </summary>
        public string VtomChisle;

        /// <summary>
        /// НДС
        /// </summary>
        public decimal NDS;

        /// <summary>
        /// Приложение
        /// </summary>
        public string Prilozenie;

        /// <summary>
        /// Главный бухгалтер
        /// </summary>
        public string GlavBuh;

        /// <summary>
        /// Конструктор для загрузки из базы данных
        /// </summary>
        /// <param name="Id">Идентификатор в базе данных</param>
        /// <param name="UreDate">Дата создания документа</param>
        /// <param name="CteateDate">Дата создания документа</param>
        /// <param name="ModifyDate">Дата изменеия документа</param>
        /// <param name="ModifyUser">Пользовтаель который изменил последний раз документ</param>
        /// <param name="CurOperation">Операция к которой относится этот документ</param>
        /// <param name="LocalDebitor">Дебитор</param>
        /// <param name="LocalCreditor">Кредитор</param>
        /// <param name="OtherDebitor">Дебитор который ввели вручную не из списка</param>
        /// <param name="OtherKreditor">Кредитор который ввели вручную не из списка</param>
        /// <param name="DocNum"> Черновик</param>
        /// <param name="IsDraft">Черновик</param>
        /// <param name="IsProcessed">Проведённый документ или нет</param>
        public DocumentPrihod(int? Id, DateTime? UreDate, DateTime CteateDate, DateTime ModifyDate, string ModifyUser, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, string OtherDebitor, string OtherKreditor, int DocNum, bool IsDraft, bool IsProcessed) : base("DocumentPrihod", CurOperation, LocalDebitor, LocalCreditor, OtherDebitor, OtherKreditor, DocNum, IsProcessed)
        {
            try
            {
                base.Id = Id;
                base.UreDate = UreDate;
                base.CteateDate = CteateDate;
                base.ModifyDate = ModifyDate;
                base.ModifyUser = ModifyUser;
                base.IsDraft = IsDraft;
                base.IsProcessed = IsProcessed;

                // Если документ читается из базы данных то нужно прочитать дополнительные параметры
                if (base.Id != null)
                {
                    DocumentPrihod MyObj = this;
                    bool tt = Com.ProviderFarm.CurrentPrv.GetDocumentPrihod(ref MyObj);
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
        public DocumentPrihod() : this(null, DateTime.Now.Date, DateTime.Now, DateTime.Now, Com.UserFarm.CurrentUser.Logon, Com.OperationFarm.CurOperationList["OperationPrihod"], null, null, null, null, Com.LocalFarm.CurLocalDepartament.LastDocNumPrih+1, true, false)
        {
            try
            {
                this.CurOperation = Com.OperationFarm.CurOperationList["OperationPrihod"];
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
                if (Com.ProviderFarm.CurrentPrv.HashDocumentPrihod(this))
                {
                    Com.ProviderFarm.CurrentPrv.UpdateDocumentPrihod(this);
                }
                else  // Если нет то вставляем
                {
                    Com.ProviderFarm.CurrentPrv.SetDocumentPrihod(this);
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
                nrowPrihodOKUD["A"] = ((BLL.OperationPlg.OperationPrihod)base.CurOperation).OKUD;
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
                nrowDolRukOrg["A"] =  Kassa.DolRukOrg;
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
                SrvStatic.StartCreateReport(Tsk);

                // открываем приложение Excel
                SrvStatic.OlpenReport(Tsk);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.PrintTitle", GetType().Name), EventEn.Error);
                throw ae;
            }
        }
    }
}
