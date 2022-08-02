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
        /// <param name="Departament">Департамент или касса в которой создан документ</param>
        /// <param name="OtherDebitor">Дебитор который ввели вручную не из списка</param>
        /// <param name="OtherKreditor">Кредитор который ввели вручную не из списка</param>
        /// <param name="DocNum"> Черновик</param>
        /// <param name="IsDraft">Черновик</param>
        public Document(string DocFullName, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, Local Departament, string OtherDebitor, string OtherKreditor, int DocNum, bool IsDraft) :base(DocFullName, CurOperation, LocalDebitor, LocalCreditor, Departament, OtherDebitor, OtherKreditor, DocNum, IsDraft)
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
                base.IsDraft = false;

                // Если это новый документ то необходимо его создать в базе данных
                if (base.Id == null)
                {
                    if (base.CreateDate==null) base.CreateDate = base.ModifyDate;

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

                // Проверка надату. Если у текущего документа дата не сегодняшняя значит пользователь редактирует документ в прошлом нужно номера документов пересторить за текущий год и предыдущий и пересчитать остатки
                if (this.UreDate!=null && 
                    (((DateTime)this.UreDate).Date!=DateTime.Now.Date)
                    || this.DocFullName == "DocumentKasBook")
                {
                    Com.ProviderFarm.CurrentPrv.SetDocNumForYear(((DateTime)this.UreDate).AddYears(-1));
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.Save", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Для того чтобы плагин мог реализовать своё специфическое удаление, который должны переписать наследуемые класы
        /// </summary>
        protected virtual void DeleteChildron()
        {
            try
            {
                Com.Log.EventSave("В наследуемомо классе не переписан этот метод получается что если что-то надо было ему удалить то он этого не сделал", string.Format("{0}.SaveChildron", GetType().Name), EventEn.Warning);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.DeleteChildron", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Удаление документа со всеми его данными
        /// </summary>
        public void Delete()
        {
            try
            {
                if (base.Id == null) throw new ApplicationException("Удаление не возможно тку как документ ещё не был сохранён.");

                base.ModifyUser = Com.UserFarm.CurrentUser.Logon;
                base.ModifyDate = DateTime.Now;

                // Запускаем удаление в нашем дочернем плагине
                this.DeleteChildron();

                // Выставляем флаги уделения в базе данных
                Com.ProviderFarm.CurrentPrv.DeleteDocument(this);
                                
                // В прошлом нужно номера документов пересторить за текущий год и предыдущий и пересчитать остатки
                Com.ProviderFarm.CurrentPrv.SetDocNumForYear(this.UreDate);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.Delete", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Печать документа Title
        /// </summary>
        public virtual void PrintTitle()
        {
            try
            {
                Com.Log.EventSave(string.Format("В наследуемомо классе не предусмотрена печать документа ({0})", base.DocFullName), string.Format("{0}.PrintTitle", GetType().Name), EventEn.Warning);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.PrintTitle", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Печать документа TitleDefault
        /// </summary>
        public virtual void PrintDefault()
        {
            try
            {
                Com.Log.EventSave(string.Format("В наследуемомо классе не предусмотрена печать документа ({0})", base.DocFullName), string.Format("{0}.PrintDefaul", GetType().Name), EventEn.Warning);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.PrintDefault", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Экспорт документа в 1С
        /// </summary>
        public virtual void ExportTo1C()
        {
            try
            {
                Com.Log.EventSave(string.Format("В наследуемомо классе не предусмотрен экспорт документа в 1С ({0})",base.DocFullName), string.Format("{0}.ExportTo1C", GetType().Name), EventEn.Warning);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.ExportTo1C", GetType().Name), EventEn.Error);
                throw ae;
            }
        }
    }
}
