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
    public class DocumentPrihod : DocumentBase.DocumentBaseInterface, DocumentInterface
    {
        // какой-то внутренний обьект
        public DateTime privObj;

        /// <summary>
        /// Конструктор
        /// </summary>
        public DocumentPrihod() : base(new Operation(100, "DocumentPrihod", "Приходный ордер"))
        {
            try
            {
                privObj = DateTime.Now;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации класса DocumentPrihod с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        public void SetTest(int Test)
        {

        }

    }
}
