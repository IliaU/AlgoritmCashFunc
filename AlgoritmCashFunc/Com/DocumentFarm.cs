using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.BLL;
using AlgoritmCashFunc.Lib;
using AlgoritmCashFunc.BLL.DocumentPlg.Lib;
using System.Reflection;
using System.Windows.Forms;

namespace AlgoritmCashFunc.Com
{
    /// <summary>
    /// Фабрика для создания документов
    /// </summary>
    public class DocumentFarm 
    {
        /// <summary>
        /// Внутренний список доступных плагинов, чтобы каждый раз не пересчитывать
        /// </summary>
        private static List<string> DocumentName;




        /// <summary>
        /// Конструктор
        /// </summary>
        public DocumentFarm()
        {
            try
            {
                if (DocumentName == null)
                {
                    // Если списка документов ещё нет то создаём его
                    ListDocumentName();
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
        /// Получить списко доступных типов документа
        /// </summary>
        /// <returns>Список доступных документов</returns>
        public static List<string> ListDocumentName()
        {
            // Если список ещё не получали то получаем его
            if (DocumentName == null)
            {
                DocumentName = new List<string>();

                Type[] typelist = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "AlgoritmCashFunc.BLL.DocumentPlg").ToArray();

                foreach (Type item in typelist)
                {
                    // Проверяем реализовывает ли класс наш интерфейс если да то это провайдер который можно подкрузить
                    bool flagI = false;
                    foreach (Type i in item.GetInterfaces())
                    {
                        if (i.FullName == "AlgoritmCashFunc.BLL.DocumentPlg.Lib.DocumentInterface")
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
                        if (mi.DeclaringType.FullName == "AlgoritmCashFunc.BLL.DocumentPlg.Lib.DocumentBase")
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
                        
                        // если в этом конструктаре 10 параметров то проверяем тип и имя параметра 
                        if (parameters.Length == 4)
                        {
                            bool flag2 = true;
                            if (parameters[0].ParameterType.Name != "Nullable`1" || parameters[0].Name != "Id") flag = false;
                            if (parameters[1].ParameterType.Name != "Operation" || parameters[1].Name != "CurOperation") flag = false;
                            if (parameters[2].ParameterType.Name != "Local" || parameters[2].Name != "LocalDebitor") flag = false;
                            if (parameters[3].ParameterType.Name != "Local" || parameters[3].Name != "LocalCreditor") flag = false;
                            flag = flag2;
                        }
                        
                        // Проверяем конструктор для создания документа пустого по умолчанию
                        if (parameters.Length == 0) flag0 = true;
                    }
                    if (!flag) continue;
                    if (!flag0) continue;

                    DocumentName.Add(item.Name);
                }
            }

            return DocumentName;
        }

        /// <summary>
        /// Создание пустого документа определённого типа
        /// </summary>
        /// <param name="PrvFullName">Имя плагина определяющего тип документа который создаём</param>
        /// <returns>Возвращаем документ</returns>
        public static Document CreateNewDocument(string PrvFullName)
        {
            // Если списка документов ещё нет то создаём его
            ListDocumentName();

            Document rez = null;

            // Проверяем наличие существование этого типа документа
            foreach (string item in ListDocumentName())
            {
                if (item == PrvFullName.Trim())
                {
                    Type myType = Type.GetType("AlgoritmCashFunc.BLL.DocumentPlg." + PrvFullName.Trim(), false, true);

                    // Создаём экземпляр объекта
                    rez = (Document)Activator.CreateInstance(myType);

                    break;
                }
            }

            return rez;
        }

        /// <summary>
        /// Создание элемента из базы данных
        /// </summary>
        /// <param name="DocFullName">Имя плагина определяющего тип Document который создаём</param>
        /// <param name="Id">Идентификатор в базе данных</param>
        /// <param name="LocalName">Имя из базы данных</param>
        /// <param name="IsSeller">Роль поставщика</param>
        /// <param name="IsСustomer">Роль покупателя</param>
        /// <param name="IsDivision">Роль подразделения или кассы</param>
        /// <returns>Возвращаем Local</returns>
        public static Document CreateNewDocument(string DocFullName, int Id, Operation CurOperation, Local LocalDebitor, Local LocalCreditor)
        {
            // Если списка документов ещё нет то создаём его
            ListDocumentName();

            Document rez = null;

            // Проверяем наличие существование этого типа документа
            foreach (string item in ListDocumentName())
            {
                if (item == DocFullName.Trim())
                {
                    Type myType = Type.GetType("AlgoritmCashFunc.BLL.DocumentPlg." + DocFullName.Trim(), false, true);

                    // Создаём экземпляр объекта  
                    object[] targ = { Id, CurOperation, LocalDebitor, LocalCreditor };
                    rez = (Document)Activator.CreateInstance(myType, targ);

                    break;
                }
            }

            return rez;
        }

    }
}
