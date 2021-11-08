using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows.Forms;
using System.Data;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.Com.Provider.Lib
{
    public abstract class ProviderBase
    {
        /// <summary>
        /// Получаем элемент меню для получения информации по провайдеру
        /// </summary>
        public ToolStripMenuItem InfoToolStripMenuItem { get; private set; }

        /// <summary>
        /// Тип палгина
        /// </summary>
        public Type PlugInType { get; private set; }

        /// <summary>
        /// Строка подключения
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Возвращает версию базы данных в виде строки
        /// </summary>
        public string VersionDB { get; private set; }

        /// <summary>
        /// Возвращаем версию драйвера
        /// </summary>
        public string Driver { get; private set; }

        /// <summary>
        /// Ссылка на универсальный провайдер
        /// </summary>
        public UProvider UPovider { get; private set; }

        /// <summary>
        /// Печать строки подключения с маскировкой секретных данных
        /// </summary>
        /// <returns>Строка подклюения с замасированной секретной информацией</returns>
        public virtual string PrintConnectionString()
        {
            return this.ConnectionString;
        }

        /// <summary>
        /// Установка строки подключения
        /// </summary>
        /// <param name="ConnectionString">Строка подключения</param>
        /// <param name="VersionDB">Возвращает версию базы данных в виде строки</param>
        /// <param name="Driver">Возвращаем версию драйвера</param>
        protected void SetupConnectionStringAndVersionDB(string ConnectionString, string VersionDB, string Driver)
        {
            this.ConnectionString = ConnectionString;
            this.VersionDB = VersionDB;
            this.Driver = Driver;
        }

        /// <summary>
        /// Установка галавного компонента который должны
        /// </summary>
        /// <param name="t">Тип плагина</param>
        /// <param name="InfoToolStripMenuItem">"Элемент меню для получения информации по плагину</param>
        /// <param name="ConnectionString">Текущий провайдер</param>
        protected void SetupProviderBase(Type t, ToolStripMenuItem InfoToolStripMenuItem, string ConnectionString)
        {
            this.PlugInType = t;
            this.InfoToolStripMenuItem = InfoToolStripMenuItem;
            this.ConnectionString = ConnectionString;
        }

        /// <summary>
        /// Доступно ли подключение или нет
        /// </summary>
        /// <returns>true Если смогли подключиться к базе данных</returns>
        public bool HashConnect()
        {
            bool rez = false;

            // проверяем наличие подключения в системе
            if (this.VersionDB != null && this.VersionDB.Trim() != string.Empty)
            {
                rez = true;
            }

            return rez;
        }

        /// <summary>
        /// Метод для записи информации в лог
        /// </summary>
        /// <param name="Message">Сообщение</param>
        /// <param name="Source">Источник</param>
        /// <param name="evn">Тип события</param>
        public void EventSave(string Message, string Source, EventEn evn)
        {
            Log.EventSave(Message, Source + " (" + this.PlugInType.Name + ")", evn, true, false);
        }

        /// <summary>
        /// Получение любых данных из источника например чтобы плагины могли что-то дополнительно читать
        /// </summary>
        /// <param name="SQL">Собственно запрос</param>
        /// <returns>Результата В виде таблицы</returns>
        public virtual DataTable getData(string SQL)
        {
            throw new ApplicationException(string.Format("В провайдере с типом {0} не реализовано переопределение метода: getData()", this.PlugInType.Name));
        }

        /// <summary>
        /// Выполнение любых запросов на источнике
        /// </summary>
        /// <param name="SQL">Собственно запрос</param>
        public virtual void setData(string SQL)
        {
            throw new ApplicationException(string.Format("В провайдере с типом {0} не реализовано переопределение метода: setData()", this.PlugInType.Name));
        }

        /*
                /// <summary>
                /// Выкачивание чеков
                /// </summary>
                /// <param name="FuncTarget">Функция которой передать строчку из чека</param>
                /// <param name="CnfL">Текущая конфигурация в которой обрабатывается строка чека</param>
                /// <param name="NextScenary">Индекс следующего элемента конфигурации который будет обрабатывать строку чека</param>
                /// <param name="FirstDate">Первая дата чека, предпологается использовать для прогресс бара</param>
                /// <param name="FilCustSid">Фильтр для получения данных по конкретному клиенту. null значит пользователь выгребает по всем клиентам</param>
                /// <returns>Успех обработки функции</returns>
                public virtual bool getCheck(Func<Check, ConfigurationList, int, DateTime?, bool> FuncTarget, ConfigurationList CnfL, int NextScenary, DateTime? FirstDate, long? FilCustSid)
                {
                    throw new ApplicationException(string.Format("В провайдере с типом {0} не реализовано переопределение метода: getCheck()", this.PlugInType.Name));
                }

                /// <summary>
                /// Заполнение справочника текущих пользователей
                /// </summary>
                /// <param name="FuncTarget">Функция котороая юудет заполнять справочник</param>
                /// <returns>Успех обработки функции</returns>
                public virtual bool getCustumers(Func<Customer, bool> FuncTarget)
                {
                    throw new ApplicationException(string.Format("В провайдере с типом {0} не реализовано переопределение метода: getCustumers()", this.PlugInType.Name));
                }

                /// <summary>
                /// Заполнение справочника причин скидок
                /// </summary>
                /// <returns>Успех обработки функции</returns>
                public virtual bool getDiscReasons()
                {
                    throw new ApplicationException(string.Format("В провайдере с типом {0} не реализовано переопределение метода: getDiscReasons()", this.PlugInType.Name));
                }



                /// <summary>
                /// Установка расчитанной скидки в базе у конкртеного клиента
                /// </summary>
                /// <param name="Cst">Клиент</param>
                /// <param name="CalkMaxDiscPerc">Процент скидки который мы устанавливаем</param>
                /// <returns>Успех обработки функции</returns>
                public virtual bool AployDMCalkMaxDiscPerc(CustomerBase Cst, decimal CalkMaxDiscPerc)
                {
                    throw new ApplicationException(string.Format("В провайдере с типом {0} не реализовано переопределение метода: AployDMCalkMaxDiscPerc(long CustSid, int CalkMaxDiscPerc)", this.PlugInType.Name));
                }

                /// <summary>
                /// Установка бонусного бала в базе у конкртеного клиента
                /// </summary>
                /// <param name="Cst">Клиент</param>
                /// <param name="CalkStoreCredit">Бонусный бал который мы устанавливаем</param>
                /// <param name="CalcScPerc">Процент по которому считался бонусный бал который мы устанавливаем</param>
                /// <param name="OldCalkStoreCredit">Старый бонусный бал который мы устанавливаем</param>
                /// <param name="OldCalcScPerc">Старый процент по которому считался бонусный бал который мы устанавливаем</param>
                /// <returns>Успех обработки функции</returns>
                public virtual bool AployDMCalkStoreCredit(CustomerBase Cst, decimal CalkStoreCredit, decimal CalcScPerc, decimal OldCalkStoreCredit, decimal OldCalcScPerc)
                {
                    throw new ApplicationException(string.Format("В провайдере с типом {0} не реализовано переопределение метода: AployDMCalkStoreCredit(long CustSid, int CalkStoreCredit)", this.PlugInType.Name));
                }*/

        /// <summary>
        /// Базовый класс универсального провайдера, он имеет доступ к закрытым базовым объектам провайдеров
        /// </summary>
        public class UProviderBase
        {
            /// <summary>
            /// Установка в базовом провайдере ссылки на универсальный провайдер, для того чтобы при написании нового плагина можно было вызывать функции универсального плагина через базовый клас
            /// </summary>
            /// <param name="Bprv">Провайдер в котором мы устанавливаем ссылку</param>
            /// <param name="Uprv">Универсальный плагин на который мы указываем</param>
            protected void UPoviderSetupForProviderBase(ProviderBase Bprv, UProvider Uprv)
            {
                Bprv.UPovider = Uprv;
            }
        }

    }
}
