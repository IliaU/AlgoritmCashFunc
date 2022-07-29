using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
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
                this.PrintDefaultExcel();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.PrintDefault", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        private void PrintDefaultExcel()
        {
            try
            {
                // Получаем текущее подразделение
                BLL.LocalPlg.LocalKassa Kassa = Com.LocalFarm.CurLocalDepartament;

                // Создаём таблицу с которой потом будем работать
                TableList TabL = new TableList();

                string SourceFile = string.Format(@"{0}\Dotx\KasBookDefault.xlsx", Environment.CurrentDirectory);
                //string TargetFile = string.Format(@"{0}\Report\{1}PrihDefault.xlsx", Environment.CurrentDirectory, ((int)this.Id).ToString());


                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpDay = new DataTable();
                //
                TabTmpDay.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDay = TabTmpDay.NewRow();
                nrowDay["A"] = ((DateTime)this.UreDate).Day.ToString("00");
                TabTmpDay.Rows.Add(nrowDay);

                // Добавлем эту таблицу в наш класс
                Table TabDay0 = new Table("1|O2", TabTmpDay);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabDay0, true);

                // Добавлем эту таблицу в наш класс
                Table TabDay1 = new Table("1|O33", TabTmpDay);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabDay1, true);

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
                Table TabMon0 = new Table("1|T2", TabTmpMon);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabMon0, true);

                // Добавлем эту таблицу в наш класс
                Table TabMon1 = new Table("1|T33", TabTmpMon);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
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
                Table TabYear0 = new Table("1|AK2", TabTmpYear);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabYear0, true);

                // Добавлем эту таблицу в наш класс
                Table TabYear1 = new Table("1|AK33", TabTmpYear);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabYear1, true);

                //////////////////////////////////////
                /*
                // Создаём временную таблицу
                DataTable TabTmpDMY = new DataTable();
                //
                TabTmpDMY.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDMY = TabTmpDMY.NewRow();
                nrowDMY["A"] = ((DateTime)this.UreDate).ToShortDateString();
                TabTmpDMY.Rows.Add(nrowDMY);

                // Добавлем эту таблицу в наш класс
                Table TabDMY0 = new Table("1|BB13", TabTmpDMY);
                TabL.Add(TabDMY0, true);
                */

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpDocNum = new DataTable();
                //
                TabTmpDocNum.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDocNum = TabTmpDocNum.NewRow();
                nrowDocNum["A"] = this.DocNum.ToString();
                TabTmpDocNum.Rows.Add(nrowDocNum);

                // Добавлем эту таблицу в наш класс
                Table TabDocNum0 = new Table("1|BD2", TabTmpDocNum);
                TabL.Add(TabDocNum0, true);

                // Добавлем эту таблицу в наш класс
                Table TabDocNum1 = new Table("1|BD33", TabTmpDocNum);
                TabL.Add(TabDocNum1, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpSummaStartDay = new DataTable();
                //
                TabTmpSummaStartDay.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowSummaStartDay = TabTmpSummaStartDay.NewRow();
                nrowSummaStartDay["A"] = this.SummaStartDay.ToString("#.00", CultureInfo.CurrentCulture);
                TabTmpSummaStartDay.Rows.Add(nrowSummaStartDay);

                // Добавлем эту таблицу в наш класс
                Table TabSummaStartDay0 = new Table("1|AS7", TabTmpSummaStartDay);
                TabL.Add(TabSummaStartDay0, true);

                // Добавлем эту таблицу в наш класс
                Table TabSummaStartDay1 = new Table("1|AS38", TabTmpSummaStartDay);
                TabL.Add(TabSummaStartDay1, true);

                //////////////////////////////////////

                if (this.DocList.Count > 30) throw new ApplicationException("Шаблон не потдерживает количество строк больше 30");

                decimal? PerenosD = null;
                decimal? PerenosE = null;
                int CountD = 0;
                int CountE = 0;

                if (this.DocList.Count > 0)
                {
                    // Создаём временную таблицу
                    DataTable TabTmpBodyA = new DataTable();
                    TabTmpBodyA.Columns.Add(new DataColumn("A", typeof(string)));
                    DataTable TabTmpBodyB = new DataTable();
                    TabTmpBodyB.Columns.Add(new DataColumn("B", typeof(string)));
                    DataTable TabTmpBodyC = new DataTable();
                    TabTmpBodyC.Columns.Add(new DataColumn("C", typeof(string)));
                    DataTable TabTmpBodyD = new DataTable();
                    TabTmpBodyD.Columns.Add(new DataColumn("D", typeof(string)));
                    DataTable TabTmpBodyE = new DataTable();
                    TabTmpBodyE.Columns.Add(new DataColumn("E", typeof(string)));
                    
                    for (int i = 0; i < this.DocList.Count && i<21; i++)
                    {
                        DataRow nrowBodyA = TabTmpBodyA.NewRow();
                        DataRow nrowBodyB = TabTmpBodyB.NewRow();
                        DataRow nrowBodyC = TabTmpBodyC.NewRow();
                        DataRow nrowBodyD = TabTmpBodyD.NewRow();
                        DataRow nrowBodyE = TabTmpBodyE.NewRow();

                        switch (this.DocList[i].DocFullName)
                        {
                            case "DocumentPrihod":
                                nrowBodyA["A"] = string.Format("no{0}", this.DocList[i].DocNum);

                                if (this.DocList[i].LocalCreditor != null) nrowBodyB["B"] = this.DocList[i].LocalCreditor.LocalName;
                                else nrowBodyB["B"] = ((DocumentPrihod)this.DocList[i]).OtherKreditor;
                                
                                nrowBodyC["C"] = ((DocumentPrihod)this.DocList[i]).KredikKorSchet;

                                nrowBodyD["D"] = ((DocumentPrihod)this.DocList[i]).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                                if (PerenosD != null) PerenosD = ((decimal)PerenosD) + ((DocumentPrihod)this.DocList[i]).Summa;
                                else PerenosD = ((DocumentPrihod)this.DocList[i]).Summa;
                                CountD++;

                                 nrowBodyE["E"] = "";
                                break;
                            case "DocumentRashod":
                                nrowBodyA["A"] = string.Format("po{0}", this.DocList[i].DocNum);

                                if (this.DocList[i].LocalDebitor != null) nrowBodyB["B"] = this.DocList[i].LocalDebitor.LocalName;
                                else nrowBodyB["B"] = ((DocumentRashod)this.DocList[i]).OtherDebitor;

                                nrowBodyC["C"] = ((DocumentRashod)this.DocList[i]).DebetKorSchet;

                                nrowBodyD["D"] = "";

                                nrowBodyE["E"] = ((DocumentRashod)this.DocList[i]).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                                if (PerenosE != null) PerenosE = ((decimal)PerenosE) + ((DocumentRashod)this.DocList[i]).Summa;
                                else PerenosE = ((DocumentRashod)this.DocList[i]).Summa;
                                CountE++;
                                break;
                            default:
                                break;
                        }
                        TabTmpBodyA.Rows.Add(nrowBodyA);
                        TabTmpBodyB.Rows.Add(nrowBodyB);
                        TabTmpBodyC.Rows.Add(nrowBodyC);
                        TabTmpBodyD.Rows.Add(nrowBodyD);
                        TabTmpBodyE.Rows.Add(nrowBodyE);
                    }

                    // Добавлем эту таблицу в наш класс
                    Table TabBodyA0 = new Table("1|D9", TabTmpBodyA);
                    TabL.Add(TabBodyA0, true);
                    Table TabBodyB0 = new Table("1|M9", TabTmpBodyB);
                    TabL.Add(TabBodyB0, true);
                    Table TabBodyC0 = new Table("1|AG9", TabTmpBodyC);
                    TabL.Add(TabBodyC0, true);
                    Table TabBodyD0 = new Table("1|AS9", TabTmpBodyD);
                    TabL.Add(TabBodyD0, true);
                    Table TabBodyE0 = new Table("1|BE9", TabTmpBodyE);
                    TabL.Add(TabBodyE0, true);

                    // Добавлем эту таблицу в наш класс
                    Table TabBodyA1 = new Table("1|D40", TabTmpBodyA);
                    TabL.Add(TabBodyA1, true);
                    Table TabBodyB1 = new Table("1|M40", TabTmpBodyB);
                    TabL.Add(TabBodyB1, true);
                    Table TabBodyC1 = new Table("1|AG40", TabTmpBodyC);
                    TabL.Add(TabBodyC1, true);
                    Table TabBodyD1 = new Table("1|AS40", TabTmpBodyD);
                    TabL.Add(TabBodyD1, true);
                    Table TabBodyE1 = new Table("1|BE40", TabTmpBodyE);
                    TabL.Add(TabBodyE1, true);

                    //------------------------- Перенос
                    DataTable TabTmpPerenosD = new DataTable();
                    TabTmpPerenosD.Columns.Add(new DataColumn("D", typeof(string)));
                    DataRow nrowPerenosD = TabTmpPerenosD.NewRow();
                    if (PerenosD != null) nrowPerenosD["D"] = ((decimal)PerenosD).ToString("#.00", CultureInfo.CurrentCulture);
                    else nrowPerenosD["D"] = "";
                    TabTmpPerenosD.Rows.Add(nrowPerenosD);
                    //
                    Table TabPerenosD0 = new Table("1|AS30", TabTmpPerenosD);
                    TabL.Add(TabPerenosD0, true);
                    Table TabPerenosD1 = new Table("1|AS61", TabTmpPerenosD);
                    TabL.Add(TabPerenosD1, true);

                    DataTable TabTmpPerenosE = new DataTable();
                    TabTmpPerenosE.Columns.Add(new DataColumn("E", typeof(string)));
                    DataRow nrowPerenosE = TabTmpPerenosE.NewRow();
                    if (PerenosE != null) nrowPerenosE["E"] = ((decimal)PerenosE).ToString("#.00", CultureInfo.CurrentCulture);
                    else nrowPerenosE["E"] = "";
                    TabTmpPerenosE.Rows.Add(nrowPerenosE);
                    //
                    Table TabPerenosE0 = new Table("1|BE30", TabTmpPerenosE);
                    TabL.Add(TabPerenosE0, true);
                    Table TabPerenosE1 = new Table("1|BE61", TabTmpPerenosE);
                    TabL.Add(TabPerenosE1, true);
                    //-------------------------
                }

                if (this.DocList.Count > 21)
                {
                    // Создаём временную таблицу
                    DataTable TabTmpBodyA = new DataTable();
                    TabTmpBodyA.Columns.Add(new DataColumn("A", typeof(string)));
                    DataTable TabTmpBodyB = new DataTable();
                    TabTmpBodyB.Columns.Add(new DataColumn("B", typeof(string)));
                    DataTable TabTmpBodyC = new DataTable();
                    TabTmpBodyC.Columns.Add(new DataColumn("C", typeof(string)));
                    DataTable TabTmpBodyD = new DataTable();
                    TabTmpBodyD.Columns.Add(new DataColumn("D", typeof(string)));
                    DataTable TabTmpBodyE = new DataTable();
                    TabTmpBodyE.Columns.Add(new DataColumn("E", typeof(string)));

                    for (int i = 21; i < this.DocList.Count; i++)
                    {
                        DataRow nrowBodyA = TabTmpBodyA.NewRow();
                        DataRow nrowBodyB = TabTmpBodyB.NewRow();
                        DataRow nrowBodyC = TabTmpBodyC.NewRow();
                        DataRow nrowBodyD = TabTmpBodyD.NewRow();
                        DataRow nrowBodyE = TabTmpBodyE.NewRow();

                        switch (this.DocList[i].DocFullName)
                        {
                            case "DocumentPrihod":
                                nrowBodyA["A"] = string.Format("no{0}", this.DocList[i].DocNum);

                                if (this.DocList[i].LocalCreditor != null) nrowBodyB["B"] = this.DocList[i].LocalCreditor.LocalName;
                                else nrowBodyB["B"] = ((BLL.DocumentPlg.DocumentPrihod)this.DocList[i]).OtherKreditor;

                                nrowBodyC["C"] = ((BLL.DocumentPlg.DocumentPrihod)this.DocList[i]).KredikKorSchet;

                                nrowBodyD["D"] = ((BLL.DocumentPlg.DocumentPrihod)this.DocList[i]).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                                if (PerenosD != null) PerenosD = ((decimal)PerenosD) + ((DocumentPrihod)this.DocList[i]).Summa;
                                else PerenosD = ((DocumentPrihod)this.DocList[i]).Summa;
                                CountD++;

                                nrowBodyE["E"] = "";
                                break;
                            case "DocumentRashod":
                                nrowBodyA["A"] = string.Format("po{0}", this.DocList[i].DocNum);

                                if (this.DocList[i].LocalDebitor != null) nrowBodyB["B"] = this.DocList[i].LocalDebitor.LocalName;
                                else nrowBodyB["B"] = ((BLL.DocumentPlg.DocumentRashod)this.DocList[i]).OtherDebitor;

                                nrowBodyC["C"] = ((BLL.DocumentPlg.DocumentRashod)this.DocList[i]).DebetKorSchet;

                                nrowBodyD["D"] = "";

                                nrowBodyE["E"] = ((BLL.DocumentPlg.DocumentRashod)this.DocList[i]).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                                if (PerenosE != null) PerenosE = ((decimal)PerenosD) + ((DocumentRashod)this.DocList[i]).Summa;
                                else PerenosE = ((DocumentRashod)this.DocList[i]).Summa;
                                CountE++;
                                break;
                            default:
                                break;
                        }
                        TabTmpBodyA.Rows.Add(nrowBodyA);
                        TabTmpBodyB.Rows.Add(nrowBodyB);
                        TabTmpBodyC.Rows.Add(nrowBodyC);
                        TabTmpBodyD.Rows.Add(nrowBodyD);
                        TabTmpBodyE.Rows.Add(nrowBodyE);
                    }

                    // Добавлем эту таблицу в наш класс
                    Table TabBodyA0 = new Table("1|BU6", TabTmpBodyA);
                    TabL.Add(TabBodyA0, true);
                    Table TabBodyB0 = new Table("1|CC6", TabTmpBodyB);
                    TabL.Add(TabBodyB0, true);
                    Table TabBodyC0 = new Table("1|CW6", TabTmpBodyC);
                    TabL.Add(TabBodyC0, true);
                    Table TabBodyD0 = new Table("1|DH6", TabTmpBodyD);
                    TabL.Add(TabBodyD0, true);
                    Table TabBodyE0 = new Table("1|DT6", TabTmpBodyE);
                    TabL.Add(TabBodyE0, true);

                    // Добавлем эту таблицу в наш класс
                    Table TabBodyA1 = new Table("1|BU37", TabTmpBodyA);
                    TabL.Add(TabBodyA1, true);
                    Table TabBodyB1 = new Table("1|CC37", TabTmpBodyB);
                    TabL.Add(TabBodyB1, true);
                    Table TabBodyC1 = new Table("1|CW37", TabTmpBodyC);
                    TabL.Add(TabBodyC1, true);
                    Table TabBodyD1 = new Table("1|DH37", TabTmpBodyD);
                    TabL.Add(TabBodyD1, true);
                    Table TabBodyE1 = new Table("1|DT637", TabTmpBodyE);
                    TabL.Add(TabBodyE1, true);
                }

                //------------------------- Итого за день
                DataTable TabTmpItogOfDayD = new DataTable();
                TabTmpItogOfDayD.Columns.Add(new DataColumn("D", typeof(string)));
                DataRow nrowItogOfDayD = TabTmpItogOfDayD.NewRow();
                if (PerenosD != null) nrowItogOfDayD["D"] = ((decimal)PerenosD).ToString("#.00", CultureInfo.CurrentCulture);
                else nrowItogOfDayD["D"] = "";
                TabTmpItogOfDayD.Rows.Add(nrowItogOfDayD);
                //
                Table TabItogOfDayD0 = new Table("1|DH15", TabTmpItogOfDayD);
                TabL.Add(TabItogOfDayD0, true);
                Table TabItogOfDayD1 = new Table("1|DH46", TabTmpItogOfDayD);
                TabL.Add(TabItogOfDayD1, true);

                DataTable TabTmpItogOfDayE = new DataTable();
                TabTmpItogOfDayE.Columns.Add(new DataColumn("E", typeof(string)));
                DataRow nrowItogOfDayE = TabTmpItogOfDayE.NewRow();
                if (PerenosE != null) nrowItogOfDayE["E"] = ((decimal)PerenosE).ToString("#.00", CultureInfo.CurrentCulture);
                else nrowItogOfDayE["E"] = "";
                TabTmpItogOfDayE.Rows.Add(nrowItogOfDayE);
                //
                Table TabItogOfDayE0 = new Table("1|DT15", TabTmpItogOfDayE);
                TabL.Add(TabItogOfDayE0, true);
                Table TabItogOfDayE1 = new Table("1|DT46", TabTmpItogOfDayE);
                TabL.Add(TabItogOfDayE1, true);
                //-------------------------

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpSummaEndDay = new DataTable();
                //
                TabTmpSummaEndDay.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowSummaEndDay = TabTmpSummaEndDay.NewRow();
                nrowSummaEndDay["A"] = this.SummaEndDay.ToString("#.00", CultureInfo.CurrentCulture);
                TabTmpSummaEndDay.Rows.Add(nrowSummaEndDay);

                // Добавлем эту таблицу в наш класс
                Table TabSummaEndDay0 = new Table("1|DH17", TabTmpSummaEndDay);
                TabL.Add(TabSummaEndDay0, true);

                // Добавлем эту таблицу в наш класс
                Table TabSummaEndDay1 = new Table("1|DH48", TabTmpSummaEndDay);
                TabL.Add(TabSummaEndDay1, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKassir = new DataTable();
                //
                TabTmpKassir.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKassir = TabTmpKassir.NewRow();
                nrowKassir["A"] = (this.LocalCreditor != null ? this.LocalCreditor.LocalName : this.OtherKreditor);
                TabTmpKassir.Rows.Add(nrowKassir);

                // Добавлем эту таблицу в наш класс
                Table TabKassir0 = new Table("1|CY22", TabTmpKassir);
                TabL.Add(TabKassir0, true);

                // Добавлем эту таблицу в наш класс
                Table TabKassir1 = new Table("1|CY53", TabTmpKassir);
                TabL.Add(TabKassir1, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpCountPrih = new DataTable();
                //
                TabTmpCountPrih.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowCountPrih = TabTmpCountPrih.NewRow();
                nrowCountPrih["A"] = Com.Utils.GetStringForInt(CountD, "", "", "", false).ToLower();
                TabTmpCountPrih.Rows.Add(nrowCountPrih);

                // Добавлем эту таблицу в наш класс
                Table TabCountPrih0 = new Table("1|CG25", TabTmpCountPrih);
                TabL.Add(TabCountPrih0, true);

                // Добавлем эту таблицу в наш класс
                Table TabCountPrih1 = new Table("1|CG56", TabTmpCountPrih);
                TabL.Add(TabCountPrih1, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpCountRash = new DataTable();
                //
                TabTmpCountRash.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowCountRash = TabTmpCountRash.NewRow();
                nrowCountRash["A"] = Com.Utils.GetStringForInt(CountE, "", "", "", false).ToLower();
                TabTmpCountRash.Rows.Add(nrowCountRash);

                // Добавлем эту таблицу в наш класс
                Table TabCountRash0 = new Table("1|BV27", TabTmpCountRash);
                TabL.Add(TabCountRash0, true);

                // Добавлем эту таблицу в наш класс
                Table TabCountRash1 = new Table("1|BV58", TabTmpCountRash);
                TabL.Add(TabCountRash1, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpBuh = new DataTable();
                //
                TabTmpBuh.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowBuh = TabTmpBuh.NewRow();
                if (this.LocalDebitor != null) nrowBuh["A"] = ((BLL.LocalPlg.LocalAccounters)this.LocalDebitor).LocalName;
                else nrowBuh["A"] = this.OtherDebitor;
                TabTmpBuh.Rows.Add(nrowBuh);

                // Добавлем эту таблицу в наш класс
                Table TabBuh0 = new Table("1|CZ30", TabTmpBuh);
                TabL.Add(TabBuh0, true);

                // Добавлем эту таблицу в наш класс
                Table TabBuh1 = new Table("1|CZ61", TabTmpBuh);
                TabL.Add(TabBuh1, true);

                //////////////////////////////////////

                // Проверяем пути прежде чем выложить файл
                string PathDir = ((DateTime)base.UreDate).Year.ToString();
                base.CreateFolder(null, PathDir, "");
                string PathDirTmp = ((DateTime)base.UreDate).Month.ToString("00");

                // Создаём задание
                TaskExcel Tsk = new TaskExcel(SourceFile
                    , base.CreateFolder(PathDir, PathDirTmp
                                            , string.Format("кассов_книга{0}{1}{2}.xlsx"
                                                        , ((DateTime)base.UreDate).Year
                                                        , ((DateTime)base.UreDate).Month.ToString("00")
                                                        , ((DateTime)base.UreDate).Day.ToString("00")))
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

        private void PrintDefaultWord()
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
                            if (item.LocalDebitor != null) nrow["A2"] = item.LocalDebitor.LocalName;
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
                Bookmark BmKassir = new Bookmark("BmKassir", (this.LocalCreditor != null ? this.LocalCreditor.LocalName : this.OtherKreditor));
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

                string FileName = string.Format("{0}_{1}_{2}{3}{4}.txt", Com.LocalFarm.CurLocalDepartament.CompanyCode
                    , Com.LocalFarm.CurLocalDepartament.StoreCode
                    , ((DateTime)this.UreDate).Year
                    , ((DateTime)this.UreDate).Month.ToString("00")
                    , ((DateTime)this.UreDate).Day.ToString("00"));

                string PolKas = null;
                if (this.LocalDebitor != null) PolKas = ((BLL.LocalPlg.LocalAccounters)this.LocalDebitor).LocalName;
                else PolKas = this.OtherDebitor;

                string Sotrudnik = null;
                if (this.LocalCreditor != null) Sotrudnik = ((BLL.LocalPlg.LocalChiefCashiers)this.LocalCreditor).LocalName;
                else Sotrudnik = this.OtherKreditor;

                // Пробегаем по списку документов
                bool FlagDelete = true;
                foreach (Document item in this.DocList)
                {
                    string OrderTyp = "";
                    string DolRukFio = "";
                    string RukFio = "";
                    string PoDocumrntu = "";
                    string OsnovsnieNum = "";
                    string OsnovanieTxt = "";
                    string Prilozenie = "";
                    string DebetNum = "";
                    string KreditNum = "";
                    string KodAnalitUch = "";
                    string KodNaznachenia = "";
                    string KodDivision = "";
                    string PoluKasOrVidKas = "";
                    string PrinztoOtVidat = "";
                    string NoDoc = null;
                    string KorShet = null;
                    string GlavBuh = null;
                    string Sum = null;
                    switch (item.DocFullName)
                    {
                        case "DocumentPrihod":

                            OrderTyp = "1";
                            OsnovsnieNum = ((DocumentPrihod)item).Osnovanie;
                            OsnovanieTxt = ((DocumentPrihod)item).PaidInReasons.LocalName;
                            Prilozenie = ((DocumentPrihod)item).Prilozenie;
                            DebetNum = ((DocumentPrihod)item).DebetNomerSchet;
                            KreditNum = ((DocumentPrihod)item).KredikKorSchet;
                            KodAnalitUch = ((DocumentPrihod)item).KredikKodAnalUch;
                            KodNaznachenia = ((DocumentPrihod)item).KodNazn;
                            
                            if (item.LocalDebitor != null) PoluKasOrVidKas = ((BLL.LocalPlg.LocalChiefCashiers)item.LocalDebitor).LocalName;
                            else PoluKasOrVidKas = ((DocumentPrihod)item).OtherDebitor;

                            if (item.LocalCreditor != null) PrinztoOtVidat = ((BLL.LocalPlg.LocalEmployees)item.LocalCreditor).LocalName;
                            else PrinztoOtVidat = ((DocumentPrihod)item).OtherKreditor;

                            GlavBuh = ((DocumentPrihod)item).GlavBuh;
                            KodDivision = ((DocumentPrihod)item).KreditKodDivision;

                            NoDoc = string.Format("no{0}", item.DocNum);
                            KorShet = ((DocumentPrihod)item).KredikKorSchet;
                            Sum = ((DocumentPrihod)item).Summa.ToString("#.00", CultureInfo.CurrentCulture);
                            break;                            
                        case "DocumentRashod":

                            OrderTyp = "2";
                            DolRukFio = ((DocumentRashod)item).DolRukFio;
                            RukFio = ((DocumentRashod)item).RukFio;
                            PoDocumrntu = ((DocumentRashod)item).PoDoc;
                            OsnovsnieNum = ((DocumentRashod)item).Osnovanie;
                            OsnovanieTxt = ((DocumentRashod)item).PaidRashReasons.LocalName;
                            Prilozenie = ((DocumentRashod)item).Prilozenie;
                            DebetNum = ((DocumentRashod)item).DebetKorSchet;
                            KreditNum = ((DocumentRashod)item).KreditNomerSchet;
                            KodAnalitUch = ((DocumentRashod)item).DebetKodAnalUch;
                            KodNaznachenia = ((DocumentRashod)item).KodNazn;
                            
                            if (item.LocalCreditor != null) PoluKasOrVidKas = ((BLL.LocalPlg.LocalChiefCashiers)item.LocalCreditor).LocalName;
                            else PoluKasOrVidKas = ((DocumentRashod)item).OtherKreditor;

                            if (item.LocalDebitor != null) PrinztoOtVidat = ((BLL.LocalPlg.LocalEmployees)item.LocalDebitor).LocalName;
                            else PrinztoOtVidat = ((DocumentRashod)item).OtherDebitor;

                            GlavBuh = ((DocumentRashod)item).GlavBuh;
                            KodDivision = ((DocumentRashod)item).DebetKodDivision;

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
                    "\t{14}\t{15}" +
                    "\t{16}\t{17}" +
                    "\t{18}\t{19}" +
                    "\t{20}\t{21}" +
                    "\t{22}\t{23}"
                    , OrderTyp, item.DocNum
                    , string.Format("{0}{1}{2}{3}{4}{5}", ((DateTime)item.UreDate).Year
                            , ((DateTime)item.UreDate).Month.ToString("00")
                            , ((DateTime)item.UreDate).Day.ToString("00")
                            , ((DateTime)item.CreateDate).Hour.ToString("00")
                            , ((DateTime)item.CreateDate).Minute.ToString("00")
                            , ((DateTime)item.CreateDate).Second.ToString("00")), Com.LocalFarm.CurLocalDepartament.INN
                    , Com.LocalFarm.CurLocalDepartament.OKPO, Com.LocalFarm.CurLocalDepartament.Organization
                    , Com.LocalFarm.CurLocalDepartament.StoreCode, Com.LocalFarm.CurLocalDepartament.StructPodrazdelenie
                    , Sum, "0,00"
                    , DolRukFio, RukFio
                    , GlavBuh, PoluKasOrVidKas
                    , PrinztoOtVidat, PoDocumrntu
                    , OsnovsnieNum, OsnovanieTxt
                    , Prilozenie, DebetNum
                    , KreditNum, KodDivision
                    , KodAnalitUch, KodNaznachenia);
                    base.ExportTo1C(FileName, Row, FlagDelete);

                    if (FlagDelete) FlagDelete = false;
                }

                MessageBox.Show("Выгрузка завершена успешно."); 
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
