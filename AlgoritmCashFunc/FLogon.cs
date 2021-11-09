using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc
{
    public partial class FLogon : Form
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public FLogon()
        {
            InitializeComponent();

            this.cmbBoxLogon.Items.Clear();
            foreach (User item in Com.UserFarm.List)
            {
                this.cmbBoxLogon.Items.Add(item.Logon);
            }
        }

        // Пользователь закрывает программу
        private void btnExit_Click(object sender, EventArgs e)
        {
            // Передаём команду закрытия программы
            this.DialogResult = DialogResult.OK;
        }

        // Пользователь авторизуется и хочет войти в систему
        private void btnLogon_Click(object sender, EventArgs e)
        {
            try
            {
                User CurUser;

                // Если админов в системе не заведено то нужно проверить логон Admin / 12345
                if (!Com.UserFarm.HashRoleUsers(Lib.RoleEn.Admin))
                {
                    // Даём возможность зайти под системной учётной записью, только если нет заведённых админов в системе
                    if (!Com.UserFarm.HashRoleUsers(Lib.RoleEn.Admin) && this.cmbBoxLogon.Text == "Admin" && this.txtBoxPassword.Text == "12345")
                    {
                        CurUser = new User("Admin", "12345", "Системная учётная запись.", RoleEn.Admin);
                        Com.UserFarm.List.Add(CurUser);
                    }
                }

                // Проверяем наличие выбранного пользователя если его нет, то выдаём ошибку
                CurUser = Com.UserFarm.List.GetUser(this.cmbBoxLogon.Text);

                // Нужно сохранить новый пароль
                if (string.IsNullOrWhiteSpace(CurUser.Password))
                {
                    if (string.IsNullOrWhiteSpace(this.txtBoxPassword.Text)) throw new ApplicationException("Вы должны задать пароль, выбранному пользователю.");
                    else Com.UserFarm.List.Update(new User(CurUser.Logon, this.txtBoxPassword.Text.Trim(), CurUser.Description, CurUser.Role));
                }

                // Пытаемся авторизоваться в системе подвыбранным пользователем и с введённым паролем
                Com.UserFarm.SetupCurrentUser(CurUser, this.txtBoxPassword.Text);

                // Закрываем форму
                this.DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Пользователь изменил логон под которым хотим авторизоваться
        private void cmbBoxLogon_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                User CurUser;

                // Проверяем наличие выбранного пользователя если его нет, то выдаём ошибку
                CurUser = Com.UserFarm.List.GetUser(this.cmbBoxLogon.Text);

                // Пароля нет нужно ввести пароль который будет у этого пользователя
                if(string.IsNullOrWhiteSpace(CurUser.Password))
                {
                    MessageBox.Show("У данного пользователя нет пароля введите. Система запомнит пароль и будет тербовать его при следующем входе.");
                    this.lblInfo.Visible = true;
                }
                else this.lblInfo.Visible = false;
            }
            catch (Exception){}
        }

        //Пользователь вводит пароль если он нажал энтер то нужно войти в систему
        private void txtBoxPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter) btnLogon_Click(null, null);
        }
    }
}
