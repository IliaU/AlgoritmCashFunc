using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.BLL_Prizm
{
    /// <summary>
    /// Строки документа из призма к чеку
    /// </summary>
    public class CheckItem
    {
        public string ProductName;

        public string Article;

        public string Size;

        public CheckItem(string ProductName, string Article, string Size)
        {
            try
            {
                this.ProductName = ProductName;
                this.Article = Article;
                this.Size = Size;
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
