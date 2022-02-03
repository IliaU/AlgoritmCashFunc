using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;
using AlgoritmCashFunc.BLL.OperationPlg.Lib;

namespace AlgoritmCashFunc.BLL
{
    /// <summary>
    /// Представляет из себя список операций из базы данных с описанием
    /// </summary>
    public class Operation : OperationBase, OperationInterface
    {

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="OpFullName">Тип плагина</param>
        /// <param name="OperationName">Имя операции для ползователя</param>
        /// <param name="KoefDebitor">Дебитор коэфициент</param>
        /// <param name="KoefCreditor">Кредитор коэфициент</param>
        public Operation(string OpFullName, string OperationName, int KoefDebitor, int KoefCreditor) : base(OpFullName, OperationName, KoefDebitor, KoefCreditor)
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
        /// Для того чтобы плагин мог реализовать своё специфическое сохранение, который должны переписать наследуемые класы
        /// </summary>
        protected virtual void SaveChildron()
        {
            try
            {
                Com.Log.EventSave("В наследуемомо классе не переписан этот метод получается что если что-то надо было ему сохранить то он этого не сделал", string.Format("{0}.SaveChildron", GetType().Name), EventEn.Warning);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.SaveChildron", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Сохранить объект в базе данных
        /// </summary>
        public void Save()
        {
            try
            {
                // Если это новый документ то необходимо его создать в базе данных
                if (base.Id == null)
                {
                    // Вставляем новую запись и сохраняем идентификатор
                    int id = Com.ProviderFarm.CurrentPrv.SetOperation(this);
                    base.Id = id;
                }
                else // Обновление уже существующего Operation
                {
                    // Пробуем обновить в базе инфу в таблице Operation  вдруг пользователь например переименовал объект
                    Com.ProviderFarm.CurrentPrv.UpdateOperation(this);
                }

                // Запускаем сохранение в нашем дочернем плагине
                this.SaveChildron();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.Save", GetType().Name), EventEn.Error);
                throw ae;
            }
        }
    }
}
