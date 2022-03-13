using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmCashFunc.Lib
{
    /// <summary>
    /// Результат который получаем при подсчёте остатка и оборота на дату процедурой GetOstatokAndOborotForDay
    /// </summary>
    public class RezultForOstatokAndOborot
    {
        /// <summary>
        /// Остаток на начало дня
        /// </summary>
        public decimal Ostatok;

        /// <summary>
        /// Оборот на день
        /// </summary>
        public decimal Oborot;
    }
}
