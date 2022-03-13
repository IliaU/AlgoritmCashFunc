using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.BLL.DocumentPlg.Lib;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.BLL.DocumentPlg
{
    /// <summary>
    /// Класс который представляет из себя докимент расхода
    /// </summary>
    public sealed class DocumentKasBook : Document
    {
        /// <summary>
        /// Сумма докмента на начало дня
        /// </summary>
        public decimal SummaStartDay;

        /// <summary>
        /// Сумма докмента на начало дня
        /// </summary>
        public decimal SummaEndDay;
        
        /// <summary>
        /// Должность руководителя
        /// </summary>
        public string DolRukFio;

        /// <summary>
        /// Руководитель
        /// </summary>
        public string RukFio;

        /// <summary>
        /// Главный бухгалтер
        /// </summary>
        public string GlavBuh;

        /// <summary>
        /// Значение которое говорит что в базе либо значение на даты начала и конца не сохранениы либо не валидны если True значит проверка совпала с тем что сохранено в базе
        /// </summary>
        public bool SaveValueNotValid = false;

        /// <summary>
        /// Конструктор для загрузки из базы данных
        /// </summary>
        /// <param name="Id">Идентификатор в базе данных</param>
        /// <param name="UreDate">Дата создания документа</param>
        /// <param name="CteateDate">Дата создания документа</param>
        /// <param name="ModifyDate">Дата изменеия документа</param>
        /// <param name="ModifyUser">Пользовтаель который изменил последний раз документ</param>
        /// <param name="CurOperation">Операция к которой относится этот документ</param>
        /// <param name="LocalDebitor">Дебитор</param>
        /// <param name="LocalCreditor">Кредитор</param>
        /// <param name="DocNum"> Черновик</param>
        /// <param name="IsDraft">Черновик</param>
        /// <param name="IsProcessed">Проведённый документ или нет</param>
        public DocumentKasBook(int? Id, DateTime? UreDate, DateTime CteateDate, DateTime ModifyDate, string ModifyUser, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, int DocNum, bool IsDraft, bool IsProcessed) : base("DocumentKasBook", CurOperation, LocalDebitor, LocalCreditor, DocNum, IsProcessed)
        {
            try
            {
                base.Id = Id;
                base.UreDate = UreDate;
                base.CteateDate = CteateDate;
                base.ModifyDate = ModifyDate;
                base.ModifyUser = ModifyUser;
                base.IsDraft = IsDraft;
                base.IsProcessed = IsProcessed;


                // На выбранную дату нужно получить остаток на начало даты и оборот на конец даты
                RezultForOstatokAndOborot OborotForDay = Com.ProviderFarm.CurrentPrv.GetOstatokAndOborotForDay((UreDate == null ? DateTime.Now : (DateTime)UreDate));

                // Если документ читается из базы данных то нужно прочитать дополнительные параметры
                if (base.Id != null)
                {
                    DocumentKasBook MyObj = this;
                    bool tt = Com.ProviderFarm.CurrentPrv.GetDocumentKasBook(ref MyObj);
                }

                // Сравниваем если данные в документе не равены данным в базе при пересчёте то нужно выставить влаг валидности в False и поправить сумму в документе чтобы пользователь сохранил изменения в базу и не забыл
                if (this.SummaStartDay != OborotForDay.Ostatok || this.SummaEndDay != (OborotForDay.Ostatok + OborotForDay.Oborot))
                {
                    this.SaveValueNotValid = false;
                    this.SummaStartDay = OborotForDay.Ostatok;
                    this.SummaEndDay = (OborotForDay.Ostatok + OborotForDay.Oborot);
                }
                else this.SaveValueNotValid = true;
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
        public DocumentKasBook() : this(null, DateTime.Now.Date, DateTime.Now, DateTime.Now, Com.UserFarm.CurrentUser.Logon, Com.OperationFarm.CurOperationList["OperationKasBook"], null, null, Com.LocalFarm.CurLocalDepartament.LastDocNumKasBook + 1, true, false)
        {
            try
            {
                this.CurOperation = Com.OperationFarm.CurOperationList["OperationKasBook"];

                // Документ ещё не сохранялся в базе он новый по этому ставим флаг что не совсем валидный может потом цветом подкрасим
                this.SaveValueNotValid = false;
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
                if (Com.ProviderFarm.CurrentPrv.HashDocumentKasBook(this))
                {
                    Com.ProviderFarm.CurrentPrv.UpdateDocumentKasBook(this);
                }
                else  // Если нет то вставляем
                {
                    Com.ProviderFarm.CurrentPrv.SetDocumentKasBook(this);
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
