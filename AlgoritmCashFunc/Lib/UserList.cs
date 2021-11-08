using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib.LibBase;

namespace AlgoritmCashFunc.Lib
{
    /// <summary>
    /// Список пользователей в системе, тут можно организовать обработку событий
    /// </summary>
    public sealed class UserList : UserBase.UserBaseList
    {
        private static UserList MyObj;

        /// <summary>
        /// Событие возникновения добавления пользователя
        /// </summary>
        public event EventHandler<EventUser> onUserListAddingUser;
        /// <summary>
        /// Событие добавления пользователя
        /// </summary>
        public event EventHandler<User> onUserListAddedUser;
        /// <summary>
        /// Событие возникновения удаления пользователя
        /// </summary>
        public event EventHandler<EventUser> onUserListDeletingUser;
        /// <summary>
        /// Событие удаления пользователя
        /// </summary>
        public event EventHandler<User> onUserListDeletedUser;
        /// <summary>
        /// Событие возникновения изменения данных пользователя
        /// </summary>
        public event EventHandler<EventUser> onUserListUpdatingUser;
        /// <summary>
        /// Событие изменения данных пользователя
        /// </summary>
        public event EventHandler<User> onUserListUpdatedUser;


        /// <summary>
        /// Получение экземпляра нашего синглетон списка пользователей
        /// </summary>
        /// <returns></returns>
        public static UserList GetInstatnce()
        {
            if (MyObj == null) MyObj = new UserList();
            return MyObj;
        }

        /// <summary>
        /// внутренний конструктор нашего листа
        /// </summary>
        private UserList()
        {

        }

        /// <summary>
        /// Вытаскивает провайдер по его индексу в эом контернере
        /// </summary>
        /// <param name="i">Индекс</param>
        /// <returns>Универсальный провайдер</returns>
        public User this[int i]
        {
            get { return ((User)base.getUserComponent(i)); }
            private set { }
        }

        /// <summary>
        /// Вытаскивает пользователя по его имени
        /// </summary>
        /// <param name="s">Имя</param>
        /// <returns>Универсальный провайдер</returns>
        public User GetUser(string s)
        {
            try
            {
                for (int i = 0; i < base.Count; i++)
                {
                    if (((User)base.getUserComponent(i)).Logon == s) return ((User)base.getUserComponent(i));
                }
                throw new ApplicationException(string.Format("Пользователя с именем {0} в системе не существует.", s));
            }
            catch (Exception) { throw; }
        }



        /// <summary>
        /// Добавление нового пользователя
        /// </summary>
        /// <param name="newUser">Пользователь которого нужно добавить в список</param>
        /// <param name="HashExeption">C отображением исключений</param>
        /// <param name="HashExeption">Записывать сообщения в лог или нет</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public bool Add(UserBase newUser, bool HashExeption, bool WriteLog)
        {
            EventUser MyArg = new EventUser((User)newUser);
            if (onUserListAddingUser != null) onUserListAddingUser.Invoke(this, MyArg);

            // Если мы одобрили добавление
            if (MyArg.Action)
            {
                bool rez = false;
                try
                {
                    rez = base.Add(newUser, HashExeption);

                    // Если добавление пользователя прошло успешно.
                    if (rez)
                    {
                        if (onUserListAddedUser != null) onUserListAddedUser.Invoke(this, MyArg.Usr);
                        if (WriteLog) Com.Log.EventSave(string.Format("Добавился новый пользователь: Logon {0}, Description {1}, Role {2}", newUser.Logon, (newUser.Description == null ? "" : newUser.Description), newUser.Role.ToString()), GetType().Name + ".Add", EventEn.Message);
                    }
                    else if (WriteLog) Com.Log.EventSave(string.Format("Произошла ошибка при добавлении нового пользователя: Logon {0}, Description {1}, Role {2}", newUser.Logon, (newUser.Description == null ? "" : newUser.Description), newUser.Role.ToString()), GetType().Name + ".Add", EventEn.Error);
                }
                catch (Exception ex)
                {
                    if (WriteLog) Com.Log.EventSave(string.Format("Произошла ошибка при добавлении нового пользователя: Logon {0}, Description {1}, Role {2} ({3})", newUser.Logon, (newUser.Description == null ? "" : newUser.Description), newUser.Role.ToString(), ex.Message), GetType().Name + ".Add", EventEn.Error);
                    throw;
                }
                return rez;
            }
            return false;
        }
        /// <summary>
        /// Добавление нового пользователя
        /// </summary>
        /// <param name="newUser">Пользователь которого нужно добавить в список</param>
        /// <param name="HashExeption">C отображением исключений</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public new bool Add(UserBase newUser, bool HashExeption)
        {
            return Add(newUser, true, true);
        }
        /// <summary>
        /// Добавление нового пользователя
        /// </summary>
        /// <param name="newUser">Пользователь которого нужно добавить в список</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public bool Add(UserBase newUser)
        {
            return Add(newUser, true);
        }

