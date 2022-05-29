using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.BLL_Prizm;

namespace AlgoritmCashFunc.Com.Provider.Lib
{
    /// <summary>
    /// Интерфейс для работы с призмоом в момент получения данных
    /// </summary>
    public interface ProviderPrizmI
    {
        /// <summary>
        /// Получаем документ по его номеру
        /// </summary>
        /// <param name="DocNumber">Номер документа</param>
        /// <returns>Документ</returns>
        Check GetCheck(int DocNumber);

        /// <summary>
        /// Установка номеров документа по правилам связанным с началом года
        /// </summary>
        void SetDocNumForYear();
    }
}
