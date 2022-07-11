using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.IO;
using System.Threading;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.BLL.DocumentPlg.Lib
{
    /// <summary>
    /// Базовый класс для всех документов
    /// </summary>
    public abstract class DocumentBase
    {
        #region Private Param
        private static object obj = new object();

        /// <summary>
        /// Количество попыток записи в лог
        /// </summary>
        private static int IOCountPoput = 5;

        /// <summary>
        /// Количество милесекунд мкжду попутками записи
        /// </summary>
        private static int IOWhileInt = 500;
        #endregion

        /// <summary>
        /// Идентификатор в базе данных
        /// </summary>
        public int? Id { get; protected set; }

        /// <summary>
        /// Идентификатор продукта в коллекции
        /// </summary>
        public int Index { get; protected set; } = -1;

        /// <summary>
        /// Тип плагина
        /// </summary>
        public string DocFullName { get; protected set; }

        /// <summary>
        /// Дата создания документа
        /// </summary>
        public DateTime CreateDate { get; protected set; } = DateTime.Now;

        /// <summary>
        /// Юридическая дата к которой относится документ
        /// </summary>
        public DateTime? UreDate { get; set; } = DateTime.Now.Date;

        /// <summary>
        /// Дата изменеия документа
        /// </summary>
        public DateTime ModifyDate { get; protected set; } = DateTime.Now;

        /// <summary>
        /// Пользовтаель который изменил последний раз документ
        /// </summary>
        public string ModifyUser { get; protected set; } = Com.UserFarm.CurrentUser.Logon;

        /// <summary>
        /// Операция к которой относится этот документ
        /// </summary>
        public Operation CurOperation { get; protected set; }

        /// <summary>
        /// Дебитор
        /// </summary>
        public Local LocalDebitor { get; set; } = null;

        /// <summary>
        /// Кредитор
        /// </summary>
        public Local LocalCreditor { get; set; } = null;

        /// <summary>
        /// Департамент или касса в которой создан документ
        /// </summary>
        public Local Departament { get; set; } = null;

        /// <summary>
        /// Дебитор который ввели вручную не из списка
        /// </summary>
        public string OtherDebitor;

        /// <summary>
        /// Кредитор который ввели вручную не из списка
        /// </summary>
        public string OtherKreditor;

        /// <summary>
        /// Номер документа
        /// </summary>
        public int DocNum { get; set; }

        /// <summary>
        /// Черновик
        /// </summary>
        public bool IsDraft { get; protected set; } = true;

        /// <summary>
        /// Проведённый документ или нет
        /// </summary>
        public bool IsProcessed { get; protected set; } = false;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="DocFullName">Тип плагина</param>
        /// <param name="CurOperation">Операция к которой относится этот документ</param>
        /// <param name="LocalDebitor">Дебитор</param>
        /// <param name="LocalCreditor">Кредитор</param>
        /// <param name="Departament">Департамент или касса в которой создан документ</param>
        /// <param name="OtherDebitor">Дебитор который ввели вручную не из списка</param>
        /// <param name="OtherKreditor">Кредитор который ввели вручную не из списка</param>
        /// <param name="DocNum"> Черновик</param>
        /// <param name="IsDraft"> Черновик</param>
        public DocumentBase(string DocFullName, Operation CurOperation, Local LocalDebitor, Local LocalCreditor, Local Departament, string OtherDebitor, string OtherKreditor, int DocNum, bool IsDraft)
        {
            try
            {
                this.DocFullName = DocFullName;
                this.CurOperation = CurOperation;
                this.LocalDebitor = LocalDebitor;
                this.LocalCreditor = LocalCreditor;
                this.OtherDebitor = OtherDebitor;
                this.OtherKreditor = OtherKreditor;
                this.DocNum = DocNum;
                this.IsDraft = IsDraft;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации класса Document с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Представляет из себя список продуктов
        /// </summary>
        public class DocumentBaseList : IEnumerable
        {
            /// <summary>
            /// Внутренний список
            /// </summary>
            private List<Document> _DocumentL = new List<Document>();

            /// <summary>
            /// Индексаторы
            /// </summary>
            /// <param name="index">Поиск по индексы</param>
            /// <returns>Возвращает Document</returns>
            public Document this[int index]
            {
                get
                {
                    return this._DocumentL[index];
                }
                private set { }
            }

            /// <summary>
            /// Индексаторы
            /// </summary>
            /// <param name="id">Поиск по id</param>
            /// <returns>Возвращает Document</returns>
            public Document this[int? id]
            {
                get
                {
                    if (id != null)
                    {
                        foreach (Document item in this._DocumentL)
                        {
                            if (item.Id != null && item.Id == id) return item;
                        }
                    }
                    return null;
                }
                private set { }
            }

            /// <summary>
            /// Конструктор в котором можно реализовать например базовое первоначальное получение списка доступных товаров
            /// </summary>
            public DocumentBaseList()
            {
                try
                {
                    // Можно было бы сделать чтобы справочник грузился сразу в конструкторе, но проблема в том что статические классы создаются до того как мы запускаем форму и из за этого возникает проблема с подписью
                    //GetProductListNotCash();
                }
                catch (Exception ex)
                {
                    ApplicationException ae = new ApplicationException(string.Format("Упали при инициализации конструктора с ошибкой: ({0})", ex.Message));
                    Com.Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                    throw ae;
                }

            }


            /// <summary>
            /// Количчество объектов в контейнере
            /// </summary>
            public int Count
            {
                get
                {
                    int rez;
                    lock (_DocumentL)
                    {
                        rez = _DocumentL.Count;
                    }
                    return rez;
                }
                private set { }
            }

            /// <summary>
            /// Добавление нового продукта
            /// </summary>
            /// <param name="nDocument">Новый Document</param>
            public void Add(Document nDocument)
            {
                try
                {
                    // Добавляет продукт в список
                    lock (_DocumentL)
                    {
                        nDocument.Index = this.Count;
                        _DocumentL.Add(nDocument);
                    }
                }
                catch (Exception ex)
                {
                    Com.Log.EventSave(string.Format("Произошла ошибка: ({0})", ex.Message), string.Format("{0}.OperationBase.Add(Operation nOperation)", GetType().Name), EventEn.Error, true, false);
                    throw ex;
                }
            }

            /// <summary>
            /// Для обращения по индексатору
            /// </summary>
            /// <returns>Возвращаем стандарнтый индексатор</returns>
            public IEnumerator GetEnumerator()
            {
                lock (_DocumentL)
                {
                    return _DocumentL.GetEnumerator();
                }
            }
        }


        /// <summary>
        /// Проверка наличия папки и если её нет то создаёт её
        /// </summary>
        /// <param name="PrefixDir">Путь подпапок от корня указанного в конфиге до папки которую нужно проверить</param>
        /// <param name="Dir">Подпапка которую нужно проверить</param>
        /// <param name="FileName">Имя файла который потом нужно воткнуть в самом конце для возврата полного путик файлу</param>
        /// <returns>Возвращаем полный путь с учётом корня из настроек, префикса и имени файла</returns>
        public string CreateFolder(string PrefixDir, string Dir, string FileName)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(Dir)) throw new ApplicationException("Не указана папка в которую производится експорт файлов для Excel");
                if (String.IsNullOrWhiteSpace(Com.LocalFarm.CurLocalDepartament.UploadExcelDir.Replace("@CurrentDirectory", Environment.CurrentDirectory))) throw new ApplicationException("Не указана корневая папка в которую производится сохранение файлов для Excel");

                // Узнаём домашний каталог убрав всякие ненужные символы и обработав разделитель каталогов в начале и в конце нашего префикса
                string StartDir = Com.LocalFarm.CurLocalDepartament.UploadExcelDir.Replace("@CurrentDirectory", Environment.CurrentDirectory);
                if(!string.IsNullOrWhiteSpace(PrefixDir))
                {
                    while (PrefixDir.IndexOf(@"\\") >= 0)
                    {
                        PrefixDir = PrefixDir.Trim().Replace(@"\\", @"\");
                    }

                    if (PrefixDir.IndexOf(@"\") == 0) PrefixDir = PrefixDir.Substring(1);
                    if (PrefixDir.Substring(PrefixDir.Length-1) == @"\") PrefixDir=PrefixDir.Substring(0, PrefixDir.Length - 1);

                    if (!string.IsNullOrWhiteSpace(PrefixDir)) StartDir += string.Format("\\{0}", PrefixDir);
                 }

                if (!Directory.Exists(StartDir))
                {
                    if (StartDir == Com.LocalFarm.CurLocalDepartament.UploadExcelDir.Replace("@CurrentDirectory", Environment.CurrentDirectory)) Directory.CreateDirectory(StartDir);
                    else throw new ApplicationException(String.Format("Папки ({0}) в которую производится сохранение файлов для Excel с учётом заданного префикса не существует", StartDir));
                }

                Dir = Dir.Replace(@"\", @"");

                if (!Directory.Exists(string.Format("{0}\\{1}", StartDir, Dir)))
                    Directory.CreateDirectory(string.Format("{0}\\{1}", StartDir, Dir));

                return string.Format("{0}\\{1}\\{2}", StartDir, Dir, FileName);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.CreateFolder", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Экспорт документа в 1С
        /// </summary>
        /// <param name="FileName">Имя файла</param>
        /// <param name="row">Строка которую вставить</param>
        /// <param name="FlagDelete">Если Истина то проверяется наличие файла и если он есть то удаляется файл</param>
        protected void ExportTo1C(string FileName, string row, Boolean FlagDelete)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(Com.LocalFarm.CurLocalDepartament.Upload1CDir)) throw new ApplicationException("Не указана папка в которую производится експорт файлов для 1С");
                if (!Directory.Exists(Com.LocalFarm.CurLocalDepartament.Upload1CDir)) throw new ApplicationException("Папки в которую производится експорт файлов для 1С не существует");

                if (FlagDelete && File.Exists(string.Format("{0}\\{1}", Com.LocalFarm.CurLocalDepartament.Upload1CDir, FileName)))
                    File.Delete(string.Format("{0}\\{1}", Com.LocalFarm.CurLocalDepartament.Upload1CDir, FileName));

                this.ExportTo1C(FileName, row, IOCountPoput);
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выполнении метода с ошибкой: ({0})", ex.Message));
                Com.Log.EventSave(ae.Message, string.Format("{0}.ExportTo1C", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        /// <summary>
        /// Метод для записи информации в лог
        /// </summary>
        /// <param name="FileName">Имя файла</param>
        /// <param name="row">Строка которую вставить</param>
        /// <param name="IOCountPoput">Количество попыток записи в лог</param>
        private void ExportTo1C(string FileName, string row, int IOCountPoput)
        {
            try
            {
                lock (obj)
                {
                    using (StreamWriter SwFileLog = new StreamWriter(Com.LocalFarm.CurLocalDepartament.Upload1CDir + @"\" + FileName, true))
                    {
                        SwFileLog.WriteLine(row);
                    }
                }
            }
            catch (Exception)
            {
                if (IOCountPoput > 0)
                {
                    Thread.Sleep(IOWhileInt);
                    this.ExportTo1C(FileName, row, IOCountPoput - 1);
                }
                else throw;
            }
        }
    }
}