        /// <summary>
        /// Удаление пользователя
        /// </summary>
        /// <param name="delUser">Пользователь которого нужно удалить из списка</param>
        /// <param name="HashExeption">C отображением исключений</param>
        /// <param name="WriteLog">Записывать сообщения в лог или нет</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public bool Remove(UserBase delUser, bool HashExeption, bool WriteLog)
        {
            EventUser MyArg = new EventUser((User)delUser);
            if (onUserListDeletingUser != null) onUserListDeletingUser.Invoke(this, MyArg);

            // Если мы одобрили удаление
            if (MyArg.Action)
            {
                bool rez = false;
                try
                {
                    rez = base.Remove(delUser, HashExeption);

                    // Если удаление пользователя прошло успешно.
                    if (rez)
                    {
                        if (onUserListDeletedUser != null) onUserListDeletedUser.Invoke(this, MyArg.Usr);
                        if (WriteLog) Com.Log.EventSave(string.Format("Удалили пользователя: Logon {0}, Description {1}, Role {2}", delUser.Logon, (delUser.Description == null ? "" : delUser.Description), delUser.Role.ToString()), GetType().Name + ".Remove", EventEn.Message);
                    }
                    else if (WriteLog) Com.Log.EventSave(string.Format("Произошла ошибка при удалении пользователя: Logon {0}, Description {1}, Role {2}", delUser.Logon, (delUser.Description == null ? "" : delUser.Description), delUser.Role.ToString()), GetType().Name + ".Remove", EventEn.Error);
                }
                catch (Exception ex)
                {
                    if (WriteLog) Com.Log.EventSave(string.Format("Произошла ошибка при удалении пользователя: Logon {0}, Description {1}, Role {2} ({3})", delUser.Logon, (delUser.Description == null ? "" : delUser.Description), delUser.Role.ToString(), ex.Message), GetType().Name + ".Remove", EventEn.Error);
                    throw;
                }
                return rez;
            }
            return false;
        }
        /// <summary>
        /// Удаление пользователя
        /// </summary>
        /// <param name="delUser">Пользователь которого нужно удалить из списка</param>
        /// <param name="HashExeption">C отображением исключений</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public new bool Remove(UserBase delUser, bool HashExeption)
        {
            return Remove(delUser, true, true);
        }
        /// <summary>
        /// Удаление пользователя
        /// </summary>
        /// <param name="delUser">Пользователь которого нужно удалить из списка</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public bool Remove(UserBase delUser)
        {
            return Remove(delUser, true);
        }

        /// <summary>
        /// Обновление пользователя
        /// </summary>
        /// <param name="updUser">Пользователь у которого нужно обновить данные в списке</param>
        /// <param name="HashExeption">C отображением исключений</param>
        /// <param name="WriteLog">Записывать сообщения в лог или нет</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public bool Update(UserBase updUser, bool HashExeption, bool WriteLog)
        {
            EventUser MyArg = new EventUser((User)updUser);
            if (onUserListUpdatingUser != null) onUserListUpdatingUser.Invoke(this, MyArg);

            // Если мы одобрили обновлени
            if (MyArg.Action)
            {
                bool rez = false;
                try
                {
                    rez = base.Update(updUser, HashExeption);

                    // Если обновление данных пользователя прошло успешно.
                    if (rez)
                    {
                        if (onUserListUpdatedUser != null) onUserListUpdatedUser.Invoke(this, MyArg.Usr);
                        if (WriteLog) Com.Log.EventSave(string.Format("Обновление данных пользователя: Logon {0}, Description {1}, Role {2}", updUser.Logon, (updUser.Description == null ? "" : updUser.Description), updUser.Role.ToString()), GetType().Name + ".Update", EventEn.Message);
                    }
                    else if (WriteLog) Com.Log.EventSave(string.Format("Произошла ошибка при обновлении данных пользователя: Logon {0}, Description {1}, Role {2}", updUser.Logon, (updUser.Description == null ? "" : updUser.Description), updUser.Role.ToString()), GetType().Name + ".Update", EventEn.Error);
                }
                catch (Exception ex)
                {
                    if (WriteLog) Com.Log.EventSave(string.Format("Произошла ошибка при обновлении данных пользователя: Logon {0}, Description {1}, Role {2} ({3})", updUser.Logon, (updUser.Description == null ? "" : updUser.Description), updUser.Role.ToString(), ex.Message), GetType().Name + ".Update", EventEn.Error);
                    throw;
                }
                return rez;
            }
            return false;
        }
        /// <summary>
        /// Обновление пользователя
        /// </summary>
        /// <param name="updUser">Пользователь у которого нужно обновить данные в списке</param>
        /// <param name="HashExeption">C отображением исключений</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public new bool Update(UserBase updUser, bool HashExeption)
        {
            return Update(updUser, true, true);
        }
        /// <summary>
        /// Обновление пользователя
        /// </summary>
        /// <param name="updUser">Пользователь у которого нужно обновить данные в списке</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public bool Update(UserBase updUser)
        {
            return Update(updUser, true);
        }
    }
}
