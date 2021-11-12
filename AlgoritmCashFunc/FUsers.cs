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
using AlgoritmCashFunc.Com;

namespace AlgoritmCashFunc
{
    public partial class FUsers : Form
    {
        private DataTable dtLogon = new System.Data.DataTable("dtLogon");
        private DataView dvLogon;

        private int selectedLogon = -1;

        /// <summary>
        /// Конструктор
        /// </summary>
        public FUsers()
        {
            InitializeComponent();

            //Инициируем механизм который проверяет необходимость блокировки пользователя
            UserFarm.ActiveStatusLogon();
            UserFarm.onEventLogOFF += UserFarm_onEventLogOFF;

            // Наполняем таблицу данными и подключаем к гриду
            this.dtLogon.Columns.Add(new DataColumn("Logon", typeof(string)));
            foreach (User item in Com.UserFarm.List)
            {
                DataRow nrow = this.dtLogon.NewRow();
                nrow["Logon"] = item.Logon;
                this.dtLogon.Rows.Add(nrow);
            }

            this.cmbBoxRoleEdit.Items.Clear();
            int iindex = -1;
            int fo = -1;
            int fa = -1;
            foreach (Lib.RoleEn item in Enum.GetValues(typeof(Lib.RoleEn)))
            {
                if (item != RoleEn.None)
                {
                    iindex++;
                    this.cmbBoxRoleEdit.Items.Add(item.ToString());
                    if (item == RoleEn.Operator) fo = iindex;
                    if (item == RoleEn.Admin) fa = iindex;
                }
            }
            if (fo != -1 && this.cmbBoxRoleEdit.Items.Count > fo && Com.UserFarm.List.Count > 0) this.cmbBoxRoleEdit.SelectedIndex = fo;
            if (fa != -1 && this.cmbBoxRoleEdit.Items.Count > fa && Com.UserFarm.List.Count >= 0 && UserFarm.CurrentUser.Role == RoleEn.Admin) this.cmbBoxRoleEdit.SelectedIndex = fa;
            if (fa != -1 && this.cmbBoxRoleEdit.Items.Count > fa && Com.UserFarm.List.Count == 0) this.cmbBoxRoleEdit.SelectedIndex = fa;

            this.dvLogon = new DataView(dtLogon);
            this.dGViewLogon.DataSource = this.dvLogon;

            // Разрешаем добавлять пользователей только если мы админы
            if (Com.UserFarm.CurrentUser.Role == RoleEn.Admin || Com.UserFarm.CurrentUser.Role == RoleEn.Manager) this.TSMItemAddUser.Visible = true;
            else this.TSMItemAddUser.Visible = false;
        }

        #region Основные методы которые нужны для блокировки пользователя если он не активен
        /// <summary>
        ///  Произошло событие блокировки пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserFarm_onEventLogOFF(object sender, EventLogOFF e)
        {
            try
            {
                UserFarm.onEventLogOFF -= UserFarm_onEventLogOFF;
                this.Close();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при выходе пользователя с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.UserFarm_onEventLogOFF", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        // Всовываем на всех ледементах чтобы смотерть активность пользователя на движение мышки
        private void ActiveStatusLogon_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                UserFarm.ActiveStatusLogon();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при регистрации активности пользователя с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.ActiveStatusLogon_MouseMove", GetType().Name), EventEn.Error);
                throw ae;
            }
        }

        #endregion

