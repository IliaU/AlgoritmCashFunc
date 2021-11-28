using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.BLL;
using AlgoritmCashFunc.Lib;
using System.Reflection;

namespace AlgoritmCashFunc.Com
{
    /// <summary>
    /// Фабрика для создания операций
    /// </summary>
    public class OperationFarm 
    {
        /// <summary>
        /// Внутренний список доступных плагинов, чтобы каждый раз не пересчитывать
        /// </summary>
        private static List<string> OpFullName;

        /// <summary>
        /// Текущий список доступных Operation
        /// </summary>
        public static OperationList CurOperationList = null;

        /// <summary>
        /// Конструктор
        /// </summary>
        public OperationFarm()
        {
            try
            {
                if (CurOperationList == null)
                {
                    // Если списка документов ещё нет то создаём его
                    ListOperationName();

                    //GetCurOperationListFromDB();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации конструктора с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Получить списко доступных Local
        /// </summary>
        /// <returns>Список доступных документов</returns>
        public static List<string> ListOperationName()
        {
            // Если список ещё не получали то получаем его
            if (OpFullName == null)
            {
                OpFullName = new List<string>();

                Type[] typelist = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "AlgoritmCashFunc.BLL.OperationPlg").ToArray();

                foreach (Type item in typelist)
                {
                    // Проверяем реализовывает ли класс наш интерфейс если да то это провайдер который можно подкрузить
                    bool flagI = false;
                    foreach (Type i in item.GetInterfaces())
                    {
                        if (i.FullName == "AlgoritmCashFunc.BLL.OperationPlg.Lib.OperationInterface")
                        {
                            flagI = true;
                            break;
                        }
                    }
                    if (!flagI) continue;

                    // Проверяем что наш клас наследует PlugInBase 
                    bool flagB = false;
                    foreach (MemberInfo mi in item.GetMembers())
                    {
                        if (mi.DeclaringType.FullName == "AlgoritmCashFunc.BLL.OperationPlg.Lib.OperationBase")
                        {
                            flagB = true;
                            break;
                        }
                    }
                    if (!flagB) continue;

                    // Проверяем конструктор нашего класса  
                    bool flag = false;
                    bool flag0 = false;
                    string nameConstructor;
                    foreach (ConstructorInfo ctor in item.GetConstructors())
                    {
                        nameConstructor = item.Name;

                        // получаем параметры конструктора  
                        ParameterInfo[] parameters = ctor.GetParameters();

                        // если в этом конструктаре 4 параметров то проверяем тип и имя параметра 
                        if (parameters.Length == 4)
                        {
                            bool flag2 = true;
                            if (parameters[0].ParameterType.Name != "Nullable`1" || parameters[0].Name != "Id") flag = false;
                            if (parameters[1].ParameterType.Name != "String" || parameters[1].Name != "OperationName") flag = false;
                            if (parameters[2].ParameterType.Name != "Int32" || parameters[2].Name != "KoefDebitor") flag = false;
                            if (parameters[3].ParameterType.Name != "Int32" || parameters[3].Name != "KoefCreditor") flag = false;
                            flag = flag2;
                        }

                        // Проверяем конструктор для создания документа пустого по умолчанию
                        if (parameters.Length == 0) flag0 = true;
                    }
                    if (!flag) continue;
                    if (!flag0) continue;

                    OpFullName.Add(item.Name);
                }
            }

            return OpFullName;
        }

        /// <summary>
        /// Создание пустого Local определённого типа по умолчанию
        /// </summary>
        /// <param name="OpFullName">Имя плагина определяющего тип Operation который создаём</param>
        /// <returns>Возвращаем Local</returns>
        public static Local CreateNewOperation(string OpFullName)
        {
            // Если списка Local ещё нет то создаём его
            ListOperationName();

            Local rez = null;

            // Проверяем наличие существование этого типа документа
            foreach (string item in ListOperationName())
            {
                if (item == OpFullName.Trim())
                {
                    Type myType = Type.GetType("AlgoritmCashFunc.BLL.OperationPlg." + OpFullName.Trim(), false, true);

                    // Создаём экземпляр объекта
                    rez = (Local)Activator.CreateInstance(myType);

                    break;
                }
            }

            return rez;
        }

        /// <summary>
        /// Создание элемента из базы данных
        /// </summary>
        /// <param name="OpFullName">Имя плагина определяющего тип Local который создаём</param>
        /// <param name="Id">Идентификатор в базе данных</param>
        /// <param name="OperationName">Имя операции для ползователя</param>
        /// <param name="KoefDebitor">Дебитор коэфициент</param>
        /// <param name="KoefCreditor">Кредитор коэфициент</param>
        /// <returns>Возвращаем Local</returns>
        public static Local CreateNewOperation(string OpFullName, int Id, string OperationName, int KoefDebitor, int KoefCreditor)
        {
            // Если списка Local ещё нет то создаём его
            ListOperationName();

            Local rez = null;

            // Проверяем наличие существование этого типа документа
            foreach (string item in ListOperationName())
            {
                if (item == OpFullName.Trim())
                {
                    Type myType = Type.GetType("AlgoritmCashFunc.BLL.OperationPlg." + OpFullName.Trim(), false, true);

                    // Создаём экземпляр объекта  
                    object[] targ = { Id, OperationName, KoefDebitor, KoefCreditor };
                    rez = (Local)Activator.CreateInstance(myType, targ);

                    break;
                }
            }

            return rez;
        }


        /*
        /// <summary>
        /// Обновление списка операций
        /// </summary>
        public static void  UpdateOperationList()
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
        */
    }
}
