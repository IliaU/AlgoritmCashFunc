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
    /// Класс который представляет из себя докимент акта инвентаризации
    /// </summary>
    public sealed class DocumentInvent : Document
    {
        /// <summary>
        /// Фактическое наличие средств строка 1
        /// </summary>
        public string FactStr1;

        /// <summary>
        /// Фактическое наличие средств строка 2
        /// </summary>
        public string FactStr2;

        /// <summary>
        /// Фактическое наличие средств строка 3
        /// </summary>
        public string FactStr3;

        /// <summary>
        /// Фактическое наличие средств строка 4
        /// </summary>
        public string FactStr4;

        /// <summary>
        /// Фактическое наличие средств строка 5
        /// </summary>
        public string FactStr5;

        /// <summary>
        /// Фактическое наличие средств (Значение) строка 1
        /// </summary>
        public decimal FactVal1;

        /// <summary>
        /// Фактическое наличие средств (Значение) строка 2
        /// </summary>
        public decimal FactVal2;

        /// <summary>
        /// Фактическое наличие средств (Значение) строка 3
        /// </summary>
        public decimal FactVal3;

        /// <summary>
        /// Фактическое наличие средств (Значение) строка 4
        /// </summary>
        public decimal FactVal4;

        /// <summary>
        /// Фактическое наличие средств (Значение) строка 5
        /// </summary>
        public decimal FactVal5;

        /// <summary>
        /// Итог по учётным данным на сумму
        /// </summary>
        public decimal ItogPoUchDan;

        /// <summary>
        /// Последний номер приходного ордера
        /// </summary>
        public int LastPrihodNum;

        /// <summary>
        /// Последний номер расходного ордера
        /// </summary>
        public int LastRashodNum;

        /// <summary>
        /// Приказ тип и номер документа
        /// </summary>
        public string PrikazTypAndDocNum;

        /// <summary>
        /// Приказ дата документа
        /// </summary>
        public DateTime? PrikazUreDate;

        /// <summary>
        /// Приказ должность материально ответсвенного
        /// </summary>
        public string PrikazDolMatOtv;

        /// <summary>
        /// Приказ расшифровка материально ответсвенного
        /// </summary>
        public string PrikazDecodeMatOtv;

        /// <summary>
        /// Должность комиссии строка 1
        /// </summary>
        public string KomissionStr1;

        /// <summary>
        /// Должность комиссии строка 2
        /// </summary>
        public string KomissionStr2;

        /// <summary>
        /// Должность комиссии строка 3
        /// </summary>
        public string KomissionStr3;

        /// <summary>
        /// Должность комиссии строка 4
        /// </summary>
        public string KomissionStr4;
        
        /// <summary>
        /// Расшифровка должности комиссии строка 1
        /// </summary>
        public string KomissionDecode1;

        /// <summary>
        /// Расшифровка должности комиссии строка 2
        /// </summary>
        public string KomissionDecode2;

        /// <summary>
        /// Расшифровка должности комиссии строка 3
        /// </summary>
        public string KomissionDecode3;

        /// <summary>
        /// Расшифровка должности комиссии строка 4
        /// </summary>
        public string KomissionDecode4;

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
        public DocumentInvent(int? Id, DateTime? UreDate, DateTime CreateDate, DateTime ModifyDate, string ModifyUser, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, Local Departament, string OtherDebitor, string OtherKreditor, int DocNum, bool IsDraft, bool IsProcessed) : base("DocumentInvent", CurOperation, LocalDebitor, LocalCreditor, Departament, OtherDebitor, OtherKreditor, DocNum, IsProcessed)
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
                    DocumentInvent MyObj = this;
                    bool tt = Com.ProviderFarm.CurrentPrv.GetDocumentInvent(ref MyObj);
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
        public DocumentInvent() : this(null, DateTime.Now.Date, DateTime.Now, DateTime.Now, Com.UserFarm.CurrentUser.Logon, Com.OperationFarm.CurOperationList["OperationInvent"], null, null, Com.LocalFarm.CurLocalDepartament, null, null, Com.LocalFarm.CurLocalDepartament.LastDocNumInvent+1, true, false)
        {
            try
            {
                this.CurOperation = Com.OperationFarm.CurOperationList["OperationInvent"];
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
                if (Com.ProviderFarm.CurrentPrv.HashDocumentInvent(this))
                {
                    Com.ProviderFarm.CurrentPrv.UpdateDocumentInvent(this);
                }
                else  // Если нет то вставляем
                {
                    Com.ProviderFarm.CurrentPrv.SetDocumentInvent(this);
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

                string SourceFile = string.Format(@"{0}\Dotx\Invent.xlsx", Environment.CurrentDirectory);
                //string TargetFile = string.Format(@"{0}\Report\{1}PrihDefault.xlsx", Environment.CurrentDirectory, ((int)this.Id).ToString());

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
                DataTable TabTmpInvent = new DataTable();
                //
                TabTmpInvent.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowInventOKUD = TabTmpInvent.NewRow();
                nrowInventOKUD["A"] = ((BLL.OperationPlg.OperationInvent)base.CurOperation).OKUD;
                TabTmpInvent.Rows.Add(nrowInventOKUD);
                DataRow nrowInventOKPO = TabTmpInvent.NewRow();
                nrowInventOKPO["A"] = Kassa.OKPO;
                TabTmpInvent.Rows.Add(nrowInventOKPO);

                // Добавлем эту таблицу в наш класс
                Table TabInvent0 = new Table("1|BM5", TabTmpInvent);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabInvent0, true);
                
                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpInventPrikazDD = new DataTable();
                //
                TabTmpInventPrikazDD.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowInventPrikazDocNum = TabTmpInventPrikazDD.NewRow();
                nrowInventPrikazDocNum["A"] = this.PrikazTypAndDocNum;
                TabTmpInventPrikazDD.Rows.Add(nrowInventPrikazDocNum);
                DataRow nrowInventPrikazUreDate = TabTmpInventPrikazDD.NewRow();
                nrowInventPrikazUreDate["A"] = ((DateTime)this.PrikazUreDate).ToShortDateString();
                TabTmpInventPrikazDD.Rows.Add(nrowInventPrikazUreDate);

                // Добавлем эту таблицу в наш класс
                Table TabInventPrikazDD0 = new Table("1|BM12", TabTmpInventPrikazDD);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabInventPrikazDD0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpDocNum = new DataTable();
                //
                TabTmpDocNum.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDocNum = TabTmpDocNum.NewRow();
                nrowDocNum["A"] = this.DocNum.ToString();
                TabTmpDocNum.Rows.Add(nrowDocNum);

                // Добавлем эту таблицу в наш класс
                Table TabDocNum0 = new Table("1|AM17", TabTmpDocNum);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabDocNum0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpDMY = new DataTable();
                //
                TabTmpDMY.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDMY = TabTmpDMY.NewRow();
                nrowDMY["A"] = ((DateTime)this.UreDate).ToShortDateString();
                TabTmpDMY.Rows.Add(nrowDMY);

                // Добавлем эту таблицу в наш класс
                Table TabDMY0 = new Table("1|BC17", TabTmpDMY);
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
                Table TabMon0 = new Table("1|AP19", TabTmpMon);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabMon0, true);

                Table TabMon1 = new Table("1|J59", TabTmpMon);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabMon1, true);

                Table TabMon2 = new Table("2|H33", TabTmpMon);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabMon2, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpYear = new DataTable();
                //
                TabTmpYear.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowYear = TabTmpYear.NewRow();
                nrowYear["A"] = ((DateTime)this.UreDate).Year;
                TabTmpYear.Rows.Add(nrowYear);

                // Добавлем эту таблицу в наш класс
                Table TabYear0 = new Table("1|AZ19", TabTmpYear);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabYear0, true);

                Table TabYear1 = new Table("1|T59", TabTmpYear);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabYear1, true);

                Table TabYear2 = new Table("2|R33", TabTmpYear);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabYear2, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpDay = new DataTable();
                //
                TabTmpDay.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDay = TabTmpDay.NewRow();
                nrowDay["A"] = ((DateTime)this.UreDate).Day.ToString("00");
                TabTmpDay.Rows.Add(nrowDay);

                // Добавлем эту таблицу в наш класс
                Table TabDay0 = new Table("1|AK19", TabTmpDay);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabDay0, true);

                Table TabDay1 = new Table("1|E59", TabTmpDay);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabDay1, true);

                Table TabDay2 = new Table("2|C33", TabTmpDay);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabDay2, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpPrikazDolMatOtv = new DataTable();
                //
                TabTmpPrikazDolMatOtv.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowPrikazDolMatOtv = TabTmpPrikazDolMatOtv.NewRow();
                nrowPrikazDolMatOtv["A"] = this.PrikazDolMatOtv;
                TabTmpPrikazDolMatOtv.Rows.Add(nrowPrikazDolMatOtv);

                // Добавлем эту таблицу в наш класс
                Table TabPrikazDolMatOtv0 = new Table("1|AD26", TabTmpPrikazDolMatOtv);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabPrikazDolMatOtv0, true);

                Table TabPrikazDolMatOtv1 = new Table("1|AA57", TabTmpPrikazDolMatOtv);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabPrikazDolMatOtv1, true);

                Table TabPrikazDolMatOtv2 = new Table("2|AD18", TabTmpPrikazDolMatOtv);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabPrikazDolMatOtv2, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpPrikazDecodeMatOtv = new DataTable();
                //
                TabTmpPrikazDecodeMatOtv.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowPrikazDecodeMatOtv = TabTmpPrikazDecodeMatOtv.NewRow();
                nrowPrikazDecodeMatOtv["A"] = this.PrikazDecodeMatOtv;
                TabTmpPrikazDecodeMatOtv.Rows.Add(nrowPrikazDecodeMatOtv);

                // Добавлем эту таблицу в наш класс
                Table TabPrikazDecodeMatOtv0 = new Table("1|BC26", TabTmpPrikazDecodeMatOtv);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabPrikazDecodeMatOtv0, true);

                Table TabPrikazDecodeMatOtv1 = new Table("1|BC57", TabTmpPrikazDecodeMatOtv);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabPrikazDecodeMatOtv1, true);

                Table TabPrikazDecodeMatOtv2 = new Table("2|BC18", TabTmpPrikazDecodeMatOtv);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabPrikazDecodeMatOtv2, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpFactStr = new DataTable();
                //
                TabTmpFactStr.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowFactStr = TabTmpFactStr.NewRow();
                nrowFactStr["A"] = this.FactStr1;
                TabTmpFactStr.Rows.Add(nrowFactStr);
                nrowFactStr = TabTmpFactStr.NewRow();
                nrowFactStr["A"] = this.FactStr2;
                TabTmpFactStr.Rows.Add(nrowFactStr);
                nrowFactStr = TabTmpFactStr.NewRow();
                nrowFactStr["A"] = this.FactStr3;
                TabTmpFactStr.Rows.Add(nrowFactStr);
                nrowFactStr = TabTmpFactStr.NewRow();
                nrowFactStr["A"] = this.FactStr4;
                TabTmpFactStr.Rows.Add(nrowFactStr);
                nrowFactStr = TabTmpFactStr.NewRow();
                nrowFactStr["A"] = this.FactStr5;
                TabTmpFactStr.Rows.Add(nrowFactStr);

                // Добавлем эту таблицу в наш класс
                Table TabFactStr0 = new Table("1|G29", TabTmpFactStr);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabFactStr0, true);

                //////////////////////////////////////

                // Получакем значение и преобразовываем к строке
                string tmpFactVal1 = decimal.Round(this.FactVal1, 2).ToString().Replace(".", ",");
                string tmpFactVal2 = decimal.Round(this.FactVal2, 2).ToString().Replace(".", ",");
                string tmpFactVal3 = decimal.Round(this.FactVal3, 2).ToString().Replace(".", ",");
                string tmpFactVal4 = decimal.Round(this.FactVal4, 2).ToString().Replace(".", ",");
                string tmpFactVal5 = decimal.Round(this.FactVal5, 2).ToString().Replace(".", ",");

                // Создаём переменные для рублей и копеек
                int? sumR1 = null;
                int? sumR2 = null;
                int? sumR3 = null;
                int? sumR4 = null;
                int? sumR5 = null;
                int? sumK1 = null;
                int? sumK2 = null;
                int? sumK3 = null;
                int? sumK4 = null;
                int? sumK5 = null;

                // Проверка на наличие копеек
                if (tmpFactVal1.IndexOf(',') > -1)
                {
                    sumR1 = int.Parse(tmpFactVal1.Substring(0, tmpFactVal1.IndexOf(',')));
                    sumK1 = int.Parse(tmpFactVal1.Substring(tmpFactVal1.IndexOf(',') + 1));
                }
                else sumR1 = (int)this.FactVal1;
                //
                if (tmpFactVal1.IndexOf(',') > -1)
                {
                    sumR2 = int.Parse(tmpFactVal2.Substring(0, tmpFactVal2.IndexOf(',')));
                    sumK2 = int.Parse(tmpFactVal2.Substring(tmpFactVal2.IndexOf(',') + 1));
                }
                else sumR2 = (int)this.FactVal2;
                //
                if (tmpFactVal3.IndexOf(',') > -1)
                {
                    sumR3 = int.Parse(tmpFactVal3.Substring(0, tmpFactVal3.IndexOf(',')));
                    sumK3 = int.Parse(tmpFactVal3.Substring(tmpFactVal3.IndexOf(',') + 1));
                }
                else sumR3 = (int)this.FactVal3;
                //
                if (tmpFactVal4.IndexOf(',') > -1)
                {
                    sumR4 = int.Parse(tmpFactVal4.Substring(0, tmpFactVal4.IndexOf(',')));
                    sumK4 = int.Parse(tmpFactVal4.Substring(tmpFactVal4.IndexOf(',') + 1));
                }
                else sumR4 = (int)this.FactVal4;
                //
                if (tmpFactVal5.IndexOf(',') > -1)
                {
                    sumR5 = int.Parse(tmpFactVal5.Substring(0, tmpFactVal5.IndexOf(',')));
                    sumK5 = int.Parse(tmpFactVal5.Substring(tmpFactVal5.IndexOf(',') + 1));
                }
                else sumR5 = (int)this.FactVal5;

                // Создаём временную таблицу
                DataTable TabTmpFactRub = new DataTable();
                //
                TabTmpFactRub.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowFactRub = TabTmpFactRub.NewRow();
                nrowFactRub["A"] = sumR1;
                TabTmpFactRub.Rows.Add(nrowFactRub);
                //
                nrowFactRub = TabTmpFactRub.NewRow();
                nrowFactRub["A"] = sumR2;
                TabTmpFactRub.Rows.Add(nrowFactRub);
                //
                nrowFactRub = TabTmpFactRub.NewRow();
                nrowFactRub["A"] = sumR3;
                TabTmpFactRub.Rows.Add(nrowFactRub);
                //
                nrowFactRub = TabTmpFactRub.NewRow();
                nrowFactRub["A"] = sumR4;
                TabTmpFactRub.Rows.Add(nrowFactRub);
                //
                nrowFactRub = TabTmpFactRub.NewRow();
                nrowFactRub["A"] = sumR5;
                TabTmpFactRub.Rows.Add(nrowFactRub);

                // Добавлем эту таблицу в наш класс
                Table TabFactRub0 = new Table("1|U29", TabTmpFactRub);
                TabL.Add(TabFactRub0, true);

                // Создаём временную таблицу
                DataTable TabTmpFactKop = new DataTable();
                //
                TabTmpFactKop.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowFactKop = TabTmpFactKop.NewRow();
                nrowFactKop["A"] = sumK1;
                TabTmpFactKop.Rows.Add(nrowFactKop);
                //
                nrowFactKop = TabTmpFactKop.NewRow();
                nrowFactKop["A"] = sumK2;
                TabTmpFactKop.Rows.Add(nrowFactKop);
                //
                nrowFactKop = TabTmpFactKop.NewRow();
                nrowFactKop["A"] = sumK3;
                TabTmpFactKop.Rows.Add(nrowFactKop);
                //
                nrowFactKop = TabTmpFactKop.NewRow();
                nrowFactKop["A"] = sumK4;
                TabTmpFactKop.Rows.Add(nrowFactKop);
                //
                nrowFactKop = TabTmpFactKop.NewRow();
                nrowFactKop["A"] = sumK5;
                TabTmpFactKop.Rows.Add(nrowFactKop);

                // Добавлем эту таблицу в наш класс
                Table TabFactKop0 = new Table("1|AP29", TabTmpFactKop);
                TabL.Add(TabFactKop0, true);

                //////////////////////////////////////

                // Получакем значение и преобразовываем к строке
                decimal ItogFact = this.FactVal1 + this.FactVal2 + this.FactVal3 + this.FactVal4 + this.FactVal5;
                string tmpItogFact = decimal.Round(ItogFact, 2).ToString().Replace(".", ",");

                // Создаём переменные для рублей и копеек
                int? sumRItogFact = null;
                int? sumKItogFact = null;

                // Проверка на наличие копеек
                if (tmpItogFact.IndexOf(',') > -1)
                {
                    sumRItogFact = int.Parse(tmpItogFact.Substring(0, tmpItogFact.IndexOf(',')));
                    sumKItogFact = int.Parse(tmpItogFact.Substring(tmpItogFact.IndexOf(',') + 1));
                }
                else sumRItogFact = (int)ItogFact;
                // Если вместо копеек нулл то заменяем на 0
                if (sumKItogFact == null) sumKItogFact = 0;

                // Создаём временную таблицу
                DataTable TabTmpItogFactRub = new DataTable();
                //
                TabTmpItogFactRub.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowItogFactRub = TabTmpItogFactRub.NewRow();
                nrowItogFactRub["A"] = sumRItogFact;
                TabTmpItogFactRub.Rows.Add(nrowItogFactRub);

                // Добавлем эту таблицу в наш класс
                Table TabItogFactRub0 = new Table("1|AH34", TabTmpItogFactRub);
                TabL.Add(TabItogFactRub0, true);

                // Создаём временную таблицу
                DataTable TabTmpItogFactKop = new DataTable();
                //
                TabTmpItogFactKop.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowItogFactKop = TabTmpItogFactKop.NewRow();
                nrowItogFactKop["A"] = sumKItogFact;
                TabTmpItogFactKop.Rows.Add(nrowItogFactKop);

                // Добавлем эту таблицу в наш класс
                Table TabItogFactKop0 = new Table("1|BM34", TabTmpItogFactKop);
                TabL.Add(TabItogFactKop0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpItogFactStr = new DataTable();
                //
                TabTmpItogFactStr.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowItogFactStr = TabTmpItogFactStr.NewRow();
                nrowItogFactStr["A"] = string.Format("{0} {1}",
                    Com.Utils.GetStringForInt((int)sumRItogFact, "рубль", "рубля", "рублей", false),
                    Com.Utils.GetStringForInt((int)sumKItogFact, "копейка", "копейки", "копеек", true).ToLower());
                TabTmpItogFactStr.Rows.Add(nrowItogFactStr);

                // Добавлем эту таблицу в наш класс
                Table TabItogFactStr0 = new Table("1|A36", TabTmpItogFactStr);
                TabL.Add(TabItogFactStr0, true);

                //////////////////////////////////////

                // Получакем значение и преобразовываем к строке
                string tmpItog = decimal.Round(this.ItogPoUchDan, 2).ToString().Replace(".", ",");

                // Создаём переменные для рублей и копеек
                int? sumRItog = null;
                int? sumKItog = null;

                // Проверка на наличие копеек
                if (tmpItog.IndexOf(',') > -1)
                {
                    sumRItog = int.Parse(tmpItog.Substring(0, tmpItog.IndexOf(',')));
                    sumKItog = int.Parse(tmpItog.Substring(tmpItog.IndexOf(',') + 1));
                }
                else sumRItog = (int)this.ItogPoUchDan;
                // Если вместо копеек нулл то заменяем на 0
                if (sumKItog == null) sumKItog = 0;

                // Создаём временную таблицу
                DataTable TabTmpItogRub = new DataTable();
                //
                TabTmpItogRub.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowItogRub = TabTmpItogRub.NewRow();
                nrowItogRub["A"] = sumRItog;
                TabTmpItogRub.Rows.Add(nrowItogRub);

                // Добавлем эту таблицу в наш класс
                Table TabItogRub0 = new Table("1|AC39", TabTmpItogRub);
                TabL.Add(TabItogRub0, true);

                // Создаём временную таблицу
                DataTable TabTmpItogKop = new DataTable();
                //
                TabTmpItogKop.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowItogKop = TabTmpItogKop.NewRow();
                nrowItogKop["A"] = sumKItog;
                TabTmpItogKop.Rows.Add(nrowItogKop);

                // Добавлем эту таблицу в наш класс
                Table TabItogKop0 = new Table("1|BM39", TabTmpItogKop);
                TabL.Add(TabItogKop0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpItogStr = new DataTable();
                //
                TabTmpItogStr.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowItogStr = TabTmpItogStr.NewRow();
                nrowItogStr["A"] = string.Format("{0} {1}" ,
                    Com.Utils.GetStringForInt((int)sumRItog, "рубль", "рубля", "рублей", false),
                    Com.Utils.GetStringForInt((int)sumKItog, "копейка", "копейки", "копеек", true).ToLower());
                TabTmpItogStr.Rows.Add(nrowItogStr);

                // Добавлем эту таблицу в наш класс
                Table TabItogStr0 = new Table("1|A41", TabTmpItogStr);
                TabL.Add(TabItogStr0, true);

                //////////////////////////////////////

                // Получакем значение и преобразовываем к строке
                bool? FlagIzlishek = null;
                decimal ItogPoUchDanDif=0;
                if (ItogFact > this.ItogPoUchDan)
                {
                    ItogPoUchDanDif = ItogFact - this.ItogPoUchDan;
                    FlagIzlishek = true;
                }
                if (ItogFact < this.ItogPoUchDan)
                {
                    ItogPoUchDanDif = this.ItogPoUchDan - ItogFact;
                    FlagIzlishek = false;
                }
                string tmpItogDif = decimal.Round(ItogPoUchDanDif, 2).ToString().Replace(".", ",");

                // Создаём переменные для рублей и копеек
                int? sumRItogDif = null;
                int? sumKItogDif = null;

                // Проверка на наличие копеек
                if (tmpItogDif.IndexOf(',') > -1)
                {
                    sumRItogDif = int.Parse(tmpItogDif.Substring(0, tmpItogDif.IndexOf(',')));
                    sumKItogDif = int.Parse(tmpItogDif.Substring(tmpItogDif.IndexOf(',') + 1));
                }
                else sumRItogDif = (int)ItogPoUchDanDif;

                // Создаём временную таблицу
                DataTable TabTmpItogDifRub = new DataTable();
                //
                TabTmpItogDifRub.Columns.Add(new DataColumn("A", typeof(string)));
                //
                DataRow nrowItogDifRub = TabTmpItogDifRub.NewRow();
                nrowItogDifRub["A"] = (FlagIzlishek == null?0:((bool)FlagIzlishek?sumRItogDif:0));
                TabTmpItogDifRub.Rows.Add(nrowItogDifRub);
                //
                nrowItogDifRub = TabTmpItogDifRub.NewRow();
                nrowItogDifRub["A"] = (FlagIzlishek == null ? 0 : (!(bool)FlagIzlishek ? sumRItogDif : 0));
                TabTmpItogDifRub.Rows.Add(nrowItogDifRub);

                // Добавлем эту таблицу в наш класс
                Table TabItogDifRub0 = new Table("1|AL44", TabTmpItogDifRub);
                TabL.Add(TabItogDifRub0, true);

                // Создаём временную таблицу
                DataTable TabTmpItogDifKop = new DataTable();
                //
                TabTmpItogDifKop.Columns.Add(new DataColumn("A", typeof(string)));
                //
                DataRow nrowItogDifKop = TabTmpItogDifKop.NewRow();
                nrowItogDifKop["A"] = (FlagIzlishek == null ? 0 : ((bool)FlagIzlishek ? sumKItogDif : 0)); 
                TabTmpItogDifKop.Rows.Add(nrowItogDifKop);
                //
                nrowItogDifKop = TabTmpItogDifKop.NewRow();
                nrowItogDifKop["A"] = (FlagIzlishek == null ? 0 : (!(bool)FlagIzlishek ? sumKItogDif : 0)); 
                TabTmpItogDifKop.Rows.Add(nrowItogDifKop);

                // Добавлем эту таблицу в наш класс
                Table TabItogDifKop0 = new Table("1|BM44", TabTmpItogDifKop);
                TabL.Add(TabItogDifKop0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpLastNum = new DataTable();
                //
                TabTmpLastNum.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowLastNum = TabTmpLastNum.NewRow();
                nrowLastNum["A"] = this.LastPrihodNum.ToString();
                TabTmpLastNum.Rows.Add(nrowLastNum);
                //
                nrowLastNum = TabTmpLastNum.NewRow();
                nrowLastNum["A"] = this.LastRashodNum.ToString();
                TabTmpLastNum.Rows.Add(nrowLastNum);

                // Добавлем эту таблицу в наш класс
                Table TabLastNum0 = new Table("1|AS46", TabTmpLastNum);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabLastNum0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKomissionStr1 = new DataTable();
                //
                TabTmpKomissionStr1.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKomissionStr1 = TabTmpKomissionStr1.NewRow();
                nrowKomissionStr1["A"] = this.KomissionStr1;
                TabTmpKomissionStr1.Rows.Add(nrowKomissionStr1);

                // Добавлем эту таблицу в наш класс
                Table TabKomissionStr1 = new Table("1|W48", TabTmpKomissionStr1);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabKomissionStr1, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKomissionStr2 = new DataTable();
                //
                TabTmpKomissionStr2.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKomissionStr2 = TabTmpKomissionStr2.NewRow();
                nrowKomissionStr2["A"] = this.KomissionStr2;
                TabTmpKomissionStr2.Rows.Add(nrowKomissionStr2);

                // Добавлем эту таблицу в наш класс
                Table TabKomissionStr2 = new Table("1|W50", TabTmpKomissionStr2);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabKomissionStr2, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKomissionStr3 = new DataTable();
                //
                TabTmpKomissionStr3.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKomissionStr3 = TabTmpKomissionStr3.NewRow();
                nrowKomissionStr3["A"] = this.KomissionStr3;
                TabTmpKomissionStr3.Rows.Add(nrowKomissionStr3);

                // Добавлем эту таблицу в наш класс
                Table TabKomissionStr3 = new Table("1|W52", TabTmpKomissionStr3);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabKomissionStr3, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKomissionStr4 = new DataTable();
                //
                TabTmpKomissionStr4.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKomissionStr4 = TabTmpKomissionStr4.NewRow();
                nrowKomissionStr4["A"] = this.KomissionStr4;
                TabTmpKomissionStr4.Rows.Add(nrowKomissionStr4);

                // Добавлем эту таблицу в наш класс
                Table TabKomissionStr4 = new Table("1|W54", TabTmpKomissionStr4);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabKomissionStr4, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKomissionDecode1 = new DataTable();
                //
                TabTmpKomissionDecode1.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKomissionDecode1 = TabTmpKomissionDecode1.NewRow();
                nrowKomissionDecode1["A"] = this.KomissionDecode1;
                TabTmpKomissionDecode1.Rows.Add(nrowKomissionDecode1);

                // Добавлем эту таблицу в наш класс
                Table TabKomissionDecode1 = new Table("1|BA48", TabTmpKomissionDecode1);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabKomissionDecode1, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKomissionDecode2 = new DataTable();
                //
                TabTmpKomissionDecode2.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKomissionDecode2 = TabTmpKomissionDecode2.NewRow();
                nrowKomissionDecode2["A"] = this.KomissionDecode2;
                TabTmpKomissionDecode2.Rows.Add(nrowKomissionDecode2);

                // Добавлем эту таблицу в наш класс
                Table TabKomissionDecode2 = new Table("1|BA50", TabTmpKomissionDecode2);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabKomissionDecode2, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKomissionDecode3 = new DataTable();
                //
                TabTmpKomissionDecode3.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKomissionDecode3 = TabTmpKomissionDecode3.NewRow();
                nrowKomissionDecode3["A"] = this.KomissionDecode3;
                TabTmpKomissionDecode3.Rows.Add(nrowKomissionDecode3);

                // Добавлем эту таблицу в наш класс
                Table TabKomissionDecode3 = new Table("1|BA52", TabTmpKomissionDecode3);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabKomissionDecode3, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpKomissionDecode4 = new DataTable();
                //
                TabTmpKomissionDecode4.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowKomissionDecode4 = TabTmpKomissionDecode4.NewRow();
                nrowKomissionDecode4["A"] = this.KomissionDecode4;
                TabTmpKomissionDecode4.Rows.Add(nrowKomissionDecode4);

                // Добавлем эту таблицу в наш класс
                Table TabKomissionDecode4 = new Table("1|BA54", TabTmpKomissionDecode4);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabKomissionDecode4, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpDolRukOrg = new DataTable();
                //
                TabTmpDolRukOrg.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowDolRukOrg = TabTmpDolRukOrg.NewRow();
                nrowDolRukOrg["A"] = Com.LocalFarm.CurLocalDepartament.DolRukOrg;
                TabTmpDolRukOrg.Rows.Add(nrowDolRukOrg);

                // Добавлем эту таблицу в наш класс
                Table TabDolRukOrg0 = new Table("2|A30", TabTmpDolRukOrg);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabDolRukOrg0, true);

                //////////////////////////////////////

                // Создаём временную таблицу
                DataTable TabTmpRukFio = new DataTable();
                //
                TabTmpRukFio.Columns.Add(new DataColumn("A", typeof(string)));
                DataRow nrowRukFio = TabTmpRukFio.NewRow();
                nrowRukFio["A"] = Com.LocalFarm.CurLocalDepartament.RukFio;
                TabTmpRukFio.Rows.Add(nrowRukFio);

                // Добавлем эту таблицу в наш класс
                Table TabRukFio0 = new Table("2|AW30", TabTmpRukFio);   // передаём индекс страницы (начинается с 1) и ячейку таблицы (её самый левый верхний угол) 
                TabL.Add(TabRukFio0, true);

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
                                            , string.Format("инвент_налич_{0}{1}{2}_{3}.xlsx"
                                                        , ((DateTime)base.UreDate).Year
                                                        , ((DateTime)base.UreDate).Month.ToString("00")
                                                        , ((DateTime)base.UreDate).Day.ToString("00")
                                                        , this.DocNum))
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
    }
}
