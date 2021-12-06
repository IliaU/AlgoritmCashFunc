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
                
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.SaveChildron", GetType().Name), EventEn.Error);
                throw ae;
            }
        }
    }
}
