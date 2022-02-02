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
        /// Получаем список текущий Local
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
    `KkmName`, `DolRukOrg`, `RukFio`, `ZavDivisionFio`
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
    `KkmName`, `DolRukOrg`, `RukFio`, `ZavDivisionFio`) 
Values({0}, '{1}', '{2}', '{3}', '{4}',
    `{5}`, `{6}`, `{7}`, `{8}`,
    `{9}`, `{10}`, `{11}`, `{12}`, 
    `{13}`, `{14}`, `{15}`, `{16}`)", NewLocalKassa.Id, NewLocalKassa.HostName, NewLocalKassa.Organization, NewLocalKassa.StructPodrazdelenie, NewLocalKassa.OKPO,
                            NewLocalKassa.LastDocNumPrih, NewLocalKassa.LastDocNumRash, NewLocalKassa.LastDocNumKasBook, NewLocalKassa.LastDocNumActVozv,
                            NewLocalKassa.LastDocNumReportKas, NewLocalKassa.LastDocNumScetKkm, NewLocalKassa.LastDocNumVerifNal, NewLocalKassa.LastDocNumInvent,
                            NewLocalKassa.INN, NewLocalKassa.ZavodKKM, NewLocalKassa.RegKKM, NewLocalKassa.GlavBuhFio,
                            NewLocalKassa.KkmName, NewLocalKassa.DolRukOrg, NewLocalKassa.RukFio, NewLocalKassa.ZavDivisionFio);

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
    `KkmName`='{16}', `DolRukOrg`='{17}', `RukFio`='{18}', `ZavDivisionFio`='{19}'
Where Id={0}", UpdLocalKassa.Id, UpdLocalKassa.Organization, UpdLocalKassa.StructPodrazdelenie, UpdLocalKassa.OKPO,
            UpdLocalKassa.LastDocNumPrih, UpdLocalKassa.LastDocNumRash, UpdLocalKassa.LastDocNumKasBook, UpdLocalKassa.LastDocNumActVozv,
            UpdLocalKassa.LastDocNumReportKas, UpdLocalKassa.LastDocNumScetKkm, UpdLocalKassa.LastDocNumVerifNal, UpdLocalKassa.LastDocNumInvent,
            UpdLocalKassa.INN, UpdLocalKassa.ZavodKKM, UpdLocalKassa.RegKKM, UpdLocalKassa.GlavBuhFio,
            UpdLocalKassa.KkmName, UpdLocalKassa.DolRukOrg, UpdLocalKassa.RukFio, UpdLocalKassa.ZavDivisionFio);

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
        /// Получаем список текущий докуменитов
        /// </summary>
        /// <param name="LastDay">Сколько последних дней грузить из базы данных если null значит весь период</param>
        /// <param name="OperationId">Какая операция нас интересует, если </param>
        /// <returns>Получает список Document из базы данных удовлетворяющий фильтрам</returns>
        private DocumentList GetDocumentListFromDbMySql(int? LastDay, int? OperationId)
        {
            string CommandSql = String.Format(@"Select `Id`, `DocFullName`, `UreDate`, `CteateDate`, `ModifyDate`, `ModifyUser`, `OperationId`, `LocalDebitorId`, `LocalCreditorId`, `DocNum`, `IsDraft`, `IsProcessed` From `aks`.`CashFunc_Document`");

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
                                    if (!dr.IsDBNull(9)) TmpDocNum = dr.GetInt32(9);
                                    if (!dr.IsDBNull(10)) TmpIsDraft = Boolean.Parse(dr.GetValue(10).ToString());
                                    if (!dr.IsDBNull(11)) TmpIsProcessed = Boolean.Parse(dr.GetValue(11).ToString());


                                    //Если данные есть то добавляем их в список
                                    if (TmpId != null && !string.IsNullOrWhiteSpace(TmpDocFullName) && TmpCteateDate!=null && TmpModifyDate!=null && !string.IsNullOrWhiteSpace(TmpModifyUser) && TmpOperationId != null && TmpLocalDebitorId != null && TmpLocalCreditorId!=null)
                                    {
                                        Operation TmpOper = OperationFarm.CurOperationList[TmpOperationId];
                                        if (TmpOper == null) throw new ApplicationException(string.Format("Операции с идентификатором {0} в документе где id={1} не существует.", TmpOperationId, TmpId));

                                        Local TmpDeb = LocalFarm.CurLocalList[TmpLocalDebitorId];
                                        if (TmpDeb == null) throw new ApplicationException(string.Format("Дебитора с идентификатором {0} в документе где id={1} не существует.", TmpLocalDebitorId, TmpId));

                                        Local TmpCred = LocalFarm.CurLocalList[TmpLocalCreditorId];
                                        if (TmpCred == null) throw new ApplicationException(string.Format("Кредитора с идентификатором {0} в документе где id={1} не существует.", TmpLocalCreditorId, TmpId));

                                        rez.Add(DocumentFarm.CreateNewDocument((int)TmpId, TmpDocFullName, TmpUreDate, (DateTime)TmpCteateDate, (DateTime)TmpModifyDate, TmpModifyUser, TmpOper, TmpDeb, TmpCred, TmpDocNum, TmpIsDraft, TmpIsProcessed));
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
    `IsDraft`
)
Values(?, ?, ?, ?, 
    ?, ?, ?, ?,
    ?)");
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
                        com.Parameters.Add(new OdbcParameter("LocalDebitorId", OdbcType.Int) { Value = NewDocument.LocalDebitor.Id });
                        com.Parameters.Add(new OdbcParameter("LocalCreditorId", OdbcType.Int) { Value = NewDocument.LocalCreditor.Id });
                        //
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
    `LocalCreditorId`={3}, `IsProcessed`={4}, `IsDraft`={5}
Where Id={0}", UpdDocument.Id,
                   UpdDocument.ModifyUser,
                   UpdDocument.LocalDebitor,
                   UpdDocument.LocalCreditor,
                   (UpdDocument.IsProcessed ? 1 : 0),
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
    `Summa`, `KodNazn`, `Osnovanie`, `Id_PaidInReasons`,
    `VtomChisle`, `NDS`, `LastDocNumScetKkm`, `LastDocNumVerifNal`, 
    `LastDocNumInvent`,`INN`, `Prilozenie`, `GlavBuh`
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
                                    BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface interf = new BLL.LocalPlg.LocalKassa.LocalKassaForProviderInterface();
                                    if (!dr.IsDBNull(0)) DocumentPrihod.DebetNomerSchet= dr.GetString(0);
                                    if (!dr.IsDBNull(1)) DocumentPrihod.KreditKodDivision = dr.GetString(1);
                                    if (!dr.IsDBNull(2)) DocumentPrihod.KredikKorSchet = dr.GetString(2);
                                    if (!dr.IsDBNull(3)) DocumentPrihod.KredikKodAnalUch = dr.GetString(3);
                                    if (!dr.IsDBNull(4)) DocumentPrihod.Summa = decimal.Parse(dr.GetValue(4).ToString());
                                    if (!dr.IsDBNull(5)) DocumentPrihod.KodNazn = dr.GetValue(5).ToString();
                                    if (!dr.IsDBNull(6)) DocumentPrihod.Osnovanie = dr.GetValue(6).ToString();
                                    if (!dr.IsDBNull(7))
                                    {
                                        int IdPaidInReasons = int.Parse(dr.GetValue(7).ToString());
                                        foreach (BLL.LocalPlg.LocalPaidInReasons itemPaidInReasons in Com.LocalFarm.CurLocalPaidInReasons)
                                        {
                                            if (itemPaidInReasons.Id!=null && (int)itemPaidInReasons.Id== IdPaidInReasons)
                                            {
                                                DocumentPrihod.PaidInReasons = itemPaidInReasons;
                                                break;
                                            }
                                        }
                                    }
                                    if (!dr.IsDBNull(8)) DocumentPrihod.VtomChisle = dr.GetValue(8).ToString();
                                    if (!dr.IsDBNull(9)) DocumentPrihod.NDS = decimal.Parse(dr.GetValue(9).ToString());
                                    if (!dr.IsDBNull(10)) DocumentPrihod.Prilozenie = dr.GetValue(10).ToString();
                                    if (!dr.IsDBNull(11)) DocumentPrihod.GlavBuh = dr.GetValue(11).ToString();
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
    `KredikKodAnalUch`, `Summa`, `KodNazn`, `Osnovanie`, 
    `Id_PaidInReasons`, `VtomChisle`, `NDS`, `Prilozenie`, 
    `GlavBuh`) 
