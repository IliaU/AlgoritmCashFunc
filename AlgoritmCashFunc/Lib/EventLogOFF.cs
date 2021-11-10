using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Com;

namespace AlgoritmCashFunc.Lib
{
    public class EventLogOFF : EventArgs
    {
        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public User CurUser { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="CurUser">Текущий пользователь</param>
        public EventLogOFF(User CurUser)
        {
            try
            {
                this.CurUser = CurUser;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при загрузке конструктора с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, GetType().Name, EventEn.FatalError);
                throw ae;
            }
        }
    }
}
