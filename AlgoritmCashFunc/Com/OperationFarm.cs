using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.BLL;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.Com
{
    public class OperationFarm : OperationList.OperationListFarmBase
    {
        private static OperationList _CurOperationList = new OperationList();

        /// <summary>
        /// Текущий список доступных операций
        /// </summary>
        public static OperationList CurOperationList
        {
            get
            {
                try
                {
                    if (_CurOperationList.Count==0) UpdateOperationList();
                }
                catch (Exception ex)
                {
                    // Если есть ошибка например по причине того что не установлено соединение с базой нам надо ругануться но не свалиться
                    Log.EventSave(ex.Message, "OperationFarm.CurOperationList", EventEn.Error, true, true);
                }
                return _CurOperationList;
            }
            private set { }
        }

        /// <summary>
        /// Обновление списка операций
        /// </summary>
        public  static void  UpdateOperationList()
        {
            try
            {
                // получаем список по умолчанию
                OperationList TmpOlist = new OperationList();
                foreach (string item in DocumentFarm.ListDocumentName())
                {
                    Document doctmp = DocumentFarm.CreateNewDocument(item);
                    AddOperationToList(TmpOlist, doctmp.CurOperation);
                }

                // Получаем список из базы данных
                if (Com.ProviderFarm.CurrentPrv != null)
                {
                    _CurOperationList = Com.ProviderFarm.CurrentPrv.GetOperationList();

                    // Пробегаем по дефолтному списку
                    foreach (Operation item in TmpOlist)
                    {
                        // Если такой операции не найдено то обогощаем список этой операцией
                        if (_CurOperationList[item.DocFullName] == null) OperationList.OperationListFarmBase.AddOperationToList(_CurOperationList, item);
                    }
                }
                else _CurOperationList = TmpOlist;
              
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации класса с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, "OperationFarm.UpdateOperationList", EventEn.Error);
                throw ae;
            }
        }
    }
}
