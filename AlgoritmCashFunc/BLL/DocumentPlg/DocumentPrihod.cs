using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.BLL.DocumentPlg.Lib;
using AlgoritmCashFunc.Lib;

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
        /// Дебитор который ввели вручную не из списка
        /// </summary>
        public string OtherDebitor;

        /// <summary>
        /// Кредитор который ввели вручную не из списка
        /// </summary>
        public string OtherKreditor;

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
        /// <param name="DocNum"> Черновик</param>
        /// <param name="IsDraft">Черновик</param>
        /// <param name="IsProcessed">Проведённый документ или нет</param>
        public DocumentPrihod(int? Id, DateTime? UreDate, DateTime CteateDate, DateTime ModifyDate, string ModifyUser, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, int DocNum, bool IsDraft, bool IsProcessed) : base("DocumentPrihod", CurOperation, LocalDebitor, LocalCreditor, DocNum, IsProcessed)
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
        public DocumentPrihod() : this(null, DateTime.Now.Date, DateTime.Now, DateTime.Now, Com.UserFarm.CurrentUser.Logon, Com.OperationFarm.CurOperationList["OperationPrihod"], null, null, Com.LocalFarm.CurLocalDepartament.LastDocNumPrih+1, true, false)
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
    }
}
