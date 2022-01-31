using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmCashFunc.Lib
{   
    /// <summary>
    /// Статус кнопки
    /// </summary>
    public enum ButtonStatusEn
    {
        /// <summary>
        /// Активная и пользователь может на неё нажимать
        /// </summary>
        Active,

        /// <summary>
        /// Пассивная и для пользователя не должна реагировать на событие
        /// </summary>
        Passive
    }
}
