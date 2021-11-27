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
    /// Фабрика для Local
    /// </summary>
    public class LocalFarm
    {

        /// <summary>
        /// Внутренний список доступных плагинов, чтобы каждый раз не пересчитывать
        /// </summary>
        private static List<string> LocFullName;

        /// <summary>
        /// Текущий список доступных Local
        /// </summary>
        public static LocalList CurLocalList = null;

        /// <summary>
        /// Конструктор
        /// </summary>
        public LocalFarm()
        {
            try
            {
               

                if (CurLocalList == null)
                {
                    // Если списка документов ещё нет то создаём его
                    ListLocalName();

                    GetCurLocalListFromDB();
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
        public static List<string> ListLocalName()
        {
            // Если список ещё не получали то получаем его
            if (LocFullName == null)
            {
                LocFullName = new List<string>();

                Type[] typelist = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "AlgoritmCashFunc.BLL.LocalPlg").ToArray();

                foreach (Type item in typelist)
                {
                    // Проверяем реализовывает ли класс наш интерфейс если да то это провайдер который можно подкрузить
                    bool flagI = false;
                    foreach (Type i in item.GetInterfaces())
                    {
                        if (i.FullName == "AlgoritmCashFunc.BLL.LocalPlg.Lib.LocalInterface")
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
                        if (mi.DeclaringType.FullName == "AlgoritmCashFunc.BLL.LocalPlg.Lib.LocalBase")
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
                        
                        // если в этом конструктаре 5 параметров то проверяем тип и имя параметра 
                        if (parameters.Length == 5)
                        {
                            bool flag2 = true;
                            if (parameters[0].ParameterType.Name != "Nullable`1" || parameters[0].Name != "Id") flag = false;
                            if (parameters[1].ParameterType.Name != "String" || parameters[1].Name != "LocalName") flag = false;
                            if (parameters[2].ParameterType.Name != "Boolean" || parameters[2].Name != "IsSeller") flag = false;
                            if (parameters[3].ParameterType.Name != "Boolean" || parameters[3].Name != "IsСustomer") flag = false;
                            if (parameters[4].ParameterType.Name != "Boolean" || parameters[4].Name != "IsDivision") flag = false;
                            flag = flag2;
                        }
                        
                        // Проверяем конструктор для создания документа пустого по умолчанию
                        if (parameters.Length == 0) flag0 = true;
                    }
                    if (!flag) continue;
                    if (!flag0) continue;

                    LocFullName.Add(item.Name);
                }
            }

            return LocFullName;
        }

        /// <summary>
        /// Создание пустого Local определённого типа по умолчанию
        /// </summary>
        /// <param name="LocFullName">Имя плагина определяющего тип Local который создаём</param>
        /// <returns>Возвращаем Local</returns>
        public static Local CreateNewLocal(string LocFullName)
        {
            // Если списка документов ещё нет то создаём его
            ListLocalName();

            Local rez = null;

            // Проверяем наличие существование этого типа документа
            foreach (string item in ListLocalName())
            {
                if (item == LocFullName.Trim())
                {
                    Type myType = Type.GetType("AlgoritmCashFunc.BLL.LocalPlg." + LocFullName.Trim(), false, true);

                    // Создаём экземпляр объекта  
                    //object[] targ = { ConnectionString };
                    rez = (Local)Activator.CreateInstance(myType/*, targ*/);

                    break;
                }
            }

            return rez;
        }

        /// <summary>
        /// Создание элемента из базы данных
        /// </summary>
        /// <param name="LocFullName">Имя плагина определяющего тип Local который создаём</param>
        /// <param name="Id">Идентификатор в базе данных</param>
        /// <param name="LocalName">Имя из базы данных</param>
        /// <param name="IsSeller">Роль поставщика</param>
        /// <param name="IsСustomer">Роль покупателя</param>
        /// <param name="IsDivision">Роль подразделения или кассы</param>
        /// <returns>Возвращаем Local</returns>
        public static Local CreateNewLocal(string LocFullName, int Id, string LocalName, bool IsSeller, bool IsСustomer, bool IsDivision)
        {
            // Если списка документов ещё нет то создаём его
            ListLocalName();

            Local rez = null;

            // Проверяем наличие существование этого типа документа
            foreach (string item in ListLocalName())
            {
                if (item == LocFullName.Trim())
                {
                    Type myType = Type.GetType("AlgoritmCashFunc.BLL.LocalPlg." + LocFullName.Trim(), false, true);

                    // Создаём экземпляр объекта  
                    object[] targ = { Id, LocalName, IsSeller, IsСustomer, IsDivision };
                    rez = (Local)Activator.CreateInstance(myType, targ);

                    break;
                }
            }

            return rez;
        }

        /// <summary>
        /// Актуализация текущего списка Local
        /// </summary>
        public static LocalList GetCurLocalListFromDB()
        {
            try
            {
                CurLocalList = Com.ProviderFarm.CurrentPrv.GetCurLocalListFromDB();

                return CurLocalList;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при вызове метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, "LocalFarm.GetCurLocalListFromDB", EventEn.Error);
                throw ae;
            }
        }
    }
}
