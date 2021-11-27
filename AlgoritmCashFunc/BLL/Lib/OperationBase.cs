using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.BLL.Lib
{
    /// <summary>
    /// Базовый класс для операций
    /// </summary>
    public abstract class OperationBase : EventArgs
    {
        /// <summary>
        /// Идентификатор продукта в коллекции
        /// </summary>
        public int Index { get; protected set; } = -1;

        /// <summary>
        /// Представляет из себя список продуктов
        /// </summary>
        public class OperationBaseList : IEnumerable
        {
            /// <summary>
            /// Внутренний список
            /// </summary>
            private List<Operation> _OperationL = new List<Operation>();

            /// <summary>
            /// Индексаторы
            /// </summary>
            /// <param name="s">Поиск по DocFullName</param>
            /// <returns>Возвращает операцию</returns>
            public Operation this[string s]
            {
                get
                {
                    foreach (Operation item in this._OperationL)
                    {
                        if (item.DocFullName == s) return item;
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
            public Operation this[int index]
            {
                get
                {
                    return this._OperationL[index];
                }
                private set { }
            }

            /// <summary>
            /// Индексаторы
            /// </summary>
            /// <param name="id">Поиск по id</param>
            /// <returns>Возвращает операцию</returns>
            public Operation this[int? id]
            {
                get
                {
                    if (id != null)
                    {
                        foreach (Operation item in this._OperationL)
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
            public OperationBaseList()
            {
                try
                {
                    // Можно было бы сделать чтобы справочник грузился сразу в конструкторе, но проблема в том что статические классы создаются до того как мы запускаем форму и из за этого возникает проблема с подписью
                    //GetProductListNotCash();
                }
                catch (Exception ex)
                {
                    ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации класса с ошибкой: ({0})", ex.Message));
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
                    lock (_OperationL)
                    {
                        rez = _OperationL.Count;
                    }
                    return rez;
                }
                private set { }
            }

            /// <summary>
            /// Добавление нового продукта
            /// </summary>
            /// <param name="nProduct">Новая операция</param>
            protected void Add(Operation nOperation)
            {
                try
                {
                    // Добавляет продукт в список
                    lock (_OperationL)
                    {
                        nOperation.Index = this.Count;
                        _OperationL.Add(nOperation);
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
                lock (_OperationL)
                {
                    return _OperationL.GetEnumerator();
                }
            }
        }
    }
}
