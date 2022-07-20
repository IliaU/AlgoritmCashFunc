using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Collections;

namespace AlgoritmCashFunc.Lib.LibBase
{
    /// <summary>
    /// Базовый класс пользователя
    /// </summary>
    public class UserBase : EventArgs
    {
        /// <summary>
        /// Логин пользователя
        /// </summary>
        public string Logon { get; private set; }

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password;

        /// <summary>
        /// Описание пользователя
        /// </summary>
        public string Description;

        /// <summary>
        /// Роль пользователя
        /// </summary>
        public RoleEn Role;

        /// <summary>
        /// Индекс элемента в списке
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Конструктор базового класса
        /// </summary>
        /// <param name="Logon">Логин пользователя (специалбно сделан только на чтение, чтобы пользователи не могли его править)</param>
        public UserBase(string Logon)
        {
            this.Index = -1;
            this.Logon = Logon;
        }

        /*
        /// <summary>
        /// Инициализация параметров базового класса
        /// </summary>
        /// <param name="Logon">Логин пользователя (специалбно сделан только на чтение, чтобы пользователи не могли его править)</param>
        protected void InitialUser(string Logon)
        {
            this.Index = -1;
            this.Logon = Logon;
        }*/

        /// <summary>
        /// Базовый класс для компонента списка пользователей
        /// </summary>
        public abstract class UserBaseList : IEnumerable
        {
            /// <summary>
            /// Внутренний список 
            /// </summary>
            private static List<UserBase> UserL = new List<UserBase>();

            /// <summary>
            /// Количчество объектов в контейнере
            /// </summary>
            public int Count
            {
                get
                {
                    int rez;
                    lock (UserL)
                    {
                        rez = UserL.Count;
                    }
                    return rez;
                }
                private set { }
            }

            /// <summary>
            /// Добавление нового пользователя
            /// </summary>
            /// <param name="newUser">Пользователь которого нужно добавить в список</param>
            /// <param name="HashExeption">C отображением исключений</param>
            /// <returns>Результат операции (Успех или нет)</returns>
            protected bool Add(UserBase newUser, bool HashExeption)
            {
                bool rez = false;

                try
                {
                    lock (UserL)
                    {
                        bool flag = false;

                        foreach (UserBase item in UserL)
                        {
                            if (item.Logon == newUser.Logon) flag = true;
                        }

                        if (!flag)
                        {
                            newUser.Index = UserL.Count;
                            UserL.Add(newUser);
                            rez = true;
                        }
                        else
                        {
                            if (HashExeption) throw new ApplicationException(string.Format("Пользователь с логоном {0} уже существует.", newUser.Logon));
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (HashExeption) throw new ApplicationException(string.Format("Не удалось добавить пользователя {0} произошла ошибка: {1}", newUser.Logon, ex.Message));
                }
                return rez;
            }

            /// <summary>
            /// Удаление пользователя
            /// </summary>
            /// <param name="delUser">Пользователь которого нужно удалить из списка</param>
            /// <param name="HashExeption">C отображением исключений</param>
            /// <returns>Результат операции (Успех или нет)</returns>
            protected bool Remove(UserBase delUser, bool HashExeption)
            {
                bool rez = false;
                try
                {
                    lock (UserL)
                    {
                        int delIndex = delUser.Index;
                        UserL.RemoveAt(delIndex);

                        for (int i = delIndex; i < UserL.Count; i++)
                        {
                            UserL[i].Index = i;
                        }

                        rez = true;
                    }
                }
                catch (Exception ex)
                {
                    if (HashExeption) throw new ApplicationException(string.Format("Не удалось удалить пользователя {0} произошла ошибка: {1}", delUser.Logon, ex.Message));
                }

                return rez;
            }

            /// <summary>
            /// Обновление данных пользователя. Ключом является logon он не обнавляется
            /// </summary>
            /// <param name="updUser">Пользователь у которого нужно изменить данные</param>
            /// <param name="HashExeption">C отображением исключений</param>
            /// <returns>Результат операции (Успех или нет)</returns>
            protected bool Update(UserBase updUser, bool HashExeption)
            {
                bool rez = false;
                try
                {
                    lock (UserL)
                    {
                        int ubpIndex = -1;
                        for (int i = 0; i < UserL.Count; i++)
                        {
                            if (UserL[i].Logon == updUser.Logon) ubpIndex = i;
                        }

                        if (ubpIndex == -1)
                        {
                            if (HashExeption) throw new ApplicationException(string.Format("Не удалось обновить данные пользователя {0} с таким логоном пользователя в текущем спискене найдено.", updUser.Logon));
                        }
                        else
                        {
                            UserL[ubpIndex].Password = updUser.Password;
                            UserL[ubpIndex].Description = updUser.Description;
                            UserL[ubpIndex].Role = updUser.Role;

                            /*
                            if(updUser.GetType().FullName == "AlgoritmCashFunc.Lib.User")
                            {
                                ((User)UserL[ubpIndex]).EmploeeId = ((User)updUser).EmploeeId;
                            }
                            */

                            try
                            {
                                ((User)UserL[ubpIndex]).EmploeeId = ((User)updUser).EmploeeId;
                            }
                            catch (Exception) {}

                            rez = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (HashExeption) throw new ApplicationException(string.Format("Не удалось обновить данные пользователя {0} произошла ошибка: {1}", updUser.Logon, ex.Message));
                }

                return rez;
            }

            /// <summary>
            /// Получение компонента по его ID
            /// </summary>
            /// <param name="i">Введите идентификатор</param>
            /// <returns></returns>
            protected UserBase getUserComponent(int i) { return UserL[i]; }

            /// <summary>
            /// Для обращения по индексатору
            /// </summary>
            /// <returns>Возвращаем стандарнтый индексатор</returns>
            public IEnumerator GetEnumerator()
            {
                return UserL.GetEnumerator();
            }
        }
    }
}
