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
        public DocumentInvent(int? Id, DateTime? UreDate, DateTime CteateDate, DateTime ModifyDate, string ModifyUser, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, string OtherDebitor, string OtherKreditor, int DocNum, bool IsDraft, bool IsProcessed) : base("DocumentInvent", CurOperation, LocalDebitor, LocalCreditor, OtherDebitor, OtherKreditor, DocNum, IsProcessed)
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
        public DocumentInvent() : this(null, DateTime.Now.Date, DateTime.Now, DateTime.Now, Com.UserFarm.CurrentUser.Logon, Com.OperationFarm.CurOperationList["OperationInvent"], null, null, null, null, Com.LocalFarm.CurLocalDepartament.LastDocNumInvent+1, true, false)
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
    }
}
