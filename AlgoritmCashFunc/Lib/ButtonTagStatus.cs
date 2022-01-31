using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Com;

namespace AlgoritmCashFunc.Lib
{
    /// <summary>
    /// Класс для отображения текущего статуса кнопок для того чтобы удобнее было ими управлять
    /// </summary>
    public class ButtonTagStatus
    {
        /// <summary>
        /// Текуший статус кнопки
        /// </summary>
        public ButtonStatusEn Stat;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Stat">Статус который присвоить кнопке по умолчанию</param>
        public ButtonTagStatus (ButtonStatusEn Stat)
        {
            try
            {
                this.Stat = Stat;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при загрузке класса ButtonTagStatus с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }
    }
}
