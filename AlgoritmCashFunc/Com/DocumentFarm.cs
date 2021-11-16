using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.BLL;
using AlgoritmCashFunc.BLL.DocumentPlg.Lib;
using System.Reflection;
using System.Windows.Forms;

namespace AlgoritmCashFunc.Com
{
    /// <summary>
    /// Фабрика для создания документов
    /// </summary>
    public class DocumentFarm : Document.DocumentBaseInterface.DocumentFarm
    {


        /// <summary>
        /// Внутренний список доступных плагинов, чтобы каждый раз не пересчитывать
        /// </summary>
        private static List<string> DocumentName;


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
                        if (i.FullName == "AlgoritmCashFunc.BLL.DocumentPlg.Lib.DocumentInterface") flagI = true;
                    }
                    if (!flagI) continue;

                    // Проверяем что наш клас наследует PlugInBase 
                    bool flagB = false;
                    foreach (MemberInfo mi in item.GetMembers())
                    {
                        if (mi.DeclaringType.FullName == "AlgoritmCashFunc.BLL.DocumentPlg.Lib.DocumentBase+DocumentBaseInterface") flagB = true;
                    }
                    if (!flagB) continue;

                    // Проверяем конструктор нашего класса  
                    //bool flag = false;
                    bool flag0 = false;
                    string nameConstructor;
                    foreach (ConstructorInfo ctor in item.GetConstructors())
                    {
                        nameConstructor = item.Name;

                        // получаем параметры конструктора  
                        ParameterInfo[] parameters = ctor.GetParameters();
                        /*
                        // если в этом конструктаре 10 параметров то проверяем тип и имя параметра 
                        if (parameters.Length == 14)
                        {
                            bool flag2 = true;
                            if (parameters[0].ParameterType.Name != "Nullable`1" || parameters[0].Name != "Document") flag = false;
                            if (parameters[1].ParameterType.Name != "Nullable`1" || parameters[1].Name != "DocNumber") flag = false;
                            if (parameters[2].ParameterType.Name != "Int16" || parameters[2].Name != "FromLocal") flag = false;
                            if (parameters[3].ParameterType.Name != "Int16" || parameters[3].Name != "ToLocal") flag = false;
                            if (parameters[4].ParameterType.Name != "Nullable`1" || parameters[4].Name != "UreDate") flag = false;
                            if (parameters[5].ParameterType.Name != "Nullable`1" || parameters[5].Name != "OldUreDate") flag = false;
                            if (parameters[6].ParameterType.Name != "Boolean" || parameters[6].Name != "IsDraft") flag = false;
                            if (parameters[7].ParameterType.Name != "Boolean" || parameters[7].Name != "IsEdited") flag = false;
                            if (parameters[8].ParameterType.Name != "Boolean" || parameters[8].Name != "IsCheck") flag = false;
                            if (parameters[9].ParameterType.Name != "Boolean" || parameters[9].Name != "IsDeleted") flag = false;
                            if (parameters[10].ParameterType.Name != "String" || parameters[10].Name != "LoockOperator") flag = false;
                            if (parameters[11].ParameterType.Name != "Nullable`1" || parameters[11].Name != "EnterDate") flag = false;
                            if (parameters[12].ParameterType.Name != "Nullable`1" || parameters[12].Name != "ModifyDate") flag = false;
                            if (parameters[13].ParameterType.Name != "String" || parameters[13].Name != "Operator") flag = false;
                            flag = flag2;
                        }
                        */
                        // Проверяем конструктор для создания документа пустого по умолчанию
                        if (parameters.Length == 0) flag0 = true;
                    }
                    //if (!flag) continue;
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
            DocumentBase.DocumentBaseInterface BasInt = null;
            Document rez = null;

            // Проверяем наличие существование этого типа документа
            foreach (string item in ListDocumentName())
            {
                if (item == PrvFullName.Trim())
                {
                    Type myType = Type.GetType("AlgoritmCashFunc.BLL.DocumentPlg." + PrvFullName.Trim(), false, true);

                    // Создаём экземпляр объекта  
                    BasInt = (DocumentBase.DocumentBaseInterface)Activator.CreateInstance(myType);

                    break;
                }
            }

