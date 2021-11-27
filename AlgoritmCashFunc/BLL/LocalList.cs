using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;
using AlgoritmCashFunc.BLL.LocalPlg.Lib;

namespace AlgoritmCashFunc.BLL
{
    /// <summary>
    /// Представляет из себя список документов
    /// </summary>
    public class LocalList: LocalBase.LocalBaseList
    {

        /// <summary>
        /// Конструктор
        /// </summary>
        public LocalList()
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
