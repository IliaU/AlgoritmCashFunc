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
        /// Сохранение Operation в базе
        /// </summary>
        /// <param name="NewOperation">Новый Operation который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        int SetOperation(Operation NewOperation);

        /// <summary>
        /// Обновление Operation в базе
        /// </summary>
        /// <param name="UpdOperation">Обновляемый Operation</param>
        void UpdateOperation(Operation UpdOperation);

        /// <summary>
        /// Проверка наличия информации объекта OperationPrihod
        /// </summary>
        /// <param name="OperationPrihod">Объект OperationPrihod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        bool HashOperationPrihod(BLL.OperationPlg.OperationPrihod OperationPrihod);

        /// <summary>
        /// Читаем информацию по объекту OperationPrihod
        /// </summary>
        /// <param name="OperationPrihod">Объект OperationPrihod который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли прочитать объект или нет</returns>
        bool GetOperationPrihod(ref BLL.OperationPlg.OperationPrihod OperationPrihod);

        /// <summary>
        /// Вставка новой информации в объект OperationPrihod
        /// </summary>
        /// <param name="NewOperationPrihod">Вставляем в базу информацию по объекту OperationPrihod</param>
        void SetOperationPrihod(BLL.OperationPlg.OperationPrihod NewOperationPrihod);

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationPrihod
        /// </summary>
        /// <param name="UpdOperationPrihod">Сам объект данные которого нужно обновить</param>
        void UpdateOperationPrihod(BLL.OperationPlg.OperationPrihod UpdOperationPrihod);

        /// <summary>
        /// Проверка наличия информации объекта OperationRashod
        /// </summary>
        /// <param name="OperationRashod">Объект OperationRashod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        bool HashOperationRashod(BLL.OperationPlg.OperationRashod OperationRashod);

        /// <summary>
        /// Читаем информацию по объекту OperationRashod
        /// </summary>
        /// <param name="OperationRashod">Объект OperationRashod который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли прочитать объект или нет</returns>
        bool GetOperationRashod(ref BLL.OperationPlg.OperationRashod OperationRashod);

        /// <summary>
        /// Вставка новой информации в объект OperationRashod
        /// </summary>
        /// <param name="NewOperationRashod">Вставляем в базу информацию по объекту OperationRashod</param>
        void SetOperationRashod(BLL.OperationPlg.OperationRashod NewOperationRashod);

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationRashod
        /// </summary>
        /// <param name="UpdOperationRashod">Сам объект данные которого нужно обновить</param>
        void UpdateOperationRashod(BLL.OperationPlg.OperationRashod UpdOperationRashod);

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
        /// Обновление Local в базе
        /// </summary>
        /// <param name="UpdLocal">Обновляемый локал</param>
        void UpdateLocal(Local UpdLocal);

        /// <summary>
        /// Проверка наличия информации объекта LocalKassa
        /// </summary>
        /// <param name="LocalKassa">Объект LocalKassa который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        bool HashLocalKassa(BLL.LocalPlg.LocalKassa LocalKassa);

        /// <summary>
        /// Читаем информацию по объекту LocalKassa
        /// </summary>
        /// <param name="LocalKassa">Объект LocalKassa который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли прочитать объект или нет</returns>
        bool GetLocalKassa(ref BLL.LocalPlg.LocalKassa LocalKassa);

        /// <summary>
        /// Вставка новой информации в объект LocalKassa
        /// </summary>
        /// <param name="NewLocalKassa">Вставляем в базу информацию по объекту LocalKassa</param>
        void SetLocalKassa(BLL.LocalPlg.LocalKassa NewLocalKassa);

        /// <summary>
        /// Обновляем в базе данных инфу по объекту LocalKassa
        /// </summary>
        /// <param name="UpdLocalKassa">Сам объект данные которого нужно обновить</param>
        void UpdateLocalKassa(BLL.LocalPlg.LocalKassa UpdLocalKassa);

        /// <summary>
        /// Проверка наличия информации объекта PaidInReasons
        /// </summary>
        /// <param name="LocalPaidInReasons">Объект LocalKassa который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        bool HashLocalPaidInReasons(BLL.LocalPlg.LocalPaidInReasons LocalPaidInReasons);

        /// <summary>
        /// Читаем информацию по объекту PaidInReasons
        /// </summary>
        /// <param name="LocalPaidInReasons">Объект PaidInReasons который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли прочитать объект или нет</returns>
        bool GetLocalPaidInReasons(ref BLL.LocalPlg.LocalPaidInReasons LocalPaidInReasons);

        /// <summary>
        /// Вставка новой информации в объект PaidInReasons
        /// </summary>
        /// <param name="NewLocalPaidInReasons">Вставляем в базу информацию по объекту LocalKassa</param>
        void SetLocalPaidInReasons(BLL.LocalPlg.LocalPaidInReasons NewLocalPaidInReasons);

        /// <summary>
        /// Обновляем в базе данных инфу по объекту PaidInReasons
        /// </summary>
        /// <param name="UpdLocalPaidInReasons">Сам объект данные которого нужно обновить</param>
        void UpdateLocalPaidInReasons(BLL.LocalPlg.LocalPaidInReasons UpdLocalPaidInReasons);

        /// <summary>
        /// Проверка наличия информации объекта PaidRashReasons
        /// </summary>
        /// <param name="LocalPaidRashReasons">Объект LocalKassa который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        bool HashLocalPaidRashReasons(BLL.LocalPlg.LocalPaidRashReasons LocalPaidRashReasons);

        /// <summary>
        /// Читаем информацию по объекту PaidRashReasons
        /// </summary>
        /// <param name="LocalPaidRashReasons">Объект PaidRashReasons который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли прочитать объект или нет</returns>
        bool GetLocalPaidRashReasons(ref BLL.LocalPlg.LocalPaidRashReasons LocalPaidRashReasons);

        /// <summary>
        /// Вставка новой информации в объект PaidRashnReasons
        /// </summary>
        /// <param name="NewLocalPaidRashReasons">Вставляем в базу информацию по объекту LocalKassa</param>
        void SetLocalPaidRashReasons(BLL.LocalPlg.LocalPaidRashReasons NewLocalPaidRashReasons);

        /// <summary>
        /// Обновляем в базе данных инфу по объекту PaidRashReasons
        /// </summary>
        /// <param name="UpdLocalPaidRashReasons">Сам объект данные которого нужно обновить</param>
        void UpdateLocalPaidRashReasons(BLL.LocalPlg.LocalPaidRashReasons UpdLocalPaidRashReasons);

        /// <summary>
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <param name="LastDay">Сколько последних дней грузить из базы данных если null значит весь период</param>
        /// <param name="OperationId">Какая операция нас интересует, если </param>
        /// <returns>Получает список Document из базы данных удовлетворяющий фильтрам</returns>
        DocumentList GetDocumentListFromDB(int? LastDay, int? OperationId);

        /// <summary>
        /// Сохранение Document в базе
        /// </summary>
        /// <param name="NewDocument">Новый документ который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        int SetDocument(Document NewDocument);

        /// <summary>
        /// Обновление Document в базе
        /// </summary>
        /// <param name="UpdDocument">Обновляемый документ</param>
        void UpdateDocument(Document UpdDocument);

        /// <summary>
        /// Проверка наличия информации объекта DocumentPrihod
        /// </summary>
        /// <param name="DocumentPrihod">Объект DocumentPrihod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        bool HashDocumentPrihod(BLL.DocumentPlg.DocumentPrihod DocumentPrihod);

        /// <summary>
        /// Читаем информацию по объекту DocumentPrihod
        /// </summary>
        /// <param name="DocumentPrihod">Объект DocumentPrihod который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли прочитать объект или нет</returns>
        bool GetDocumentPrihod(ref BLL.DocumentPlg.DocumentPrihod DocumentPrihod);

        /// <summary>
        /// Вставка новой информации в объект DocumentPrihod
        /// </summary>
        /// <param name="NewDocumentPrihod">Вставляем в базу информацию по объекту DocumentPrihod</param>
        void SetDocumentPrihod(BLL.DocumentPlg.DocumentPrihod NewDocumentPrihod);

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentPrihod
        /// </summary>
        /// <param name="UpdDocumentPrihod">Сам объект данные которого нужно обновить</param>
        void UpdateDocumentPrihod(BLL.DocumentPlg.DocumentPrihod UpdDocumentPrihod);
    }
}
