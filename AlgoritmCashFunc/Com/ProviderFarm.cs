using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.Com
{
    /// <summary>
    /// Текущий провайдер доступный по умолчанию
    /// </summary>
    public static class ProviderFarm
    {
        /// <summary>
        /// Текущий провайдер
        /// </summary>
        private static UProvider _CurrentPrv;

        /// <summary>
        /// Текущий провайдер
        /// </summary>
        public static UProvider CurrentPrv
        {
            get
            {
                return _CurrentPrv;
            }
            private set
            {
                _CurrentPrv = value;
            }
        }

        /// <summary>
        /// Событие изменения текущего универсального провайдера
        /// </summary>
        public static event EventHandler<EventProviderFarm> onEventSetup;

        /// <summary>
        /// Доступно ли подключение или нет
        /// </summary>
        /// <returns>true Если смогли подключиться к базе данных</returns>
        public static bool HashConnect()
        {
            bool rez = false;

            // проверяем наличие подключения в системе
            if (_CurrentPrv != null && _CurrentPrv.VersionDB != null && _CurrentPrv.VersionDB.Trim() != string.Empty)
            {
                rez = _CurrentPrv.HashConnect;
            }

            return rez;
        }

        /// <summary>
        /// Получаем список доступных провайдеров
        /// </summary>
        /// <returns>Список имён доступных провайдеров</returns>
        public static List<string> ListProviderName()
        {
            return UProvider.ListProviderName();
        }

        /// <summary>
        /// Установка нового провайдера
        /// </summary>
        /// <param name="Uprov">Универсальный провайдер класса Lib.UProvider</param>
        /// <param name="WriteLog">Запись в лог</param>
        public static void Setup(UProvider Uprov, bool WriteLog)
        {
            // Собственно обработка события
            EventProviderFarm myArg = new EventProviderFarm(Uprov);
            if (onEventSetup != null)
            {
                onEventSetup.Invoke(Uprov, myArg);
            }

            // Логируем изменение подключения
            _CurrentPrv = Uprov;
            if (WriteLog) Log.EventSave(string.Format("Пользователь настроил новое подключениe: {0} ({1})", Uprov.PrintConnectionString(), Uprov.PrvInType), "ProviderFarm", EventEn.Message);
        }
        //
        /// <summary>
        /// Установка нового провайдера
        /// </summary>
        /// <param name="Uprov">Универсальный провайдер класса Lib.UProvider</param>
        public static void Setup(UProvider Uprov)
        {
            Setup(Uprov, true);
        }

        /// <summary>
        /// Получение нового экземпляра универсального провайдера
        /// </summary>
        /// <returns></returns>
        public static UProvider GetUprovider()
        {
            try
            {
                return new UProvider(_CurrentPrv.PrvInType, _CurrentPrv.ConnectionString);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
