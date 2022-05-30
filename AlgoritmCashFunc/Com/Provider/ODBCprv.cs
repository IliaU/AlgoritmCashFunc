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
using AlgoritmCashFunc.BLL_Prizm;

namespace AlgoritmCashFunc.Com.Provider
{
    /// <summary>
    /// Провайдер для работы по подключению типа ODBC
    /// </summary>
    public sealed class ODBCprv : ProviderBase, ProviderI, ProviderPrizmI
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
        /// Установка номеров документа по правилам связанным с началом года
        /// </summary>
        public void SetDocNumForYear()
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
                            SetDocNumForYearORA();
                            break;
                        case "myodbc8a.dll":
                            SetDocNumForYearMySql();
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                //return 0;
            }
            catch (Exception ex)
            {
                // Логируем ошибку если её должен видеть пользователь или если взведён флаг трассировке в файле настройки программы
                if (Com.Config.Trace) base.EventSave(ex.Message, "SetDocNumForYear", EventEn.Error);

                throw ex;
            }
        }

        /// <summary>
        /// Получаем последний номер документа по типу который задан в документе за год в котором юридическая дата документа на основе которого получаем номер
        /// </summary>
        /// <param name="doc">Документ откуда получаем тип и юридическую дату</param>
        /// <returns>Номер последнего документа если он найден если не найден то 0</returns>
        public int MaxDocNumForYaer(Document doc)
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
                            return MaxDocNumForYaerORA(doc);
                            //break;
                        case "myodbc8a.dll":
                            return MaxDocNumForYaerMySql(doc);
                            //break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                // Логируем ошибку если её должен видеть пользователь или если взведён флаг трассировке в файле настройки программы
                if (Com.Config.Trace) base.EventSave(ex.Message, "MaxDocNumForYaer", EventEn.Error);

                throw ex;
            }
        }

        /// <summary>
        /// Обновление документов при встаке документа в прошлое
        /// </summary>
        /// <param name="doc">Документ на который ориентируемся</param>
        public void UpdateNumDocForAdd(Document doc)
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
                            UpdateNumDocForAddORA(doc);
                            break;
                        case "myodbc8a.dll":
                            UpdateNumDocForAddMySql(doc);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                //return 0;
            }
            catch (Exception ex)
            {
                // Логируем ошибку если её должен видеть пользователь или если взведён флаг трассировке в файле настройки программы
                if (Com.Config.Trace) base.EventSave(ex.Message, "UpdateNumDocForAdd", EventEn.Error);

                throw ex;
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
        /// Сохранение Operation в базе
        /// </summary>
        /// <param name="NewOperation">Новый Operation который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        public int SetOperation(Operation NewOperation)
        {
            try
            {
                int rez = 0;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = SetOperationORA(NewOperation);
                            break;
                        case "myodbc8a.dll":
                            rez = SetOperationMySql(NewOperation);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperation", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(NewOperation.OperationName, GetType().Name + ".SetOperation", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновление Operation в базе
        /// </summary>
        /// <param name="UpdOperation">Обновляемый Operation</param>
        public void UpdateOperation(Operation UpdOperation)
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
                            UpdateOperationORA(UpdOperation);
                            break;
                        case "myodbc8a.dll":
                            UpdateOperationMySql(UpdOperation);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperation", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(UpdOperation.OperationName, GetType().Name + ".UpdateOperation", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationPrihod
        /// </summary>
        /// <param name="OperationPrihod">Объект OperationPrihod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashOperationPrihod(BLL.OperationPlg.OperationPrihod OperationPrihod)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = HashOperationPrihodORA(OperationPrihod);
                            break;
                        case "myodbc8a.dll":
                            rez = HashOperationPrihodMySql(OperationPrihod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту OperationPrihod
        /// </summary>
        /// <param name="OperationPrihod">Объект OperationPrihod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        public bool GetOperationPrihod(ref BLL.OperationPlg.OperationPrihod OperationPrihod)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = GetOperationPrihodORA(ref OperationPrihod);
                            break;
                        case "myodbc8a.dll":
                            rez = GetOperationPrihodMySql(ref OperationPrihod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationPrihod
        /// </summary>
        /// <param name="NewOperationPrihod">Вставляем в базу информацию по объекту OperationPrihod</param>
        public void SetOperationPrihod(BLL.OperationPlg.OperationPrihod NewOperationPrihod)
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
                            SetOperationPrihodORA(NewOperationPrihod);
                            break;
                        case "myodbc8a.dll":
                            SetOperationPrihodMySql(NewOperationPrihod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationPrihod
        /// </summary>
        /// <param name="UpdOperationPrihod">Сам объект данные которого нужно обновить</param>
        public void UpdateOperationPrihod(BLL.OperationPlg.OperationPrihod UpdOperationPrihod)
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
                            UpdateOperationPrihodORA(UpdOperationPrihod);
                            break;
                        case "myodbc8a.dll":
                            UpdateOperationPrihodMySql(UpdOperationPrihod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationRashod
        /// </summary>
        /// <param name="OperationRashod">Объект OperationRashod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashOperationRashod(BLL.OperationPlg.OperationRashod OperationRashod)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = HashOperationRashodORA(OperationRashod);
                            break;
                        case "myodbc8a.dll":
                            rez = HashOperationRashodMySql(OperationRashod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту OperationRashod
        /// </summary>
        /// <param name="OperationRashod">Объект OperationRashod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        public bool GetOperationRashod(ref BLL.OperationPlg.OperationRashod OperationRashod)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = GetOperationRashodORA(ref OperationRashod);
                            break;
                        case "myodbc8a.dll":
                            rez = GetOperationRashodMySql(ref OperationRashod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationRashod
        /// </summary>
        /// <param name="NewOperationRashod">Вставляем в базу информацию по объекту OperationRashod</param>
        public void SetOperationRashod(BLL.OperationPlg.OperationRashod NewOperationRashod)
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
                            SetOperationRashodORA(NewOperationRashod);
                            break;
                        case "myodbc8a.dll":
                            SetOperationRashodMySql(NewOperationRashod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationRashod
        /// </summary>
        /// <param name="UpdOperationRashod">Сам объект данные которого нужно обновить</param>
        public void UpdateOperationRashod(BLL.OperationPlg.OperationRashod UpdOperationRashod)
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
                            UpdateOperationRashodORA(UpdOperationRashod);
                            break;
                        case "myodbc8a.dll":
                            UpdateOperationRashodMySql(UpdOperationRashod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationKasBook
        /// </summary>
        /// <param name="OperationKasBook">Объект OperationKasBook который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashOperationKasBook(BLL.OperationPlg.OperationKasBook OperationKasBook)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = HashOperationKasBookORA(OperationKasBook);
                            break;
                        case "myodbc8a.dll":
                            rez = HashOperationKasBookMySql(OperationKasBook);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту OperationKasBook
        /// </summary>
        /// <param name="OperationKasBook">Объект OperationKasBook который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        public bool GetOperationKasBook(ref BLL.OperationPlg.OperationKasBook OperationKasBook)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = GetOperationKasBookORA(ref OperationKasBook);
                            break;
                        case "myodbc8a.dll":
                            rez = GetOperationKasBookMySql(ref OperationKasBook);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationKasBook
        /// </summary>
        /// <param name="NewOperationKasBook">Вставляем в базу информацию по объекту OperationKasBook</param>
        public void SetOperationKasBook(BLL.OperationPlg.OperationKasBook NewOperationKasBook)
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
                            SetOperationKasBookORA(NewOperationKasBook);
                            break;
                        case "myodbc8a.dll":
                            SetOperationKasBookMySql(NewOperationKasBook);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationKasBook
        /// </summary>
        /// <param name="UpdOperationKasBook">Сам объект данные которого нужно обновить</param>
        public void UpdateOperationKasBook(BLL.OperationPlg.OperationKasBook UpdOperationKasBook)
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
                            UpdateOperationKasBookORA(UpdOperationKasBook);
                            break;
                        case "myodbc8a.dll":
                            UpdateOperationKasBookMySql(UpdOperationKasBook);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationInvent
        /// </summary>
        /// <param name="OperationInvent">Объект OperationInvent который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashOperationInvent(BLL.OperationPlg.OperationInvent OperationInvent)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = HashOperationInventORA(OperationInvent);
                            break;
                        case "myodbc8a.dll":
                            rez = HashOperationInventMySql(OperationInvent);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationInvent", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту OperationInvent
        /// </summary>
        /// <param name="OperationInvent">Объект OperationInvent который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        public bool GetOperationInvent(ref BLL.OperationPlg.OperationInvent OperationInvent)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = GetOperationInventORA(ref OperationInvent);
                            break;
                        case "myodbc8a.dll":
                            rez = GetOperationInventMySql(ref OperationInvent);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationInvent", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationInvent
        /// </summary>
        /// <param name="NewOperationInvent">Вставляем в базу информацию по объекту OperationInvent</param>
        public void SetOperationInvent(BLL.OperationPlg.OperationInvent NewOperationInvent)
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
                            SetOperationInventORA(NewOperationInvent);
                            break;
                        case "myodbc8a.dll":
                            SetOperationInventMySql(NewOperationInvent);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationInvent", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationInvent
        /// </summary>
        /// <param name="UpdOperationInvent">Сам объект данные которого нужно обновить</param>
        public void UpdateOperationInvent(BLL.OperationPlg.OperationInvent UpdOperationInvent)
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
                            UpdateOperationInventORA(UpdOperationInvent);
                            break;
                        case "myodbc8a.dll":
                            UpdateOperationInventMySql(UpdOperationInvent);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationInvent", EventEn.Error);
                throw;
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
        /// Сохранение Local в базе
        /// </summary>
        /// <param name="NewLocal">Новый локал который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        public int SetLocal(Local NewLocal)
        {
            try
            {
                int rez=0;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez=SetLocalORA(NewLocal);
                            break;
                        case "myodbc8a.dll":
                            rez=SetLocalMySql(NewLocal);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocal", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(NewLocal.LocalName, GetType().Name + ".SetLocal", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновление Local в базе
        /// </summary>
        /// <param name="UpdLocal">Обновляемый локал</param>
        public void UpdateLocal(Local UpdLocal)
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
                            UpdateLocalORA(UpdLocal);
                            break;
                        case "myodbc8a.dll":
                            UpdateLocalMySql(UpdLocal);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocal", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(UpdLocal.LocalName, GetType().Name + ".UpdateLocal", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalKassa
        /// </summary>
        /// <param name="LocalKassa">Объект LocalKassa который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashLocalKassa(BLL.LocalPlg.LocalKassa LocalKassa)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = HashLocalKassaORA(LocalKassa);
                            break;
                        case "myodbc8a.dll":
                            rez = HashLocalKassaMySql(LocalKassa);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalKassa", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту LocalKassa
        /// </summary>
        /// <param name="LocalKassa">Объект LocalKassa который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        public bool GetLocalKassa(ref BLL.LocalPlg.LocalKassa LocalKassa)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = GetLocalKassaORA(ref LocalKassa);
                            break;
                        case "myodbc8a.dll":
                            rez = GetLocalKassaMySql(ref LocalKassa);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalKassa", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект LocalKassa
        /// </summary>
        /// <param name="NewLocalKassa">Вставляем в базу информацию по объекту LocalKassa</param>
        public void SetLocalKassa(BLL.LocalPlg.LocalKassa NewLocalKassa)
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
                            SetLocalKassaORA(NewLocalKassa);
                            break;
                        case "myodbc8a.dll":
                            SetLocalKassaMySql(NewLocalKassa);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalKassa", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту LocalKassa
        /// </summary>
        /// <param name="UpdLocalKassa">Сам объект данные которого нужно обновить</param>
        public void UpdateLocalKassa(BLL.LocalPlg.LocalKassa UpdLocalKassa)
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
                            UpdateLocalKassaORA(UpdLocalKassa);
                            break;
                        case "myodbc8a.dll":
                            UpdateLocalKassaMySql(UpdLocalKassa);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalKassa", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalPaidInReasons
        /// </summary>
        /// <param name="LocalPaidInReasons">Объект LocalPaidInReasons который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashLocalPaidInReasons(BLL.LocalPlg.LocalPaidInReasons LocalPaidInReasons)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = HashLocalPaidInReasonsORA(LocalPaidInReasons);
                            break;
                        case "myodbc8a.dll":
                            rez = HashLocalPaidInReasonsMySql(LocalPaidInReasons);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalPaidInReasons", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту LocalPaidInReasons
        /// </summary>
        /// <param name="LocalPaidInReasons">Объект LocalPaidInReasons который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        public bool GetLocalPaidInReasons(ref BLL.LocalPlg.LocalPaidInReasons LocalPaidInReasons)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = GetLocalPaidInReasonsORA(ref LocalPaidInReasons);
                            break;
                        case "myodbc8a.dll":
                            rez = GetLocalPaidInReasonsMySql(ref LocalPaidInReasons);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalPaidInReasons", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект LocalPaidInReasons
        /// </summary>
        /// <param name="NewLocalPaidInReasons">Вставляем в базу информацию по объекту LocalPaidInReasons</param>
        public void SetLocalPaidInReasons(BLL.LocalPlg.LocalPaidInReasons NewLocalPaidInReasons)
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
                            SetLocalPaidInReasonsORA(NewLocalPaidInReasons);
                            break;
                        case "myodbc8a.dll":
                            SetLocalPaidInReasonsMySql(NewLocalPaidInReasons);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalPaidInReasons", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту LocalPaidInReasons
        /// </summary>
        /// <param name="UpdLocalPaidInReasons">Сам объект данные которого нужно обновить</param>
        public void UpdateLocalPaidInReasons(BLL.LocalPlg.LocalPaidInReasons UpdLocalPaidInReasons)
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
                            UpdateLocalPaidInReasonsORA(UpdLocalPaidInReasons);
                            break;
                        case "myodbc8a.dll":
                            UpdateLocalPaidInReasonsMySql(UpdLocalPaidInReasons);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalPaidInReasons", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalPaidRashReasons
        /// </summary>
        /// <param name="LocalPaidRashReasons">Объект LocalPaidRashReasons который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashLocalPaidRashReasons(BLL.LocalPlg.LocalPaidRashReasons LocalPaidRashReasons)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = HashLocalPaidRashReasonsORA(LocalPaidRashReasons);
                            break;
                        case "myodbc8a.dll":
                            rez = HashLocalPaidRashReasonsMySql(LocalPaidRashReasons);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalPaidRashReasons", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту LocalPaidRashReasons
        /// </summary>
        /// <param name="LocalPaidRashReasons">Объект LocalPaidRashReasons который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        public bool GetLocalPaidRashReasons(ref BLL.LocalPlg.LocalPaidRashReasons LocalPaidRashReasons)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = GetLocalPaidRashReasonsORA(ref LocalPaidRashReasons);
                            break;
                        case "myodbc8a.dll":
                            rez = GetLocalPaidRashReasonsMySql(ref LocalPaidRashReasons);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalPaidRashReasons", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект LocalPaidRashReasons
        /// </summary>
        /// <param name="NewLocalPaidRashReasons">Вставляем в базу информацию по объекту LocalPaidRashReasons</param>
        public void SetLocalPaidRashReasons(BLL.LocalPlg.LocalPaidRashReasons NewLocalPaidRashReasons)
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
                            SetLocalPaidRashReasonsORA(NewLocalPaidRashReasons);
                            break;
                        case "myodbc8a.dll":
                            SetLocalPaidRashReasonsMySql(NewLocalPaidRashReasons);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalPaidRashReasons", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту LocalPaidRashReasons
        /// </summary>
        /// <param name="UpdLocalPaidRashReasons">Сам объект данные которого нужно обновить</param>
        public void UpdateLocalPaidRashReasons(BLL.LocalPlg.LocalPaidRashReasons UpdLocalPaidRashReasons)
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
                            UpdateLocalPaidRashReasonsORA(UpdLocalPaidRashReasons);
                            break;
                        case "myodbc8a.dll":
                            UpdateLocalPaidRashReasonsMySql(UpdLocalPaidRashReasons);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalPaidRashReasons", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Получение остатка на начало заданной даты и оборота за день
        /// </summary>
        /// <param name="Dt">Дата на которую ищем данные</param>
        /// <returns>Результат остаток на начало даты и оборот за эту дату</returns>
        public RezultForOstatokAndOborot GetOstatokAndOborotForDay(DateTime Dt)
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
                            return GetOstatokAndOborotForDayORA(Dt);
                        case "myodbc8a.dll":
                            return GetOstatokAndOborotForDayMySql(Dt);
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку если её должен видеть пользователь или если взведён флаг трассировке в файле настройки программы
                if (Com.Config.Trace) base.EventSave(ex.Message, "GetOstatokAndOborotForDay", EventEn.Error);

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

        /// <summary>
        /// Получаем список докуменитов
        /// </summary>
        /// <param name="Dt">За конкретную дату время будет отброшено</param>
        /// <param name="OperationId">Какая операция нас интересует, если null значит все операции за эту дату</param>
        /// <param name="HasNotin">Если true то будет смотреть все операции кроме операции указанной в параметре OperationId</param>
        /// <returns>Получает список Document из базы данных удовлетворяющий фильтрам</returns>
        public DocumentList GetDocumentListFromDB(DateTime? Dt, int? OperationId, bool HasNotin)
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
                            return GetDocumentListFromDbORA(Dt, OperationId, HasNotin);
                        case "myodbc8a.dll":
                            return GetDocumentListFromDbMySql(Dt, OperationId, HasNotin);
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

        /// <summary>
        /// Сохранение Document в базе
        /// </summary>
        /// <param name="NewDocument">Новый документ который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        public int SetDocument(Document NewDocument)
        {
            try
            {
                int rez = 0;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = SetDocumentORA(NewDocument);
                            break;
                        case "myodbc8a.dll":
                            rez = SetDocumentMySql(NewDocument);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocument", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(string.Format("{0} ({1})",NewDocument.DocFullName, NewDocument.UreDate), GetType().Name + ".SetDocument", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновление Document в базе
        /// </summary>
        /// <param name="UpdDocument">Обновляемый документ</param>
        public void UpdateDocument(Document UpdDocument)
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
                            UpdateDocumentORA(UpdDocument);
                            break;
                        case "myodbc8a.dll":
                            UpdateDocumentMySql(UpdDocument);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocument", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(string.Format("{0} ({1})", UpdDocument.DocFullName, UpdDocument.UreDate), GetType().Name + ".UpdateDocument", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentPrihod
        /// </summary>
        /// <param name="DocumentPrihod">Объект DocumentPrihod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashDocumentPrihod(BLL.DocumentPlg.DocumentPrihod DocumentPrihod)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = HashDocumentPrihodORA(DocumentPrihod);
                            break;
                        case "myodbc8a.dll":
                            rez = HashDocumentPrihodMySql(DocumentPrihod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту DocumentPrihod
        /// </summary>
        /// <param name="DocumentPrihod">Объект DocumentPrihod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        public bool GetDocumentPrihod(ref BLL.DocumentPlg.DocumentPrihod DocumentPrihod)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = GetDocumentPrihodORA(ref DocumentPrihod);
                            break;
                        case "myodbc8a.dll":
                            rez = GetDocumentPrihodMySql(ref DocumentPrihod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".DocumentPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentPrihod
        /// </summary>
        /// <param name="NewDocumentPrihod">Вставляем в базу информацию по объекту DocumentPrihod</param>
        public void SetDocumentPrihod(BLL.DocumentPlg.DocumentPrihod NewDocumentPrihod)
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
                            SetDocumentPrihodORA(NewDocumentPrihod);
                            break;
                        case "myodbc8a.dll":
                            SetDocumentPrihodMySql(NewDocumentPrihod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentPrihod
        /// </summary>
        /// <param name="UpdDocumentPrihod">Сам объект данные которого нужно обновить</param>
        public void UpdateDocumentPrihod(BLL.DocumentPlg.DocumentPrihod UpdDocumentPrihod)
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
                            UpdateDocumentPrihodORA(UpdDocumentPrihod);
                            break;
                        case "myodbc8a.dll":
                            UpdateDocumentPrihodMySql(UpdDocumentPrihod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentPrihod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentRashod
        /// </summary>
        /// <param name="DocumentRashod">Объект DocumentRashod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashDocumentRashod(BLL.DocumentPlg.DocumentRashod DocumentRashod)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = HashDocumentRashodORA(DocumentRashod);
                            break;
                        case "myodbc8a.dll":
                            rez = HashDocumentRashodMySql(DocumentRashod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentRashod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту DocumentRashod
        /// </summary>
        /// <param name="DocumentRashod">Объект DocumentRashod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        public bool GetDocumentRashod(ref BLL.DocumentPlg.DocumentRashod DocumentRashod)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = GetDocumentRashodORA(ref DocumentRashod);
                            break;
                        case "myodbc8a.dll":
                            rez = GetDocumentRashodMySql(ref DocumentRashod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".DocumentRashod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentRashod
        /// </summary>
        /// <param name="NewDocumentRashod">Вставляем в базу информацию по объекту DocumentRashod</param>
        public void SetDocumentRashod(BLL.DocumentPlg.DocumentRashod NewDocumentRashod)
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
                            SetDocumentRashodORA(NewDocumentRashod);
                            break;
                        case "myodbc8a.dll":
                            SetDocumentRashodMySql(NewDocumentRashod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentRashod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentRashod
        /// </summary>
        /// <param name="UpdDocumentRashod">Сам объект данные которого нужно обновить</param>
        public void UpdateDocumentRashod(BLL.DocumentPlg.DocumentRashod UpdDocumentRashod)
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
                            UpdateDocumentRashodORA(UpdDocumentRashod);
                            break;
                        case "myodbc8a.dll":
                            UpdateDocumentRashodMySql(UpdDocumentRashod);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentRashod", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentKasBook
        /// </summary>
        /// <param name="DocumentKasBook">Объект DocumentKasBook который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashDocumentKasBook(BLL.DocumentPlg.DocumentKasBook DocumentKasBook)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = HashDocumentKasBookORA(DocumentKasBook);
                            break;
                        case "myodbc8a.dll":
                            rez = HashDocumentKasBookMySql(DocumentKasBook);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentKasBook", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту DocumentKasBook
        /// </summary>
        /// <param name="DocumentKasBook">Объект DocumentKasBook который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        public bool GetDocumentKasBook(ref BLL.DocumentPlg.DocumentKasBook DocumentKasBook)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = GetDocumentKasBookORA(ref DocumentKasBook);
                            break;
                        case "myodbc8a.dll":
                            rez = GetDocumentKasBookMySql(ref DocumentKasBook);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".DocumentKasBook", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentKasBook
        /// </summary>
        /// <param name="NewDocumentKasBook">Вставляем в базу информацию по объекту DocumentKasBook</param>
        public void SetDocumentKasBook(BLL.DocumentPlg.DocumentKasBook NewDocumentKasBook)
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
                            SetDocumentKasBookORA(NewDocumentKasBook);
                            break;
                        case "myodbc8a.dll":
                            SetDocumentKasBookMySql(NewDocumentKasBook);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentKasBook", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentKasBook
        /// </summary>
        /// <param name="UpdDocumentKasBook">Сам объект данные которого нужно обновить</param>
        public void UpdateDocumentKasBook(BLL.DocumentPlg.DocumentKasBook UpdDocumentKasBook)
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
                            UpdateDocumentKasBookORA(UpdDocumentKasBook);
                            break;
                        case "myodbc8a.dll":
                            UpdateDocumentKasBookMySql(UpdDocumentKasBook);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentKasBook", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentInvent
        /// </summary>
        /// <param name="DocumentInvent">Объект DocumentInvent который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        public bool HashDocumentInvent(BLL.DocumentPlg.DocumentInvent DocumentInvent)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = HashDocumentInventORA(DocumentInvent);
                            break;
                        case "myodbc8a.dll":
                            rez = HashDocumentInventMySql(DocumentInvent);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentInvent", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту DocumentInvent
        /// </summary>
        /// <param name="DocumentInvent">Объект DocumentInvent который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        public bool GetDocumentInvent(ref BLL.DocumentPlg.DocumentInvent DocumentInvent)
        {
            try
            {
                bool rez = false;
                if (!this.HashConnect()) new ApplicationException("Нет подключение к базе данных." + this.Driver);
                else
                {
                    // Проверка типа трайвера мы не можем обрабатьывать любой тип у каждого типа могут быть свои особенности
                    switch (this.Driver)
                    {
                        case "SQORA32.DLL":
                        case "SQORA64.DLL":
                            rez = GetDocumentInventORA(ref DocumentInvent);
                            break;
                        case "myodbc8a.dll":
                            rez = GetDocumentInventMySql(ref DocumentInvent);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return rez;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".DocumentInvent", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentInvent
        /// </summary>
        /// <param name="NewDocumentInvent">Вставляем в базу информацию по объекту DocumentInvent</param>
        public void SetDocumentInvent(BLL.DocumentPlg.DocumentInvent NewDocumentInvent)
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
                            SetDocumentInventORA(NewDocumentInvent);
                            break;
                        case "myodbc8a.dll":
                            SetDocumentInventMySql(NewDocumentInvent);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentInvent", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentInvent
        /// </summary>
        /// <param name="UpdDocumentInvent">Сам объект данные которого нужно обновить</param>
        public void UpdateDocumentInvent(BLL.DocumentPlg.DocumentInvent UpdDocumentInvent)
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
                            UpdateDocumentInventORA(UpdDocumentInvent);
                            break;
                        case "myodbc8a.dll":
                            UpdateDocumentInventMySql(UpdDocumentInvent);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentInvent", EventEn.Error);
                throw;
            }
        }

        /// <summary>
        /// Получаем документ по его номеру
        /// </summary>
        /// <param name="DocNumber">Номер документа</param>
        /// <returns>Документ</returns>
        public Check GetCheck(int DocNumber)
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
                            //GetCheckORA(DocNumber);
                            break;
                        case "myodbc8a.dll":
                            //GetCheckMySql(DocNumber);
                            break;
                        default:
                            throw new ApplicationException("Извините. Мы не умеем работать с драйвером: " + this.Driver);
                            //break;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetCheck", EventEn.Error);
                throw;
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
        /// Установка номеров документа по правилам связанным с началом года
        /// </summary>
        public void SetDocNumForYearORA()
        {
            string CommandSql = String.Format(@"With T As (Select `Id`, `UreDate`, `OperationId`, `DocNum`,
      row_number() over(partition by YEAR(`UreDate`), `OperationId` order by `UreDate`) As PRN
    From `aks`.`cashfunc_document`
    Where `UreDate`>=str_to_date(ConCat('01.01.',convert(YEAR(curdate())-1, char)),'%d.%m.%Y'))
Update `aks`.`cashfunc_document` D 
  inner join  T On D.Id=T.Id
Set D.`DocNum`=T.`DocNum`



With T As (Select `OperationId`, Max(`DocNum`) As DocNum
    From `aks`.`cashfunc_document`
    Where `UreDate`>=str_to_date(ConCat('01.01.',convert(YEAR(curdate()), char)),'%d.%m.%Y')
    Group By `OperationId`),
    R As (Select Max(Case When `OperationId`=1 Then `DocNum` else 0 End) Prih,
      Max(Case When `OperationId`=2 Then `DocNum` else 0 End) Rash,
      Max(Case When `OperationId`=3 Then `DocNum` else 0 End) KasBook,
      Max(Case When `OperationId`=4 Then `DocNum` else 0 End) Invent
    From  T)
Update `aks`.`cashfunc_local_kassa` K
Set K.LastDocNumPrih=(Select Prih From R),
  K.LastDocNumRash=(Select Rash From R),
  K.LastDocNumKasBook=(Select KasBook From R),
  K.LastDocNumInvent=(Select Invent From R)
Where K.Id=2", Com.LocalFarm.CurLocalDepartament.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocNumForYearORA", EventEn.Dump);

                int rez = 0;

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
                                    for (int i = 0; i < dr.FieldCount; i++)
                                    {
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("MaxDocNum").ToUpper()) rez = int.Parse(dr.GetValue(i).ToString());
                                    }
                                }
                            }
                        }
                    }
                }

                //return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocNumForYearORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocNumForYearORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocNumForYearORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocNumForYearORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Получаем последний номер документа по типу который задан в документе за год в котором юридическая дата документа на основе которого получаем номер
        /// </summary>
        /// <param name="doc">Документ откуда получаем тип и юридическую дату</param>
        /// <returns>Номер последнего документа если он найден если не найден то 0</returns>
        private int MaxDocNumForYaerORA(Document doc)
        {
            string CommandSql = String.Format(@"Select Max(DocNum) As MaxDocNum
From `aks`.`cashfunc_document`
Where `DocFullName`='{0}'
  and `UreDate` >= str_to_date('01.01.{1}','%d.%m.%Y')", doc.DocFullName, ((DateTime)doc.UreDate).Year);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".MaxDocNumForYaerORA", EventEn.Dump);

                int rez = 0;

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
                                    for (int i = 0; i < dr.FieldCount; i++)
                                    {
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("MaxDocNum").ToUpper()) rez = int.Parse(dr.GetValue(i).ToString());
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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".MaxDocNumForYaerORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".MaxDocNumForYaerORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".MaxDocNumForYaerORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".MaxDocNumForYaerORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновление документов при встаке документа в прошлое
        /// </summary>
        /// <param name="doc">Документ на который ориентируемся</param>
        private void UpdateNumDocForAddORA(Document doc)
        {
            string CommandSql = String.Format(@"Update `aks`.`cashfunc_document`
Set DocNum=DocNum+1
Where `DocFullName`='{0}'
  and `UreDate` >= str_to_date('{1}','%d.%m.%Y')
  and `UreDate` < str_to_date('01.01.{2}','%d.%m.%Y')", doc.DocFullName, 
                ((DateTime)doc.UreDate).AddDays(1),
                ((DateTime)doc.UreDate).AddYears(1).Year);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateNumDocForAddORA", EventEn.Dump);

                //int rez = 0;

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

                //return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateNumDocForAddORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateNumDocForAddORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateNumDocForAddORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateNumDocForAddORA", EventEn.Dump);
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
        /// Сохранение Operation в базе
        /// </summary>
        /// <param name="NewOperation">Новый Operation который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        private int SetOperationORA(Operation NewOperation)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationORA", EventEn.Dump);

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

                return 0;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновление Operation в базе
        /// </summary>
        /// <param name="UpdOperation">Обновляемый Operation</param>
        private void UpdateOperationORA(Operation UpdOperation)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationPrihod
        /// </summary>
        /// <param name="OperationPrihod">Объект OperationPrihod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashOperationPrihodORA(BLL.OperationPlg.OperationPrihod OperationPrihod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationPrihodORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationPrihodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationPrihodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту OperationPrihod
        /// </summary>
        /// <param name="OperationPrihod">Объект OperationPrihod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetOperationPrihodORA(ref BLL.OperationPlg.OperationPrihod OperationPrihod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationPrihodORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationPrihodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationPrihodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationPrihod
        /// </summary>
        /// <param name="NewOperationPrihod">Вставляем в базу информацию по объекту OperationPrihod</param>
        private void SetOperationPrihodORA(BLL.OperationPlg.OperationPrihod NewOperationPrihod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationPrihodORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationPrihodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationPrihodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationPrihod
        /// </summary>
        /// <param name="UpdOperationPrihod">Сам объект данные которого нужно обновить</param>
        private void UpdateOperationPrihodORA(BLL.OperationPlg.OperationPrihod UpdOperationPrihod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationPrihodORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationPrihodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationPrihodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationRashod
        /// </summary>
        /// <param name="OperationRashod">Объект OperationRashod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashOperationRashodORA(BLL.OperationPlg.OperationRashod OperationRashod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationRashodORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationRashodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationRashodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту OperationRashod
        /// </summary>
        /// <param name="OperationRashod">Объект OperationRashod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetOperationRashodORA(ref BLL.OperationPlg.OperationRashod OperationRashod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationRashodORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationRashodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationRashodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationRashod
        /// </summary>
        /// <param name="NewOperationRashod">Вставляем в базу информацию по объекту OperationRashod</param>
        private void SetOperationRashodORA(BLL.OperationPlg.OperationRashod NewOperationRashod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationRashodORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationRashodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationRashodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationRashod
        /// </summary>
        /// <param name="UpdOperationRashod">Сам объект данные которого нужно обновить</param>
        private void UpdateOperationRashodORA(BLL.OperationPlg.OperationRashod UpdOperationRashod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationRashodORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationRashodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationRashodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationKasBook
        /// </summary>
        /// <param name="OperationKasBook">Объект OperationKasBook который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashOperationKasBookORA(BLL.OperationPlg.OperationKasBook OperationKasBook)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationKasBookORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationKasBookORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationKasBookORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту OperationKasBook
        /// </summary>
        /// <param name="OperationKasBook">Объект OperationKasBook который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetOperationKasBookORA(ref BLL.OperationPlg.OperationKasBook OperationKasBook)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationKasBookORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationKasBookORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationKasBookORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationKasBook
        /// </summary>
        /// <param name="NewOperationKasBook">Вставляем в базу информацию по объекту OperationKasBook</param>
        private void SetOperationKasBookORA(BLL.OperationPlg.OperationKasBook NewOperationKasBook)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationKasBookORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationKasBookORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationKasBookORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationKasBook
        /// </summary>
        /// <param name="UpdOperationKasBook">Сам объект данные которого нужно обновить</param>
        private void UpdateOperationKasBookORA(BLL.OperationPlg.OperationKasBook UpdOperationKasBook)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationKasBookORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationKasBookORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationKasBookORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationInvent
        /// </summary>
        /// <param name="OperationInvent">Объект OperationInvent который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashOperationInventORA(BLL.OperationPlg.OperationInvent OperationInvent)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationInventORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationInventORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationInventORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту OperationInvent
        /// </summary>
        /// <param name="OperationInvent">Объект OperationInvent который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetOperationInventORA(ref BLL.OperationPlg.OperationInvent OperationInvent)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationInventORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationInventORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationInventORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationInvent
        /// </summary>
        /// <param name="NewOperationInvent">Вставляем в базу информацию по объекту OperationInvent</param>
        private void SetOperationInventORA(BLL.OperationPlg.OperationInvent NewOperationInvent)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationInventORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationInventORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationInventORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationInvent
        /// </summary>
        /// <param name="UpdOperationInvent">Сам объект данные которого нужно обновить</param>
        private void UpdateOperationInventORA(BLL.OperationPlg.OperationInvent UpdOperationInvent)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationInventORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationInventORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationInventORA", EventEn.Dump);
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
        /// Сохранение Local в базе
        /// </summary>
        /// <param name="NewLocal">Новый локал который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        private int SetLocalORA(Local NewLocal)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalORA", EventEn.Dump);

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

                return 0;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновление Local в базе
        /// </summary>
        /// <param name="UpdLocal">Обновляемый локал</param>
        private void UpdateLocalORA(Local UpdLocal)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalKassa
        /// </summary>
        /// <param name="LocalKassa">Объект LocalKassa который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashLocalKassaORA(BLL.LocalPlg.LocalKassa LocalKassa)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalKassaORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalKassaORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalKassaORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalKassaORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalKassaORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту LocalKassa
        /// </summary>
        /// <param name="LocalKassa">Объект LocalKassa который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetLocalKassaORA(ref BLL.LocalPlg.LocalKassa LocalKassa)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalKassaORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalKassaORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalKassaORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalKassaORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalKassaORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект LocalKassa
        /// </summary>
        /// <param name="NewLocalKassa">Вставляем в базу информацию по объекту LocalKassa</param>
        private void SetLocalKassaORA(BLL.LocalPlg.LocalKassa NewLocalKassa)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalKassaORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalKassaORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalKassaORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalKassaORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalKassaORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту LocalKassa
        /// </summary>
        /// <param name="UpdLocalKassa">Сам объект данные которого нужно обновить</param>
        private void UpdateLocalKassaORA(BLL.LocalPlg.LocalKassa UpdLocalKassa)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalKassaORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalKassaORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalKassaORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalKassaORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalKassaORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalPaidInReasons
        /// </summary>
        /// <param name="LocalPaidInReasons">Объект LocalPaidInReasons который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashLocalPaidInReasonsORA(BLL.LocalPlg.LocalPaidInReasons LocalPaidInReasons)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidInReasonsORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalPaidInReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidInReasonsORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalPaidInReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidInReasonsORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту LocalPaidInReasons
        /// </summary>
        /// <param name="LocalKassa">Объект LocalPaidInReasons который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetLocalPaidInReasonsORA(ref BLL.LocalPlg.LocalPaidInReasons LocalPaidInReasons)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidInReasonsORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalPaidInReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidInReasonsORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalPaidInReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidInReasonsORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект LocalPaidInReasons
        /// </summary>
        /// <param name="NewLocalPaidInReasons">Вставляем в базу информацию по объекту LocalPaidInReasons</param>
        private void SetLocalPaidInReasonsORA(BLL.LocalPlg.LocalPaidInReasons NewLocalPaidInReasons)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidInReasonsORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalPaidInReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidInReasonsORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalPaidInReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidInReasonsORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту LocalPaidInReasons
        /// </summary>
        /// <param name="UpdLocalPaidInReasons">Сам объект данные которого нужно обновить</param>
        private void UpdateLocalPaidInReasonsORA(BLL.LocalPlg.LocalPaidInReasons UpdLocalPaidInReasons)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidInReasonsORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalPaidInReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidInReasonsORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalPaidInReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidInReasonsORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalPaidRashReasons
        /// </summary>
        /// <param name="LocalPaidRashReasons">Объект LocalPaidRashReasons который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashLocalPaidRashReasonsORA(BLL.LocalPlg.LocalPaidRashReasons LocalPaidRashReasons)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidRashReasonsORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalPaidRashReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidRashReasonsORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalPaidRashReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidRashReasonsORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту LocalPaidRashReasons
        /// </summary>
        /// <param name="LocalKassa">Объект LocalPaidRashReasons который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetLocalPaidRashReasonsORA(ref BLL.LocalPlg.LocalPaidRashReasons LocalPaidRashReasons)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidRashReasonsORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalPaidRashReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidRashReasonsORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalPaidRashReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidRashReasonsORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект LocalPaidRashReasons
        /// </summary>
        /// <param name="NewLocalPaidRashReasons">Вставляем в базу информацию по объекту LocalPaidRashReasons</param>
        private void SetLocalPaidRashReasonsORA(BLL.LocalPlg.LocalPaidRashReasons NewLocalPaidRashReasons)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidRashReasonsORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalPaidRashReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidRashReasonsORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalPaidRashReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidRashReasonsORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту LocalPaidRashReasons
        /// </summary>
        /// <param name="UpdLocalPaidRashReasons">Сам объект данные которого нужно обновить</param>
        private void UpdateLocalPaidRashReasonsORA(BLL.LocalPlg.LocalPaidRashReasons UpdLocalPaidRashReasons)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidRashReasonsORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalPaidRashReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidRashReasonsORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalPaidRashReasonsORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidRashReasonsORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Получение остатка на начало заданной даты и оборота за день
        /// </summary>
        /// <param name="Dt">Дата на которую ищем данные</param>
        /// <returns>Результат остаток на начало даты и оборот за эту дату</returns>
        private RezultForOstatokAndOborot GetOstatokAndOborotForDayORA(DateTime Dt)
        {
            string CommandSql = String.Format(@"Select `Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision` 
From `aks`.`cashfunc_local`");

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOstatokAndOborotForDayORA", EventEn.Dump);

                RezultForOstatokAndOborot rez = new RezultForOstatokAndOborot();

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOstatokAndOborotForDayORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOstatokAndOborotForDayORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOstatokAndOborotForDayORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOstatokAndOborotForDayORA", EventEn.Dump);
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
From `aks`.`cashfunc_local`
Order by `Id`");

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

        /// <summary>
        /// Получаем список докуменитов
        /// </summary>
        /// <param name="Dt">За конкретную дату время будет отброшено</param>
        /// <param name="OperationId">Какая операция нас интересует, если null значит все операции за эту дату</param>
        /// <param name="HasNotin">Если true то будет смотреть все операции кроме операции указанной в параметре OperationId</param>
        /// <returns>Получает список Document из базы данных удовлетворяющий фильтрам</returns>
        private DocumentList GetDocumentListFromDbORA(DateTime? Dt, int? OperationId, bool HasNotin)
        {
            string CommandSql = String.Format(@"Select `Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision` 
From `aks`.`cashfunc_local`
Order by `Id`");

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

        /// <summary>
        /// Сохранение Local в базе
        /// </summary>
        /// <param name="NewDocument">Новый документ который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        private int SetDocumentORA(Document NewDocument)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentORA", EventEn.Dump);

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

                return 0;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновление Document в базе
        /// </summary>
        /// <param name="UpdDocument">Обновляемый документ</param>
        private void UpdateDocumentORA(Document UpdDocument)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentPrihod
        /// </summary>
        /// <param name="DocumentPrihod">Объект DocumentPrihod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashDocumentPrihodORA(BLL.DocumentPlg.DocumentPrihod DocumentPrihod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentPrihodORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentPrihodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentPrihodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту DocumentPrihod
        /// </summary>
        /// <param name="DocumentPrihod">Объект DocumentPrihod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetDocumentPrihodORA(ref BLL.DocumentPlg.DocumentPrihod DocumentPrihod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentPrihodORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentPrihodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentPrihodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentPrihod
        /// </summary>
        /// <param name="NewDocumentPrihod">Вставляем в базу информацию по объекту DocumentPrihod</param>
        private void SetDocumentPrihodORA(BLL.DocumentPlg.DocumentPrihod NewDocumentPrihod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentPrihodORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentPrihodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentPrihodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentPrihod
        /// </summary>
        /// <param name="UpdDocumentPrihod">Сам объект данные которого нужно обновить</param>
        private void UpdateDocumentPrihodORA(BLL.DocumentPlg.DocumentPrihod UpdDocumentPrihod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentPrihodORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentPrihodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentPrihodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentPrihodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentRashod
        /// </summary>
        /// <param name="DocumentRashod">Объект DocumentRashod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashDocumentRashodORA(BLL.DocumentPlg.DocumentRashod DocumentRashod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentRashodORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentRashodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentRashodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту DocumentRashod
        /// </summary>
        /// <param name="DocumentRashod">Объект DocumentRashod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetDocumentRashodORA(ref BLL.DocumentPlg.DocumentRashod DocumentRashod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentRashodORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentRashodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentRashodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentRashod
        /// </summary>
        /// <param name="NewDocumentRashod">Вставляем в базу информацию по объекту DocumentRashod</param>
        private void SetDocumentRashodORA(BLL.DocumentPlg.DocumentRashod NewDocumentRashod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentRashodORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentRashodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentRashodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentRashod
        /// </summary>
        /// <param name="UpdDocumentRashod">Сам объект данные которого нужно обновить</param>
        private void UpdateDocumentRashodORA(BLL.DocumentPlg.DocumentRashod UpdDocumentRashod)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentRashodORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentRashodORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentRashodORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentRashodORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentKasBook
        /// </summary>
        /// <param name="DocumentKasBook">Объект DocumentKasBook который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashDocumentKasBookORA(BLL.DocumentPlg.DocumentKasBook DocumentKasBook)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentKasBookORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentKasBookORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentKasBookORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту DocumentKasBook
        /// </summary>
        /// <param name="DocumentKasBook">Объект DocumentKasBook который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetDocumentKasBookORA(ref BLL.DocumentPlg.DocumentKasBook DocumentKasBook)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentKasBookORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentKasBookORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentKasBookORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentKasBook
        /// </summary>
        /// <param name="NewDocumentKasBook">Вставляем в базу информацию по объекту DocumentKasBook</param>
        private void SetDocumentKasBookORA(BLL.DocumentPlg.DocumentKasBook NewDocumentKasBook)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentKasBookORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentKasBookORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentKasBookORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentKasBook
        /// </summary>
        /// <param name="UpdDocumentKasBook">Сам объект данные которого нужно обновить</param>
        private void UpdateDocumentKasBookORA(BLL.DocumentPlg.DocumentKasBook UpdDocumentKasBook)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentKasBookORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentKasBookORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentKasBookORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentKasBookORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentInvent
        /// </summary>
        /// <param name="DocumentInvent">Объект DocumentInvent который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashDocumentInventORA(BLL.DocumentPlg.DocumentInvent DocumentInvent)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentInventORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentInventORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentInventORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Читаем информацию по объекту DocumentInvent
        /// </summary>
        /// <param name="DocumentInvent">Объект DocumentInvent который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetDocumentInventORA(ref BLL.DocumentPlg.DocumentInvent DocumentInvent)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentInventORA", EventEn.Dump);

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

                return false;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentInventORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentInventORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentInvent
        /// </summary>
        /// <param name="NewDocumentInvent">Вставляем в базу информацию по объекту DocumentInvent</param>
        private void SetDocumentInventORA(BLL.DocumentPlg.DocumentInvent NewDocumentInvent)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentInventORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentInventORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentInventORA", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentInvent
        /// </summary>
        /// <param name="UpdDocumentInvent">Сам объект данные которого нужно обновить</param>
        private void UpdateDocumentInventORA(BLL.DocumentPlg.DocumentInvent UpdDocumentInvent)
        {
            string CommandSql = "";// String.Format(@"insert into aks.prizm_cust_porog(cust_inn, invc_no, dt, pos_date, total_cash_sum) Values('{0}', '{1}', TO_DATE('{2}.{3}.{4}', 'YYYY.MM.DD'), STR_TO_DATE('{2}.{3}.{4} {5}:{6}:{7}', 'YYYY.MM.DD HH24:MI:SS'), {8})", CustInn, InvcNo, PosDate.Year, PosDate.Month, PosDate.Day, PosDate.Hour, PosDate.Minute, PosDate.Second, TotalCashSum.ToString().Replace(',', '.'));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentInventORA", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentInventORA", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentInventORA", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentInventORA", EventEn.Dump);
                throw;
            }
        }
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
        /// Установка номеров документа по правилам связанным с началом года
        /// </summary>
        public void SetDocNumForYearMySql()
        {
            string CommandSql1 = String.Format(@"With T As (Select `Id`, `UreDate`, `OperationId`, `DocNum`,
      row_number() over(partition by YEAR(`UreDate`), `OperationId` order by `UreDate`) As PRN
    From `aks`.`cashfunc_document`
    Where `UreDate`>=str_to_date(ConCat('01.01.',convert(YEAR(curdate())-1, char)),'%d.%m.%Y'))
Update `aks`.`cashfunc_document` D 
  inner join  T On D.Id=T.Id
Set D.`DocNum`=T.`PRN`");

            string CommandSql2 = String.Format(@"With T As (Select `OperationId`, Max(`DocNum`) As DocNum
    From `aks`.`cashfunc_document`
    Where `UreDate`>=str_to_date(ConCat('01.01.',convert(YEAR(curdate()), char)),'%d.%m.%Y')
    Group By `OperationId`),
    R As (Select Max(Case When `OperationId`=1 Then `DocNum` else 0 End) Prih,
      Max(Case When `OperationId`=2 Then `DocNum` else 0 End) Rash,
      Max(Case When `OperationId`=3 Then `DocNum` else 0 End) KasBook,
      Max(Case When `OperationId`=4 Then `DocNum` else 0 End) Invent
    From  T)
Update `aks`.`cashfunc_local_kassa` K
Set K.LastDocNumPrih=(Select Prih From R),
  K.LastDocNumRash=(Select Rash From R),
  K.LastDocNumKasBook=(Select KasBook From R),
  K.LastDocNumInvent=(Select Invent From R)
Where K.Id={0}", Com.LocalFarm.CurLocalDepartament.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql1, GetType().Name + ".SetDocNumForYearMySql (Qwery1)", EventEn.Dump);
                   
                

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql1, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        com.ExecuteNonQuery();
                    }
                }

                //return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocNumForYearMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql1, GetType().Name + ".SetDocNumForYearMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocNumForYearMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql1, GetType().Name + ".SetDocNumForYearMySql", EventEn.Dump);
                throw;
            }

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql2, GetType().Name + ".SetDocNumForYearMySql  (Qwery2)", EventEn.Dump);
                
                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql2, con))
                    {
                        com.CommandTimeout = 900;  // 15 минут
                        com.ExecuteNonQuery();
                    }
                }

                //return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocNumForYearMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql2, GetType().Name + ".SetDocNumForYearMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocNumForYearMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql2, GetType().Name + ".SetDocNumForYearMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Получаем последний номер документа по типу который задан в документе за год в котором юридическая дата документа на основе которого получаем номер
        /// </summary>
        /// <param name="doc">Документ откуда получаем тип и юридическую дату</param>
        /// <returns>Номер последнего документа если он найден если не найден то 0</returns>
        private int MaxDocNumForYaerMySql(Document doc)
        {
            string CommandSql = String.Format(@"Select Max(DocNum) As MaxDocNum
From `aks`.`cashfunc_document`
Where `DocFullName`='{0}'
  and `UreDate` >= str_to_date('01.01.{1}','%d.%m.%Y')", doc.DocFullName, ((DateTime)doc.UreDate).Year);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".MaxDocNumForYaerMySql", EventEn.Dump);

                int rez = 0;

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
                                    for (int i = 0; i < dr.FieldCount; i++)
                                    {
                                        if (!dr.IsDBNull(i) && dr.GetName(i).ToUpper() == ("MaxDocNum").ToUpper()) rez = int.Parse(dr.GetValue(i).ToString());
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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".MaxDocNumForYaerMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".MaxDocNumForYaerMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".MaxDocNumForYaerMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".MaxDocNumForYaerMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновление документов при встаке документа в прошлое
        /// </summary>
        /// <param name="doc">Документ на который ориентируемся</param>
        private void UpdateNumDocForAddMySql(Document doc)
        {
            string CommandSql = String.Format(@"Update `aks`.`cashfunc_document`
Set DocNum=DocNum+1
Where `DocFullName`='{0}'
  and `UreDate` >= str_to_date('{1}','%d.%m.%Y')
  and `UreDate` < str_to_date('01.01.{2}','%d.%m.%Y')", doc.DocFullName,
                ((DateTime)doc.UreDate).AddDays(1).ToShortDateString(),
                ((DateTime)doc.UreDate).AddYears(1).Year);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateNumDocForAddMySql", EventEn.Dump);

                //int rez = 0;

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

                //return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateNumDocForAddMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateNumDocForAddMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateNumDocForAddMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateNumDocForAddMySql", EventEn.Dump);
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
        /// Сохранение Operation в базе
        /// </summary>
        /// <param name="NewOperation">Новый Operation который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        private int SetOperationMySql(Operation NewOperation)
        {
            int rez = 0;

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_operation`(`OpFullName`, `OperationName`, `KoefDebitor`, `KoefCreditor`)
Values(?, ?, ?, ?)");
            string CommandSql2 = "SELECT LAST_INSERT_ID()  As Id";


            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.Parameters.Add(new OdbcParameter("OpFullName", OdbcType.VarChar, 100) { Value = NewOperation.OpFullName });
                        com.Parameters.Add(new OdbcParameter("OperationName", OdbcType.VarChar, 100) { Value = NewOperation.OperationName });
                        com.Parameters.Add(new OdbcParameter("KoefDebitor", OdbcType.Int) { Value = NewOperation.KoefDebitor });
                        com.Parameters.Add(new OdbcParameter("KoefCreditor", OdbcType.Int) { Value = NewOperation.KoefCreditor });


                        com.CommandTimeout = 900;  // 15 минут
                        com.ExecuteNonQuery();
                    }
                    using (OdbcCommand com2 = new OdbcCommand(CommandSql2, con))
                    {

                        com2.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com2.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                // пробегаем по строкам
                                while (dr.Read())
                                {
                                    if (!dr.IsDBNull(0))
                                    {
                                        string tmp = dr.GetValue(0).ToString();
                                        rez = int.Parse(tmp);
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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновление Operation в базе
        /// </summary>
        /// <param name="UpdOperation">Обновляемый Operation</param>
        private void UpdateOperationMySql(Operation UpdOperation)
        {
            if (UpdOperation.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"Update `aks`.`cashfunc_operation`
Set `OperationName`= '{1}', `KoefDebitor`={2}, `KoefCreditor`={3}
Where Id={0}", UpdOperation.Id,
                    UpdOperation.OperationName,
                    UpdOperation.KoefDebitor,
                    UpdOperation.KoefCreditor);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при обновлении данных в источнике. {0}", ex.Message), GetType().Name + ".UpdateOperationMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при обновлении данных в источнике. {0}", ex.Message), GetType().Name + ".UpdateOperationMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationPrihod
        /// </summary>
        /// <param name="OperationPrihod">Объект OperationPrihod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashOperationPrihodMySql(BLL.OperationPlg.OperationPrihod OperationPrihod)
        {
            if (OperationPrihod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select *
From `aks`.`cashfunc_Operation_Prihod`
Where Id={0}", (int)OperationPrihod.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationPrihodMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                rez = true;
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationPrihodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationPrihodMySql", EventEn.Dump);
                throw;
            }

        }

        /// <summary>
        /// Читаем информацию по объекту OperationPrihod
        /// </summary>
        /// <param name="OperationPrihod">Объект OperationPrihod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetOperationPrihodMySql(ref BLL.OperationPlg.OperationPrihod OperationPrihod)
        {
            if (OperationPrihod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select `OKUD`
From `aks`.`cashfunc_Operation_Prihod`
Where Id={0}", (int)OperationPrihod.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationPrihodMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = true;

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
                                    if (!dr.IsDBNull(0)) OperationPrihod.OKUD= dr.GetString(0);
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationPrihodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationPrihodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationPrihod
        /// </summary>
        /// <param name="NewOperationPrihod">Вставляем в базу информацию по объекту OperationPrihod</param>
        private void SetOperationPrihodMySql(BLL.OperationPlg.OperationPrihod NewOperationPrihod)
        {
            if (NewOperationPrihod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_Operation_Prihod`(id, `OKUD`) 
Values({0}, {1})", NewOperationPrihod.Id,
                (string.IsNullOrWhiteSpace(NewOperationPrihod.OKUD) ? "null" : string.Format("'{0}'",NewOperationPrihod.OKUD)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationPrihodMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationPrihodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationPrihodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationPrihod
        /// </summary>
        /// <param name="UpdOperationPrihod">Сам объект данные которого нужно обновить</param>
        private void UpdateOperationPrihodMySql(BLL.OperationPlg.OperationPrihod UpdOperationPrihod)
        {
            if (UpdOperationPrihod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"update `aks`.`cashfunc_Operation_Prihod`
Set `OKUD`={1}
Where Id={0}", UpdOperationPrihod.Id, 
                (string.IsNullOrWhiteSpace(UpdOperationPrihod.OKUD)?"null": string.Format("'{0}'", UpdOperationPrihod.OKUD)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationPrihodMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationPrihodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationPrihodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationRashod
        /// </summary>
        /// <param name="OperationRashod">Объект OperationRashod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashOperationRashodMySql(BLL.OperationPlg.OperationRashod OperationRashod)
        {
            if (OperationRashod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select *
From `aks`.`cashfunc_Operation_Rashod`
Where Id={0}", (int)OperationRashod.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationRashodMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                rez = true;
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationRashodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationRashodMySql", EventEn.Dump);
                throw;
            }

        }

        /// <summary>
        /// Читаем информацию по объекту OperationRashod
        /// </summary>
        /// <param name="OperationRashod">Объект OperationRashod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetOperationRashodMySql(ref BLL.OperationPlg.OperationRashod OperationRashod)
        {
            if (OperationRashod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select `OKUD`
From `aks`.`cashfunc_Operation_Rashod`
Where Id={0}", (int)OperationRashod.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationRashodMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = true;

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
                                    if (!dr.IsDBNull(0)) OperationRashod.OKUD = dr.GetString(0);
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationRashodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationRashodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationRashod
        /// </summary>
        /// <param name="NewOperationRashod">Вставляем в базу информацию по объекту OperationRashod</param>
        private void SetOperationRashodMySql(BLL.OperationPlg.OperationRashod NewOperationRashod)
        {
            if (NewOperationRashod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_Operation_Rashod`(id, `OKUD`) 
Values({0}, {1})", NewOperationRashod.Id,
                (string.IsNullOrWhiteSpace(NewOperationRashod.OKUD) ? "null" : string.Format("'{0}'", NewOperationRashod.OKUD)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationRashodMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationRashodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationRashodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationRashod
        /// </summary>
        /// <param name="UpdOperationRashod">Сам объект данные которого нужно обновить</param>
        private void UpdateOperationRashodMySql(BLL.OperationPlg.OperationRashod UpdOperationRashod)
        {
            if (UpdOperationRashod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"update `aks`.`cashfunc_Operation_Prihod`
Set `OKUD`={1}
Where Id={0}", UpdOperationRashod.Id,
                (string.IsNullOrWhiteSpace(UpdOperationRashod.OKUD) ? "null" : string.Format("'{0}'", UpdOperationRashod.OKUD)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationRashodMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationRashodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationRashodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationKasBook
        /// </summary>
        /// <param name="OperationKasBook">Объект OperationKasBook который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashOperationKasBookMySql(BLL.OperationPlg.OperationKasBook OperationKasBook)
        {
            if (OperationKasBook.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select *
From `aks`.`cashfunc_Operation_KasBook`
Where Id={0}", (int)OperationKasBook.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationKasBookMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                rez = true;
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationKasBookMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationKasBookMySql", EventEn.Dump);
                throw;
            }

        }

        /// <summary>
        /// Читаем информацию по объекту OperationKasBook
        /// </summary>
        /// <param name="OperationKasBook">Объект OperationKasBook который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetOperationKasBookMySql(ref BLL.OperationPlg.OperationKasBook OperationKasBook)
        {
            if (OperationKasBook.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select `OKUD`
From `aks`.`cashfunc_Operation_KasBook`
Where Id={0}", (int)OperationKasBook.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationKasBookMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = true;

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
                                    if (!dr.IsDBNull(0)) OperationKasBook.OKUD = dr.GetString(0);
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationKasBookMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationKasBookMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationKasBook
        /// </summary>
        /// <param name="NewOperationKasBook">Вставляем в базу информацию по объекту OperationKasBook</param>
        private void SetOperationKasBookMySql(BLL.OperationPlg.OperationKasBook NewOperationKasBook)
        {
            if (NewOperationKasBook.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_Operation_KasBook`(id, `OKUD`) 
Values({0}, {1})", NewOperationKasBook.Id,
                (string.IsNullOrWhiteSpace(NewOperationKasBook.OKUD) ? "null" : string.Format("'{0}'", NewOperationKasBook.OKUD)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationKasBookMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationKasBookMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationKasBookMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationKasBook
        /// </summary>
        /// <param name="UpdOperationKasBook">Сам объект данные которого нужно обновить</param>
        private void UpdateOperationKasBookMySql(BLL.OperationPlg.OperationKasBook UpdOperationKasBook)
        {
            if (UpdOperationKasBook.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"update `aks`.`cashfunc_Operation_KasBook`
Set `OKUD`={1}
Where Id={0}", UpdOperationKasBook.Id,
                (string.IsNullOrWhiteSpace(UpdOperationKasBook.OKUD) ? "null" : string.Format("'{0}'", UpdOperationKasBook.OKUD)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationKasBookMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationKasBookMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationKasBookMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта OperationInvent
        /// </summary>
        /// <param name="OperationInvent">Объект OperationInvent который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashOperationInventMySql(BLL.OperationPlg.OperationInvent OperationInvent)
        {
            if (OperationInvent.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select *
From `aks`.`cashfunc_Operation_Invent`
Where Id={0}", (int)OperationInvent.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationInventMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                rez = true;
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationInventMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashOperationInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashOperationInventMySql", EventEn.Dump);
                throw;
            }

        }

        /// <summary>
        /// Читаем информацию по объекту OperationInvent
        /// </summary>
        /// <param name="OperationInvent">Объект OperationInvent который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetOperationInventMySql(ref BLL.OperationPlg.OperationInvent OperationInvent)
        {
            if (OperationInvent.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select `OKUD`
From `aks`.`cashfunc_Operation_Invent`
Where Id={0}", (int)OperationInvent.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationInventMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = true;

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
                                    if (!dr.IsDBNull(0)) OperationInvent.OKUD = dr.GetString(0);
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationInventMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOperationInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOperationInventMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект OperationInvent
        /// </summary>
        /// <param name="NewOperationInvent">Вставляем в базу информацию по объекту OperationInvent</param>
        private void SetOperationInventMySql(BLL.OperationPlg.OperationInvent NewOperationInvent)
        {
            if (NewOperationInvent.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_Operation_Invent`(id, `OKUD`) 
Values({0}, {1})", NewOperationInvent.Id,
                (string.IsNullOrWhiteSpace(NewOperationInvent.OKUD) ? "null" : string.Format("'{0}'", NewOperationInvent.OKUD)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationInventMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationInventMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetOperationInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetOperationInventMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту OperationInvent
        /// </summary>
        /// <param name="UpdOperationInvent">Сам объект данные которого нужно обновить</param>
        private void UpdateOperationInventMySql(BLL.OperationPlg.OperationInvent UpdOperationInvent)
        {
            if (UpdOperationInvent.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"update `aks`.`cashfunc_Operation_Invent`
Set `OKUD`={1}
Where Id={0}", UpdOperationInvent.Id,
                (string.IsNullOrWhiteSpace(UpdOperationInvent.OKUD) ? "null" : string.Format("'{0}'", UpdOperationInvent.OKUD)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationInventMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationInventMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateOperationInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateOperationInventMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Получаем список текущий Local
        /// </summary>
        /// <returns>Получает текущий список Local из базы данных</returns>
        private LocalList GetLocalListFromDbMySql()
        {
            string CommandSql = String.Format(@"Select `Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`, `IsDraft` From `aks`.`cashfunc_local` Where `IsDraft`=0");

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
        /// Сохранение Local в базе
        /// </summary>
        /// <param name="NewLocal">Новый локал который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        private int SetLocalMySql(Local NewLocal)
        {
            int rez = 0;

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_local`(`LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`, `IsDraft`)
Values(?, ?, ?, ?, ?, ?)");
            string CommandSql2 = "SELECT LAST_INSERT_ID()  As Id";


            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.Parameters.Add(new OdbcParameter("LocFullName", OdbcType.VarChar, 100) { Value = NewLocal.LocFullName });
                        com.Parameters.Add(new OdbcParameter("LocalName", OdbcType.VarChar, 100) { Value = NewLocal.LocalName });
                        com.Parameters.Add(new OdbcParameter("IsSeller", OdbcType.Bit) { Value = NewLocal.IsSeller });
                        com.Parameters.Add(new OdbcParameter("IsСustomer", OdbcType.Bit) { Value = NewLocal.IsСustomer });
                        com.Parameters.Add(new OdbcParameter("IsDivision", OdbcType.Bit) { Value = NewLocal.IsDivision });
                        com.Parameters.Add(new OdbcParameter("IsDraft", OdbcType.Bit) { Value = NewLocal.IsDraft });


                        com.CommandTimeout = 900;  // 15 минут
                        com.ExecuteNonQuery();
                    }
                    using (OdbcCommand com2 = new OdbcCommand(CommandSql2, con))
                    {

                        com2.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com2.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                // пробегаем по строкам
                                while (dr.Read())
                                {
                                    if (!dr.IsDBNull(0))
                                    {
                                        string tmp = dr.GetValue(0).ToString();
                                        rez = int.Parse(tmp);
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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновление Local в базе
        /// </summary>
        /// <param name="UpdLocal">Обновляемый локал</param>
        private void UpdateLocalMySql(Local UpdLocal)
        {
            if (UpdLocal.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"Update `aks`.`cashfunc_local`
Set `LocalName` = '{0}', `IsSeller`={1}, `IsСustomer`={2}, `IsDivision`={3}, `IsDraft`={4}
Where Id={5}", UpdLocal.LocalName, 
                    (UpdLocal.IsSeller?1:0) , 
                    (UpdLocal.IsСustomer?1:0), 
                    (UpdLocal.IsDivision?1:0),
                    (UpdLocal.IsDraft?1:0),
                    UpdLocal.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при обновлении данных в источнике. {0}", ex.Message), GetType().Name + ".UpdateLocalMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при обновлении данных в источнике. {0}", ex.Message), GetType().Name + ".UpdateLocalMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalKassa
        /// </summary>
        /// <param name="LocalKassa">Объект LocalKassa который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashLocalKassaMySql(BLL.LocalPlg.LocalKassa LocalKassa)
        {
            if (LocalKassa.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select *
From `aks`.`cashfunc_local_kassa`
Where Id={0}", (int)LocalKassa.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalKassaMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                rez = true;
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalKassaMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalKassaMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalKassaMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalKassaMySql", EventEn.Dump);
                throw;
            }

        }

        /// <summary>
        /// Читаем информацию по объекту LocalKassa
        /// </summary>
        /// <param name="LocalKassa">Объект LocalKassa который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetLocalKassaMySql(ref BLL.LocalPlg.LocalKassa LocalKassa)
        {
            if (LocalKassa.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select `HostName`, `Organization`, `StructPodr`, `OKPO`,
    `LastDocNumPrih`, `LastDocNumRash`, `LastDocNumKasBook`, `LastDocNumActVozv`, 
    `LastDocNumReportKas`, `LastDocNumScetKkm`, `LastDocNumVerifNal`, `LastDocNumInvent`,
    `INN`, `ZavodKKM`, `RegKKM`, `GlavBuhFio`,
    `KkmName`, `DolRukOrg`, `RukFio`, `ZavDivisionFio`,
    `CompanyCode`, `StoreCode`, `Upload1CDir`
From `aks`.`cashfunc_local_kassa`
Where Id={0}", (int)LocalKassa.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalKassaMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = true;

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
                                    BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface interf = new BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface();
                                    if (!dr.IsDBNull(0)) interf.SetHostName(LocalKassa, dr.GetString(0));
                                    if (!dr.IsDBNull(1)) LocalKassa.Organization = dr.GetString(1);
                                    if (!dr.IsDBNull(2)) LocalKassa.StructPodrazdelenie = dr.GetString(2);
                                    if (!dr.IsDBNull(3)) LocalKassa.OKPO = dr.GetString(3);
                                    if (!dr.IsDBNull(4)) LocalKassa.LastDocNumPrih = int.Parse(dr.GetValue(4).ToString());
                                    if (!dr.IsDBNull(5)) LocalKassa.LastDocNumRash = int.Parse(dr.GetValue(5).ToString());
                                    if (!dr.IsDBNull(6)) LocalKassa.LastDocNumKasBook = int.Parse(dr.GetValue(6).ToString());
                                    if (!dr.IsDBNull(7)) LocalKassa.LastDocNumActVozv = int.Parse(dr.GetValue(7).ToString());
                                    if (!dr.IsDBNull(8)) LocalKassa.LastDocNumReportKas = int.Parse(dr.GetValue(8).ToString());
                                    if (!dr.IsDBNull(9)) LocalKassa.LastDocNumScetKkm = int.Parse(dr.GetValue(9).ToString());
                                    if (!dr.IsDBNull(10)) LocalKassa.LastDocNumVerifNal = int.Parse(dr.GetValue(10).ToString());
                                    if (!dr.IsDBNull(11)) LocalKassa.LastDocNumInvent = int.Parse(dr.GetValue(11).ToString());
                                    if (!dr.IsDBNull(12)) LocalKassa.INN = dr.GetString(12);
                                    if (!dr.IsDBNull(13)) LocalKassa.ZavodKKM = dr.GetString(13);
                                    if (!dr.IsDBNull(14)) LocalKassa.RegKKM = dr.GetString(14);
                                    if (!dr.IsDBNull(15)) LocalKassa.GlavBuhFio = dr.GetString(15);
                                    if (!dr.IsDBNull(16)) LocalKassa.KkmName = dr.GetString(16);
                                    if (!dr.IsDBNull(17)) LocalKassa.DolRukOrg = dr.GetString(17);
                                    if (!dr.IsDBNull(18)) LocalKassa.RukFio = dr.GetString(18);
                                    if (!dr.IsDBNull(19)) LocalKassa.ZavDivisionFio = dr.GetString(19);
                                    if (!dr.IsDBNull(20)) LocalKassa.CompanyCode = dr.GetString(20);
                                    if (!dr.IsDBNull(21)) LocalKassa.StoreCode = dr.GetString(21);
                                    if (!dr.IsDBNull(22)) LocalKassa.Upload1CDir = dr.GetString(22);
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalKassaMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalKassaMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalKassaMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalKassaMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект LocalKassa
        /// </summary>
        /// <param name="NewLocalKassa">Вставляем в базу информацию по объекту LocalKassa</param>
        private void SetLocalKassaMySql(BLL.LocalPlg.LocalKassa NewLocalKassa)
        {
            if (NewLocalKassa.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_local_kassa`(id, `HostName`, `Organization`, `StructPodr`, `OKPO`,
    `LastDocNumPrih`, `LastDocNumRash`, `LastDocNumKasBook`, `LastDocNumActVozv`, 
    `LastDocNumReportKas`, `LastDocNumScetKkm`, `LastDocNumVerifNal`, `LastDocNumInvent`,
    `INN`, `ZavodKKM`, `RegKKM`, `GlavBuhFio`,
    `KkmName`, `DolRukOrg`, `RukFio`, `ZavDivisionFio`,
    `CompanyCode`, `StoreCode`, `Upload1CDir`) 
Values({0}, '{1}', '{2}', '{3}', '{4}',
    {5}, {6}, {7}, {8},
    {9}, {10}, {11}, {12}, 
    '{13}', '{14}', '{15}', '{16}',
    '{17}', '{18}', '{19}', '{20}',
    '{21}', '{22}', '{23}')", NewLocalKassa.Id, NewLocalKassa.HostName, NewLocalKassa.Organization, NewLocalKassa.StructPodrazdelenie, NewLocalKassa.OKPO,
                            NewLocalKassa.LastDocNumPrih, NewLocalKassa.LastDocNumRash, NewLocalKassa.LastDocNumKasBook, NewLocalKassa.LastDocNumActVozv,
                            NewLocalKassa.LastDocNumReportKas, NewLocalKassa.LastDocNumScetKkm, NewLocalKassa.LastDocNumVerifNal, NewLocalKassa.LastDocNumInvent,
                            NewLocalKassa.INN, NewLocalKassa.ZavodKKM, NewLocalKassa.RegKKM, NewLocalKassa.GlavBuhFio,
                            NewLocalKassa.KkmName, NewLocalKassa.DolRukOrg, NewLocalKassa.RukFio, NewLocalKassa.ZavDivisionFio,
                            NewLocalKassa.CompanyCode, NewLocalKassa.StoreCode, NewLocalKassa.Upload1CDir);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalKassaMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalKassaMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalKassaMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalKassaMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalKassaMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту LocalKassa
        /// </summary>
        /// <param name="UpdLocalKassa">Сам объект данные которого нужно обновить</param>
        private void UpdateLocalKassaMySql(BLL.LocalPlg.LocalKassa UpdLocalKassa)
        {
            if (UpdLocalKassa.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"update `aks`.`cashfunc_local_kassa`
Set `Organization`='{1}', `StructPodr`='{2}', `OKPO`='{3}',
    `LastDocNumPrih`={4}, `LastDocNumRash`={5}, `LastDocNumKasBook`={6}, `LastDocNumActVozv`={7}, 
    `LastDocNumReportKas`={8}, `LastDocNumScetKkm`={9}, `LastDocNumVerifNal`={10}, `LastDocNumInvent`={11},
    `INN`='{12}', `ZavodKKM`='{13}', `RegKKM`='{14}', `GlavBuhFio`='{15}',
    `KkmName`='{16}', `DolRukOrg`='{17}', `RukFio`='{18}', `ZavDivisionFio`='{19}',
    `CompanyCode`='{20}', `StoreCode`='{21}', `Upload1CDir`='{22}'
Where Id={0}", UpdLocalKassa.Id, UpdLocalKassa.Organization, UpdLocalKassa.StructPodrazdelenie, UpdLocalKassa.OKPO,
            UpdLocalKassa.LastDocNumPrih, UpdLocalKassa.LastDocNumRash, UpdLocalKassa.LastDocNumKasBook, UpdLocalKassa.LastDocNumActVozv,
            UpdLocalKassa.LastDocNumReportKas, UpdLocalKassa.LastDocNumScetKkm, UpdLocalKassa.LastDocNumVerifNal, UpdLocalKassa.LastDocNumInvent,
            UpdLocalKassa.INN, UpdLocalKassa.ZavodKKM, UpdLocalKassa.RegKKM, UpdLocalKassa.GlavBuhFio,
            UpdLocalKassa.KkmName, UpdLocalKassa.DolRukOrg, UpdLocalKassa.RukFio, UpdLocalKassa.ZavDivisionFio,
            UpdLocalKassa.CompanyCode, UpdLocalKassa.StoreCode, UpdLocalKassa.Upload1CDir);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalKassaMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalKassaMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalKassaMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalKassaMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalKassaMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalPaidInReasons
        /// </summary>
        /// <param name="LocalPaidInReasons">Объект LocalPaidInReasons который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashLocalPaidInReasonsMySql(BLL.LocalPlg.LocalPaidInReasons LocalPaidInReasons)
        {
            if (LocalPaidInReasons.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select *
From `aks`.`cashfunc_local_PaidInReasons`
Where Id={0}", (int)LocalPaidInReasons.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidInReasonsMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                rez = true;
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalPaidInReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidInReasonsMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalPaidInReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidInReasonsMySql", EventEn.Dump);
                throw;
            }

        }

        /// <summary>
        /// Читаем информацию по объекту LocalPaidInReasons
        /// </summary>
        /// <param name="LocalPaidInReasons">Объект LocalPaidInReasons который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetLocalPaidInReasonsMySql(ref BLL.LocalPlg.LocalPaidInReasons LocalPaidInReasons)
        {
            if (LocalPaidInReasons.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select `Osnovanie`, `DebetNomerSchet`, `KredikKorSchet`
From `aks`.`cashfunc_local_PaidInReasons`
Where Id={0}", (int)LocalPaidInReasons.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidInReasonsMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = true;

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
                                    if (!dr.IsDBNull(0)) LocalPaidInReasons.Osnovanie = dr.GetString(0);
                                    if (!dr.IsDBNull(1)) LocalPaidInReasons.DebetNomerSchet = dr.GetString(1);
                                    if (!dr.IsDBNull(2)) LocalPaidInReasons.KredikKorSchet = dr.GetString(2);
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalPaidInReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidInReasonsMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalPaidInReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidInReasonsMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект LocalPaidInReasons
        /// </summary>
        /// <param name="NewLocalPaidInReasons">Вставляем в базу информацию по объекту LocalPaidInReasons</param>
        private void SetLocalPaidInReasonsMySql(BLL.LocalPlg.LocalPaidInReasons NewLocalPaidInReasons)
        {
            if (NewLocalPaidInReasons.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_local_PaidInReasons`(id, `Osnovanie`, `DebetNomerSchet`, `KredikKorSchet`) 
Values({0}, '{1}', '{2}', '{3}')", NewLocalPaidInReasons.Id, NewLocalPaidInReasons.Osnovanie, NewLocalPaidInReasons.DebetNomerSchet, NewLocalPaidInReasons.KredikKorSchet);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidInReasonsMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalPaidInReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidInReasonsMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalPaidInReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidInReasonsMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту LocalPaidInReasons
        /// </summary>
        /// <param name="UpdLocalPaidInReasons">Сам объект данные которого нужно обновить</param>
        private void UpdateLocalPaidInReasonsMySql(BLL.LocalPlg.LocalPaidInReasons UpdLocalPaidInReasons)
        {
            if (UpdLocalPaidInReasons.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"update `aks`.`cashfunc_local_PaidInReasons`
Set `Osnovanie`='{1}', `DebetNomerSchet`='{2}', `KredikKorSchet`='{3}'
Where Id={0}", UpdLocalPaidInReasons.Id, UpdLocalPaidInReasons.Osnovanie, UpdLocalPaidInReasons.DebetNomerSchet, UpdLocalPaidInReasons.KredikKorSchet);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidInReasonsMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalPaidInReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidInReasonsMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalPaidInReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidInReasonsMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта LocalPaidRashReasons
        /// </summary>
        /// <param name="LocalPaidRashReasons">Объект LocalPaidRashReasons который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashLocalPaidRashReasonsMySql(BLL.LocalPlg.LocalPaidRashReasons LocalPaidRashReasons)
        {
            if (LocalPaidRashReasons.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select *
From `aks`.`cashfunc_local_PaidRashReasons`
Where Id={0}", (int)LocalPaidRashReasons.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidRashReasonsMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                rez = true;
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalPaidRashReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidRashReasonsMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashLocalPaidRashReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashLocalPaidRashReasonsMySql", EventEn.Dump);
                throw;
            }

        }

        /// <summary>
        /// Читаем информацию по объекту LocalPaidRashReasons
        /// </summary>
        /// <param name="LocalPaidRashReasons">Объект LocalPaidRashReasons который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetLocalPaidRashReasonsMySql(ref BLL.LocalPlg.LocalPaidRashReasons LocalPaidRashReasons)
        {
            if (LocalPaidRashReasons.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select `Osnovanie`, `KreditNomerSchet`, `DebetKorSchet`, `FlagFormReturn`
From `aks`.`cashfunc_local_PaidRashReasons`
Where Id={0}", (int)LocalPaidRashReasons.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidRashReasonsMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = true;

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
                                    if (!dr.IsDBNull(0)) LocalPaidRashReasons.Osnovanie = dr.GetString(0);
                                    if (!dr.IsDBNull(1)) LocalPaidRashReasons.KreditNomerSchet = dr.GetString(1);
                                    if (!dr.IsDBNull(2)) LocalPaidRashReasons.DebetKorSchet = dr.GetString(2);
                                    if (!dr.IsDBNull(3) && dr.GetInt32(3) == 0) LocalPaidRashReasons.FlagFormReturn = false;
                                    else LocalPaidRashReasons.FlagFormReturn = true;
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalPaidRashReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidRashReasonsMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetLocalPaidRashReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetLocalPaidRashReasonsMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект LocalPaidRashReasons
        /// </summary>
        /// <param name="NewLocalPaidRashReasons">Вставляем в базу информацию по объекту LocalPaidRashReasons</param>
        private void SetLocalPaidRashReasonsMySql(BLL.LocalPlg.LocalPaidRashReasons NewLocalPaidRashReasons)
        {
            if (NewLocalPaidRashReasons.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_local_PaidRashReasons`(id, `Osnovanie`, `KreditNomerSchet`, `DebetKorSchet`, 
`FlagFormReturn`) 
Values({0}, '{1}', '{2}', '{3}',
    {4})", NewLocalPaidRashReasons.Id, NewLocalPaidRashReasons.Osnovanie, NewLocalPaidRashReasons.KreditNomerSchet, NewLocalPaidRashReasons.DebetKorSchet,
        (NewLocalPaidRashReasons.FlagFormReturn?1:0));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidRashReasonsMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalPaidRashReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidRashReasonsMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetLocalPaidRashReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetLocalPaidRashReasonsMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту LocalPaidRashReasons
        /// </summary>
        /// <param name="UpdLocalPaidRashReasons">Сам объект данные которого нужно обновить</param>
        private void UpdateLocalPaidRashReasonsMySql(BLL.LocalPlg.LocalPaidRashReasons UpdLocalPaidRashReasons)
        {
            if (UpdLocalPaidRashReasons.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"update `aks`.`cashfunc_local_PaidRashReasons`
Set `Osnovanie`='{1}', `KreditNomerSchet`='{2}', `DebetKorSchet`='{3}', `FlagFormReturn`={4}
Where Id={0}", UpdLocalPaidRashReasons.Id, 
            UpdLocalPaidRashReasons.Osnovanie, UpdLocalPaidRashReasons.KreditNomerSchet, UpdLocalPaidRashReasons.DebetKorSchet,
            (UpdLocalPaidRashReasons.FlagFormReturn?1:0));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidRashReasonsMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalPaidRashReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidRashReasonsMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateLocalPaidRashReasonsMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateLocalPaidRashReasonsMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Получение остатка на начало заданной даты и оборота за день
        /// </summary>
        /// <param name="Dt">Дата на которую ищем данные</param>
        /// <returns>Результат остаток на начало даты и оборот за эту дату</returns>
        private RezultForOstatokAndOborot GetOstatokAndOborotForDayMySql(DateTime Dt)
        {
            string CommandSql = String.Format(@"With T As (Select Str_To_Date('{0}','%d.%m.%Y') As FindDt),
    # Получаем дату начиная с которой нужно искать документы и если есть ближайший кассовый документ то подтягиваем из него дату и остаток на начало дня который потом будем использовать как стартовое значение суммы
    Conf As (Select Coalesce(Trim(Max(D.`UreDate`)), (Select Trim(Min(`UreDate`)) From `aks`.`cashfunc_document`) ) As FindStart
        , (Select Trim(Min(FindDt)) From T) As FindDt
        , Coalesce(Min(`SummaStartDay`),0) As StartSumm
      From `aks`.`cashfunc_document` D
        inner join `aks`.`cashfunc_document_kasbook` K On D.Id=K.Id
      where D.`UreDate` < (Select Min(FindDt) From T)
        and D.`DocFullName`='DocumentKasBook'),
    # Получаем список отфильтрованных документов с признаком даты на которую ищем и стартовой суммой
    Doc As (Select C.FindDt, C.StartSumm, D.Id, Trim(D.UreDate) As UreDate, O.KoefDebitor
      From Conf C
        inner join `aks`.`cashfunc_document` D On D.UreDate>=C.FindStart
        inner join `aks`.`cashfunc_operation` O On D.OperationId=O.Id
      Where D.`DocFullName` in ('DocumentPrihod', 'DocumentRashod')
        and D.`UreDate`>=C.FindStart
        and D.`UreDate`<Date_Add(FindDt,interval 1 day)),
    # Считаем по документу прихода
    DocPrihod As (Select Sum(case when D.FindDt=D.UreDate Then 0 else DP.Summa*D.KoefDebitor end) As Ostatok
        , Sum(case when D.FindDt=D.UreDate Then DP.Summa*D.KoefDebitor else 0 end) As Oborot
      From Doc D
        inner join `aks`.`cashfunc_document_prihod` DP On D.Id=DP.Id),
    # Считаем по документам расхода
    DocRash As (Select Sum(case when D.FindDt=D.UreDate Then 0 else DP.Summa*D.KoefDebitor end) As Ostatok
        , Sum(case when D.FindDt=D.UreDate Then DP.Summa*D.KoefDebitor else 0 end) As Oborot
      From Doc D
        inner join `aks`.`cashfunc_document_rashod` DP On D.Id=DP.Id),
    # Соединяем все документы
    DocUnion As (Select Max(StartSumm) As Ostatok, 0 As Oborot From Conf
        Union All
        Select * From DocPrihod
        Union All
        Select * From DocRash)
# Считаем результат
Select Sum(Ostatok) As Ostatok, Sum(Oborot) As Oborot
From  DocUnion ",  Dt.ToShortDateString());

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOstatokAndOborotForDayMySql", EventEn.Dump);

                RezultForOstatokAndOborot rez = new RezultForOstatokAndOborot();

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
                                    if (!dr.IsDBNull(0)) rez.Ostatok = decimal.Parse(dr.GetValue(0).ToString());
                                    if (!dr.IsDBNull(1)) rez.Oborot = decimal.Parse(dr.GetValue(1).ToString());
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOstatokAndOborotForDayMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOstatokAndOborotForDayMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetOstatokAndOborotForDayMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetOstatokAndOborotForDayMySql", EventEn.Dump);
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
            string CommandSql = String.Format(@"Select `Id`, `DocFullName`, `UreDate`, `CteateDate`, `ModifyDate`, `ModifyUser`, `OperationId`, `LocalDebitorId`, `LocalCreditorId`, `OtherDebitor`, `OtherKreditor`, `DocNum`, `IsDraft`, `IsProcessed` 
From `aks`.`CashFunc_Document`
Where `UreDate`>={0} 
    and `OperationId`={1}
Order by `Id`", (LastDay == null ? "`OperationId`" : string.Format("Str_To_Date('{0}', '%d.%m.%Y')", DateTime.Now.AddDays((double)LastDay*-1).ToShortDateString()))
            , (OperationId == null ? "`OperationId`" : OperationId.ToString())
            );

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
                                    string TmpOtherDebitor = null;
                                    string TmpOtherKreditor = null;
                                    int TmpDocNum = 0;
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
                                    if (!dr.IsDBNull(9)) TmpOtherDebitor = dr.GetString(9);
                                    if (!dr.IsDBNull(10)) TmpOtherKreditor = dr.GetString(10);
                                    if (!dr.IsDBNull(11)) TmpDocNum = dr.GetInt32(11);
                                    if (!dr.IsDBNull(12)) TmpIsDraft = Boolean.Parse(dr.GetValue(12).ToString());
                                    if (!dr.IsDBNull(13)) TmpIsProcessed = Boolean.Parse(dr.GetValue(13).ToString());


                                    //Если данные есть то добавляем их в список
                                    if (TmpId != null && !string.IsNullOrWhiteSpace(TmpDocFullName) && TmpCteateDate!=null && TmpModifyDate!=null && !string.IsNullOrWhiteSpace(TmpModifyUser) && TmpOperationId != null)
                                    {
                                        Operation TmpOper = OperationFarm.CurOperationList[TmpOperationId];
                                        if (TmpOper == null) throw new ApplicationException(string.Format("Операции с идентификатором {0} в документе где id={1} не существует.", TmpOperationId, TmpId));


                                        Local TmpDeb = null;
                                        if (TmpLocalDebitorId != null && TmpLocalDebitorId>-1)
                                        {
                                            TmpDeb = LocalFarm.CurLocalList[TmpLocalDebitorId];
                                            if (TmpDeb == null) throw new ApplicationException(string.Format("Дебитора с идентификатором {0} в документе где id={1} не существует.", TmpLocalDebitorId, TmpId));
                                        }

                                        Local TmpCred = null;
                                        if (TmpLocalCreditorId != null && TmpLocalCreditorId>-1)
                                        {
                                            TmpCred = LocalFarm.CurLocalList[TmpLocalCreditorId];
                                            if (TmpCred == null) throw new ApplicationException(string.Format("Кредитора с идентификатором {0} в документе где id={1} не существует.", TmpLocalCreditorId, TmpId));
                                        }
                                        
                                        rez.Add(DocumentFarm.CreateNewDocument((int)TmpId, TmpDocFullName, TmpUreDate, (DateTime)TmpCteateDate, (DateTime)TmpModifyDate, TmpModifyUser, TmpOper, TmpDeb, TmpCred, TmpOtherDebitor, TmpOtherKreditor, TmpDocNum, TmpIsDraft, TmpIsProcessed));
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

        /// <summary>
        /// Получаем список докуменитов
        /// </summary>
        /// <param name="Dt">За конкретную дату время будет отброшено</param>
        /// <param name="OperationId">Какая операция нас интересует, если null значит все операции за эту дату</param>
        /// <param name="HasNotin">Если true то будет смотреть все операции кроме операции указанной в параметре OperationId</param>
        /// <returns>Получает список Document из базы данных удовлетворяющий фильтрам</returns>
        private DocumentList GetDocumentListFromDbMySql(DateTime? Dt, int? OperationId, bool HasNotin)
        {
            string CommandSql = String.Format(@"Select `Id`, `DocFullName`, `UreDate`, `CteateDate`, `ModifyDate`, `ModifyUser`, `OperationId`, `LocalDebitorId`, `LocalCreditorId`, `OtherDebitor`, `OtherKreditor`, `DocNum`, `IsDraft`, `IsProcessed` 
From `aks`.`CashFunc_Document`
Where `UreDate`={0} 
    and `OperationId`{1}{2}
Order by `Id`", (Dt==null ? "`OperationId`" : string.Format("Str_To_Date('{0}', '%d.%m.%Y')", ((DateTime)Dt).ToShortDateString()))
            , (HasNotin?"<>":"=")
            , (OperationId==null ? "`OperationId`" : OperationId.ToString())
            );

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
                                    string TmpOtherDebitor = null;
                                    string TmpOtherKreditor = null;
                                    int TmpDocNum = 0;
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
                                    if (!dr.IsDBNull(9)) TmpOtherDebitor = dr.GetString(9);
                                    if (!dr.IsDBNull(10)) TmpOtherKreditor = dr.GetString(10);
                                    if (!dr.IsDBNull(11)) TmpDocNum = dr.GetInt32(11);
                                    if (!dr.IsDBNull(12)) TmpIsDraft = Boolean.Parse(dr.GetValue(12).ToString());
                                    if (!dr.IsDBNull(13)) TmpIsProcessed = Boolean.Parse(dr.GetValue(13).ToString());


                                    //Если данные есть то добавляем их в список
                                    if (TmpId != null && !string.IsNullOrWhiteSpace(TmpDocFullName) && TmpCteateDate != null && TmpModifyDate != null && !string.IsNullOrWhiteSpace(TmpModifyUser) && TmpOperationId != null)
                                    {
                                        Operation TmpOper = OperationFarm.CurOperationList[TmpOperationId];
                                        if (TmpOper == null) throw new ApplicationException(string.Format("Операции с идентификатором {0} в документе где id={1} не существует.", TmpOperationId, TmpId));

                                        Local TmpDeb = null;
                                        if (TmpLocalDebitorId != null && TmpLocalDebitorId>-1)
                                        {
                                            TmpDeb=LocalFarm.CurLocalList[TmpLocalDebitorId];
                                            if (TmpDeb == null) throw new ApplicationException(string.Format("Дебитора с идентификатором {0} в документе где id={1} не существует.", TmpLocalDebitorId, TmpId));
                                        }

                                        Local TmpCred = null;
                                        if (TmpLocalCreditorId != null && TmpLocalCreditorId>-1)
                                        {
                                            TmpCred = LocalFarm.CurLocalList[TmpLocalCreditorId];
                                            if (TmpCred == null) throw new ApplicationException(string.Format("Кредитора с идентификатором {0} в документе где id={1} не существует.", TmpLocalCreditorId, TmpId));
                                        }

                                        rez.Add(DocumentFarm.CreateNewDocument((int)TmpId, TmpDocFullName, TmpUreDate, (DateTime)TmpCteateDate, (DateTime)TmpModifyDate, TmpModifyUser, TmpOper, TmpDeb, TmpCred, TmpOtherDebitor, TmpOtherKreditor, TmpDocNum, TmpIsDraft, TmpIsProcessed));
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

        /// <summary>
        /// Сохранение Document в базе
        /// </summary>
        /// <param name="NewDocument">Новый документ который нужно сохранить</param>
        /// <returns>Идентификатор из базы данных под которым сохранили</returns>
        private int SetDocumentMySql(Document NewDocument)
        {
            int rez = 0;

            string CommandSql = String.Format(@"insert into `aks`.`CashFunc_Document`(`DocFullName`, `UreDate`, `CteateDate`, `ModifyDate`,
    `ModifyUser`, `OperationId`, `LocalDebitorId`, `LocalCreditorId`, 
    `OtherDebitor`, `OtherKreditor`, `DocNum`, `IsDraft`
)
Values(?, ?, ?, ?, 
    ?, ?, ?, ?,
    ?, ?, ?, ?)");
            string CommandSql2 = "SELECT LAST_INSERT_ID()  As Id";


            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.Parameters.Add(new OdbcParameter("DocFullName", OdbcType.VarChar, 100) { Value = NewDocument.DocFullName });
                        com.Parameters.Add(new OdbcParameter("UreDate", OdbcType.Date) { Value = NewDocument.UreDate });
                        com.Parameters.Add(new OdbcParameter("CteateDate", OdbcType.DateTime) { Value = NewDocument.CteateDate });
                        com.Parameters.Add(new OdbcParameter("ModifyDate", OdbcType.DateTime) { Value = NewDocument.ModifyDate });
                        //
                        com.Parameters.Add(new OdbcParameter("ModifyUser", OdbcType.VarChar, 100) { Value = NewDocument.ModifyUser });
                        com.Parameters.Add(new OdbcParameter("OperationId", OdbcType.Int) { Value = NewDocument.CurOperation.Id });
                        com.Parameters.Add(new OdbcParameter("LocalDebitorId", OdbcType.Int) { Value = (NewDocument.LocalDebitor==null?-1:NewDocument.LocalDebitor.Id) });
                        com.Parameters.Add(new OdbcParameter("LocalCreditorId", OdbcType.Int) { Value = (NewDocument.LocalCreditor==null?-1:NewDocument.LocalCreditor.Id) });
                        //
                        com.Parameters.Add(new OdbcParameter("OtherDebitor", OdbcType.VarChar, 200) { Value = NewDocument.OtherDebitor });
                        com.Parameters.Add(new OdbcParameter("OtherKreditor", OdbcType.VarChar, 200) { Value = NewDocument.OtherKreditor });
                        com.Parameters.Add(new OdbcParameter("DocNum", OdbcType.Int) { Value = NewDocument.DocNum });
                        com.Parameters.Add(new OdbcParameter("IsDraft", OdbcType.Bit) { Value = NewDocument.IsDraft });

                        com.CommandTimeout = 900;  // 15 минут
                        com.ExecuteNonQuery();
                    }
                    using (OdbcCommand com2 = new OdbcCommand(CommandSql2, con))
                    {

                        com2.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com2.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                // пробегаем по строкам
                                while (dr.Read())
                                {
                                    if (!dr.IsDBNull(0))
                                    {
                                        string tmp = dr.GetValue(0).ToString();
                                        rez = int.Parse(tmp);
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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновление Document в базе
        /// </summary>
        /// <param name="UpdDocument">Обновляемый документ</param>
        private void UpdateDocumentMySql(Document UpdDocument)
        {
            if (UpdDocument.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"Update `aks`.`CashFunc_Document`
Set `UreDate`=?, `ModifyDate`=?, `ModifyUser`='{1}', `LocalDebitorId` = {2}, 
    `LocalCreditorId`={3}, `OtherDebitor`={4}, `OtherKreditor`={5}, `IsProcessed`={6}, 
    `DocNum`={7}, `IsDraft`={8}
Where Id={0}", UpdDocument.Id,
                   UpdDocument.ModifyUser,
                   (UpdDocument.LocalDebitor == null ? -1: UpdDocument.LocalDebitor.Id),
                   //
                   (UpdDocument.LocalCreditor == null ? -1 :UpdDocument.LocalCreditor.Id),
                   (string.IsNullOrWhiteSpace(UpdDocument.OtherDebitor) ? "null" : string.Format("'{0}'", UpdDocument.OtherDebitor)),
                   (string.IsNullOrWhiteSpace(UpdDocument.OtherKreditor) ? "null" : string.Format("'{0}'", UpdDocument.OtherKreditor)),
                   (UpdDocument.IsProcessed ? 1 : 0),
                    //
                   UpdDocument.DocNum,
                   (UpdDocument.IsDraft ? 1 : 0));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.Parameters.Add(new OdbcParameter("UreDate", OdbcType.Date) { Value = UpdDocument.UreDate });
                        com.Parameters.Add(new OdbcParameter("ModifyDate", OdbcType.DateTime) { Value = UpdDocument.ModifyDate });

                        com.CommandTimeout = 900;  // 15 минут
                        com.ExecuteNonQuery();
                    }
                }
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при обновлении данных в источнике. {0}", ex.Message), GetType().Name + ".UpdateDocumentMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при обновлении данных в источнике. {0}", ex.Message), GetType().Name + ".UpdateDocumentMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentPrihod
        /// </summary>
        /// <param name="DocumentPrihod">Объект DocumentPrihod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashDocumentPrihodMySql(BLL.DocumentPlg.DocumentPrihod DocumentPrihod)
        {
            if (DocumentPrihod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select *
From `aks`.`cashfunc_document_Prihod`
Where Id={0}", (int)DocumentPrihod.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentPrihodMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                rez = true;
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentPrihodaMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentPrihodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentPrihodMySql", EventEn.Dump);
                throw;
            }

        }

        /// <summary>
        /// Читаем информацию по объекту DocumentPrihod
        /// </summary>
        /// <param name="DocumentPrihod">Объект DocumentPrihod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetDocumentPrihodMySql(ref BLL.DocumentPlg.DocumentPrihod DocumentPrihod)
        {
            if (DocumentPrihod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select `DebetNomerSchet`, `KreditKodDivision`, `KredikKorSchet`, `KredikKodAnalUch`,
    `Summa`, `SummaStr`, `KodNazn`, `Osnovanie`,
    `Id_PaidInReasons`, `VtomChisle`, `NDS`, `Prilozenie`,
    `GlavBuh`
From `aks`.`cashfunc_document_Prihod`
Where Id={0}", (int)DocumentPrihod.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentPrihodMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = true;

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
                                    //BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface interf = new BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface();
                                    if (!dr.IsDBNull(0)) DocumentPrihod.DebetNomerSchet= dr.GetString(0);
                                    if (!dr.IsDBNull(1)) DocumentPrihod.KreditKodDivision = dr.GetString(1);
                                    if (!dr.IsDBNull(2)) DocumentPrihod.KredikKorSchet = dr.GetString(2);
                                    if (!dr.IsDBNull(3)) DocumentPrihod.KredikKodAnalUch = dr.GetString(3);
                                    if (!dr.IsDBNull(4)) DocumentPrihod.Summa = decimal.Parse(dr.GetValue(4).ToString());
                                    if (!dr.IsDBNull(5)) DocumentPrihod.SummaStr = dr.GetValue(5).ToString();
                                    if (!dr.IsDBNull(6)) DocumentPrihod.KodNazn = dr.GetValue(6).ToString();
                                    if (!dr.IsDBNull(7)) DocumentPrihod.Osnovanie = dr.GetValue(7).ToString();
                                    if (!dr.IsDBNull(8))
                                    {
                                        int IdPaidInReasons = int.Parse(dr.GetValue(8).ToString());
                                        foreach (BLL.LocalPlg.LocalPaidInReasons itemPaidInReasons in Com.LocalFarm.CurLocalPaidInReasons)
                                        {
                                            if (itemPaidInReasons.Id!=null && (int)itemPaidInReasons.Id== IdPaidInReasons)
                                            {
                                                DocumentPrihod.PaidInReasons = itemPaidInReasons;
                                                break;
                                            }
                                        }
                                    }
                                    if (!dr.IsDBNull(9)) DocumentPrihod.VtomChisle = dr.GetValue(9).ToString();
                                    if (!dr.IsDBNull(10)) DocumentPrihod.NDS = decimal.Parse(dr.GetValue(10).ToString());
                                    if (!dr.IsDBNull(11)) DocumentPrihod.Prilozenie = dr.GetValue(11).ToString();
                                    if (!dr.IsDBNull(12)) DocumentPrihod.GlavBuh = dr.GetValue(12).ToString();
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentPrihodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentPrihodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentPrihod
        /// </summary>
        /// <param name="NewDocumentPrihod">Вставляем в базу информацию по объекту DocumentPrihod</param>
        private void SetDocumentPrihodMySql(BLL.DocumentPlg.DocumentPrihod NewDocumentPrihod)
        {
            if (NewDocumentPrihod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_document_Prihod`(id, `DebetNomerSchet`, `KreditKodDivision`, `KredikKorSchet`, 
    `KredikKodAnalUch`, `Summa`, `SummaStr`, `KodNazn`,
    `Osnovanie`, `Id_PaidInReasons`, `VtomChisle`, `NDS`,
    `Prilozenie`, `GlavBuh`) 
Values({0}, {1}, {2}, {3},
    {4}, {5}, {6}, {7}, 
    {8}, {9}, {10}, {11}, 
    {12}, {13})", (NewDocumentPrihod.Id==null?"null": NewDocumentPrihod.Id.ToString()),
            (string.IsNullOrWhiteSpace(NewDocumentPrihod.DebetNomerSchet)?"null":string.Format("'{0}'", NewDocumentPrihod.DebetNomerSchet)),
            (string.IsNullOrWhiteSpace(NewDocumentPrihod.KreditKodDivision) ? "null" : string.Format("'{0}'", NewDocumentPrihod.KreditKodDivision)),
            (string.IsNullOrWhiteSpace(NewDocumentPrihod.KredikKorSchet) ? "null" : string.Format("'{0}'", NewDocumentPrihod.KredikKorSchet)),
            //
            (string.IsNullOrWhiteSpace(NewDocumentPrihod.KredikKodAnalUch) ? "null" : string.Format("'{0}'", NewDocumentPrihod.KredikKodAnalUch)),
            NewDocumentPrihod.Summa.ToString().Replace(",","."),
            (string.IsNullOrWhiteSpace(NewDocumentPrihod.SummaStr) ? "null" : string.Format("'{0}'", NewDocumentPrihod.SummaStr)),
            (string.IsNullOrWhiteSpace(NewDocumentPrihod.KodNazn) ? "null" : string.Format("'{0}'", NewDocumentPrihod.KodNazn)),
            //
            (string.IsNullOrWhiteSpace(NewDocumentPrihod.Osnovanie) ? "null" : string.Format("'{0}'", NewDocumentPrihod.Osnovanie)),
            (NewDocumentPrihod.PaidInReasons==null || NewDocumentPrihod.PaidInReasons.Id==null?"null": NewDocumentPrihod.PaidInReasons.Id.ToString()),
            (string.IsNullOrWhiteSpace(NewDocumentPrihod.VtomChisle) ? "null" : string.Format("'{0}'", NewDocumentPrihod.VtomChisle)),
            NewDocumentPrihod.NDS.ToString().Replace(",", "."),
            //
            (string.IsNullOrWhiteSpace(NewDocumentPrihod.Prilozenie) ? "null" : string.Format("'{0}'", NewDocumentPrihod.Prilozenie)),
            (string.IsNullOrWhiteSpace(NewDocumentPrihod.GlavBuh)) ? "null" : string.Format("'{0}'", NewDocumentPrihod.GlavBuh));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentPrihodMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentPrihodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentPrihodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentPrihod
        /// </summary>
        /// <param name="UpdDocumentPrihod">Сам объект данные которого нужно обновить</param>
        private void UpdateDocumentPrihodMySql(BLL.DocumentPlg.DocumentPrihod UpdDocumentPrihod)
        {
            if (UpdDocumentPrihod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"update `aks`.`cashfunc_document_Prihod`
Set `DebetNomerSchet`={1}, `KreditKodDivision`={2}, `KredikKorSchet`={3}, `KredikKodAnalUch`={4}, 
    `Summa`={5}, `SummaStr`={6}, `KodNazn`={7}, `Osnovanie`={8},
    `Id_PaidInReasons`={9}, `VtomChisle`={10}, `NDS`={11}, `Prilozenie`={12},
    `GlavBuh`={13}
Where Id={0}", UpdDocumentPrihod.Id,
            //
            (string.IsNullOrWhiteSpace(UpdDocumentPrihod.DebetNomerSchet) ? "null" : string.Format("'{0}'", UpdDocumentPrihod.DebetNomerSchet)),
            (string.IsNullOrWhiteSpace(UpdDocumentPrihod.KreditKodDivision) ? "null" : string.Format("'{0}'", UpdDocumentPrihod.KreditKodDivision)),
            (string.IsNullOrWhiteSpace(UpdDocumentPrihod.KredikKorSchet) ? "null" : string.Format("'{0}'", UpdDocumentPrihod.KredikKorSchet)),
            (string.IsNullOrWhiteSpace(UpdDocumentPrihod.KredikKodAnalUch) ? "null" : string.Format("'{0}'", UpdDocumentPrihod.KredikKodAnalUch)),
            //
            UpdDocumentPrihod.Summa.ToString().Replace(",", "."),
            (string.IsNullOrWhiteSpace(UpdDocumentPrihod.SummaStr) ? "null" : string.Format("'{0}'", UpdDocumentPrihod.SummaStr)),
            (string.IsNullOrWhiteSpace(UpdDocumentPrihod.KodNazn) ? "null" : string.Format("'{0}'", UpdDocumentPrihod.KodNazn)),
            (string.IsNullOrWhiteSpace(UpdDocumentPrihod.Osnovanie) ? "null" : string.Format("'{0}'", UpdDocumentPrihod.Osnovanie)),
            //
            (UpdDocumentPrihod.PaidInReasons == null || UpdDocumentPrihod.PaidInReasons.Id == null ? "null" : UpdDocumentPrihod.PaidInReasons.Id.ToString()),
            (string.IsNullOrWhiteSpace(UpdDocumentPrihod.VtomChisle) ? "null" : string.Format("'{0}'", UpdDocumentPrihod.VtomChisle)),
            UpdDocumentPrihod.NDS.ToString().Replace(",", "."),
            (string.IsNullOrWhiteSpace(UpdDocumentPrihod.Prilozenie) ? "null" : string.Format("'{0}'", UpdDocumentPrihod.Prilozenie)),
            //
            (string.IsNullOrWhiteSpace(UpdDocumentPrihod.GlavBuh) ? "null" : string.Format("'{0}'", UpdDocumentPrihod.GlavBuh)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentPrihodMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentPrihodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentPrihodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentPrihodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentRashod
        /// </summary>
        /// <param name="DocumentRashod">Объект DocumentRashod который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashDocumentRashodMySql(BLL.DocumentPlg.DocumentRashod DocumentRashod)
        {
            if (DocumentRashod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select *
From `aks`.`cashfunc_document_rashod`
Where Id={0}", (int)DocumentRashod.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentRashodMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                rez = true;
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentRashodaMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentRashodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentRashodMySql", EventEn.Dump);
                throw;
            }

        }

        /// <summary>
        /// Читаем информацию по объекту DocumentRashod
        /// </summary>
        /// <param name="DocumentRashod">Объект DocumentRashod который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetDocumentRashodMySql(ref BLL.DocumentPlg.DocumentRashod DocumentRashod)
        {
            if (DocumentRashod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select `DebetKodDivision`, `DebetKorSchet`, `DebetKodAnalUch`, `KreditNomerSchet`,
    `Summa`, `SummaStr`, `KodNazn`, `PoDoc`,
    `Osnovanie`, `Id_PaidRashReasons`, `Prilozenie`, `DolRukFio`,
    `RukFio`, `GlavBuh`
From `aks`.`cashfunc_document_rashod`
Where Id={0}", (int)DocumentRashod.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentRashodMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = true;

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
                                    //BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface interf = new BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface();
                                    if (!dr.IsDBNull(0)) DocumentRashod.DebetKodDivision = dr.GetString(0);
                                    if (!dr.IsDBNull(1)) DocumentRashod.DebetKorSchet = dr.GetString(1);
                                    if (!dr.IsDBNull(2)) DocumentRashod.DebetKodAnalUch = dr.GetString(2);
                                    if (!dr.IsDBNull(3)) DocumentRashod.KreditNomerSchet = dr.GetString(3);
                                    if (!dr.IsDBNull(4)) DocumentRashod.Summa = decimal.Parse(dr.GetValue(4).ToString());
                                    if (!dr.IsDBNull(5)) DocumentRashod.SummaStr = dr.GetValue(5).ToString();
                                    if (!dr.IsDBNull(6)) DocumentRashod.KodNazn = dr.GetValue(6).ToString();
                                    if (!dr.IsDBNull(7)) DocumentRashod.PoDoc = dr.GetValue(7).ToString();
                                    if (!dr.IsDBNull(8)) DocumentRashod.Osnovanie = dr.GetValue(8).ToString();
                                    if (!dr.IsDBNull(9))
                                    {
                                        int IdPaidRashReasons = int.Parse(dr.GetValue(9).ToString());
                                        foreach (BLL.LocalPlg.LocalPaidRashReasons itemPaidRashReasons in Com.LocalFarm.CurLocalPaidRashReasons)
                                        {
                                            if (itemPaidRashReasons.Id != null && (int)itemPaidRashReasons.Id == IdPaidRashReasons)
                                            {
                                                DocumentRashod.PaidRashReasons = itemPaidRashReasons;
                                                break;
                                            }
                                        }
                                    }
                                    if (!dr.IsDBNull(10)) DocumentRashod.Prilozenie = dr.GetValue(10).ToString();
                                    if (!dr.IsDBNull(11)) DocumentRashod.DolRukFio =  dr.GetValue(11).ToString();
                                    if (!dr.IsDBNull(12)) DocumentRashod.RukFio = dr.GetValue(12).ToString();
                                    if (!dr.IsDBNull(13)) DocumentRashod.GlavBuh = dr.GetValue(13).ToString();
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentRashodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentRashodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentRashod
        /// </summary>
        /// <param name="NewDocumentRashod">Вставляем в базу информацию по объекту DocumentRashod</param>
        private void SetDocumentRashodMySql(BLL.DocumentPlg.DocumentRashod NewDocumentRashod)
        {
            if (NewDocumentRashod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_document_rashod`(id, `DebetKodDivision`, `DebetKorSchet`, `DebetKodAnalUch`, 
    `KreditNomerSchet`, `Summa`, `SummaStr`, `KodNazn`, 
    `PoDoc`, `Osnovanie`, `Id_PaidRashReasons`, `Prilozenie`, 
    `DolRukFio`,`RukFio`, `GlavBuh`) 
Values({0}, {1}, {2}, {3},
    {4}, {5}, {6}, {7}, 
    {8}, {9}, {10}, {11}, 
    {12}, {13}, {14})", (NewDocumentRashod.Id == null ? "null" : NewDocumentRashod.Id.ToString()),
            (string.IsNullOrWhiteSpace(NewDocumentRashod.DebetKodDivision) ? "null" : string.Format("'{0}'", NewDocumentRashod.DebetKodDivision)),
            (string.IsNullOrWhiteSpace(NewDocumentRashod.DebetKorSchet) ? "null" : string.Format("'{0}'", NewDocumentRashod.DebetKorSchet)),
            (string.IsNullOrWhiteSpace(NewDocumentRashod.DebetKodAnalUch) ? "null" : string.Format("'{0}'", NewDocumentRashod.DebetKodAnalUch)),
            //
            (string.IsNullOrWhiteSpace(NewDocumentRashod.KreditNomerSchet) ? "null" : string.Format("'{0}'", NewDocumentRashod.KreditNomerSchet)),
            NewDocumentRashod.Summa.ToString().Replace(",", "."),
            (string.IsNullOrWhiteSpace(NewDocumentRashod.SummaStr) ? "null" : string.Format("'{0}'", NewDocumentRashod.SummaStr)),
            (string.IsNullOrWhiteSpace(NewDocumentRashod.KodNazn) ? "null" : string.Format("'{0}'", NewDocumentRashod.KodNazn)),
            //
            (string.IsNullOrWhiteSpace(NewDocumentRashod.PoDoc) ? "null" : string.Format("'{0}'", NewDocumentRashod.PoDoc)),
            (string.IsNullOrWhiteSpace(NewDocumentRashod.Osnovanie) ? "null" : string.Format("'{0}'", NewDocumentRashod.Osnovanie)),
            (NewDocumentRashod.PaidRashReasons == null || NewDocumentRashod.PaidRashReasons.Id == null ? "null" : NewDocumentRashod.PaidRashReasons.Id.ToString()),
            (string.IsNullOrWhiteSpace(NewDocumentRashod.Prilozenie) ? "null" : string.Format("'{0}'", NewDocumentRashod.Prilozenie)),
            //
            (string.IsNullOrWhiteSpace(NewDocumentRashod.DolRukFio) ? "null" : string.Format("'{0}'", NewDocumentRashod.DolRukFio)),
            (string.IsNullOrWhiteSpace(NewDocumentRashod.RukFio) ? "null" : string.Format("'{0}'", NewDocumentRashod.RukFio)),
            (string.IsNullOrWhiteSpace(NewDocumentRashod.GlavBuh) ? "null" : string.Format("'{0}'", NewDocumentRashod.GlavBuh)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentRashodMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentRashodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentRashodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentRashod
        /// </summary>
        /// <param name="UpdDocumentRashod">Сам объект данные которого нужно обновить</param>
        private void UpdateDocumentRashodMySql(BLL.DocumentPlg.DocumentRashod UpdDocumentRashod)
        {
            if (UpdDocumentRashod.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"update `aks`.`cashfunc_document_rashod`
Set `DebetKodDivision`={1}, `DebetKorSchet`={2}, `DebetKodAnalUch`={3}, `KreditNomerSchet`={4}, 
    `Summa`={5}, `SummaStr`={6}, `KodNazn`={7}, `PoDoc`={8},
    `Osnovanie`={9}, `Id_PaidRashReasons`={10}, `Prilozenie`={11}, `DolRukFio`={12},
    `RukFio`={13}, `GlavBuh`={14}
Where Id={0}", UpdDocumentRashod.Id,
            //
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.DebetKodDivision) ? "null" : string.Format("'{0}'", UpdDocumentRashod.DebetKodDivision)),
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.DebetKorSchet) ? "null" : string.Format("'{0}'", UpdDocumentRashod.DebetKorSchet)),
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.DebetKodAnalUch) ? "null" : string.Format("'{0}'", UpdDocumentRashod.DebetKodAnalUch)),
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.KreditNomerSchet) ? "null" : string.Format("'{0}'", UpdDocumentRashod.KreditNomerSchet)),
            //
            UpdDocumentRashod.Summa.ToString().Replace(",", "."),
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.SummaStr) ? "null" : string.Format("'{0}'", UpdDocumentRashod.SummaStr)),
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.KodNazn) ? "null" : string.Format("'{0}'", UpdDocumentRashod.KodNazn)),
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.PoDoc) ? "null" : string.Format("'{0}'", UpdDocumentRashod.PoDoc)),
            //
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.Osnovanie) ? "null" : string.Format("'{0}'", UpdDocumentRashod.Osnovanie)),
            (UpdDocumentRashod.PaidRashReasons == null || UpdDocumentRashod.PaidRashReasons.Id == null ? "null" : UpdDocumentRashod.PaidRashReasons.Id.ToString()),
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.Prilozenie) ? "null" : string.Format("'{0}'", UpdDocumentRashod.Prilozenie)),
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.DolRukFio) ? "null" : string.Format("'{0}'", UpdDocumentRashod.DolRukFio)),
            //
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.RukFio) ? "null" : string.Format("'{0}'", UpdDocumentRashod.RukFio)),
            (string.IsNullOrWhiteSpace(UpdDocumentRashod.GlavBuh) ? "null" : string.Format("'{0}'", UpdDocumentRashod.GlavBuh)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentRashodMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentRashodMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentRashodMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentRashodMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentKasBook
        /// </summary>
        /// <param name="DocumentKasBook">Объект DocumentKasBook который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashDocumentKasBookMySql(BLL.DocumentPlg.DocumentKasBook DocumentKasBook)
        {
            if (DocumentKasBook.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select *
From `aks`.`cashfunc_document_KasBook`
Where Id={0}", (int)DocumentKasBook.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentKasBookMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                rez = true;
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentKasBookaMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentKasBookMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentKasBookMySql", EventEn.Dump);
                throw;
            }

        }

        /// <summary>
        /// Читаем информацию по объекту DocumentKasBook
        /// </summary>
        /// <param name="DocumentKasBook">Объект DocumentKasBook который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetDocumentKasBookMySql(ref BLL.DocumentPlg.DocumentKasBook DocumentKasBook)
        {
            if (DocumentKasBook.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select `SummaStartDay`, `SummaEndDay`, `DolRukFio`, `RukFio`, 
    `GlavBuh`
From `aks`.`cashfunc_document_KasBook`
Where Id={0}", (int)DocumentKasBook.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentKasBookMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = true;

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
                                    //BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface interf = new BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface();
                                    if (!dr.IsDBNull(0)) DocumentKasBook.SummaStartDay = decimal.Parse(dr.GetValue(0).ToString());
                                    if (!dr.IsDBNull(1)) DocumentKasBook.SummaEndDay = decimal.Parse(dr.GetValue(1).ToString());
                                    if (!dr.IsDBNull(2)) DocumentKasBook.DolRukFio = dr.GetValue(2).ToString();
                                    if (!dr.IsDBNull(3)) DocumentKasBook.RukFio = dr.GetValue(3).ToString();
                                    //
                                    if (!dr.IsDBNull(4)) DocumentKasBook.GlavBuh = dr.GetValue(4).ToString();
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentKasBookMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentKasBookMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentKasBook
        /// </summary>
        /// <param name="NewDocumentKasBook">Вставляем в базу информацию по объекту DocumentKasBook</param>
        private void SetDocumentKasBookMySql(BLL.DocumentPlg.DocumentKasBook NewDocumentKasBook)
        {
            if (NewDocumentKasBook.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_document_KasBook`(id, `SummaStartDay`, `SummaEndDay`, `DolRukFio`,
    `RukFio`, `GlavBuh`) 
Values({0}, {1}, {2}, {3},
    {4}, {5})", (NewDocumentKasBook.Id == null ? "null" : NewDocumentKasBook.Id.ToString()),
            NewDocumentKasBook.SummaStartDay.ToString().Replace(",", "."),
            NewDocumentKasBook.SummaEndDay.ToString().Replace(",", "."),
            (string.IsNullOrWhiteSpace(NewDocumentKasBook.DolRukFio) ? "null" : string.Format("'{0}'", NewDocumentKasBook.DolRukFio)),
            //
            (string.IsNullOrWhiteSpace(NewDocumentKasBook.RukFio) ? "null" : string.Format("'{0}'", NewDocumentKasBook.RukFio)),
            (string.IsNullOrWhiteSpace(NewDocumentKasBook.GlavBuh)) ? "null" : string.Format("'{0}'", NewDocumentKasBook.GlavBuh));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentKasBookMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentKasBookMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentKasBookMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentKasBook
        /// </summary>
        /// <param name="UpdDocumentKasBook">Сам объект данные которого нужно обновить</param>
        private void UpdateDocumentKasBookMySql(BLL.DocumentPlg.DocumentKasBook UpdDocumentKasBook)
        {
            if (UpdDocumentKasBook.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"update `aks`.`cashfunc_document_KasBook`
Set `SummaStartDay`={1}, `SummaEndDay`={2}, `DolRukFio`={3}, `RukFio`={4}, 
    `GlavBuh`={5}
Where Id={0}", UpdDocumentKasBook.Id,
            //
            UpdDocumentKasBook.SummaStartDay.ToString().Replace(",", "."),
            UpdDocumentKasBook.SummaEndDay.ToString().Replace(",", "."),
            (string.IsNullOrWhiteSpace(UpdDocumentKasBook.DolRukFio) ? "null" : string.Format("'{0}'", UpdDocumentKasBook.DolRukFio)),
            (string.IsNullOrWhiteSpace(UpdDocumentKasBook.RukFio) ? "null" : string.Format("'{0}'", UpdDocumentKasBook.RukFio)),
            //
            (string.IsNullOrWhiteSpace(UpdDocumentKasBook.GlavBuh) ? "null" : string.Format("'{0}'", UpdDocumentKasBook.GlavBuh)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentKasBookMySql", EventEn.Dump);

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
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentKasBookMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentKasBookMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentKasBookMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Проверка наличия информации объекта DocumentInvent
        /// </summary>
        /// <param name="DocumentInvent">Объект DocumentInvent который нужно проверить в базе данных</param>
        /// <returns>Возвращаем флаг смогли найти объект или нет</returns>
        private bool HashDocumentInventMySql(BLL.DocumentPlg.DocumentInvent DocumentInvent)
        {
            if (DocumentInvent.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select *
From `aks`.`cashfunc_document_Invent`
Where Id={0}", (int)DocumentInvent.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentInventMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                rez = true;
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentInventMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".HashDocumentInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".HashDocumentInventMySql", EventEn.Dump);
                throw;
            }

        }
        
        /// <summary>
        /// Читаем информацию по объекту DocumentInvent
        /// </summary>
        /// <param name="DocumentInvent">Объект DocumentInvent который нужно обновить в соответсвии с параметрами из базы</param>
        /// <returns>Возвращаем флаг смогли обновить объект или нет</returns>
        private bool GetDocumentInventMySql(ref BLL.DocumentPlg.DocumentInvent DocumentInvent)
        {
            if (DocumentInvent.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            bool rez = false;

            string CommandSql = String.Format(@"Select `FactStr1`, `FactStr2`, `FactStr3`, `FactStr4`,
    `FactStr5`, `FactVal1`, `FactVal2`, `FactVal3`,
    `FactVal4`, `FactVal5`, `ItogPoUchDan`, `LastPrihodNum`,
    `LastRashodNum`, `PrikazTypAndDocNum`, `PrikazUreDate`, `PrikazDolMatOtv`,
    `PrikazDecodeMatOtv`, `KomissionStr1`, `KomissionStr2`, `KomissionStr3`, 
    `KomissionStr4`, `KomissionDecode1`, `KomissionDecode2`, `KomissionDecode3`, 
    `KomissionDecode4`
From `aks`.`cashfunc_document_Invent`
Where Id={0}", (int)DocumentInvent.Id);

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentInventMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        //com.Parameters.Add(new OdbcParameter("Id", OdbcType.Int) { Value = (int)LocalKassa.Id });

                        com.CommandTimeout = 900;  // 15 минут
                        using (OdbcDataReader dr = com.ExecuteReader())
                        {

                            if (dr.HasRows)
                            {
                                rez = true;

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
                                    //BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface interf = new BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface();
                                    if (!dr.IsDBNull(0)) DocumentInvent.FactStr1 = dr.GetString(0);
                                    if (!dr.IsDBNull(1)) DocumentInvent.FactStr2 = dr.GetString(1);
                                    if (!dr.IsDBNull(2)) DocumentInvent.FactStr3 = dr.GetString(2);
                                    if (!dr.IsDBNull(3)) DocumentInvent.FactStr4 = dr.GetString(3);
                                    //
                                    if (!dr.IsDBNull(4)) DocumentInvent.FactStr5 = dr.GetString(4);
                                    if (!dr.IsDBNull(5)) DocumentInvent.FactVal1 = decimal.Parse(dr.GetValue(5).ToString());
                                    if (!dr.IsDBNull(6)) DocumentInvent.FactVal2 = decimal.Parse(dr.GetValue(6).ToString());
                                    if (!dr.IsDBNull(7)) DocumentInvent.FactVal3 = decimal.Parse(dr.GetValue(7).ToString());
                                    //
                                    if (!dr.IsDBNull(8)) DocumentInvent.FactVal4 = decimal.Parse(dr.GetValue(8).ToString());
                                    if (!dr.IsDBNull(9)) DocumentInvent.FactVal5 = decimal.Parse(dr.GetValue(9).ToString());
                                    if (!dr.IsDBNull(10)) DocumentInvent.ItogPoUchDan = decimal.Parse(dr.GetValue(10).ToString());
                                    if (!dr.IsDBNull(11)) DocumentInvent.LastPrihodNum = int.Parse(dr.GetValue(11).ToString());
                                    //
                                    if (!dr.IsDBNull(12)) DocumentInvent.LastRashodNum = int.Parse(dr.GetValue(12).ToString());
                                    if (!dr.IsDBNull(13)) DocumentInvent.PrikazTypAndDocNum = dr.GetString(13);
                                    if (!dr.IsDBNull(14)) DocumentInvent.PrikazUreDate = dr.GetDateTime(14);
                                    if (!dr.IsDBNull(15)) DocumentInvent.PrikazDolMatOtv = dr.GetString(15);
                                    //
                                    if (!dr.IsDBNull(16)) DocumentInvent.PrikazDecodeMatOtv = dr.GetString(16);
                                    if (!dr.IsDBNull(17)) DocumentInvent.KomissionStr1 = dr.GetString(17);
                                    if (!dr.IsDBNull(18)) DocumentInvent.KomissionStr2 = dr.GetString(18);
                                    if (!dr.IsDBNull(19)) DocumentInvent.KomissionStr3 = dr.GetString(19);
                                    //
                                    if (!dr.IsDBNull(20)) DocumentInvent.KomissionStr4 = dr.GetString(20);
                                    if (!dr.IsDBNull(21)) DocumentInvent.KomissionDecode1 = dr.GetString(21);
                                    if (!dr.IsDBNull(22)) DocumentInvent.KomissionDecode2 = dr.GetString(22);
                                    if (!dr.IsDBNull(23)) DocumentInvent.KomissionDecode3 = dr.GetString(23);
                                    //
                                    if (!dr.IsDBNull(24)) DocumentInvent.KomissionDecode4 = dr.GetString(24);
                                }
                            }
                        }
                    }
                }

                return rez;
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentInventMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".GetDocumentInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".GetDocumentInventMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Вставка новой информации в объект DocumentInvent
        /// </summary>
        /// <param name="NewDocumentInvent">Вставляем в базу информацию по объекту DocumentInvent</param>
        private void SetDocumentInventMySql(BLL.DocumentPlg.DocumentInvent NewDocumentInvent)
        {
            if (NewDocumentInvent.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"insert into `aks`.`cashfunc_document_Invent`(`id`, `FactStr1`, `FactStr2`, `FactStr3`, 
    `FactStr4`, `FactStr5`, `FactVal1`, `FactVal2`, 
    `FactVal3`, `FactVal4`, `FactVal5`, `ItogPoUchDan`, 
    `LastPrihodNum`, `LastRashodNum`, `PrikazTypAndDocNum`, `PrikazUreDate`, `PrikazDolMatOtv`, 
    `PrikazDecodeMatOtv`, `KomissionStr1`, `KomissionStr2`, `KomissionStr3`, 
    `KomissionStr4`, `KomissionDecode1`, `KomissionDecode2`, `KomissionDecode3`, 
    `KomissionDecode4`) 
Values({0}, {1}, {2}, {3},
    {4}, {5}, {6}, {7}, 
    {8}, {9}, {10}, {11}, 
    {12}, {13}, {14}, ?, {15}, 
    {16}, {17}, {18}, {19}, 
    {20}, {21}, {22}, {23}, 
    {24})", (NewDocumentInvent.Id == null ? "null" : NewDocumentInvent.Id.ToString()),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.FactStr1) ? "null" : string.Format("'{0}'", NewDocumentInvent.FactStr1)),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.FactStr2) ? "null" : string.Format("'{0}'", NewDocumentInvent.FactStr2)),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.FactStr3) ? "null" : string.Format("'{0}'", NewDocumentInvent.FactStr3)),
            //
            (string.IsNullOrWhiteSpace(NewDocumentInvent.FactStr4) ? "null" : string.Format("'{0}'", NewDocumentInvent.FactStr4)),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.FactStr5) ? "null" : string.Format("'{0}'", NewDocumentInvent.FactStr5)),
            NewDocumentInvent.FactVal1.ToString().Replace(",", "."),
            NewDocumentInvent.FactVal2.ToString().Replace(",", "."),
            //
            NewDocumentInvent.FactVal3.ToString().Replace(",", "."),
            NewDocumentInvent.FactVal4.ToString().Replace(",", "."),
            NewDocumentInvent.FactVal5.ToString().Replace(",", "."),
            NewDocumentInvent.ItogPoUchDan.ToString().Replace(",", "."),
            //
            NewDocumentInvent.LastPrihodNum.ToString(),
            NewDocumentInvent.LastRashodNum.ToString(),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.PrikazTypAndDocNum) ? "null" : string.Format("'{0}'", NewDocumentInvent.PrikazTypAndDocNum)),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.PrikazDolMatOtv) ? "null" : string.Format("'{0}'", NewDocumentInvent.PrikazDolMatOtv)),
            //
            (string.IsNullOrWhiteSpace(NewDocumentInvent.PrikazDecodeMatOtv) ? "null" : string.Format("'{0}'", NewDocumentInvent.PrikazDecodeMatOtv)),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.KomissionStr1) ? "null" : string.Format("'{0}'", NewDocumentInvent.KomissionStr1)),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.KomissionStr2) ? "null" : string.Format("'{0}'", NewDocumentInvent.KomissionStr2)),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.KomissionStr3) ? "null" : string.Format("'{0}'", NewDocumentInvent.KomissionStr3)),
            //
            (string.IsNullOrWhiteSpace(NewDocumentInvent.KomissionStr4) ? "null" : string.Format("'{0}'", NewDocumentInvent.KomissionStr4)),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.KomissionDecode1) ? "null" : string.Format("'{0}'", NewDocumentInvent.KomissionDecode1)),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.KomissionDecode2) ? "null" : string.Format("'{0}'", NewDocumentInvent.KomissionDecode2)),
            (string.IsNullOrWhiteSpace(NewDocumentInvent.KomissionDecode3) ? "null" : string.Format("'{0}'", NewDocumentInvent.KomissionDecode3)),
            //
            (string.IsNullOrWhiteSpace(NewDocumentInvent.KomissionDecode4) ? "null" : string.Format("'{0}'", NewDocumentInvent.KomissionDecode4)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentInventMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.Parameters.Add(new OdbcParameter("PrikazUreDate", OdbcType.Date) { Value = NewDocumentInvent.PrikazUreDate });

                        com.CommandTimeout = 900;  // 15 минут
                        com.ExecuteNonQuery();
                    }
                }
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentInventMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".SetDocumentInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".SetDocumentInventMySql", EventEn.Dump);
                throw;
            }
        }

        /// <summary>
        /// Обновляем в базе данных инфу по объекту DocumentInvent
        /// </summary>
        /// <param name="UpdDocumentInvent">Сам объект данные которого нужно обновить</param>
        private void UpdateDocumentInventMySql(BLL.DocumentPlg.DocumentInvent UpdDocumentInvent)
        {
            if (UpdDocumentInvent.Id == null) new ApplicationException("Id не может быть пустым если его нет то тогда что искать?");

            string CommandSql = String.Format(@"update `aks`.`cashfunc_document_Invent`
Set `FactStr1`={1}, `FactStr2`={2}, `FactStr3`={3}, `FactStr4`={4},
    `FactStr5`={5}, `FactVal1`={6}, `FactVal2`={7}, `FactVal3`={8},
    `FactVal4`={9}, `FactVal5`={10}, `ItogPoUchDan`={11}, `LastPrihodNum`={12},
    `LastRashodNum`={13}, `PrikazTypAndDocNum`={14}, `PrikazUreDate`=?, `PrikazDolMatOtv`={15}, `PrikazDecodeMatOtv` = {16},
    `KomissionStr1`={17}, `KomissionStr2`={18}, `KomissionStr3`={19}, `KomissionStr4`={20},
    `KomissionDecode1`={21}, `KomissionDecode2`={22}, `KomissionDecode3`={23}, `KomissionDecode4`={24}
Where Id={0}", UpdDocumentInvent.Id,
            //
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.FactStr1) ? "null" : string.Format("'{0}'", UpdDocumentInvent.FactStr1)),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.FactStr2) ? "null" : string.Format("'{0}'", UpdDocumentInvent.FactStr2)),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.FactStr3) ? "null" : string.Format("'{0}'", UpdDocumentInvent.FactStr3)),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.FactStr4) ? "null" : string.Format("'{0}'", UpdDocumentInvent.FactStr4)),
            //
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.FactStr5) ? "null" : string.Format("'{0}'", UpdDocumentInvent.FactStr5)),
            UpdDocumentInvent.FactVal1.ToString().Replace(",", "."),
            UpdDocumentInvent.FactVal2.ToString().Replace(",", "."),
            UpdDocumentInvent.FactVal3.ToString().Replace(",", "."),
            //
            UpdDocumentInvent.FactVal4.ToString().Replace(",", "."),
            UpdDocumentInvent.FactVal5.ToString().Replace(",", "."),
            UpdDocumentInvent.ItogPoUchDan.ToString().Replace(",", "."),
            UpdDocumentInvent.LastPrihodNum.ToString(),
            //
            UpdDocumentInvent.LastRashodNum.ToString(),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.PrikazTypAndDocNum) ? "null" : string.Format("'{0}'", UpdDocumentInvent.PrikazTypAndDocNum)),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.PrikazDolMatOtv) ? "null" : string.Format("'{0}'", UpdDocumentInvent.PrikazDolMatOtv)),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.PrikazDecodeMatOtv) ? "null" : string.Format("'{0}'", UpdDocumentInvent.PrikazDecodeMatOtv)),
            //
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.KomissionStr1) ? "null" : string.Format("'{0}'", UpdDocumentInvent.KomissionStr1)),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.KomissionStr2) ? "null" : string.Format("'{0}'", UpdDocumentInvent.KomissionStr2)),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.KomissionStr3) ? "null" : string.Format("'{0}'", UpdDocumentInvent.KomissionStr3)),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.KomissionStr4) ? "null" : string.Format("'{0}'", UpdDocumentInvent.KomissionStr4)),
            //
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.KomissionDecode1) ? "null" : string.Format("'{0}'", UpdDocumentInvent.KomissionDecode1)),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.KomissionDecode2) ? "null" : string.Format("'{0}'", UpdDocumentInvent.KomissionDecode2)),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.KomissionDecode3) ? "null" : string.Format("'{0}'", UpdDocumentInvent.KomissionDecode3)),
            (string.IsNullOrWhiteSpace(UpdDocumentInvent.KomissionDecode4) ? "null" : string.Format("'{0}'", UpdDocumentInvent.KomissionDecode4)));

            try
            {
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentInventMySql", EventEn.Dump);

                // Закрывать конект не нужно он будет закрыт деструктором
                using (OdbcConnection con = new OdbcConnection(base.ConnectionString))
                {
                    con.Open();

                    using (OdbcCommand com = new OdbcCommand(CommandSql, con))
                    {
                        com.Parameters.Add(new OdbcParameter("PrikazUreDate", OdbcType.Date) { Value = UpdDocumentInvent.PrikazUreDate });

                        com.CommandTimeout = 900;  // 15 минут
                        com.ExecuteNonQuery();
                    }
                }
            }
            catch (OdbcException ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentInventMySql", EventEn.Dump);
                throw;
            }
            catch (Exception ex)
            {
                base.EventSave(string.Format("Произожла ошибка при получении данных с источника. {0}", ex.Message), GetType().Name + ".UpdateDocumentInventMySql", EventEn.Error);
                if (Com.Config.Trace) base.EventSave(CommandSql, GetType().Name + ".UpdateDocumentInventMySql", EventEn.Dump);
                throw;
            }
        }
        #endregion
    }
}
