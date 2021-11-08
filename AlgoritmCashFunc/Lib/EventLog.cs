using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmCashFunc.Lib
{
    public class EventLog : EventArgs
    {
        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Источник
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Тип собятия
        /// </summary>
        public EventEn Evn { get; private set; }

        /// <summary>
        /// Писать в лог
        /// </summary>
        public bool isLog;

        /// <summary>
        /// Показывать пользователю
        /// </summary>
        public bool Show;

        /// <summary>
        /// Конструктор собятия
        /// </summary>
        /// <param name="Message">Сообщение</param>
        /// <param name="Source">Источник</param>
        /// <param name="Evn">Тип собятия</param>
        /// <param name="isLog">Писать в лог</param>
        /// <param name="Show">Показывать пользователю</param>
        public EventLog(string Message, string Source, EventEn Evn, bool isLog, bool Show)
        {
            this.Message = Message;
            this.Source = Source;
            this.Evn = Evn;
            this.isLog = isLog;
            this.Show = Show;
        }
    }
}
