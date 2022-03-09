﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using AlgoritmCashFunc.Com.Provider.Lib;
using AlgoritmCashFunc.BLL;

namespace AlgoritmCashFunc.Lib
{
    /// <summary>
    /// Универсальный провайдер
    /// </summary>
    public class UProvider : ProviderBase.UProviderBase, ProviderI
    {
        /// <summary>
        /// Базовый провайдер
        /// </summary>
        private ProviderBase PrvB;

        /// <summary>
        /// Интерфейс провайдера
        /// </summary>
        private ProviderI PrvI;

        /// <summary>
        /// Тип провайдера
        /// </summary>
        public string PrvInType
        {
            get { return (this.PrvB == null ? null : this.PrvB.PlugInType.Name); }
            private set { }
        }

        /// <summary>
        /// Строка подключения
        /// </summary>
        public string ConnectionString
        {
            get { return this.PrvB.ConnectionString; }
            private set { }
        }

        /// <summary>
        /// Версия источника данных
        /// </summary>
        /// <returns>Возвращет значение версии источника данных в случае возможности получения подключения</returns>
        public string VersionDB
        {
            get { return this.PrvB.VersionDB; }
            private set { }
        }

        /// <summary>
        /// Возвращаем версию драйвера
        /// </summary>
        /// <returns></returns>
        public string Driver
        {
            get { return this.PrvB.Driver; }
            private set { }
        }

        /// <summary>
        /// Доступно ли подключение или нет
        /// </summary>
        /// <returns>true Если смогли подключиться к базе данных</returns>
        public bool HashConnect
        {
            get { return this.PrvB.HashConnect(); }
            private set { }
        }

