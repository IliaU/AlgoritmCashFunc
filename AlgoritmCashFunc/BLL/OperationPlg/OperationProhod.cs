using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;
using AlgoritmCashFunc.BLL.OperationPlg.Lib;

namespace AlgoritmCashFunc.BLL.OperationPlg
{
    /// <summary>
    /// Представляет из себя Операцию
    /// </summary>
    public class OperationProhod:Operation
    {
        /// <summary>
        /// Конструктор для загрузки из базы данных
        /// </summary>
        /// <param name="Id">Идентификатор в базе данных</param>
        /// <param name="OperationName">Имя операции для ползователя</param>
        /// <param name="KoefDebitor">Дебитор коэфициент</param>
        /// <param name="KoefCreditor">Кредитор коэфициент</param>
        public OperationProhod(int? Id, string OperationName, int KoefDebitor, int KoefCreditor) : base("OperationProhod",  OperationName, KoefDebitor, KoefCreditor)
        {
            try
            {
                base.Id = Id;
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
        public OperationProhod() : this(null, "Приходный ордер", 1, 0)
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
