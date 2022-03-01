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
        /// <param name="CteateDate">Дата создания документа</param>
        /// <param name="ModifyDate">Дата изменеия документа</param>
        /// <param name="ModifyUser">Пользовтаель который изменил последний раз документ</param>
        /// <param name="CurOperation">Операция к которой относится этот документ</param>
        /// <param name="LocalDebitor">Дебитор</param>
        /// <param name="LocalCreditor">Кредитор</param>
        /// <param name="DocNum"> Черновик</param>
        /// <param name="IsDraft">Черновик</param>
        /// <param name="IsProcessed">Проведённый документ или нет</param>
        public DocumentRashod(int? Id, DateTime? UreDate, DateTime CteateDate, DateTime ModifyDate, string ModifyUser, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, int DocNum, bool IsDraft, bool IsProcessed) : base("DocumentRashod", CurOperation, LocalDebitor, LocalCreditor, DocNum, IsProcessed)
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
                    DocumentRashod MyObj = this;
                    //bool tt = Com.ProviderFarm.CurrentPrv.GetDocumentRashod(ref MyObj);
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
        public DocumentRashod() : this(null, DateTime.Now.Date, DateTime.Now, DateTime.Now, Com.UserFarm.CurrentUser.Logon, Com.OperationFarm.CurOperationList["OperationRashod"], null, null, Com.LocalFarm.CurLocalDepartament.LastDocNumPrih+1, true, false)
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
        /// Метод заставляет переписать родительский не новый поверз создаёт а переписывает. Для того чтобы плагин мог реализовать своё специфическое сохранение
        /// </summary>
        protected override void SaveChildron()
        {
            try
            {
 //               if (Com.ProviderFarm.CurrentPrv.HashDocumentRashod(this))
 //               {
//                    Com.ProviderFarm.CurrentPrv.UpdateDocumentRashod(this);
 //               }
//                else  // Если нет то вставляем
 //               {
//                    Com.ProviderFarm.CurrentPrv.SetDocumentRashod(this);
 //               }
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