        /// <summary>
        /// Печать строки подключения с маскировкой секретных данных
        /// </summary>
        /// <returns>Строка подклюения с замасированной секретной информацией</returns>
        public string PrintConnectionString()
        {
            try
            {
                return this.PrvI.PrintConnectionString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Получаем элемент меню для получения информации по плагину
        /// </summary>
        public ToolStripMenuItem InfoToolStripMenuItem()
        {
            return (this.PrvB == null ? null : this.PrvB.InfoToolStripMenuItem);
        }

        /// <summary>
        /// Конструктор по созданию универсального плагина
        /// </summary>
        /// <param name="PrvFullName">Имя плагина с которым хотим работать</param>
        /// <param name="ConnectionString">Строка подключения</param>
        public UProvider(string PrvFullName, string ConnectionString)
        {
            if (PrvFullName == null || PrvFullName.Trim() == string.Empty) throw new ApplicationException(string.Format("Не можем создать провайдер указанного типа: ({0})", PrvFullName == null ? "" : PrvFullName.Trim()));

            // Получаем инфу о класса 1 параметр полный путь например "EducationAnyProvider.Provider.MSSQL.MsSqlProvider", 2 параметр пропускать или не пропускать ошибки сейчас пропускаем, а третий учитывать или нет регистр из первого параметра
            //, если первый параметр нужно взять из другой зборки то сначала её загружаем Assembly asm = Assembly.LoadFrom("MyApp.exe"); а потом тоже самое только первый параметр кажется будет так "Reminder.Common.PLUGIN.MonitoringSetNedost, РЕШЕНИЕ" 
            Type myType = Type.GetType("AlgoritmCashFunc.Com.Provider." + PrvFullName.Trim(), false, true);

            // Проверяем реализовывает ли класс наш интерфейс если да то это провайдер который можно подкрузить
            bool flagI = false;
            foreach (Type i in myType.GetInterfaces())
            {
                if (i.FullName == "AlgoritmCashFunc.Com.Provider.Lib.ProviderI") flagI = true;
            }
            if (!flagI) throw new ApplicationException("Класс который вы грузите не реализовывает интерфейс (ProviderI)");

            // Проверяем что наш клас наследует PlugInBase 
            bool flagB = false;
            foreach (MemberInfo mi in myType.GetMembers())
            {
                if (mi.DeclaringType.FullName == "AlgoritmCashFunc.Com.Provider.Lib.ProviderBase") flagB = true;
            }
            if (!flagB) throw new ApplicationException("Класс который вы грузите не наследует от класса ProviderBase");


            // Проверяем конструктор нашего класса  
            bool flag = false;
            string nameConstructor;
            foreach (ConstructorInfo ctor in myType.GetConstructors())
            {
                nameConstructor = myType.Name;

                // получаем параметры конструктора  
                ParameterInfo[] parameters = ctor.GetParameters();

                // если в этом конструктаре 1 параметр то проверяем тип и имя параметра  
                if (parameters.Length == 1)
                {

                    if (parameters[0].ParameterType.Name == "String" && parameters[0].Name == "ConnectionString") flag = true;

                }
            }
            if (!flag) throw new ApplicationException("Класс который вы грузите не имеет конструктора (string ConnectionString)");

            // Создаём экземпляр объекта  
            object[] targ = { ConnectionString };
            object obj = Activator.CreateInstance(myType, targ);
            this.PrvB = (ProviderBase)obj;
            this.PrvI = (ProviderI)obj;

            base.UPoviderSetupForProviderBase(this.PrvB, this);
        }
        public UProvider(string PrvFullName)
            : this(PrvFullName, null)
        { }


        /// <summary>
        /// Метод для записи информации в лог
        /// </summary>
        /// <param name="Message">Сообщение</param>
        /// <param name="Source">Источник</param>
        /// <param name="evn">Тип события</param>
        public void EventSave(string Message, string Source, EventEn evn)
        {
            this.PrvB.EventSave(Message, Source, evn);
        }

        /// <summary>
        /// Получаем список доступных провайдеров
        /// </summary>
        /// <returns>Список имён доступных провайдеров</returns>
        public static List<string> ListProviderName()
        {
            List<string> ProviderName = new List<string>();

            Type[] typelist = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "AlgoritmCashFunc.Com.Provider").ToArray();


            foreach (Type item in typelist)
            {
                // Проверяем реализовывает ли класс наш интерфейс если да то это провайдер который можно подкрузить
                bool flagI = false;
                foreach (Type i in item.GetInterfaces())
                {
                    if (i.FullName == "AlgoritmCashFunc.Com.Provider.Lib.ProviderI") flagI = true;
                }
                if (!flagI) continue;

                // Проверяем что наш клас наследует PlugInBase 
                bool flagB = false;
                foreach (MemberInfo mi in item.GetMembers())
                {
                    if (mi.DeclaringType.FullName == "AlgoritmCashFunc.Com.Provider.Lib.ProviderBase") flagB = true;
                }
                if (!flagB) continue;


                // Проверяем конструктор нашего класса  
                bool flag = false;
                string nameConstructor;
                foreach (ConstructorInfo ctor in item.GetConstructors())
                {
                    nameConstructor = item.Name;

                    // получаем параметры конструктора  
                    ParameterInfo[] parameters = ctor.GetParameters();

                    // если в этом конструктаре 1 параметр то проверяем тип и имя параметра  
                    if (parameters.Length == 1)
                    {

                        if (parameters[0].ParameterType.Name == "String" && parameters[0].Name == "ConnectionString") flag = true;

                    }
                }
                if (!flag) continue;

                ProviderName.Add(item.Name);
            }

            return ProviderName;
        }

        /// <summary>
        /// Процедура вызывающая настройку подключения
        /// </summary>
        /// <param name="Uprv">Ссылка на универсальный провайдер</param>
        /// <returns>Возвращает значение требуется ли сохранить подключение как основное или нет</returns>
        public bool SetupConnectDB()
        {
            return this.PrvI.SetupConnectDB();
        }

        /// <summary>
        /// Получение любых данных из источника например чтобы плагины могли что-то дополнительно читать
        /// </summary>
        /// <param name="SQL">Собственно запрос</param>
        /// <returns>Результата В виде таблицы</returns>
        public DataTable getData(string SQL)
        {
            return this.PrvB.getData(SQL);
        }

        /// <summary>
        /// Выполнение любых запросов на источнике
        /// </summary>
        /// <param name="SQL">Собственно запрос</param>
        public void setData(string SQL)
        {
            this.PrvB.setData(SQL);
        }

        /// <summary>
        /// Получение списка операций из базы данных 
        /// </summary>
        /// <returns>Стандартный список операций</returns>
        public OperationList GetOperationList()
        {
            return this.PrvI.GetOperationList();
        }

        /// <summary>
        /// Сохранение Operation в базе
        /// </summary>
        /// <param name="NewOperation">Новый Operation который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        public int SetOperation(Operation NewOperation)
        {
            return this.PrvI.SetOperation(NewOperation);
        }

        /// <summary>
        /// Обновление Operation в базе
        /// </summary>
        /// <param name="UpdOperation">Обновляемый Operation</param>
        public void UpdateOperation(Operation UpdOperation)
        {
            this.PrvI.UpdateOperation(UpdOperation);
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationPrihod
        /// </summary>
        /// <param name="OperationPrihod">Объект OperationPrihod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashOperationPrihod(BLL.OperationPlg.OperationPrihod OperationPrihod)
        {
            return this.PrvI.HashOperationPrihod(OperationPrihod);
        }

        /// <summary>
        /// Читаем информацию по объекту OperationPrihod
        /// </summary>
        /// <param name="OperationPrihod">Объект OperationPrihod который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли проччитать объект или нет</returns>
        public bool GetOperationPrihod(ref BLL.OperationPlg.OperationPrihod OperationPrihod)
        {
            return this.PrvI.GetOperationPrihod(ref OperationPrihod);
        }

        /// <summary>
        /// Вставка новой информации в объект OperationPrihod
        /// </summary>
        /// <param name="NewOperationPrihod">Вставляем в базу информацию по объекту OperationPrihod</param>
        public void SetOperationPrihod(BLL.OperationPlg.OperationPrihod NewOperationPrihod)
        {
            this.PrvI.SetOperationPrihod(NewOperationPrihod);
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationPrihod
        /// </summary>
        /// <param name="UpdOperationPrihod">Сам объект данные которого нужно обновить</param>
        public void UpdateOperationPrihod(BLL.OperationPlg.OperationPrihod UpdOperationPrihod)
        {
            this.PrvI.UpdateOperationPrihod(UpdOperationPrihod);
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationRashod
        /// </summary>
        /// <param name="OperationRashod">Объект OperationRashod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashOperationRashod(BLL.OperationPlg.OperationRashod OperationRashod)
        {
            return this.PrvI.HashOperationRashod(OperationRashod);
        }

        /// <summary>
        /// Читаем информацию по объекту OperationRashod
        /// </summary>
        /// <param name="OperationRashod">Объект OperationRashod который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли проччитать объект или нет</returns>
        public bool GetOperationRashod(ref BLL.OperationPlg.OperationRashod OperationRashod)
        {
            return this.PrvI.GetOperationRashod(ref OperationRashod);
        }

        /// <summary>
        /// Вставка новой информации в объект OperationRashod
        /// </summary>
        /// <param name="NewOperationRashod">Вставляем в базу информацию по объекту OperationRashod</param>
        public void SetOperationRashod(BLL.OperationPlg.OperationRashod NewOperationRashod)
        {
            this.PrvI.SetOperationRashod(NewOperationRashod);
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationRashod
        /// </summary>
        /// <param name="UpdOperationRashod">Сам объект данные которого нужно обновить</param>
        public void UpdateOperationRashod(BLL.OperationPlg.OperationRashod UpdOperationRashod)
        {
            this.PrvI.UpdateOperationRashod(UpdOperationRashod);
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationKasBook
        /// </summary>
        /// <param name="OperationKasBook">Объект OperationKasBook который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashOperationKasBook(BLL.OperationPlg.OperationKasBook OperationKasBook)
        {
            return this.PrvI.HashOperationKasBook(OperationKasBook);
        }

        /// <summary>
        /// Читаем информацию по объекту OperationKasBook
        /// </summary>
        /// <param name="OperationKasBook">Объект OperationKasBook который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли проччитать объект или нет</returns>
        public bool GetOperationKasBook(ref BLL.OperationPlg.OperationKasBook OperationKasBook)
        {
            return this.PrvI.GetOperationKasBook(ref OperationKasBook);
        }

        /// <summary>
        /// Вставка новой информации в объект OperationKasBook
        /// </summary>
        /// <param name="NewOperationKasBook">Вставляем в базу информацию по объекту OperationKasBook</param>
        public void SetOperationKasBook(BLL.OperationPlg.OperationKasBook NewOperationKasBook)
        {
            this.PrvI.SetOperationKasBook(NewOperationKasBook);
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationKasBook
        /// </summary>
        /// <param name="UpdOperationKasBook">Сам объект данные которого нужно обновить</param>
        public void UpdateOperationKasBook(BLL.OperationPlg.OperationKasBook UpdOperationKasBook)
        {
            this.PrvI.UpdateOperationKasBook(UpdOperationKasBook);
        }

        /// <summary>
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <returns>Получает текущий список Local из базы данных</returns>
        public LocalList GetLocalListFromDB()
        {
            return this.PrvI.GetLocalListFromDB();
        }

        /// <summary>
        /// Сохранение Local в базе
        /// </summary>
        /// <param name="NewLocal">Новый локал который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        public int SetLocal(Local NewLocal)
        {
            return this.PrvI.SetLocal(NewLocal);
        }

        /// <summary>
        /// Обновление Local в базе
        /// </summary>
        /// <param name="UpdLocal">Обновляемый локал</param>
        public void UpdateLocal(Local UpdLocal)
        {
            this.PrvI.UpdateLocal(UpdLocal);
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalKassa
        /// </summary>
        /// <param name="LocalKassa">Объект LocalKassa который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashLocalKassa(BLL.LocalPlg.LocalKassa LocalKassa)
        {
            return this.PrvI.HashLocalKassa(LocalKassa);
        }

        /// <summary>
        /// Читаем информацию по объекту LocalKassa
        /// </summary>
        /// <param name="LocalKassa">Объект LocalKassa который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли проччитать объект или нет</returns>
        public bool GetLocalKassa(ref BLL.LocalPlg.LocalKassa LocalKassa)
        {
            return this.PrvI.GetLocalKassa(ref LocalKassa);
        }

        /// <summary>
        /// Вставка новой информации в объект LocalKassa
        /// </summary>
        /// <param name="NewLocalKassa">Вставляем в базу информацию по объекту LocalKassa</param>
        public void SetLocalKassa(BLL.LocalPlg.LocalKassa NewLocalKassa)
        {
            this.PrvI.SetLocalKassa(NewLocalKassa);
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту LocalKassa
        /// </summary>
        /// <param name="UpdLocalKassa">Сам объект данные которого нужно обновить</param>
        public void UpdateLocalKassa(BLL.LocalPlg.LocalKassa UpdLocalKassa)
        {
            this.PrvI.UpdateLocalKassa(UpdLocalKassa);
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalKassa
        /// </summary>
        /// <param name="LocalPaidInReasons">Объект LocalPaidInReasons который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashLocalPaidInReasons(BLL.LocalPlg.LocalPaidInReasons LocalPaidInReasons)
        {
            return this.PrvI.HashLocalPaidInReasons(LocalPaidInReasons);
        }

        /// <summary>
        /// Читаем информацию по объекту LocalPaidInReasons
        /// </summary>
        /// <param name="LocalPaidInReasons">Объект LocalPaidInReasons который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли проччитать объект или нет</returns>
        public bool GetLocalPaidInReasons(ref BLL.LocalPlg.LocalPaidInReasons LocalPaidInReasons)
        {
            return this.PrvI.GetLocalPaidInReasons(ref LocalPaidInReasons);
        }

        /// <summary>
        /// Вставка новой информации в объект PaidInReasons
        /// </summary>
        /// <param name="NewLocalPaidInReasons">Вставляем в базу информацию по объекту LocalPaidInReasons</param>
        public void SetLocalPaidInReasons(BLL.LocalPlg.LocalPaidInReasons NewLocalPaidInReasons)
        {
            this.PrvI.SetLocalPaidInReasons(NewLocalPaidInReasons);
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту PaidInReasons
        /// </summary>
        /// <param name="UpdLocalPaidInReasons">Сам объект данные которого нужно обновить</param>
        public void UpdateLocalPaidInReasons(BLL.LocalPlg.LocalPaidInReasons UpdLocalPaidInReasons)
        {
            this.PrvI.UpdateLocalPaidInReasons(UpdLocalPaidInReasons);
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalKassa
        /// </summary>
        /// <param name="LocalPaidRashReasons">Объект LocalPaidRashReasons который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashLocalPaidRashReasons(BLL.LocalPlg.LocalPaidRashReasons LocalPaidRashReasons)
        {
            return this.PrvI.HashLocalPaidRashReasons(LocalPaidRashReasons);
        }

        /// <summary>
        /// Читаем информацию по объекту LocalPaidRashReasons
        /// </summary>
        /// <param name="LocalPaidRashReasons">Объект LocalPaidRashReasons который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли проччитать объект или нет</returns>
        public bool GetLocalPaidRashReasons(ref BLL.LocalPlg.LocalPaidRashReasons LocalPaidRashReasons)
        {
            return this.PrvI.GetLocalPaidRashReasons(ref LocalPaidRashReasons);
        }

        /// <summary>
        /// Вставка новой информации в объект PaidRashReasons
        /// </summary>
        /// <param name="NewLocalPaidRashReasons">Вставляем в базу информацию по объекту LocalPaidRashReasons</param>
        public void SetLocalPaidRashReasons(BLL.LocalPlg.LocalPaidRashReasons NewLocalPaidRashReasons)
        {
            this.PrvI.SetLocalPaidRashReasons(NewLocalPaidRashReasons);
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту PaidRashReasons
        /// </summary>
        /// <param name="UpdLocalPaidRashReasons">Сам объект данные которого нужно обновить</param>
        public void UpdateLocalPaidRashReasons(BLL.LocalPlg.LocalPaidRashReasons UpdLocalPaidRashReasons)
        {
            this.PrvI.UpdateLocalPaidRashReasons(UpdLocalPaidRashReasons);
        }
        
        /// <summary>
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <param name="LastDay">Сколько последних дней грузить из базы данных если null значит весь период</param>
        /// <param name="OperationId">Какая операция нас интересует, если </param>
        /// <returns>Получает список Document из базы данных удовлетворяющий фильтрам</returns>
        public DocumentList GetDocumentListFromDB(int? LastDay, int? OperationId)
        {
            return this.PrvI.GetDocumentListFromDB(LastDay, OperationId);
        }

        /// <summary>
        /// Сохранение Local в базе
        /// </summary>
        /// <param name="NewDocument">Новый документ который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        public int SetDocument(Document NewDocument)
        {
            return this.PrvI.SetDocument(NewDocument);
        }

        /// <summary>
        /// Обновление Documentl в базе
        /// </summary>
        /// <param name="UpdDocument">Обновляемый документ</param>
        public void UpdateDocument(Document UpdDocument)
        {
            this.PrvI.UpdateDocument(UpdDocument);
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentPrihod
        /// </summary>
        /// <param name="DocumentPrihod">Объект DocumentPrihod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashDocumentPrihod(BLL.DocumentPlg.DocumentPrihod DocumentPrihod)
        {
            return this.PrvI.HashDocumentPrihod(DocumentPrihod);
        }

        /// <summary>
        /// Читаем информацию по объекту DocumentPrihod
        /// </summary>
        /// <param name="DocumentPrihod">Объект DocumentPrihod который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли проччитать объект или нет</returns>
        public bool GetDocumentPrihod(ref BLL.DocumentPlg.DocumentPrihod DocumentPrihod)
        {
            return this.PrvI.GetDocumentPrihod(ref DocumentPrihod);
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentPrihod
        /// </summary>
        /// <param name="NewDocumentPrihod">Вставляем в базу информацию по объекту DocumentPrihod</param>
        public void SetDocumentPrihod(BLL.DocumentPlg.DocumentPrihod NewDocumentPrihod)
        {
            this.PrvI.SetDocumentPrihod(NewDocumentPrihod);
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentPrihod
        /// </summary>
        /// <param name="UpdDocumentPrihod">Сам объект данные которого нужно обновить</param>
        public void UpdateDocumentPrihod(BLL.DocumentPlg.DocumentPrihod UpdDocumentPrihod)
        {
            this.PrvI.UpdateDocumentPrihod(UpdDocumentPrihod);
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentRashod
        /// </summary>
        /// <param name="DocumentRashod">Объект DocumentRashod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashDocumentRashod(BLL.DocumentPlg.DocumentRashod DocumentRashod)
        {
            return this.PrvI.HashDocumentRashod(DocumentRashod);
        }

        /// <summary>
        /// Читаем информацию по объекту DocumentRashod
        /// </summary>
        /// <param name="DocumentRashod">Объект DocumentRashod который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли проччитать объект или нет</returns>
        public bool GetDocumentRashod(ref BLL.DocumentPlg.DocumentRashod DocumentRashod)
        {
            return this.PrvI.GetDocumentRashod(ref DocumentRashod);
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentRashod
        /// </summary>
        /// <param name="NewDocumentRashod">Вставляем в базу информацию по объекту DocumentRashod</param>
        public void SetDocumentRashod(BLL.DocumentPlg.DocumentRashod NewDocumentRashod)
        {
            this.PrvI.SetDocumentRashod(NewDocumentRashod);
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentRashod
        /// </summary>
        /// <param name="UpdDocumentRashod">Сам объект данные которого нужно обновить</param>
        public void UpdateDocumentRashod(BLL.DocumentPlg.DocumentRashod UpdDocumentRashod)
        {
            this.PrvI.UpdateDocumentRashod(UpdDocumentRashod);
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentKasBook
        /// </summary>
        /// <param name="DocumentKasBook">Объект DocumentKasBook который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashDocumentKasBook(BLL.DocumentPlg.DocumentKasBook DocumentKasBook)
        {
            return this.PrvI.HashDocumentKasBook(DocumentKasBook);
        }

        /// <summary>
        /// Читаем информацию по объекту DocumentKasBook
        /// </summary>
        /// <param name="DocumentKasBook">Объект DocumentKasBook который нужно прочитать в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли проччитать объект или нет</returns>
        public bool GetDocumentKasBook(ref BLL.DocumentPlg.DocumentKasBook DocumentKasBook)
        {
            return this.PrvI.GetDocumentKasBook(ref DocumentKasBook);
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentKasBook
        /// </summary>
        /// <param name="NewDocumentKasBook">Вставляем в базу информацию по объекту DocumentKasBook</param>
        public void SetDocumentKasBook(BLL.DocumentPlg.DocumentKasBook NewDocumentKasBook)
        {
            this.PrvI.SetDocumentKasBook(NewDocumentKasBook);
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentKasBook
        /// </summary>
        /// <param name="UpdDocumentKasBook">Сам объект данные которого нужно обновить</param>
        public void UpdateDocumentKasBook(BLL.DocumentPlg.DocumentKasBook UpdDocumentKasBook)
        {
            this.PrvI.UpdateDocumentKasBook(UpdDocumentKasBook);
        }
    }
}
