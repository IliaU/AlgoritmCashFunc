using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.BLL_Prizm
{
    /// <summary>
    /// Заголовок чека из призма
    /// </summary>
    public class Check
    {
        public int DocNumber;

        public DateTime UreDate;

        public double SumCheck;

        public List<CheckItem> ChkItem = new List<CheckItem>();

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="DocNumber"></param>
        /// <param name="UreDate"></param>
        /// <param name="SuumCheck"></param>
        /// <param name="ChkItem"></param>
        public Check (int DocNumber, DateTime UreDate, double SumCheck)
        {
            try
            {
                this.DocNumber = DocNumber;
                this.UreDate = UreDate;
                this.SumCheck = SumCheck;
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
        /// <param name="DocNumber"></param>
        /// <param name="UreDate"></param>
        /// <param name="SuumCheck"></param>
        /// <param name="ChkItem"></param>
        public Check(int DocNumber, DateTime UreDate, double SumCheck, List<CheckItem> ChkItem) :this (DocNumber, UreDate, SumCheck)
        {
            try
            {
                this.ChkItem = ChkItem;
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
