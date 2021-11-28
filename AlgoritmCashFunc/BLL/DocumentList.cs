using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.BLL.DocumentPlg.Lib;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.BLL
{
    /// <summary>
    /// Список документов
    /// </summary>
    public class DocumentList:DocumentBase.DocumentBaseList
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public DocumentList()
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
