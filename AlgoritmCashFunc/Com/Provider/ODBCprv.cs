using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Data.Odbc;
using System.Data;
using AlgoritmCashFunc.Com.Provider.Lib;
using AlgoritmCashFunc.Lib;
using System.Threading;
using AlgoritmCashFunc.BLL;

namespace AlgoritmCashFunc.Com.Provider
{
    /// <summary>
    /// Провайдер для работы по подключению типа ODBC
    /// </summary>
    public sealed class ODBCprv : ProviderBase, ProviderI
    {
        #region Private Param
        private string ServerVersion;
        public string DriverOdbc;
        #endregion

        #region Puplic Param
        // Билдер строки подключения
        public OdbcConnectionStringBuilder Ocsb = new OdbcConnectionStringBuilder();
        #endregion

        #region Puplic metod

        /// <summary>
        /// Контруктор
        /// </summary>
        /// <param name="ConnectionString">Строка подключения</param>
        public ODBCprv(string ConnectionString)
        {
            try
            {
                //Генерим ячейку элемент меню для отображения информации по плагину
                using (ToolStripMenuItem InfoToolStripMenuItem = new ToolStripMenuItem(this.GetType().Name))
                {
                    InfoToolStripMenuItem.Text = "Провайдер для работы с базой через OLEDB";
                    InfoToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F);
                    //InfoToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
                    //InfoToolStripMenuItem.Image = (Image)(new Icon(Type.GetType("Reminder.Common.PLUGIN.DwMonPlg.DwMonInfo"), "DwMon.ico").ToBitmap()); // для нормальной раьботы ресурс должен быть внедрённый
                    InfoToolStripMenuItem.Click += new EventHandler(InfoToolStripMenuItem_Click);

                    // Настраиваем компонент
                    base.SetupProviderBase(this.GetType(), InfoToolStripMenuItem, ConnectionString);
                }
                // Тестируем подключение и получаем информациию по версии базе данных
                if (ConnectionString != null && ConnectionString.Trim() != string.Empty)
                {
                    testConnection(ConnectionString, false);

                    // Устанавливаем в базовом классе строку подключения (котрая не меняется) и версию источника, чтобы не нужно было делать проверки коннектов
                    base.SetupConnectionStringAndVersionDB(ConnectionString, this.ServerVersion, this.DriverOdbc);
                }
            }
            catch (Exception ex) { base.UPovider.EventSave(ex.Message, "ODBCprv", EventEn.Error); }
        }

        /// <summary>
        /// Печать строки подключения с маскировкой секретных данных
        /// </summary>
        /// <returns>Строка подклюения с замасированной секретной информацией</returns>
        public override string PrintConnectionString()
        {
            try
            {
                if (base.ConnectionString != null && base.ConnectionString.Trim() != string.Empty)
                {
                    this.Ocsb = new OdbcConnectionStringBuilder(base.ConnectionString);
                    object P;
                    this.Ocsb.TryGetValue("Pwd", out P);
                    string Pssword = P.ToString();

                    return base.ConnectionString.Replace(Pssword, "*****");
                }
            }
            catch (Exception ex) { base.UPovider.EventSave(ex.Message, "PrintConnectionString", EventEn.Error); }

            return null;
        }

        /// <summary>
        /// Процедура вызывающая настройку подключения
        /// </summary>
        /// <returns>Возвращаем значение диалога</returns>
        public bool SetupConnectDB()
        {
            bool rez = false;

            using (ODBC.FSetupConnectDB Frm = new ODBC.FSetupConnectDB(this))
            {
                DialogResult drez = Frm.ShowDialog();

                // Если пользователь сохраняет новую строку подключения то сохраняем её в нашем объекте
                if (drez == DialogResult.Yes)
                {
                    base.SetupConnectionStringAndVersionDB(Frm.ConnectionString, this.ServerVersion, this.DriverOdbc);
                    rez = true;
                }
            }

            return rez;
        }

