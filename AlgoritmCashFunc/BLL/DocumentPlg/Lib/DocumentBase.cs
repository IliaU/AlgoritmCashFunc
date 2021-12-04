using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.BLL.DocumentPlg.Lib
{
    /// <summary>
    /// Базовый класс для всех документов
    /// </summary>
    public abstract class DocumentBase
    {
        /// <summary>
        /// Идентификатор в базе данных
        /// </summary>
        public int? Id { get; protected set; }

        /// <summary>
        /// Идентификатор продукта в коллекции
        /// </summary>
        public int Index { get; protected set; } = -1;

        /// <summary>
        /// Тип плагина
        /// </summary>
        public string DocFullName { get; protected set; }

        /// <summary>
        /// Дата создания документа
        /// </summary>
        public DateTime CteateDate { get; protected set; } = DateTime.Now;

        /// <summary>
        /// Юридическая дата к которой относится документ
        /// </summary>
        public DateTime? UreDate { get; protected set; } = DateTime.Now.Date;

        /// <summary>
        /// Дата изменеия документа
        /// </summary>
        public DateTime ModifyDate { get; protected set; } = DateTime.Now;

        /// <summary>
        /// Пользовтаель который изменил последний раз документ
        /// </summary>
        public string ModifyUser { get; protected set; } = Com.UserFarm.CurrentUser.Logon;

        /// <summary>
        /// Операция к которой относится этот документ
        /// </summary>
        public Operation CurOperation { get; private set; }

        /// <summary>
        /// Дебитор
        /// </summary>
        public Local LocalDebitor { get; private set; } = null;

        /// <summary>
        /// Кредитор
        /// </summary>
        public Local LocalCreditor { get; private set; } = null;

        /// <summary>
        /// Черновик
        /// </summary>
        public bool IsDraft { get; protected set; } = true;

        /// <summary>
        /// Проведённый документ или нет
        /// </summary>
        public bool IsProcessed { get; protected set; } = false;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="DocFullName">Тип плагина</param>
        /// <param name="CurOperation">Операция к которой относится этот документ</param>
        /// <param name="LocalDebitor">Дебитор</param>
        /// <param name="LocalCreditor">Кредитор</param>
        /// <param name="IsDraft"> Черновик</param>
        public DocumentBase(string DocFullName, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, bool IsDraft)
        {
            try
            {
                this.DocFullName = DocFullName;
                this.CurOperation = CurOperation;
                this.LocalDebitor = LocalDebitor;
                this.LocalCreditor = LocalCreditor;
                this.IsDraft = IsDraft;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации класса Document с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Представляет из себя список продуктов
        /// </summary>
        public class DocumentBaseList : IEnumerable
        {
            /// <summary>
            /// Внутренний список
            /// </summary>
            private List<Document> _DocumentL = new List<Document>();

            /// <summary>
            /// Индексаторы
            /// </summary>
            /// <param name="index">Поиск по индексы</param>
            /// <returns>Возвращает Document</returns>
            public Document this[int index]
            {
                get
                {
                    return this._DocumentL[index];
                }
                private set { }
            }

            /// <summary>
            /// Индексаторы
            /// </summary>
            /// <param name="id">Поиск по id</param>
            /// <returns>Возвращает Document</returns>
            public Document this[int? id]
            {
                get
                {
                    if (id != null)
                    {
                        foreach (Document item in this._DocumentL)
                        {
                            if (item.Id != null && item.Id == id) return item;
                        }
                    }
                    return null;
                }
                private set { }
            }

            /// <summary>
            /// Конструктор в котором можно реализовать например базовое первоначальное получение списка доступных товаров
            /// </summary>
            public DocumentBaseList()
            {
                try
                {
                    // Можно было бы сделать чтобы справочник грузился сразу в конструкторе, но проблема в том что статические классы создаются до того как мы запускаем форму и из за этого возникает проблема с подписью
                    //GetProductListNotCash();
                }
                catch (Exception ex)
                {
                    ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации конструктора с ошибкой: ({0})", ex.Message));
                    Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                    throw ae;
                }

            }


            /// <summary>
            /// Количчество объектов в контейнере
            /// </summary>
            public int Count
            {
                get
                {
                    int rez;
                    lock (_DocumentL)
                    {
                        rez = _DocumentL.Count;
                    }
                    return rez;
                }
                private set { }
            }

            /// <summary>
            /// Добавление нового продукта
            /// </summary>
            /// <param name="nDocument">Новый Document</param>
            public void Add(Document nDocument)
            {
                try
                {
                    // Добавляет продукт в список
                    lock (_DocumentL)
                    {
                        nDocument.Index = this.Count;
                        _DocumentL.Add(nDocument);
                    }
                }
                catch (Exception ex)
                {
                    Com.Log.EventSave(string.Format("Произошла ошибка: ({0})", ex.Message), string.Format("{0}.OperationBase.Add(Operation nOperation)", GetType().Name), EventEn.Error, true, false);
                    throw ex;
                }
            }

            /// <summary>
            /// Для обращения по индексатору
            /// </summary>
            /// <returns>Возвращаем стандарнтый индексатор</returns>
            public IEnumerator GetEnumerator()
            {
                lock (_DocumentL)
                {
                    return _DocumentL.GetEnumerator();
                }
            }
        }

    }
}
