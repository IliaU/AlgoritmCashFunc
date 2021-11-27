using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;
using AlgoritmCashFunc.BLL.Lib;

namespace AlgoritmCashFunc.BLL
{
    /// <summary>
    /// Объект представляет из себя список операций доступных в системе
    /// </summary>
    public class OperationList: OperationBase.OperationBaseList
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public OperationList()
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

        /// <summary>
        /// Класс который наследует ферма чтобы получить доступ к закрытым методам этого класса
        /// </summary>
        public class OperationListFarmBase
        {
            /// <summary>
            /// Добавление компонента в список кастомный со списком операций
            /// </summary>
            /// <param name="OList">Кастомный список операций в который надо добавить элемент</param>
            /// <param name="O">Сама операция которую надо добавить</param>
            public static void AddOperationToList(OperationList OList, Operation O)
            {
                try
                {
                    OList.Add(O);
                }
                catch (Exception ex)
                {
                    ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации класса DocumentPrihod с ошибкой: ({0})", ex.Message));
                    Com.Log.EventSave(ae.Message, string.Format("{0}.AddOperationToList", "OperationList.OperationListFarmBase"), EventEn.Error);
                    throw ae;
                }

            }
        }
    }
}