        /// <summary>
        /// Установка параметров через форму провайдера(плагина)
        /// </summary>
        /// <param name="DSN">DSN</param>
        /// <param name="Login">Логин</param>
        /// <param name="Password">Пароль</param>
        /// <param name="VisibleError">Выкидывать сообщения при неудачных попытках подключения</param>
        /// <param name="WriteLog">Писать в лог информацию о проверке побключения или нет</param>
        /// <param name="InstalConnect">Установить текущую строку подключения в билдере или нет</param>
        public string InstalProvider(string DSN, string Login, string Password, bool VisibleError, bool WriteLog, bool InstalConnect)
        {
            OdbcConnectionStringBuilder OcsbTmp = new OdbcConnectionStringBuilder();
            OcsbTmp.Dsn = DSN;
            OcsbTmp.Add("Uid", Login);
            OcsbTmp.Add("Pwd", Password);

            try
            {
                if (testConnection(OcsbTmp.ConnectionString, VisibleError))
                {
                    if (InstalConnect) this.Ocsb = OcsbTmp;
                    return OcsbTmp.ConnectionString;
                }
                else return null;
            }
            catch (Exception)
            {
                if (WriteLog) Log.EventSave("Не удалось создать подключение: " + DSN, this.ToString(), EventEn.Error);
                throw;
            }
        }


