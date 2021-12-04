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
    public class Document : DocumentBase, DocumentInterface
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="DocFullName">Тип плагина</param>
        /// <param name="CurOperation">Операция к которой относится этот документ</param>
        /// <param name="LocalDebitor">Дебитор</param>
        /// <param name="LocalCreditor">Кредитор</param>
        /// <param name="IsDraft">Черновик</param>
        public Document(string DocFullName, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, bool IsDraft) :base(DocFullName, CurOperation, LocalDebitor, LocalCreditor, IsDraft)
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
