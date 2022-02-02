using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.BLL.DocumentPlg.Lib;
using AlgoritmCashFunc.Lib;


namespace AlgoritmCashFunc.BLL
{
    /// <summary>
    /// Документ универсалльный который используем в программах
    /// </summary>
    public class Document : DocumentBase, DocumentInterface
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="DocFullName">Тип плагина</param>
        /// <param name="CurOperation">Операция к которой относится этот документ</param>
        /// <param name="LocalDebitor">Дебитор</param>
        /// <param name="LocalCreditor">Кредитор</param>
        /// <param name="DocNum"> Черновик</param>
        /// <param name="IsDraft">Черновик</param>
        public Document(string DocFullName, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, int DocNum, bool IsDraft) :base(DocFullName, CurOperation, LocalDebitor, LocalCreditor, DocNum, IsDraft)
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
                base.ModifyUser = Com.UserFarm.CurrentUser.Logon;
                base.ModifyDate = DateTime.Now;
                base.IsProcessed = false;

                // Если это новый документ то необходимо его создать в базе данных
                if (base.Id == null)
                {
                    base.CteateDate = base.ModifyDate;

                    // Вставляем новую запись и сохраняем идентификатор
                    int id = Com.ProviderFarm.CurrentPrv.SetDocument(this);
                    base.Id = id;
                }
                else // Обновление уже существующего Local
                {
                    // Пробуем обновить в базе инфу в таблице Local  вдруг пользователь например переименовал объект
                    Com.ProviderFarm.CurrentPrv.UpdateDocument(this);
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