        /// <summary>
        /// Получение любых данных из источника например чтобы плагины могли что-то дополнительно читать
        /// </summary>
        /// <param name="SQL">Собственно запрос</param>
        /// <returns>Результата В виде таблицы</returns>
        public override DataTable getData(string SQL)
        {
            try
            {
                if (!this.HashConnect()) throw new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            return getDataORA(SQL);
                        case "myodbc8a.dll":
                            return getDataMySql(SQL);
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                //return true;
            }
            catch (Exception ex)
            {
                // Логируем ошибку если её должен видеть пользователь или если взведён флаг трассировке в файле настройки программы
                if (Com.Config.Trace) base.EventSave(ex.Message, "getData", EventEn.Error);

                // Отображаем ошибку если это нужно
                MessageBox.Show(ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Выполнение любых запросов на источнике
        /// </summary>
        /// <param name="SQL">Собственно запрос</param>
        public override void setData(string SQL)
        {
            try
            {
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            setDataORA(SQL);
                            break;
                        case "myodbc8a.dll":
                            setDataMySql(SQL);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                //return true;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".getData", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".setData", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Получение списка операций из базы данных 
        /// </summary>
        /// <returns>Стандартный список операций</returns>
        public OperationList GetOperationList()
        {
            try
            {
                // Если мы работаем в режиме без базы то выводим тестовые записи
                if (!this.HashConnect()) throw new ApplicationException("Не установлено подключение с базой данных.");
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            return GetOperationListORA();
                        case "myodbc8a.dll":
                            return GetOperationListMySql();
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку если её должен видеть пользователь или если взведён флаг трассировке в файле настройки программы
                if (Com.Config.Trace) base.EventSave(ex.Message, "GetOperationList", EventEn.Error);

                throw ex;
            }
        }

        /// <summary>
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <returns>Получает текущий список Local из базы данных</returns>
        public LocalList GetLocalListFromDB()
        {
            try
            {
                // Если мы работаем в режиме без базы то выводим тестовые записи
                if (!this.HashConnect()) throw new ApplicationException("Не установлено подключение с базой данных.");
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            return GetLocalListFromDbORA();
                        case "myodbc8a.dll":
                            return GetLocalListFromDbMySql();
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку если её должен видеть пользователь или если взведён флаг трассировке в файле настройки программы
                if (Com.Config.Trace) base.EventSave(ex.Message, "GetLocalListFromDB", EventEn.Error);

                throw ex;
            }
        }


        /// <summary>
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <param name="LastDay">Сколько последних дней грузить из базы данных если null значит весь период</param>
        /// <param name="OperationId">Какая операция нас интересует, если </param>
        /// <returns>Получает список Document из базы данных удовлетворяющий фильтрам</returns>
        public DocumentList GetDocumentListFromDB(int? LastDay, int? OperationId)
        {
            try
            {
                // Если мы работаем в режиме без базы то выводим тестовые записи
                if (!this.HashConnect()) throw new ApplicationException("Не установлено подключение с базой данных.");
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            return GetDocumentListFromDbORA(LastDay, OperationId);
                        case "myodbc8a.dll":
                            return GetDocumentListFromDbMySql(LastDay, OperationId);
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку если её должен видеть пользователь или если взведён флаг трассировке в файле настройки программы
                if (Com.Config.Trace) base.EventSave(ex.Message, "GetDocumentListFromDB", EventEn.Error);

                throw ex;
            }
        }
        #endregion

        #region Private metod
        // Пользователь вызвал меню информации по провайдеру
        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ODBC.FInfo Frm = new ODBC.FInfo(this))
            {
                Frm.ShowDialog();
            }
        }

        /// <summary>
        /// Проверка валидности подключения
        /// </summary>
        /// <param name="ConnectionString">Строка подключения которую нужно проверить</param>
        /// <returns>Возврощает результат проверки</returns>
        private bool testConnection(string ConnectionString, bool VisibleError)
        {
            try
            {
                string tmpServerVersion;
                string tmpDriver;

                // Проверка подключения
                using (OdbcConnection con = new OdbcConnection(ConnectionString))
                {
                    con.Open();
                    tmpDriver = con.Driver;
                    tmpServerVersion = con.ServerVersion; // Если не упали, значит подключение создано верно, тогда сохраняем переданные параметры
                }


                // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                switch (tmpDriver)
                {
                    //case "SQLSRV32.DLL":
                    case "SQORA32.DLL":
                    case "SQORA64.DLL":
                        // Оракловая логика
                        break;
                    case "myodbc8a.dll":
                        // MySql логика
                        break;
                    default:
                        throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + tmpDriver);
                }

                // Если не упали значит можно сохранить текущую версию
                this.DriverOdbc = tmpDriver;
                this.ServerVersion = tmpServerVersion; // Сохраняем версию базы

                return true;
            }
            catch (Exception ex)
            {
                // Логируем ошибку если её должен видеть пользователь или если взведён флаг трассировке в файле настройки программы
                if (VisibleError || Com.Config.Trace) base.EventSave(ex.Message, "testConnection", EventEn.Error);

                // Отображаем ошибку если это нужно
                if (VisibleError) MessageBox.Show(ex.Message);
                return false;
            }
        }
        #endregion

        #region Private metod For ORACLE

        /// <summary>
        /// Получение любых данных из источника например чтобы плагины могли что-то дополнительно читать
        /// </summary>
        /// <param name="SQL">Собственно запрос</param>
        /// <returns>Результата В виде таблицы</returns>
        private DataTable getDataORA(string SQL)
        {
            DataTable rez = null;

            try
            {
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".getDataORA", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(SQL, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = new DataTable();

                                // Получаем схему таблицы
                                DataTable tt = dr.GetSchemaTable();
                                foreach (DataRow item in tt.Rows)
                                {
                                    rez.Columns.Add(new DataColumn(item["ColumnName"].ToString().ToUpper(), Type.GetType(item["DataType"].ToString())));
                                }


                                // пробегаем по строкам
                                while (dr.Read())
                                {
                                    DataRow newr = rez.NewRow();
                                    for (int i = 0; i < tt.Rows.Count; i++)
                                    {
                                        if (!dr.IsDBNull(i) && tt.Rows[i]["DataType"].ToString() == "System.Double") { newr[i] = (double)dr.GetValue(i); }
                                        if (!dr.IsDBNull(i) && tt.Rows[i]["DataType"].ToString() == "System.Decimal") { newr[i] = (Decimal)dr.GetValue(i); }
                                        if (!dr.IsDBNull(i) && tt.Rows[i]["DataType"].ToString() == "System.String") { newr[i] = (string)dr.GetValue(i); }
                                        if (!dr.IsDBNull(i) && tt.Rows[i]["DataType"].ToString() == "System.DateTime") { newr[i] = (DateTime)dr.GetValue(i); }
                                    }
                                    rez.Rows.Add(newr);
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".getDataORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".getDataORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".getDataORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".getDataORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Выполнение любых запросов на источнике
        /// </summary>
        /// <param name="SQL">Собственно запрос</param>
        private void setDataORA(string SQL)
        {
            try
            {
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".setDataORA", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(SQL, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        com.ExecuteNonQuery();
                    }
                }
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".setDataORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".setDataORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".setDataORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".setDataORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Получение списка операций из базы данных 
        /// </summary>
        /// <returns>Стандартный список операций</returns>
        private OperationList GetOperationListORA()
        {
            string CommandSql = String.Format(@"Select Sum(total_cash_sum) As total_cash_sum 
From aks.prizm_cust_porog");

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationListORA", EventEn.Dump);

                OperationList rez = new OperationList();

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                // Получаем схему таблицы
                                //DataTable tt = dr.GetSchemaTable();

                                //foreach (DataRow item in tt.Rows)
                                //{
                                //    DataColumn ncol = new DataColumn(item["ColumnName"].ToString(), Type.GetType(item["DataType"].ToString()));
                                //ncol.SetOrdinal(int.Parse(item["ColumnOrdinal"].ToString()));
                                //ncol.MaxLength = (int.Parse(item["ColumnSize"].ToString()) < 300 ? 300 : int.Parse(item["ColumnSize"].ToString()));
                                //rez.Columns.Add(ncol);
                                //}

                                // пробегаем по строкам
                                while (dr.Read())
                                {
                                    int? TmpOperation = null;
                                    string TmpDocFullName = null;
                                    string TmpOperationName = null;
                                    for (int i = 0; i < dr.FieldCount; i++)
                                    {
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("Operation").ToUpper()) TmpOperation = int.Parse(dr.GetValue(i).ToString());
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("DocFullName").ToUpper()) TmpDocFullName = dr.GetValue(i).ToString();
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("OperationName").ToUpper()) TmpOperationName = dr.GetValue(i).ToString();
                                    }

                                    //Если данные есть то добавляем их в список
                                    if (TmpOperation != null && !string.IsNullOrWhiteSpace(TmpDocFullName) && !string.IsNullOrWhiteSpace(TmpOperationName))
                                    {
                                        //OperationList.OperationListFarmBase.AddOperationToList(rez, new Operation((int)TmpOperation, TmpDocFullName, TmpOperationName));
                                    }
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationListORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationListORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationListORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationListORA", EventEn.Dump);
                throw;
            }
        }


        /// <summary>
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <returns>Получает текущий список Local из базы данных</returns>
        private LocalList GetLocalListFromDbORA()
        {
            string CommandSql = String.Format(@"Select `Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision` 
From `aks`.`cashfunc_local`");

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetCurLocalListFromDbORA", EventEn.Dump);

                LocalList rez = new LocalList();

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                // Получаем схему таблицы
                                //DataTable tt = dr.GetSchemaTable();

                                //foreach (DataRow item in tt.Rows)
                                //{
                                //    DataColumn ncol = new DataColumn(item["ColumnName"].ToString(), Type.GetType(item["DataType"].ToString()));
                                //ncol.SetOrdinal(int.Parse(item["ColumnOrdinal"].ToString()));
                                //ncol.MaxLength = (int.Parse(item["ColumnSize"].ToString()) < 300 ? 300 : int.Parse(item["ColumnSize"].ToString()));
                                //rez.Columns.Add(ncol);
                                //}

                                // пробегаем по строкам
                                while (dr.Read())
                                {
                                    int? TmpOperation = null;
                                    string TmpDocFullName = null;
                                    string TmpOperationName = null;
                                    for (int i = 0; i < dr.FieldCount; i++)
                                    {
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("Operation").ToUpper()) TmpOperation = int.Parse(dr.GetValue(i).ToString());
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("DocFullName").ToUpper()) TmpDocFullName = dr.GetValue(i).ToString();
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("OperationName").ToUpper()) TmpOperationName = dr.GetValue(i).ToString();
                                    }

                                    //Если данные есть то добавляем их в список
                                    if (TmpOperation != null && !string.IsNullOrWhiteSpace(TmpDocFullName) && !string.IsNullOrWhiteSpace(TmpOperationName))
                                    {
                                        //OperationList.OperationListFarmBase.AddOperationToList(rez, new Operation((int)TmpOperation, TmpDocFullName, TmpOperationName));
                                    }
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalListFromDbORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalListFromDbORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalListFromDbORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalListFromDbORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <param name="LastDay">Сколько последних дней грузить из базы данных если null значит весь период</param>
        /// <param name="OperationId">Какая операция нас интересует, если </param>
        /// <returns>Получает список Document из базы данных удовлетворяющий фильтрам</returns>
        private DocumentList GetDocumentListFromDbORA(int? LastDay, int? OperationId)
        {
            string CommandSql = String.Format(@"Select `Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision` 
From `aks`.`cashfunc_local`");

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentListFromDbORA", EventEn.Dump);

                DocumentList rez = new DocumentList();

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                // Получаем схему таблицы
                                //DataTable tt = dr.GetSchemaTable();

                                //foreach (DataRow item in tt.Rows)
                                //{
                                //    DataColumn ncol = new DataColumn(item["ColumnName"].ToString(), Type.GetType(item["DataType"].ToString()));
                                //ncol.SetOrdinal(int.Parse(item["ColumnOrdinal"].ToString()));
                                //ncol.MaxLength = (int.Parse(item["ColumnSize"].ToString()) < 300 ? 300 : int.Parse(item["ColumnSize"].ToString()));
                                //rez.Columns.Add(ncol);
                                //}

                                // пробегаем по строкам
                                while (dr.Read())
                                {
                                    int? TmpOperation = null;
                                    string TmpDocFullName = null;
                                    string TmpOperationName = null;
                                    for (int i = 0; i < dr.FieldCount; i++)
                                    {
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("Operation").ToUpper()) TmpOperation = int.Parse(dr.GetValue(i).ToString());
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("DocFullName").ToUpper()) TmpDocFullName = dr.GetValue(i).ToString();
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("OperationName").ToUpper()) TmpOperationName = dr.GetValue(i).ToString();
                                    }

                                    //Если данные есть то добавляем их в список
                                    if (TmpOperation != null && !string.IsNullOrWhiteSpace(TmpDocFullName) && !string.IsNullOrWhiteSpace(TmpOperationName))
                                    {
                                        //OperationList.OperationListFarmBase.AddOperationToList(rez, new Operation((int)TmpOperation, TmpDocFullName, TmpOperationName));
                                    }
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentListFromDbORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentListFromDbORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentListFromDbORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentListFromDbORA", EventEn.Dump);
                throw;
            }
        }
/*
        /// <summary>
        /// Устанавливаем факт по чеку
        /// </summary>
        /// <param name="CustInn">Инн покупателя</param>
        /// <param name="InvcNo">Сид докумнета</param>
        /// <param name="PosDate">Дата документа</param>
        /// <param name="TotalCashSum">Сумма по документу уплаченная налом</param>
        public void SetPrizmCustPorogORA(string CustInn, string InvcNo, DateTime PosDate, decimal TotalCashSum)
        {
            string CommandSql = String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetPrizmCustPorogORA", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        com.ExecuteNonQuery();
                    }
                }
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetPrizmCustPorogORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetPrizmCustPorogORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetPrizmCustPorogORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetPrizmCustPorogORA", EventEn.Dump);
                throw;
            }
        }


*/

        #endregion

        #region Private method MySql

        /// <summary>
        /// Получение любых данных из источника например чтобы плагины могли что-то дополнительно читать
        /// </summary>
        /// <param name="SQL">Собственно запрос</param>
        /// <returns>Результата В виде таблицы</returns>
        private DataTable getDataMySql(string SQL)
        {
            DataTable rez = null;

            try
            {
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".getDataMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(SQL, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = new DataTable();

                                // Получаем схему таблицы
                                DataTable tt = dr.GetSchemaTable();
                                foreach (DataRow item in tt.Rows)
                                {
                                    rez.Columns.Add(new DataColumn(item["ColumnName"].ToString().ToUpper(), Type.GetType(item["DataType"].ToString())));
                                }


                                // пробегаем по строкам
                                while (dr.Read())
                                {
                                    DataRow newr = rez.NewRow();
                                    for (int i = 0; i < tt.Rows.Count; i++)
                                    {
                                        if (!dr.IsDBNull(i) && tt.Rows[i]["DataType"].ToString() == "System.Int32") { newr[i] = (Int32)dr.GetValue(i); }
                                        if (!dr.IsDBNull(i) && tt.Rows[i]["DataType"].ToString() == "System.Int64") { newr[i] = (Int64)dr.GetValue(i); }
                                        if (!dr.IsDBNull(i) && tt.Rows[i]["DataType"].ToString() == "System.Double") { newr[i] = (double)dr.GetValue(i); }
                                        if (!dr.IsDBNull(i) && tt.Rows[i]["DataType"].ToString() == "System.Decimal") { newr[i] = (Decimal)dr.GetValue(i); }
                                        if (!dr.IsDBNull(i) && tt.Rows[i]["DataType"].ToString() == "System.String") { newr[i] = (string)dr.GetValue(i); }
                                        if (!dr.IsDBNull(i) && tt.Rows[i]["DataType"].ToString() == "System.DateTime") { newr[i] = (DateTime)dr.GetValue(i); }
                                    }
                                    rez.Rows.Add(newr);
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".getDataMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".getDataMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".getDataMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".getDataMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Выполнение любых запросов на источнике
        /// </summary>
        /// <param name="SQL">Собственно запрос</param>
        private void setDataMySql(string SQL)
        {
            try
            {
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".setDataMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(SQL, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        com.ExecuteNonQuery();
                    }
                }
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".setDataMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".setDataMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".setDataMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(SQL, GetType().Name + ".setDataMySql", EventEn.Dump);
                throw;
            }
        }


        /// <summary>
        /// Получение списка операций из базы данных 
        /// </summary>
        /// <returns>Стандартный список операций</returns>
        private OperationList GetOperationListMySql()
        {
            string CommandSql = String.Format(@"Select `Id`, `OpFullName`, `OperationName`, `KoefDebitor`, `KoefCreditor` From `aks`.`CashFunc_Operation`");

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationListMySql", EventEn.Dump);

                OperationList rez = new OperationList();

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                // Получаем схему таблицы
                                //DataTable tt = dr.GetSchemaTable();

                                //foreach (DataRow item in tt.Rows)
                                //{
                                //    DataColumn ncol = new DataColumn(item["ColumnName"].ToString(), Type.GetType(item["DataType"].ToString()));
                                //ncol.SetOrdinal(int.Parse(item["ColumnOrdinal"].ToString()));
                                //ncol.MaxLength = (int.Parse(item["ColumnSize"].ToString()) < 300 ? 300 : int.Parse(item["ColumnSize"].ToString()));
                                //rez.Columns.Add(ncol);
                                //}

                                // пробегаем по строкам
                                while (dr.Read())
                                {
                                    int? TmpId = null;
                                    string TmpOpFullName = null;
                                    string TmpOperationName = null;
                                    int? TmpKoefDebitor = null;
                                    int? TmpKoefCreditor = null;
                                    for (int i = 0; i < dr.FieldCount; i++)
                                    {
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("Id").ToUpper()) TmpId = int.Parse(dr.GetValue(i).ToString());
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("OpFullName").ToUpper()) TmpOpFullName = dr.GetValue(i).ToString();
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("OperationName").ToUpper()) TmpOperationName = dr.GetValue(i).ToString();
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("KoefDebitor").ToUpper()) TmpKoefDebitor = int.Parse(dr.GetValue(i).ToString());
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("KoefCreditor").ToUpper()) TmpKoefCreditor = int.Parse(dr.GetValue(i).ToString());
                                    }

                                    //Если данные есть то добавляем их в список
                                    if (TmpId!=null && !string.IsNullOrWhiteSpace(TmpOpFullName) && !string.IsNullOrWhiteSpace(TmpOperationName) && TmpKoefDebitor!=null && TmpKoefCreditor!=null)
                                    {
                                        Operation TmpOper = OperationFarm.CreateNewOperation((int)TmpId, TmpOpFullName, TmpOperationName, (int)TmpKoefDebitor, (int)TmpKoefCreditor);
                                        rez.Add(TmpOper);
                                    }
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationListMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationListMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationListMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationListMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <returns>Получает текущий список Local из базы данных</returns>
        private LocalList GetLocalListFromDbMySql()
        {
            string CommandSql = String.Format(@"Select `Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`, `IsDraft` From `aks`.`cashfunc_local`");

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetCurLocalListFromDbMySql", EventEn.Dump);

                LocalList rez = new LocalList();

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                // Получаем схему таблицы
                                //DataTable tt = dr.GetSchemaTable();

                                //foreach (DataRow item in tt.Rows)
                                //{
                                //    DataColumn ncol = new DataColumn(item["ColumnName"].ToString(), Type.GetType(item["DataType"].ToString()));
                                //ncol.SetOrdinal(int.Parse(item["ColumnOrdinal"].ToString()));
                                //ncol.MaxLength = (int.Parse(item["ColumnSize"].ToString()) < 300 ? 300 : int.Parse(item["ColumnSize"].ToString()));
                                //rez.Columns.Add(ncol);
                                //}

                                // пробегаем по строкам
                                while (dr.Read())
                                {
                                    int? Id = null;
                                    string LocFullName = null;
                                    string LocalName = null;
                                    bool IsSeller = false;
                                    bool IsСustomer = false;
                                    bool IsDivision = false;
                                    bool IsDraft = true;


                                    if (!dr.IsDBNull(0)) Id = dr.GetInt32(0);
                                    if (!dr.IsDBNull(1)) LocFullName = dr.GetString(1);
                                    if (!dr.IsDBNull(2)) LocalName = dr.GetString(2);
                                    if (!dr.IsDBNull(3)) IsSeller = Boolean.Parse(dr.GetValue(3).ToString());
                                    if (!dr.IsDBNull(4)) IsСustomer = Boolean.Parse(dr.GetValue(4).ToString());
                                    if (!dr.IsDBNull(5)) IsDivision = Boolean.Parse(dr.GetValue(5).ToString());
                                    if (!dr.IsDBNull(6)) IsDraft = Boolean.Parse(dr.GetValue(6).ToString());


                                    //Если данные есть то добавляем их в список
                                    if (Id != null && !string.IsNullOrWhiteSpace(LocFullName) && !string.IsNullOrWhiteSpace(LocalName))
                                    {
                                        rez.Add(LocalFarm.CreateNewLocal((int)Id, LocFullName, LocalName, IsSeller, IsСustomer, IsDivision, IsDraft));
                                    }
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalListFromDbMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalListFromDbMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalListFromDbMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalListFromDbMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <param name="LastDay">Сколько последних дней грузить из базы данных если null значит весь период</param>
        /// <param name="OperationId">Какая операция нас интересует, если </param>
        /// <returns>Получает список Document из базы данных удовлетворяющий фильтрам</returns>
        private DocumentList GetDocumentListFromDbMySql(int? LastDay, int? OperationId)
        {
            string CommandSql = String.Format(@"Select `Id`, `DocFullName`, `UreDate`, `CteateDate`, `ModifyDate`, `ModifyUser`, `OperationId`, `LocalDebitorId`, `LocalCreditorId`, `IsDraft`, `IsProcessed` From `aks`.`CashFunc_Document`");

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentListFromDbMySql", EventEn.Dump);

                DocumentList rez = new DocumentList();

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                // Получаем схему таблицы
                                //DataTable tt = dr.GetSchemaTable();

                                //foreach (DataRow item in tt.Rows)
                                //{
                                //    DataColumn ncol = new DataColumn(item["ColumnName"].ToString(), Type.GetType(item["DataType"].ToString()));
                                //ncol.SetOrdinal(int.Parse(item["ColumnOrdinal"].ToString()));
                                //ncol.MaxLength = (int.Parse(item["ColumnSize"].ToString()) < 300 ? 300 : int.Parse(item["ColumnSize"].ToString()));
                                //rez.Columns.Add(ncol);
                                //}

                                // пробегаем по строкам
                                while (dr.Read())
                                {
                                    int? TmpId = null;
                                    string TmpDocFullName = null;
                                    DateTime? TmpUreDate = null;
                                    DateTime? TmpCteateDate = null;
                                    DateTime? TmpModifyDate = null;
                                    string TmpModifyUser = null;
                                    int? TmpOperationId = null;
                                    int? TmpLocalDebitorId = null;
                                    int? TmpLocalCreditorId = null;
                                    bool TmpIsDraft = true;
                                    bool TmpIsProcessed = false;


                                    if (!dr.IsDBNull(0)) TmpId = dr.GetInt32(0);
                                    if (!dr.IsDBNull(1)) TmpDocFullName = dr.GetString(1);
                                    if (!dr.IsDBNull(2)) TmpUreDate = dr.GetDateTime(2);
                                    if (!dr.IsDBNull(3)) TmpCteateDate = dr.GetDateTime(3);
                                    if (!dr.IsDBNull(4)) TmpModifyDate = dr.GetDateTime(4);
                                    if (!dr.IsDBNull(5)) TmpModifyUser = dr.GetString(5); 
                                    if (!dr.IsDBNull(6)) TmpOperationId = dr.GetInt32(6);
                                    if (!dr.IsDBNull(7)) TmpLocalDebitorId = dr.GetInt32(7);
                                    if (!dr.IsDBNull(8)) TmpLocalCreditorId = dr.GetInt32(8);
                                    if (!dr.IsDBNull(9)) TmpIsDraft = Boolean.Parse(dr.GetValue(9).ToString());
                                    if (!dr.IsDBNull(10)) TmpIsProcessed = Boolean.Parse(dr.GetValue(10).ToString());


                                    //Если данные есть то добавляем их в список
                                    if (TmpId != null && !string.IsNullOrWhiteSpace(TmpDocFullName) && TmpCteateDate!=null && TmpModifyDate!=null && !string.IsNullOrWhiteSpace(TmpModifyUser) && TmpOperationId != null && TmpLocalDebitorId != null && TmpLocalCreditorId!=null)
                                    {
                                        Operation TmpOper = OperationFarm.CurOperationList[TmpOperationId];
                                        if (TmpOper == null) throw new ApplicationException(string.Format("Операции с идентификатором {0} в документе где id={1} не существует.", TmpOperationId, TmpId));

                                        Local TmpDeb = LocalFarm.CurLocalList[TmpLocalDebitorId];
                                        if (TmpDeb == null) throw new ApplicationException(string.Format("Дебитора с идентификатором {0} в документе где id={1} не существует.", TmpLocalDebitorId, TmpId));

                                        Local TmpCred = LocalFarm.CurLocalList[TmpLocalCreditorId];
                                        if (TmpCred == null) throw new ApplicationException(string.Format("Кредитора с идентификатором {0} в документе где id={1} не существует.", TmpLocalCreditorId, TmpId));

                                        rez.Add(DocumentFarm.CreateNewDocument((int)TmpId, TmpDocFullName, TmpUreDate, (DateTime)TmpCteateDate, (DateTime)TmpModifyDate, TmpModifyUser, TmpOper, TmpDeb, TmpCred, TmpIsDraft, TmpIsProcessed));
                                    }
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentListFromDbMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentListFromDbMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentListFromDbMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentListFromDbMySql", EventEn.Dump);
                throw;
            }
        }

        /*
                /// <summary>
                /// Устанавливаем факт по чеку
                /// </summary>
                /// <param name="CustInn">Инн покупателя</param>
                /// <param name="InvcNo">Сид докумнета</param>
                /// <param name="PosDate">Дата документа</param>
                /// <param name="TotalCashSum">Сумма по документу уплаченная налом</param>
                public void SetPrizmCustPorogMySql(string CustInn, string InvcNo, DateTime PosDate, decimal TotalCashSum)
                {
                    string CommandSql = String.Format(@"insert into `aks`.`prizm_cust_porog`(`cust_inn`,`invc_no`,`dt`,`pos_date`, `total_cash_sum`) Values('{0}', {1}, STR_TO_DATE('{2},{3},{4}', '%Y,%m,%d'), STR_TO_DATE('{2},{3},{4} {5},{6},{7}', '%Y,%m,%d %H,%i,%s'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

                    try
                    {
                        if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetPrizmCustPorogMySql", EventEn.Dump);

                        // Закрывать конект не нужно он будет закрыт деструктором
                        using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                        {
                            con.Open();

                            using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                            {
                                com.CommandTimeout = 900;  // 15 минут
                                com.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (OdbcException ex)
                    {
                        base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetPrizmCustPorogMySql", EventEn.Error);
                        if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetPrizmCustPorogMySql", EventEn.Dump);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetPrizmCustPorogMySql", EventEn.Error);
                        if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetPrizmCustPorogMySql", EventEn.Dump);
                        throw;
                    }
                }


            */
        #endregion

        }
}
