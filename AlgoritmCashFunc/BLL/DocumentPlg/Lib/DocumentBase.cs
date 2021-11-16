using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.BLL.DocumentPlg.Lib
{
    /// <summary>
    /// Базовый класс для всех документов
    /// </summary>
    public abstract class DocumentBase
    {
        /// <summary>
        /// Операция к которой относится этот документ
        /// </summary>
        public Operation CurOperation { get; private set; }

        /// <summary>
        /// Внутренняя сылка на интерфейс реализованный в плагине по логике тотже самый класс просто перобразован
        /// </summary>
        public DocumentInterface PlgMethod { get; private set; }

        /// <summary>
        /// Класс базового интерфейса Кастомизированный можно преобразовать например вот так (DocumentPrihod)DocBsInterfaceCustom;
        /// </summary>
        public DocumentBaseInterface DocBsInterfaceCustom;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="CurOperation">Операция к которой относится этот документ</param>
        public DocumentBase()
        {
            try
            {

            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации класса Document с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }



        /// <summary>
        /// Базовый класс для интерфейсов чтобы они могли настраивать базовый класс
        /// </summary>
        public abstract class DocumentBaseInterface
        {
            /// <summary>
            /// Операция к которой относится этот документ
            /// </summary>
            public Operation CurOperation { get; private set; }

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="CurOperation"></param>
            public DocumentBaseInterface(Operation CurOperation)
            {
                try
                {
                    this.CurOperation = CurOperation;
                }
                catch (Exception ex)
                {
                    ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации класса DocumentBaseInterface с ошибкой: ({0})", ex.Message));
                    Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                    throw ae;
                }
            }


            /// <summary>
            /// Класс для фермы чтобы он мог заполнить перекрёсные ссылки и в классе документа и в базовом на интерфейс
            /// </summary>
            public abstract class DocumentFarm
            {
                /// <summary>
                /// Инициализация документа
                /// </summary>
                /// <param name="Doc">Документ который нужно инициализировать</param>
                protected static void InitDocumentBase(DocumentBase DocB, DocumentBaseInterface BasInt)
                {
                    try
                    {
                        DocB.DocBsInterfaceCustom = BasInt;
                        DocB.CurOperation = BasInt.CurOperation;
                        DocB.PlgMethod = (DocumentInterface)BasInt;
                    }
                    catch (Exception ex)
                    {
                        ApplicationException ae = new ApplicationException(string.Format("Упали с ошибкой: ({0})", ex.Message));
                        Com.Log.EventSave(ae.Message, "DocumentBase.DocumentBaseInterface.DocumentFarm", EventEn.Error);
                        throw ae;
                    }
                }
            }

        }
    }
}
