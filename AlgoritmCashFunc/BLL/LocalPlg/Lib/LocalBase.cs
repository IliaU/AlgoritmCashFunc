using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.BLL.LocalPlg.Lib
{
    /// <summary>
    /// Базовый класс для операций
    /// </summary>
    public class LocalBase
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
        public string LocFullName { get; protected set; }

        /// <summary>
        /// Имя в базе данных уникальное. Возможно sid
        /// </summary>
        public string LocalName { get; protected set; }

        /// <summary>
        /// Роль поставщика
        /// </summary>
        public bool IsSeller { get; protected set; } = false;

        /// <summary>
        /// Роль покупатнеля
        /// </summary>
        public bool IsСustomer { get; protected set; } = false;

        /// <summary>
        /// Роль подразделения
        /// </summary>
        public bool IsDivision { get; protected set; } = false;

        /// <summary>
        /// Черновик
        /// </summary>
        public bool IsDraft { get; protected set; } = true;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="LocFullName">Тип плагина</param>
        /// <param name="LocalName">Имя в базе данных уникальное. Возможно sid</param>
        /// <param name="IsSeller">Роль поставщика</param>
        /// <param name="IsСustomer">Роль покупатнеля</param>
        /// <param name="IsDivision">Роль подразделения</param>
        /// <param name="IsDraft">Черновик</param>
        public LocalBase(string LocFullName, string LocalName, bool IsSeller, bool IsСustomer, bool IsDivision, bool IsDraft)
        {
            try
            {
                this.LocFullName = LocFullName;
                this.LocalName = LocalName;
                this.IsSeller = IsSeller;
                this.IsСustomer = IsСustomer;
                this.IsDivision = IsDivision;
                this.IsDraft = IsDraft;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации конструктора с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Представляет из себя список продуктов
        /// </summary>
        public class LocalBaseList : IEnumerable
        {
            /// <summary>
            /// Внутренний список
            /// </summary>
            private List<Local> _LocalL = new List<Local>();

            /// <summary>
            /// Индексаторы
            /// </summary>
            /// <param name="s">Поиск по LocalName</param>
            /// <returns>Возвращает операцию</returns>
            public Local this[string s]
            {
                get
                {
                    foreach (Local item in this._LocalL)
                    {
                        if (item.LocalName == s) return item;
                    }

                    return null;
                }
                private set { }
            }

            /// <summary>
            /// Индексаторы
            /// </summary>
            /// <param name="index">Поиск по индексы</param>
            /// <returns>Возвращает операцию</returns>
            public Local this[int index]
            {
                get
                {
                    return this._LocalL[index];
                }
                private set { }
            }

            /// <summary>
            /// Индексаторы
            /// </summary>
            /// <param name="id">Поиск по id</param>
            /// <returns>Возвращает операцию</returns>
            public Local this[int? id]
            {
                get
                {
                    if (id != null)
                    {
                        foreach (Local item in this._LocalL)
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
            public LocalBaseList()
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
                    lock (_LocalL)
                    {
                        rez = _LocalL.Count;
                    }
                    return rez;
                }
                private set { }
            }

            /// <summary>
            /// Добавление нового продукта
            /// </summary>
            /// <param name="nLocal">Новый Local</param>
            public void Add(Local nLocal)
            {
                try
                {
                    // Добавляет продукт в список
                    lock (_LocalL)
                    {
                        if (nLocal.Id!=null && this[nLocal.Id] != null) throw new ApplicationException(String.Format("В этом списке Local с таким Id ({0}) уже существует", nLocal.Id));

                        nLocal.Index = this.Count;
                        _LocalL.Add(nLocal);
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
                lock (_LocalL)
                {
                    return _LocalL.GetEnumerator();
                }
            }
        }
    }
}
