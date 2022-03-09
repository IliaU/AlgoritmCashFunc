using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;
using AlgoritmCashFunc.BLL.OperationPlg.Lib;

namespace AlgoritmCashFunc.BLL.OperationPlg
{
    /// <summary>
    /// Представляет из себя Операцию
    /// </summary>
    public sealed class OperationKasBook : Operation
    {
        /// <summary>
        /// ОКУД
        /// </summary>
        public string OKUD;

        /// <summary>
        /// Конструктор для загрузки из базы данных
        /// </summary>
        /// <param name="Id">Идентификатор в базе данных</param>
        /// <param name="OperationName">Имя операции для ползователя</param>
        /// <param name="KoefDebitor">Дебитор коэфициент</param>
        /// <param name="KoefCreditor">Кредитор коэфициент</param>
        public OperationKasBook(int? Id, string OperationName, int KoefDebitor, int KoefCreditor) : base("OperationKasBook",  OperationName, KoefDebitor, KoefCreditor)
        {
            try
            {
                base.Id = Id;

                // Если документ читается из базы данных то нужно прочитать дополнительные параметры
                if (base.Id != null)
                {
                    OperationKasBook MyObj = this;
                    bool tt = Com.ProviderFarm.CurrentPrv.GetOperationKasBook(ref MyObj);
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
        public OperationKasBook() : this(null, "Кассовая книга", 0, 0)
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
                if (Com.ProviderFarm.CurrentPrv.HashOperationKasBook(this))
                {
                    Com.ProviderFarm.CurrentPrv.UpdateOperationKasBook(this);
                }
                else  // Если нет то вставляем
                {
                    Com.ProviderFarm.CurrentPrv.SetOperationKasBook(this);
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