            if (BasInt != null)
            {
                rez = new Document();
                InitDocumentBase(rez, BasInt);
            }

            return rez;
        }
        /*
        /// <summary>
        /// Создание документа определённого типа заполненного исходными данными, которые потом будем редактировать
        /// </summary>
        /// <param name="PrvFullName">Имя плагина определяющего тип документа который создаём</param>
        /// <param name="Document">Идентификатор документа</param>
        /// <param name="DocNumber">Номер документа</param>
        /// <param name="FromLocal">Откуда делает перемещение</param>
        /// <param name="ToLocal">Куда делает перемещение</param>
        /// <param name="UreDate">Юридическая дата документа</param>
        /// <param name="OldUreDate">Предыдущая юридическая дата документа</param>
        /// <param name="IsDraft">Документ является черновиком</param>
        /// <param name="IsEdited">Документ находится в состоянии редактирования</param>
        /// <param name="IsCheck">Проведён документ или нет</param>
        /// <param name="IsDeleted">Документ удалён</param>
        /// <param name="LoockOperator">Кем заблокирован документ</param>
        /// <param name="EnterDate">Дата создания докемента</param>
        /// <param name="ModifyDate">Последняя дата изменения</param>
        /// <param name="Operator">Оператор который редактировал или правил документ</param>
        /// <returns>Возвращаем документ</returns>
        public static Document CreateNewDocument(string PrvFullName, int? Document, int? DocNumber, Int16 FromLocal, Int16 ToLocal, DateTime? UreDate, DateTime? OldUreDate, bool IsDraft, bool IsEdited, bool IsCheck, bool IsDeleted, string LoockOperator, DateTime? EnterDate, DateTime? ModifyDate, string Operator)
        {
            Document rez = null;

            // Проверяем наличие существование этого типа документа
            foreach (string item in ListDocumentName())
            {
                if (item == PrvFullName.Trim())
                {
                    Type myType = Type.GetType("AlgoritmCRPT.BLL.DocumentPlg." + PrvFullName.Trim(), false, true);

                    // Создаём экземпляр объекта
                    object[] targ = { Document, DocNumber, FromLocal, ToLocal, UreDate, OldUreDate, IsDraft, IsEdited, IsCheck, IsDeleted, LoockOperator, EnterDate, ModifyDate, Operator };
                    rez = (Document)Activator.CreateInstance(myType, targ);
                }
            }

            return rez;
        }

        /// <summary>
        /// Получение менюшки для данного типа документа, который нужно вытащить на главную форму
        /// </summary>
        /// <param name="PrvFullName">Имя плагина определяющего тип документа</param>
        /// <returns>Менюшка для списка данного типа документов</returns>
        public static ToolStripMenuItem GetMenuShipDocumentList(string PrvFullName)
        {
            ToolStripMenuItem rez = null;
            string TypPlg = null;

            try
            {
                // Проверяем наличие существование этого типа документа
                foreach (string item in ListDocumentName())
                {
                    if (item == PrvFullName.Trim())
                    {
                        TypPlg = item;

                        Type myType = Type.GetType("AlgoritmCRPT.BLL.DocumentPlg." + PrvFullName.Trim(), false, true);

                        if (myType == null) throw new ApplicationException("В этом плагине не реализован статический метод GetMenuShipDocumentList() для получения менюшки");
                        else
                        {
                            MethodInfo mi = myType.GetMethod("GetMenuShipDocumentList", BindingFlags.Public | BindingFlags.Static);
                            rez = (ToolStripMenuItem)mi.Invoke(myType, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Com.Log.EventSave(string.Format("Произошла ошибка при получении менюшки для документа с типом {0}. {1}", TypPlg, ex.Message), "DocumentFarm.GetMenuShipDocumentList", AlgoritmCRPT.Lib.EventEn.Error, true, false);
                throw;
            }

            return rez;
        }
        */
    }
}
