using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmCashFunc.Lib
{
    public class EventUser : EventArgs
    {
        /// <summary>
        /// Пользователь
        /// </summary>
        public User Usr { get; private set; }

        /// <summary>
        /// Обрабатывать или нет
        /// </summary>
        public Boolean Action = true;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Usr">Пользователь</param>
        public EventUser(User Usr)
        {
            this.Usr = Usr;
        }
    }
}
