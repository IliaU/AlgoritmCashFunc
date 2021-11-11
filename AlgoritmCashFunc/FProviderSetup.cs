using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AlgoritmCashFunc.Com;
using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc
{
    public partial class FProviderSetup : Form
    {
        // Список типов провайдер
        private List<string> cmbBoxRepTypList = new List<string>();

        // Текущий провайдер
        private Lib.UProvider CurrentPrv;

        // Конструктор
        public FProviderSetup()
        {
            InitializeComponent();

            //Инициируем механизм который проверяет необходимость блокировки пользователя
            UserFarm.ActiveStatusLogon();
            UserFarm.onEventLogOFF += UserFarm_onEventLogOFF;

            // Подгружаем список возможных провайдеров
            this.cmbBoxRepTyp.Items.Clear();
            cmbBoxRepTypList = Com.ProviderFarm.ListProviderName();
            foreach (string item in cmbBoxRepTypList)
            {
                this.cmbBoxRepTyp.Items.Add(item);
            }

            // Если всего один тип провайдеров существует то устанавливаем по умолчанию этот тип
            if (this.cmbBoxRepTyp.Items.Count == 1) this.cmbBoxRepTyp.SelectedIndex = 0;
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
                this.Close();
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при чтении конфигурации с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, string.Format("{0}.FStart_FormClosing", GetType().Name), EventEn.Error);
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
        private void FRepositorySetup_Load(object sender, EventArgs e)
        {
            // Получаем текущий провайдер
            this.CurrentPrv = Com.ProviderFarm.GetUprovider();

            // Если текущий провайдер есть и он не выбран то нужно указать его тип
            if (this.CurrentPrv != null)
            {
                for (int i = 0; i < this.cmbBoxRepTyp.Items.Count; i++)
                {
                    if (this.cmbBoxRepTyp.Items[i].ToString() == this.CurrentPrv.PrvInType) this.cmbBoxRepTyp.SelectedIndex = i;
                }
            }

            //  Если текущий провайдер не установлен то на выход
            if (this.CurrentPrv == null) return;
            this.txtBoxConnectionString.Text = this.CurrentPrv.PrintConnectionString();
        }

        // Пользователь решил изменить 
        private void btnConfig_Click(object sender, EventArgs e)
        {
            if (this.cmbBoxRepTyp.SelectedIndex == -1)
            {
                MessageBox.Show("Вы не выбрали тип провайдера который вы будите использовать.");
                return;
            }

            // Создаём ссылку на подключение которое будем править
            Lib.UProvider RepTmp = null;
            //
            // Если текущий провайдер не установлен то иницилизируем его новый экземпляр или создаём его на основе уже существующего провайдера
            if (this.CurrentPrv == null || (this.CurrentPrv != null && this.CurrentPrv.PrvInType != this.cmbBoxRepTyp.Items[this.cmbBoxRepTyp.SelectedIndex].ToString()))
            {
                RepTmp = new Lib.UProvider(this.cmbBoxRepTyp.Items[this.cmbBoxRepTyp.SelectedIndex].ToString());
            }
            else RepTmp = new Lib.UProvider(this.CurrentPrv.PrvInType, this.CurrentPrv.ConnectionString);
            // 
            // Запускаем правку нового подключения
            bool HashSaveRepository = RepTmp.SetupConnectDB();

            // Пользователь сохраняет данный провайдер в качестве текущего
            if (HashSaveRepository)
            {
                Com.ProviderFarm.Setup(RepTmp);
            }

            // Перечитываем текущую форму
            FRepositorySetup_Load(null, null);
        }
    }
}
