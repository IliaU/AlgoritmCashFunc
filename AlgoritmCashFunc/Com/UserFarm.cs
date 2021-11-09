using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.Com
{
    /// <summary>
    /// Управлнеия пользователями
    /// </summary>
    public static class UserFarm
    {
        /// <summary>
        /// Пользователи зарегистрированные в сисиетме
        /// </summary>
        public static UserList List = UserList.GetInstatnce();

        /// <summary>
        /// Текущий пользователь авторизованный в системе
        /// </summary>
        public static User CurrentUser { get; private set; }

        /// <summary>
        /// Установка нового текущего пользователя
        /// </summary>
        /// <param name="SCurrentUser">Пользователь который хочет авторизоваться</param>
        /// <param name="Password">Пароль который ввёл пользователь при авторизации</param>
        /// <param name="HashExeption">C отображением исключений</param>
        /// <param name="WriteLog">Записывать сообщения в лог или нет</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public static bool SetupCurrentUser(User SCurrentUser, string Password, bool HashExeption, bool WriteLog)
        {
            if (SCurrentUser.Password == Password)
            {
                CurrentUser = List.GetUser(SCurrentUser.Logon);
                if (WriteLog) Com.Log.EventSave(string.Format("Пользователь {0} авторизовался успешно.", SCurrentUser.Logon), "UserFarm.SetupCurrentUser", EventEn.Message);
                return true;
            }
            else
            {
                if (HashExeption)
                {
                    string Mes = "Пароль введён не верный.";
                    if (WriteLog) Com.Log.EventSave(string.Format("Попытка авторизоваться под пользователем {0} была блокированна: {1}", SCurrentUser.Logon, Mes), "UserFarm.SetupCurrentUser", EventEn.Message);
                    throw new ApplicationException(Mes);
                }
            }

            return false;
        }
        //
        /// <summary>
        /// Установка нового текущего пользователя
        /// </summary>
        /// <param name="SCurrentUser">Пользователь который хочет авторизоваться</param>
        /// <param name="Password">Пароль который ввёл пользователь при авторизации</param>
        /// <param name="HashExeption">C отображением исключений</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public static bool SetupCurrentUser(User SCurrentUser, string Password, bool HashExeption)
        {
            return SetupCurrentUser(SCurrentUser, Password, HashExeption, true);
        }
        //
        /// <summary>
        /// Установка нового текущего пользователя
        /// </summary>
        /// <param name="SCurrentUser">Пользователь который хочет авторизоваться</param>
        /// <param name="Password">Пароль который ввёл пользователь при авторизации</param>
        /// <returns>Результат операции (Успех или нет)</returns>
        public static bool SetupCurrentUser(User SCurrentUser, string Password)
        {
            return SetupCurrentUser(SCurrentUser, Password, true, true);
        }

        /// <summary>
        /// Проверка наличия заведённых в системе пользователей с указаной ролью
        /// </summary>
        /// <param name="FildRole">Роль которую нужно искать среди заведённых пользователей</param>
        /// <returns>Возвращает true если в системе заведён хоть оди пользователь с указанной ролью</returns>
        public static bool HashRoleUsers(RoleEn FildRole)
        {
            foreach (User item in List)
            {
                if (item.Role == FildRole) return true;
            }
            return false;
        }
        
        /// <summary>
        /// Блокировка пользователя 
        /// </summary>
        public static void LogOff()
        {
            try
            {
                CurrentUser = null;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при блокировке пользователя с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.LogOff", "UserFarm"), EventEn.Error);
                throw ae;
            }
        }
    }
}
