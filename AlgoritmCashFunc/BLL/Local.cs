using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;
using AlgoritmCashFunc.BLL.LocalPlg.Lib;

namespace AlgoritmCashFunc.BLL
{
    /// <summary>
    /// Класс для операций
    /// </summary>
    public class Local :LocalBase, LocalInterface
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="LocFullName">Тип плагина</param>
        /// <param name="LocalName">Имя в базе данных уникальное. Возможно sid</param>
        /// <param name="IsSeller">Роль поставщика</param>
        /// <param name="IsСustomer">Роль покупатнеля</param>
        /// <param name="IsDivision">Роль подразделения</param>
        /// <param name="IsDraft">Черновик</param>
        public Local(string LocFullName, string LocalName, bool IsSeller, bool IsСustomer, bool IsDivision, bool IsDraft) :base(LocFullName, LocalName, IsSeller, IsСustomer, IsDivision, IsDraft)
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
                // Проверка параметров
                if (string.IsNullOrWhiteSpace(base.LocalName)) throw new ApplicationException("Необходимо задать имя перед сохраненнием. Это обязательное поле.");

                // Если это новый локал то необходимо его создать в базе данных
                if (base.Id == null)
                {
                    // Предварительно проверяем на уникальность имя
                    if (Com.LocalFarm.CurLocalList[base.LocalName] != null) throw new ApplicationException("С таким именем Local уже существует. Это уникальное поле.");

                    // Вставляем новую запись и сохраняем идентификатор
                    int id = Com.ProviderFarm.CurrentPrv.SetLocal(this);
                    base.Id = id;
                }
                else // Обновление уже существующего Local
                {
                    Local LocTmp = Com.LocalFarm.CurLocalList[base.LocalName];
                    if (LocTmp != null && LocTmp.Id != null && LocTmp.Id != base.Id) new ApplicationException("С таким именем Local уже существует. Это уникальное поле.");

                    // Пробуем обновить в базе инфу в таблице Local  вдруг пользователь например переименовал объект
                    Com.ProviderFarm.CurrentPrv.UpdateLocal(this);
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
