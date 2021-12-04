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
    public class DocumentPrihod : Document
    {
        // какой-то внутренний обьект
        public DateTime privObj;

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
        /// <param name="IsDraft">Черновик</param>
        /// <param name="IsProcessed">Проведённый документ или нет</param>
        public DocumentPrihod(int? Id, DateTime? UreDate, DateTime CteateDate, DateTime ModifyDate, string ModifyUser, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, bool IsDraft, bool IsProcessed) : base("DocumentPrihod", CurOperation, LocalDebitor, LocalCreditor, IsProcessed)
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
        public DocumentPrihod() : this(null, DateTime.Now.Date, DateTime.Now, DateTime.Now, Com.UserFarm.CurrentUser.Logon, Com.OperationFarm.CurOperationList["OperationProhod"], null, null, true, false)
        {
            try
            {
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
