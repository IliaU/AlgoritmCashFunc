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
    /// Класс для операций
    /// </summary>
    public class Local :LocalBase, LocalInterface
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="LocFullName">Тип плагина</param>
        /// <param name="LocalName">Имя в базе данных уникальное. Возможно sid</param>
        /// <param name="IsSeller">Роль поставщика</param>
        /// <param name="IsСustomer">Роль покупатнеля</param>
        /// <param name="IsDivision">Роль подразделения</param>
        /// <param name="IsDraft">Черновик</param>
        public Local(string LocFullName, string LocalName, bool IsSeller, bool IsСustomer, bool IsDivision, bool IsDraft) :base(LocFullName, LocalName, IsSeller, IsСustomer, IsDivision, IsDraft)
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