        // Чтение формы
        private void FUsers_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < this.dGViewLogon.Rows.Count; i++)
            {
                if (Com.UserFarm.CurrentUser != null)
                {
                    if (this.dGViewLogon.Rows[i].Cells["Logon"].Value.ToString() == Com.UserFarm.CurrentUser.Logon)
                    {
                        this.dGViewLogon.Rows[i].Cells["Logon"].Selected = true;
                        this.selectedLogon = i;
                    }
                    else this.dGViewLogon.Rows[i].Cells["Logon"].Selected = false;
                }
            }
            this.myLoadpnlFill();
        }

        // Пользователь выделил какого-то пользователя
        private void dGViewLogon_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Если мы не выделили конкретного пользователя то ничего не делаем
            if (e.ColumnIndex == -1 || e.RowIndex == -1)
            {
                this.selectedLogon = -1;
                return;
            }
            else this.selectedLogon = e.RowIndex;

            this.myLoadpnlFill();
        }

        // Необходимо перерисовать панель элементов относительно выбранного в переменной selectedLogon
        private void myLoadpnlFill()
        {
            try
            {
                User editUser = null;
                try { editUser = Com.UserFarm.List.GetUser(dGViewLogon.Rows[this.selectedLogon].Cells["Logon"].Value.ToString()); }
                catch (Exception) { }

                if (editUser == null)
                {
                    this.lblLogonEdit.Text = "Создание пользователя.";
                    this.txtBoxLogonEdit.Text = "";
                    this.txtBoxLogonEdit.ReadOnly = false;
                    this.txtBoxPasswordEdit.Text = "";
                    this.txtBoxDescriptionEdit.Text = "";
                    this.btnDelete.Visible = false;
                    this.btnSave.Text = "Добавить.";
                }
                else
                {
                    this.lblLogonEdit.Text = string.Format("Редактирование пользователя: {0}", editUser.Logon);
                    this.txtBoxLogonEdit.Text = editUser.Logon;
                    this.txtBoxLogonEdit.ReadOnly = true;
                    this.txtBoxPasswordEdit.Text = editUser.Password;
                    this.txtBoxDescriptionEdit.Text = editUser.Description;
                    this.btnDelete.Visible = true;
                    this.btnSave.Text = "Изменить.";

                    int fCurR = -1;
                    for (int i = 0; i < this.cmbBoxRoleEdit.Items.Count; i++)
                    {
                        if (editUser.Role.ToString() == this.cmbBoxRoleEdit.Items[i].ToString()) fCurR = i;
                    }
                    if (fCurR > -1) this.cmbBoxRoleEdit.SelectedIndex = fCurR;

                    if (Com.UserFarm.List.Count == 0) this.txtBoxPasswordEdit.ReadOnly = false;
                    else this.txtBoxPasswordEdit.ReadOnly = true;
                }

                // Пользователь редактирует свой пароль или он является админом
                if ((editUser != null && Com.UserFarm.CurrentUser.Logon == editUser.Logon) 
                    || Com.UserFarm.CurrentUser.Role == RoleEn.Admin 
                    || (Com.UserFarm.CurrentUser.Role == RoleEn.Manager && editUser.Role != RoleEn.Admin))
                {
                    this.txtBoxPasswordEdit.ReadOnly = false;
                    this.cmbBoxRoleEdit.Enabled = true;
                    this.txtBoxDescriptionEdit.ReadOnly = false;
                }
                else
                {
                    this.txtBoxPasswordEdit.ReadOnly = true;
                    this.cmbBoxRoleEdit.Enabled = false;
                    this.txtBoxDescriptionEdit.ReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                string ErrMessage = string.Format("Возникла ошибка в интерфейсе FUsers: {0}", ex.Message);
                Com.Log.EventSave(ErrMessage, GetType().Name, EventEn.Error);
                MessageBox.Show(ErrMessage);
            }

        }

        // Хотят отредактировать добавление нового пользователя
        private void TSMItemAddUser_Click(object sender, EventArgs e)
        {
            this.selectedLogon = -1;
            this.myLoadpnlFill();
        }

        // Пользователь удаляет пользователя
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                Com.UserFarm.List.Remove(Com.UserFarm.List.GetUser(this.txtBoxLogonEdit.Text));
                this.dGViewLogon.Rows.RemoveAt(this.selectedLogon);
                this.selectedLogon = -1;
                this.myLoadpnlFill();
            }
            catch (Exception ex)
            {
                Com.Log.EventSave(ex.Message, GetType().Name + "btnDelete_Click", EventEn.Error, true, true);
            }
        }

        // Плользовватель сохраняет или менят информацию о пользователе
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                User editUser = null;
                try { editUser = Com.UserFarm.List.GetUser(dGViewLogon.Rows[this.selectedLogon].Cells["Logon"].Value.ToString()); }
                catch (Exception) { }

                User nUser = new User(this.txtBoxLogonEdit.Text, this.txtBoxPasswordEdit.Text, this.txtBoxDescriptionEdit.Text, Lib.EventConvertor.Convert(this.cmbBoxRoleEdit.Text, RoleEn.Operator));

                //Добавление нового пользователя
                if (editUser == null)
                {
                    if (!string.IsNullOrWhiteSpace(this.txtBoxLogonEdit.Text) && this.txtBoxLogonEdit.Text == "Console") throw new ApplicationException("Нельзя создать полязователя с именем: Console это имя зарезервировано.");

                    Com.UserFarm.List.Add(nUser);
                    DataRow nrow = this.dtLogon.NewRow();
                    nrow["Logon"] = this.txtBoxLogonEdit.Text;
                    this.dtLogon.Rows.Add(nrow);

                    // Подкрашиваем редактируемый логон
                    for (int i = 0; i < this.dGViewLogon.Rows.Count; i++)
                    {
                        if (this.dGViewLogon.Rows[i].Cells["Logon"].Value.ToString() == this.txtBoxLogonEdit.Text) { this.dGViewLogon.Rows[i].Cells["Logon"].Selected = true; }
                        else this.dGViewLogon.Rows[i].Cells["Logon"].Selected = false;
                    }
                    //this.dGViewLogon.Rows[this.dGViewLogon.RowCount-1].Cells["Logon"].Selected=true;

                    this.selectedLogon = this.dGViewLogon.RowCount - 1;
                    this.myLoadpnlFill();
                }
                else  // Редактирование пользователя
                {
                    Com.UserFarm.List.Update(nUser);
                }
            }
            catch (Exception ex)
            {
                Com.Log.EventSave(ex.Message, GetType().Name + "btnSave_Click", EventEn.Error, true, true);
            }
        }
    }
}
