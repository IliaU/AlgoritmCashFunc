using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib.LibBase;

namespace AlgoritmCashFunc.Lib
{
    public class User : UserBase
    {
        /// <summary>
        /// Идентификатор кассира в базе двнных по умолчанию
        /// </summary>
        public int? EmploeeId = null;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Logon">Имя пользователя</param>
        /// <param name="Password">Пароль пользователя</param>
        /// <param name="Description">Описание пользователя</param>
        /// <param name="Role">Роль пользователя</param>
        /// <param name="EmploeeId">Идентификатор кассира в базе двнных по умолчанию</param>
        public User(string Logon, string Password, string Description, RoleEn Role, int? EmploeeId) : base(Logon)
        {
            base.Password = Password;
            base.Description = Description;
            base.Role = Role;
            this.EmploeeId = EmploeeId;

            //base.InitialUser(Logon);
        }

    }
}
