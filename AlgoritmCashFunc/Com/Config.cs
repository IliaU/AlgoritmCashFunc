using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.IO;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.Com
{
    /// <summary>
    /// Класс для работы с конфигурационным фалом
    /// </summary>
    public class Config
    {
        #region Private Param
        private static Config obj = null;

        /// <summary>
        /// Версия XML файла
        /// </summary>
        private static int _Version = 1;

        /// <summary>
        /// Флаг трассировки
        /// </summary>
        private static bool _Trace = false;

        /// <summary>
        /// Объект XML файла
        /// </summary>
        private static XmlDocument Document = new XmlDocument();

        /// <summary>
        /// Корневой элемент нашего документа
        /// </summary>
        private static XmlElement xmlRoot;

        /// <summary>
        /// Корневой элемент лицензий
        /// </summary>
        private static XmlElement xmlLics;

        /// <summary>
        /// Корневой элемент пользователей
        /// </summary>
        private static XmlElement xmlUsers;
        
        #endregion

        #region Public Param

        /// <summary>
        /// Файл в котором мы храним конфиг
        /// </summary>
        public static string FileXml { get; private set; }

        /// <summary>
        /// Версия XML файла
        /// </summary>
        public static int Version { get { return _Version; } private set { } }

        /// <summary>
        /// Флаг трассировки
        /// </summary>
        public static bool Trace
        {
            get
            {
                return _Trace;
            }
            set
            {
                xmlRoot.SetAttribute("Trace", value.ToString());
                Save();
                _Trace = value;
            }
        }
        #endregion

        #region Puplic Method

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="FileConfig"></param>
        public Config(string FileConfig)
        {
            try
            {
                if (obj == null) FileXml = "AlgoritmCashFunc.xml";
                else FileXml = FileConfig;

                obj = this;
                Log.EventSave("Чтение конфигурационного файла", GetType().Name, EventEn.Message);

                // Читаем файл или создаём
                if (File.Exists(Environment.CurrentDirectory + @"\" + FileXml)) { Load(); }
                else { Create(); }

                // Получаем кастомизированный объект
                GetDate();

                // Подписываемся на события
                Com.Lic.onCreatedLicKey += new EventHandler<LicLib.onLicEventKey>(Lic_onCreatedLicKey);
                Com.Lic.onRegNewKey += new EventHandler<LicLib.onLicItem>(Lic_onRegNewKey);
                Com.ProviderFarm.onEventSetup += new EventHandler<EventProviderFarm>(ProviderFarm_onEventSetup);
                Com.UserFarm.List.onUserListAddedUser += new EventHandler<User>(List_onUserListAddedUser);
                Com.UserFarm.List.onUserListDeletedUser += new EventHandler<User>(List_onUserListDeletedUser);
                Com.UserFarm.List.onUserListUpdatedUser += new EventHandler<User>(List_onUserListUpdatedUser);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при загрузке конфигурации с ошибкой: {0}", ex.Message));
                Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="FileConfig"></param>
        public Config()
            : this(null)
        {
        }

        
        #endregion

        #region Private Method

        /// <summary>
        /// Читеам файл конфигурации
        /// </summary>
        private static void Load()
        {
            try
            {
                lock (obj)
                {
                    Document.Load(Environment.CurrentDirectory + @"\" + FileXml);
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при загрузке конфигурации с ошибкой: {0}", ex.Message));
                Log.EventSave(ae.Message, ".Load()", EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Сохраняем конфигурацию  в файл
        /// </summary>
        private static void Save()
        {
            try
            {
                lock (obj)
                {
                    Document.Save(Environment.CurrentDirectory + @"\" + FileXml);
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при сохранении конфигурации в файл с ошибкой: {0}", ex.Message));
                Log.EventSave(ae.Message, ".Save()", EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Создание нового файла
        /// </summary>
        private static void Create()
        {
            try
            {
                lock (obj)
                {
                    // создаём строку инициализации
                    XmlElement wbRoot = Document.DocumentElement;
                    XmlDeclaration wbxmdecl = Document.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    Document.InsertBefore(wbxmdecl, wbRoot);

                    // Создаём начальное тело с которым мы будем потом работать
                    XmlElement xmlMain = Document.CreateElement("AlgoritmCashFunc");
                    xmlMain.SetAttribute("Version", _Version.ToString());
                    xmlMain.SetAttribute("Trace", _Trace.ToString());
                    xmlMain.SetAttribute("PrvFullName", null);
                    xmlMain.SetAttribute("ConnectionString", "");
                    Document.AppendChild(xmlMain);

                    XmlElement xmlLics = Document.CreateElement("Lics");
                    xmlMain.AppendChild(xmlLics);

                    XmlElement xmlUsers = Document.CreateElement("Users");
                    xmlMain.AppendChild(xmlUsers);
                    
                    // Сохраняем документ
                    Save();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при соpдании конфигурационного файла с ошибкой: {0}", ex.Message));
                Log.EventSave(ae.Message, ".Create()", EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Получение кастомизированного запроса
        /// </summary>
        private static void GetDate()
        {
            ApplicationException appM = new ApplicationException("Неправильный настроечный файл, скорее всего не от этой программы.");
            ApplicationException appV = new ApplicationException(string.Format("Неправильная версия настроечного яайла, требуется {0} версия", _Version));
            try
            {
                lock (obj)
                {
                    xmlRoot = Document.DocumentElement;

                    // Проверяем значения заголовка
                    if (xmlRoot.Name != "AlgoritmCashFunc") throw appM;
                    if (Version < int.Parse(xmlRoot.GetAttribute("Version"))) throw appV;
                    if (Version > int.Parse(xmlRoot.GetAttribute("Version"))) UpdateVersionXml(xmlRoot, int.Parse(xmlRoot.GetAttribute("Version")));

                    string PrvFullName = null;
                    string ConnectionString = null;

                    // Получаем значения из заголовка
                    for (int i = 0; i < xmlRoot.Attributes.Count; i++)
                    {
                        if (xmlRoot.Attributes[i].Name == "Trace") try { _Trace = bool.Parse(xmlRoot.Attributes[i].Value.ToString()); } catch (Exception) { }
                        if (xmlRoot.Attributes[i].Name == "PrvFullName") PrvFullName = xmlRoot.Attributes[i].Value.ToString();
                        try { if (xmlRoot.Attributes[i].Name == "ConnectionString") ConnectionString = Com.Lic.DeCode(xmlRoot.Attributes[i].Value.ToString()); }
                        catch (Exception) { }
                    }

                    // Подгружаем провайдер
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(PrvFullName) && !string.IsNullOrWhiteSpace(ConnectionString))
                        {
                            Com.ProviderFarm.Setup(new UProvider(PrvFullName, ConnectionString), false);
                        }
                    }
                    catch (Exception) { }

                    // Получаем список вложенных объектов
                    foreach (XmlElement iMain in xmlRoot.ChildNodes)
                    {
                        switch (iMain.Name)
                        {
                            case "Lics":
                                xmlLics = iMain;
                                foreach (XmlElement xkey in iMain.ChildNodes)
                                {
                                    try
                                    {
                                        string MachineName = null;
                                        string UserName = null;
                                        string ActivNumber = null;
                                        string LicKey = null;
                                        int ValidToYYYYMMDD = 0;
                                        string Info = null;
                                        bool HashUserOS = false;
                                        List<string> ScnFullNameList = new List<string>();

                                        //Получаем данные по параметру из файла
                                        for (int i = 0; i < xkey.Attributes.Count; i++)
                                        {
                                            if (xkey.Attributes[i].Name == "MachineName") { MachineName = xkey.Attributes[i].Value; }
                                            if (xkey.Attributes[i].Name == "UserName") { UserName = xkey.Attributes[i].Value; }
                                            if (xkey.Attributes[i].Name == "ActivNumber") { ActivNumber = xkey.Attributes[i].Value; }
                                            if (xkey.Attributes[i].Name == "LicKey") { LicKey = xkey.Attributes[i].Value; }
                                            if (xkey.Attributes[i].Name == "ValidToYYYYMMDD") { try { ValidToYYYYMMDD = int.Parse(xkey.Attributes[i].Value); } catch { } }
                                            if (xkey.Attributes[i].Name == "Info") { Info = xkey.Attributes[i].Value; }
                                            try { if (xkey.Attributes[i].Name == "HashUserOS") { HashUserOS = bool.Parse(xkey.Attributes[i].Value); } }
                                            catch (Exception) { }
                                        }
                                        if (!string.IsNullOrWhiteSpace(xkey.InnerText))
                                        {
                                            foreach (string sitem in xkey.InnerText.Split(','))
                                            {
                                                ScnFullNameList.Add(sitem);
                                            }
                                        }

                                        // Проверяем валидность подгруженного ключа
                                        if (!string.IsNullOrWhiteSpace(LicKey)) //&& Com.Lic.IsValidLicKey(LicKey)
                                        {
                                            Com.Lic.IsValidLicKey(LicKey);
                                            // Если ключь валидный то сохраняем его в списке ключей
                                            //Com.LicLib.onLicEventKey newKey = new Com.LicLib.onLicEventKey(MachineName, UserName, ActivNumber, LicKey, ValidToYYYYMMDD, Info, HashUserOS, ScnFullNameList);
                                            //Com.Lic.IsValidLicKey( .Add(newKey);
                                        }
                                    }
                                    catch { } // Если ключь прочитать не удалось или он не подходит, то исключения выдавать не нужно
                                }
                                break;
                            case "Users":
                                xmlUsers = iMain;
                                foreach (XmlElement xuser in iMain.ChildNodes)
                                {
                                    string Logon = xuser.Name;
                                    string Password = null;
                                    string Description = null;
                                    Lib.RoleEn Role = RoleEn.None;
                                    int? EmploeeId = null;

                                    foreach (XmlAttribute auser in xuser.Attributes)
                                    {
                                        if (auser.Name == "Password") Password = Com.Lic.DeCode(xuser.GetAttribute(auser.Name));
                                        if (auser.Name == "Description") Description = xuser.GetAttribute(auser.Name);
                                        if (auser.Name == "Role") Role = Lib.EventConvertor.Convert(xuser.GetAttribute(auser.Name), Role);
                                        if (auser.Name == "EmploeeId" && !string.IsNullOrWhiteSpace(xuser.GetAttribute(auser.Name))) EmploeeId = int.Parse(xuser.GetAttribute(auser.Name));
                                    }

                                    // Если пароль не указан, то пользователя всё равно нужно добавить, просто при запуске он должен будет придумать пароль
                                    if (!string.IsNullOrWhiteSpace(Logon) && Role != RoleEn.None)
                                    {
                                        try
                                        {
                                            UserFarm.List.Add(new Lib.User(Logon, Password, Description, Role, EmploeeId), true, false);
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.EventSave(string.Format("Не смогли добавить пользователя с именем {0} при чтении конфигурационного файла: {1}", Logon, ex.Message), obj.GetType().Name + ".GetDate()", EventEn.Error);
                                        }

                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при разборе конфигурационного файла с ошибкой: {0}", ex.Message));
                Log.EventSave(ae.Message, ".GetDate()", EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Обновление до нужной версии
        /// </summary>
        /// <param name="root">Корневой элемент</param>
        /// <param name="oldVersion">Версия файла из конфига</param>
        private static void UpdateVersionXml(XmlElement root, int oldVersion)
        {
            try
            {
                if (oldVersion <= 2)
                {

                }

                root.SetAttribute("Version", _Version.ToString());
                Save();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при обновлении конфигурационного в файла с ошибкой: {0}}", ex.Message));
                Log.EventSave(ae.Message, ".UpdateVersionXml(XmlElement root, int oldVersion)", EventEn.Error);
                throw ae;
            }
        }

        // Событие регистрации нового ключа
        void Lic_onRegNewKey(object sender, LicLib.onLicItem e)
        {
            try
            {
                if (xmlLics == null)
                {
                    xmlLics = Document.CreateElement("Lics");
                    xmlRoot.AppendChild(xmlLics);
                }

                XmlElement k = Document.CreateElement("Key");
                if (e._LicEventKey.MachineName != null) k.SetAttribute("MachineName", e._LicEventKey.MachineName);
                if (e._LicEventKey.UserName != null) k.SetAttribute("UserName", e._LicEventKey.UserName);
                if (e._LicEventKey.ActivNumber != null) k.SetAttribute("ActivNumber", e._LicEventKey.ActivNumber);
                if (e._LicEventKey.LicKey != null) k.SetAttribute("LicKey", e._LicEventKey.LicKey);
                if (e._LicEventKey.ValidToYYYYMMDD != 0) k.SetAttribute("ValidToYYYYMMDD", e._LicEventKey.ValidToYYYYMMDD.ToString());
                if (e._LicEventKey.Info != null) k.SetAttribute("Info", e._LicEventKey.Info);
                k.SetAttribute("HashUserOS", e._LicEventKey.HashUserOS.ToString());
                k.InnerText = string.Join(",", e._LicEventKey.ScnFullNameList.ToArray());
                xmlLics.AppendChild(k);

                Save();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при сохранении во время создания нового ключа в файл xml: {0}", ex.Message));
                Log.EventSave(ae.Message, obj.GetType().Name + ".Lic_onCreatedLicKey()", EventEn.Error);
                throw ae;
            }
        }
        //
        // Событие создания нового ключа
        void Lic_onCreatedLicKey(object sender, LicLib.onLicEventKey e)
        {
            try
            {
                if (xmlLics == null)
                {
                    xmlLics = Document.CreateElement("Lics");
                    xmlRoot.AppendChild(xmlLics);
                }

                XmlElement k = Document.CreateElement("Key");
                if (e.MachineName != null) k.SetAttribute("MachineName", e.MachineName);
                if (e.UserName != null) k.SetAttribute("UserName", e.UserName);
                if (e.ActivNumber != null) k.SetAttribute("ActivNumber", e.ActivNumber);
                if (e.LicKey != null) k.SetAttribute("LicKey", e.LicKey);
                if (e.ValidToYYYYMMDD != 0) k.SetAttribute("ValidToYYYYMMDD", e.ValidToYYYYMMDD.ToString());
                if (e.Info != null) k.SetAttribute("Info", e.Info);
                k.SetAttribute("HashUserOS", e.HashUserOS.ToString());
                k.InnerText = string.Join(",", e.ScnFullNameList.ToArray());
                xmlLics.AppendChild(k);

                Save();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при сохранении во время создания нового ключа в файл xml: {0}", ex.Message));
                Log.EventSave(ae.Message, obj.GetType().Name + ".Lic_onCreatedLicKey()", EventEn.Error);
                throw ae;
            }
        }

        // Событие изменения текщего провайдера
        private void ProviderFarm_onEventSetup(object sender, EventProviderFarm e)
        {
            try
            {
                XmlElement root = Document.DocumentElement;

                root.SetAttribute("PrvFullName", e.Uprv.PrvInType);
                try { root.SetAttribute("ConnectionString", Com.Lic.InCode(e.Uprv.ConnectionString)); }
                catch (Exception) { }

                Com.LocalFarm.UpdateLocalListFromDB();
                Com.OperationFarm.UpdateOperationList();
                Com.DocumentFarm.UpdateDocumentListFromDB();

                Save();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException("Упали при изменении файла конфигурации с ошибкой: " + ex.Message);
                Log.EventSave(ae.Message, obj.GetType().Name + ".ProviderFarm_onEventSetup()", EventEn.Error);
                throw ae;
            }
        }

        // Событие добавления пользователя
        private void List_onUserListAddedUser(object sender, User e)
        {
            try
            {
                if (e.Logon == "Console" && e.Password == "123456" && e.Description == "Console" && e.Role == RoleEn.Admin)
                {
                    // Это системная запись её нельзя использовать
                }
                else
                {
                    XmlElement u = Document.CreateElement(e.Logon);
                    if (e.Password != null) u.SetAttribute("Password", Com.Lic.InCode(e.Password));
                    if (e.Description != null) u.SetAttribute("Description", e.Description);
                    if (e.Role != RoleEn.None) u.SetAttribute("Role", e.Role.ToString());
                    u.SetAttribute("EmploeeId", string.Format("{0}", e.EmploeeId));

                    xmlUsers.AppendChild(u);

                    Save();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при добавлении пользователя {0} в файл xml: {1}", e.Logon, ex.Message));
                Log.EventSave(ae.Message, obj.GetType().Name + ".List_onUserListAddedUser()", EventEn.Error);
                throw ae;
            }
        }

        // Событие удаления пользователя
        private void List_onUserListDeletedUser(object sender, User e)
        {
            try
            {
                // Получаем список объектов
                foreach (XmlElement item in xmlUsers.ChildNodes)
                {
                    if (item.Name == e.Logon)
                    {
                        xmlUsers.RemoveChild(item);
                    }
                }

                Save();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при удалении пользователя {0} из файла xml: {1}", e.Logon, ex.Message));
                Log.EventSave(ae.Message, obj.GetType().Name + ".List_onUserListDeletedUser()", EventEn.Error);
                throw ae;
            }
        }

        // Событие изменения данных пользователя
        private void List_onUserListUpdatedUser(object sender, User e)
        {
            try
            {
                if (e.Logon == "Console" && e.Password == "123456" && e.Description == "Console" && e.Role == RoleEn.Admin)
                {
                    // Это системная запись её нельзя использовать
                }
                else
                {

                    // Получаем список объектов
                    foreach (XmlElement item in xmlUsers.ChildNodes)
                    {
                        if (item.Name == e.Logon)
                        {
                            if (e.Password != null) item.SetAttribute("Password", Com.Lic.InCode(e.Password));
                            if (e.Description != null) item.SetAttribute("Description", e.Description);
                            if (e.Role != RoleEn.None) item.SetAttribute("Role", e.Role.ToString());
                            item.SetAttribute("EmploeeId", string.Format("{0}",e.EmploeeId)); 
                        }
                    }

                    Save();
                }
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при изменении данных о пользователе {0} в файл xml: {1}", e.Logon, ex.Message));
                Log.EventSave(ae.Message, obj.GetType().Name + ".List_onUserListUpdatedUser()", EventEn.Error);
                throw ae;
            }
        }

        #endregion

        #region Вложенные классы
        #endregion
    }
}
