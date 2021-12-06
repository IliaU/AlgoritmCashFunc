using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using AlgoritmCashFunc.BLL;

namespace AlgoritmCashFunc.Com.Provider.Lib
{
    /// <summary>
    /// Инитерфейс для всех провайдеров
    /// </summary>
    public interface ProviderI
    {
        /// <summary>
        /// Получение версии базы данных
        /// </summary>
        /// <returns>Возвращает версию базы данных в виде строки</returns>
        //string VersionDB();

        /// <summary>
        /// Процедура вызывающая настройку подключения
        /// </summary>
        /// <returns>Возвращает значение требуется ли сохранить подключение как основное или нет</returns>
        bool SetupConnectDB();

        /// <summary>
        /// Печать строки подключения с маскировкой секретных данных
        /// </summary>
        /// <returns>Строка подклюения с замасированной секретной информацией</returns>
        string PrintConnectionString();

        /// <summary>
        /// Получение списка операций из базы данных 
        /// </summary>
        /// <returns>Стандартный список операций</returns>
        OperationList GetOperationList();

        /// <summary>
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <returns>Получает текущий список Local из базы данных</returns>
        LocalList GetLocalListFromDB();

        /// <summary>
        /// Сохранение Local в базе
        /// </summary>
        /// <param name="NewLocal">Новый локал который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        int SetLocal(Local NewLocal);

        /// <summary>
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <param name="LastDay">Сколько последних дней грузить из базы данных если null значит весь период</param>
        /// <param name="OperationId">Какая операция нас интересует, если </param>
        /// <returns>Получает список Document из базы данных удовлетворяющий фильтрам</returns>
        DocumentList GetDocumentListFromDB(int? LastDay, int? OperationId);
    }
}
