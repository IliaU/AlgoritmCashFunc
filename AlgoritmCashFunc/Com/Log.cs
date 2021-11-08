using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;
using System.Windows.Forms;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.Com
{
    /// <summary>
    /// Класс для записи в лог
    /// </summary>
    public class Log
    {
        #region Private Param
        private static Log obj = null;

        /// <summary>
        /// Количество попыток записи в лог
        /// </summary>
        private static int IOCountPoput = 5;

        /// <summary>
        /// Количество милесекунд мкжду попутками записи
        /// </summary>
        private static int IOWhileInt = 500;
        #endregion

        #region Public Param
        /// <summary>
        /// Файл в который будем сохранять лог
        /// </summary>
        public static string File { get; private set; }

        /// <summary>
        /// Возникновение события в приложении
        /// </summary>
        public static event EventHandler<EventLog> onEventLog;
        #endregion

        #region Public Method
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="FileLog">Имя файла для лога программы</param>
        public Log(string FileLog)
        {
            if (obj == null) File = "Log.txt";
            else File = FileLog;

            obj = this;

            EventSave("Запуск программы", GetType().Name, EventEn.Message);
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public Log()
            : this(null)
        { }


        /// <summary>
        /// Событие программы
        /// </summary>
        /// <param name="Message">Сообщение</param>
        /// <param name="Source">Источник</param>
        /// <param name="evn">Тип события</param>
        public static void EventSave(string Message, string Source, EventEn evn)
        {
            EventSave(File, Message, Source, evn, true, false);
        }

        /// <summary>
        /// Событие программы
        /// </summary>
        /// <param name="Message">Сообщение</param>
        /// <param name="Source">Источник</param>
        /// <param name="evn">Тип события</param>
        /// <param name="IsLog">Писать в лог или нет</param>
        /// <param name="Show">Отобразить сообщение пользователю или нет</param>
        public static void EventSave(string Message, string Source, EventEn evn, bool IsLog, bool Show)
        {
            EventSave(File, Message, Source, evn, IsLog, Show);
        }

        /// <summary>
        /// Событие программы
        /// </summary>
        /// <param name="FileName">Имя файла</param>
        /// <param name="Message">Сообщение</param>
        /// <param name="Source">Источник</param>
        /// <param name="evn">Тип события</param>
        public static void EventSave(string FileName, string Message, string Source, EventEn evn)
        {
            EventSave(FileName, Message, Source, evn, true, false);
        }


        /// <summary>
        /// Событие программы
        /// </summary>
        /// <param name="FileName">Имя файла</param>
        /// <param name="Message">Сообщение</param>
        /// <param name="Source">Источник</param>
        /// <param name="evn">Тип события</param>
        /// <param name="IsLog">Писать в лог или нет</param>
        /// <param name="Show">Отобразить сообщение пользователю или нет</param>
        public static void EventSave(string FileName, string Message, string Source, EventEn evn, bool IsLog, bool Show)
        {
            if (obj == null) throw new ApplicationException("Класс Log ещё не инициирован. Сначала запустите конструктор а потом используйте методы");

            lock (obj)
            {
                EventLog myArg = new EventLog(Message, Source, evn, IsLog, Show);
                if (onEventLog != null)
                {
                    onEventLog.Invoke(obj, myArg);
                }

                if ((evn == EventEn.Dump && Config.Trace)
                    || (evn != EventEn.Dump && IsLog))
                {
                    EventSave(File, Message, Source, evn, IOCountPoput);
                }

                if (Show)
                {
                    MessageBox.Show(Message);
                }
            }
        }

        #endregion

        #region Private Method

        /// <summary>
        /// Метод для записи информации в лог
        /// </summary>
        /// <param name="FileName">Файл в котрый пишем лог</param>
        /// <param name="Message">Сообщение</param>
        /// <param name="Source">Источник</param>
        /// <param name="evn">Тип события</param>
        /// <param name="IOCountPoput">Количество попыток записи в лог</param>
        private static void EventSave(string FileName, string Message, string Source, EventEn evn, int IOCountPoput)
        {
            try
            {
                lock (obj)
                {
                    string newFile = (string.IsNullOrWhiteSpace(FileName) ? File : FileName);

                    using (StreamWriter SwFileLog = new StreamWriter(Environment.CurrentDirectory + @"\" + newFile, true))
                    {
                        SwFileLog.WriteLine(DateTime.Now.ToString() + "\t" + evn.ToString() + "\t" + Source + "\t" + Message);
                    }
                }
            }
            catch (Exception)
            {
                if (IOCountPoput > 0)
                {
                    Thread.Sleep(IOWhileInt);
                    EventSave(FileName, Message, Source, evn, IOCountPoput - 1);
                }
                else throw;
            }
        }

        #endregion
    }
}
