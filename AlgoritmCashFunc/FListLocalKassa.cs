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
using AlgoritmCashFunc.BLL.LocalPlg;

namespace AlgoritmCashFunc
{
    public partial class FListLocalKassa : Form
    {
        /// <summary>
        /// Текущая касса
        /// </summary>
        LocalKassa CurKassa = Com.LocalFarm.CurLocalDepartament;

        public FListLocalKassa()
        {
            try
            {
                InitializeComponent();
                this.lblKassaName.Text = this.lblKassaName.Tag.ToString().Replace(@"@KassaName", CurKassa.HostName);
                this.txtBoxOrganization.Text = CurKassa.Organization;
                this.txtBoxStructPodrazdelenie.Text = CurKassa.StructPodrazdelenie;
                this.txtBoxOKPO.Text = CurKassa.OKPO;
                this.txtBoxINN.Text = CurKassa.INN;
                this.txtBoxGlavBuhFio.Text = CurKassa.GlavBuhFio;
                this.txtBoxDolRukOrg.Text = CurKassa.DolRukOrg;
                this.txtBoxRukFio.Text = CurKassa.RukFio;
                this.txtBoxZavDivisionFio.Text = CurKassa.ZavDivisionFio;
                this.txtBoxCompanyCode.Text = CurKassa.CompanyCode;
                this.txtBoxStoreCode.Text = CurKassa.StoreCode;
                this.txtBoxUpload1CDir.Text = CurKassa.Upload1CDir;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при загрузке формы FListLocalKassa с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        // Сохранение
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                CurKassa.Organization = this.txtBoxOrganization.Text;
                CurKassa.StructPodrazdelenie = this.txtBoxStructPodrazdelenie.Text;
                CurKassa.OKPO = this.txtBoxOKPO.Text;
                CurKassa.INN = this.txtBoxINN.Text;
                CurKassa.GlavBuhFio = this.txtBoxGlavBuhFio.Text;
                CurKassa.DolRukOrg = this.txtBoxDolRukOrg.Text;
                CurKassa.RukFio = this.txtBoxRukFio.Text;
                CurKassa.ZavDivisionFio = this.txtBoxZavDivisionFio.Text;
                CurKassa.CompanyCode = this.txtBoxCompanyCode.Text;
                CurKassa.StoreCode = this.txtBoxStoreCode.Text;
                CurKassa.Upload1CDir = this.txtBoxUpload1CDir.Text;
                this.CurKassa.Save();
                this.Close();
            }
            catch (Exception ex)
            {
                Com.Log.EventSave(string.Format(@"Ошибка в методе {0}:""{1}""", "btnSave_Click", ex.Message), this.GetType().FullName, EventEn.Error, true, true);
            }
        }
    }
}
