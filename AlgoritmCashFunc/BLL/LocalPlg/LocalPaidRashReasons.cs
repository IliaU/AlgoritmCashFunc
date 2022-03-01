using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.BLL.LocalPlg
{
    /// <summary>
    /// Основание для расхода
    /// </summary>
    public class LocalPaidRashReasons : Local
    {
        /// <summary>
        /// Основание
        /// </summary>
        public string Osnovanie;

        /// <summary>
        /// Дебет номер счёта
        /// </summary>
        public string KreditNomerSchet;

        /// <summary>
        /// Кредит номер счёта
        /// </summary>
        public string DebetKorSchet;

        /// <summary>
        /// Конструктор для загрузки из базы данных
        /// </summary>
        /// <param name="Id">Идентификатор в базе данных</param>
        /// <param name="LocalName">Имя из базы данных</param>
        /// <param name="IsSeller">Роль поставщика</param>
        /// <param name="IsСustomer">Роль покупателя</param>
        /// <param name="IsDivision">Роль подразделения или кассы</param>
        /// <param name="IsDraft">Черновик</param>
        public LocalPaidRashReasons(int? Id, string LocalName, bool IsSeller, bool IsСustomer, bool IsDivision, bool IsDraft) : base("LocalPaidRashReasons", LocalName, IsSeller, IsСustomer, IsDivision, IsDraft)
        {
            try
            {
                base.Id = Id;

                // Если документ читается из базы данных то нужно прочитать дополнительные параметры
                if (base.Id != null)
                {
                    LocalPaidRashReasons MyObj = this;
                    bool tt = Com.ProviderFarm.CurrentPrv.GetLocalPaidRashReasons(ref MyObj);
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
        public LocalPaidRashReasons():this(null, Guid.NewGuid().ToString(), false, false, false, true)
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
                if (Com.ProviderFarm.CurrentPrv.HashLocalPaidRashReasons(this))
                {
                    Com.ProviderFarm.CurrentPrv.UpdateLocalPaidRashReasons(this);
                }
                else  // Если нет то вставляем
                {
                    Com.ProviderFarm.CurrentPrv.SetLocalPaidRashReasons(this);
                }
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
