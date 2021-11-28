﻿using System;
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
    }
}
