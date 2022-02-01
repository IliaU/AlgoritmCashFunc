using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.BLL.LocalPlg
{
    /// <summary>
    /// Это касса
    /// </summary>
    public sealed class LocalKassa : Local
    {
        /// <summary>
        /// Имя хоста к которому привязана текущая локаль
        /// </summary>
        public string HostName { get; private set; } = Environment.MachineName;

        /// <summary>
        /// Организация например OOO "ГУЧЧИ РУС"
        /// </summary>
        public string Organization;

        /// <summary>
        /// Структурное подразделение например ГУЧЧИ Художественный
        /// </summary>
        public string StructPodrazdelenie;

        /// <summary>
        /// ОКПО (Общероссийский классификатор предприятий и организаций) например 17608361
        /// </summary>
        public string OKPO;

        /// <summary>
        /// Последний зарегистрированный номер документа "Приходный ордер"
        /// </summary>
        public int LastDocNumPrih;

        /// <summary>
        /// Последний зарегистрированный номер документа "Расходный ордер"
        /// </summary>
        public int LastDocNumRash;

        /// <summary>
        /// Последний зарегистрированный номер документа "Кассовая книга"
        /// </summary>
        public int LastDocNumKasBook;

        /// <summary>
        /// Последний зарегистрированный номер документа "Акт о возврате денег"
        /// </summary>
        public int LastDocNumActVozv;

        /// <summary>
        /// Последний зарегистрированный номер документа "Отчёт кассира"
        /// </summary>
        public int LastDocNumReportKas;

        /// <summary>
        /// Последний зарегистрированный номер документа "Счётчики ККМ"
        /// </summary>
        public int LastDocNumScetKkm;

        /// <summary>
        /// Последний зарегистрированный номер документа "Проверка наличных"
        /// </summary>
        public int LastDocNumVerifNal;

        /// <summary>
        /// Последний зарегистрированный номер документа "Инвентаризация средств"
        /// </summary>
        public int LastDocNumInvent;

        /// <summary>
        /// ИНН
        /// </summary>
        public string INN;

        /// <summary>
        /// Заводской номер ККМ
        /// </summary>
        public string ZavodKKM;

        /// <summary>
        /// Регистрационный номер ККМ
        /// </summary>
        public string RegKKM;

        /// <summary>
        /// Главный бухгалтер
        /// </summary>
        public string GlavBuhFio;

        /// <summary>
        /// Контрольно кассовая машина
        /// </summary>
        public string KkmName;

        /// <summary>
        /// Должность руководителя организации
        /// </summary>
        public string DolRukOrg;

        /// <summary>
        /// ФИО руководителя
        /// </summary>
        public string RukFio;

        /// <summary>
        /// ФИО Заведующий подразделением
        /// </summary>
        public string ZavDivisionFio;

        /// <summary>
        /// Конструктор для загрузки из базы данных
        /// </summary>
        /// <param name="Id">Идентификатор в базе данных</param>
        /// <param name="LocalName">Имя из базы данных</param>
        /// <param name="IsSeller">Роль поставщика</param>
        /// <param name="IsСustomer">Роль покупателя</param>
        /// <param name="IsDivision">Роль подразделения или кассы</param>
        /// <param name="IsDraft">Черновик</param>
        public LocalKassa(int? Id, string LocalName, bool IsSeller, bool IsСustomer, bool IsDivision, bool IsDraft) : base("LocalKassa", LocalName, IsSeller, IsСustomer, IsDivision, IsDraft)
        {
            try
            {
                base.Id = Id;

                // Если документ читается из базы данных то нужно прочитать дополнительные параметры
                if(base.Id != null)
                {
                    LocalKassa MyObj = this;
                    bool tt = Com.ProviderFarm.CurrentPrv.GetLocalKassa(ref MyObj);
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации конструктора с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }
        //
        /// <summary>
        /// Конструктор
        /// </summary>
        public LocalKassa():this(null, Guid.NewGuid().ToString(), false, false, true, true)
        {
            try
            {
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации конструктора с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Метод заставляет переписать родительский не новый поверз создаёт а переписывает. Для того чтобы плагин мог реализовать своё специфическое сохранение
        /// </summary>
        protected override void SaveChildron()
        {
            try
            {
                if (Com.ProviderFarm.CurrentPrv.HashLocalKassa(this))
                {
                    Com.ProviderFarm.CurrentPrv.UpdateLocalKassa(this);
                }
                else  // Если нет то вставляем
                {
                    Com.ProviderFarm.CurrentPrv.SetLocalKassa(this);
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.SaveChildron", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// специальный класс для провайдера чтобы он мог править закрытые свойства
        /// </summary>
        public class LocalKassaForProviderInterface
        {
            /// <summary>
            /// Установка значения в поле HostHame
            /// </summary>
            /// <param name="LocKassa"></param>
            /// <param name="NewHostHame"></param>
            public void SetHostName(LocalKassa LocKassa, string NewHostHame)
            {
                try
                {
                    LocKassa.HostName = NewHostHame;
                }
                catch (Exception ex)
                {
                    ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                    Com.Log.EventSave(ae.Message, string.Format("{0}.SetHostName", GetType().Name), EventEn.Error);
                    throw ae;
                }
            }
        }
    }
}
