using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmCashFunc.Lib
{
    /// <summary>
    /// Класс для конверсации cобытий из строк в энумератор
    /// </summary>
    public static class EventConvertor
    {
        /// <summary>
        /// Конвертация в объект eventEn
        /// </summary>
        /// <param name="EventStr">Строка которую надо конвертнуть</param>
        /// <param name="DefaulfEvent">Если не можем конвертнуть что в этом случае вернуть</param>
        /// <returns></returns>
        public static EventEn Convert(string EventStr, EventEn DefaulfEvent)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(EventStr))
                {
                    foreach (EventEn item in EventEn.GetValues(typeof(EventEn)))
                    {
                        if (item.ToString().ToUpper() == EventStr.Trim().ToUpper()) return item;
                    }
                }
                return DefaulfEvent;
            }
            catch (Exception)
            {
                return DefaulfEvent;
            }
        }

        /// <summary>
        /// Конвертация в RoleEn
        /// </summary>
        /// <param name="Role">Роль указанная в виде строки</param>
        /// <param name="DefaultRole">Роль которую вернуть в случае невозможности отпарсить её</param>
        /// <returns>Возвращает костамизированную роль в которой может работает пользователь</returns>
        public static RoleEn Convert(string Role, RoleEn DefaultRole)
        {
            if (Role != null && Role.Trim() != string.Empty)
            {
                foreach (RoleEn item in RoleEn.GetValues(typeof(RoleEn)))
                {
                    if (item.ToString() == Role.Trim()) return item;
                }
            }
            return DefaultRole;
        }
    }
}
