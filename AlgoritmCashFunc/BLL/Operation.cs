using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.BLL.DocumentPlg.Lib;

namespace AlgoritmCashFunc.BLL
{
    /// <summary>
    /// Представляет из себя список операций из базы данных с описанием
    /// </summary>
    public sealed class Operation : OperationBase
    {
        /// <summary>
        /// Идентификатор операции из базы данных
        /// </summary>
        public int? Id { get; private set; }

        /// <summary>
        /// Тип соответствующий плагину
        /// </summary>
        public string DocFullName { get; private set; }

        /// <summary>
        /// Описание операции которое видит пользователь
        /// </summary>
        public string OperationName { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор операции</param>
        /// <param name="DocFullName">Тип соответствующий плагину</param>
        /// <param name="OperationName">Описание операции которое видит пользователь</param>
        public Operation(int Id, string DocFullName, string OperationName)
        {
            this.Id = Id;
            this.DocFullName = DocFullName;
            this.OperationName = OperationName;
        }
    }
}