Values({0}, '{1}', '{2}', '{3}',
    '{4}', {5}, `{6}`, `{7}`, 
    {8}, `{9}`, {10}, `{11}`, 
    `{12}`)", NewDocumentPrihod.Id, NewDocumentPrihod.DebetNomerSchet, NewDocumentPrihod.KreditKodDivision, NewDocumentPrihod.KredikKorSchet,
                            NewDocumentPrihod.KredikKodAnalUch, NewDocumentPrihod.Summa, NewDocumentPrihod.KodNazn, NewDocumentPrihod.Osnovanie,
                            NewDocumentPrihod.PaidInReasons.Id, NewDocumentPrihod.VtomChisle, NewDocumentPrihod.NDS, NewDocumentPrihod.Prilozenie,
                            NewDocumentPrihod.GlavBuh);

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
Set `DebetNomerSchet`='{1}', `KreditKodDivision`='{2}', `KredikKorSchet`='{3}', `KredikKodAnalUch`='{4}', 
    `Summa`={5}, `KodNazn`='{6}', `Osnovanie`='{7}', `Id_PaidInReasons`={8}, 
    `VtomChisle`='{9}', `NDS`={10}, `Prilozenie`={11}, `GlavBuh`='{12}'
Where Id={0}", UpdDocumentPrihod.Id, 
            UpdDocumentPrihod.DebetNomerSchet, UpdDocumentPrihod.KreditKodDivision, UpdDocumentPrihod.KredikKorSchet, UpdDocumentPrihod.KredikKodAnalUch,
            UpdDocumentPrihod.Summa, UpdDocumentPrihod.KodNazn, UpdDocumentPrihod.Osnovanie, UpdDocumentPrihod.PaidInReasons.Id,
            UpdDocumentPrihod.VtomChisle, UpdDocumentPrihod.NDS, UpdDocumentPrihod.Prilozenie, UpdDocumentPrihod.GlavBuh);

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
        #endregion

    }
}
