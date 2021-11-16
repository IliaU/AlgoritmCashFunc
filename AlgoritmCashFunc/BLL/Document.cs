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
    /// Документ универсалльный который используем в программах
    /// </summary>
    public class Document : DocumentBase
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public Document()
        {
            try
            {

            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации класса DocumentPrihod с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }
    }
}
