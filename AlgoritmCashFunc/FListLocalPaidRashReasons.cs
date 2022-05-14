using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data;
using AlgoritmCashFunc.Lib;
using AlgoritmCashFunc.Com;
using AlgoritmCashFunc.BLL.LocalPlg;
using AlgoritmCashFunc.BLL;

namespace AlgoritmCashFunc
{
    public partial class FListLocalPaidRashReasons : Form
    {
        DataTable dtData = null;
        DataView dvData = null;

        /// <summary>
        /// Конструктор
        /// </summary>
        public FListLocalPaidRashReasons()
        {
            try
            {
                InitializeComponent();

                // Наполняем таблицу данными и подключаем к гриду
                if (this.dtData == null)
                {
                    this.dtData = new DataTable();
                    this.dtData.Columns.Add(new DataColumn("CId", typeof(int)));
                    this.dtData.Columns.Add(new DataColumn("LocalName", typeof(string)));
                    this.dtData.Columns.Add(new DataColumn("Osnovanie", typeof(string)));
                    this.dtData.Columns.Add(new DataColumn("KreditNomerSchet", typeof(string)));
                    this.dtData.Columns.Add(new DataColumn("DebetKorSchet", typeof(string)));
                    this.dtData.Columns.Add(new DataColumn("FlagFormReturn", typeof(bool)));

                    foreach (LocalPaidRashReasons item in Com.LocalFarm.CurLocalPaidRashReasons)
                    {
                        if (!item.IsDraft)
                        {
                            DataRow nRow = dtData.NewRow();
                            if (item.Id != null) nRow["CId"] = (int)item.Id;
                            nRow["LocalName"] = item.LocalName;
                            nRow["Osnovanie"] = item.Osnovanie;
                            nRow["KreditNomerSchet"] = item.KreditNomerSchet;
                            nRow["DebetKorSchet"] = item.DebetKorSchet;
                            nRow["FlagFormReturn"] = item.FlagFormReturn;
                            this.dtData.Rows.Add(nRow);
                        }
                    }
                }
                this.dvData = new DataView(dtData);
                this.dgData.DataSource = this.dvData;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при загрузке формы FListLocalPaidInReasons с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, GetType().Name, EventEn.Error);
                throw ae;
            }
        }

        // Пользователь нажал на сохранить
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Находим что нужно удалить
                foreach (LocalPaidRashReasons item in LocalFarm.CurLocalPaidRashReasons)
                {
                    bool IsDelete = true;
                    for (int i = 0; i < this.dtData.Rows.Count; i++)
                    {
                        if (item.Id != null
                            && !string.IsNullOrWhiteSpace(this.dtData.Rows[i]["CId"].ToString())
                            && ((int)item.Id).ToString() == this.dtData.Rows[i]["CId"].ToString())
                        {
                            IsDelete = false;
                            break;
                        }
                    }

                    if (IsDelete)
                    {
                        item.Deleted();
                    }
                }

                // Обновляем эелементы и вставляем новые
                for (int i = 0; i < this.dtData.Rows.Count; i++)
                {
                    // Находим элемент по идентификатору в базе
                    Local updLocal = null;
                    if (!string.IsNullOrWhiteSpace(this.dtData.Rows[i]["CId"].ToString()))
                    {
                        foreach (Local item in LocalFarm.CurLocalPaidRashReasons)
                        {
                            if (item.Id == int.Parse(this.dtData.Rows[i]["CId"].ToString())) updLocal = item;
                        }
                    }

                    // Проверка на уникальность
                    string TmpColData = this.dtData.Rows[i]["LocalName"].ToString();
                    string TmpOsnovanie = this.dtData.Rows[i]["Osnovanie"].ToString();
                    string TmpKreditNomerSchet = this.dtData.Rows[i]["KreditNomerSchet"].ToString();
                    string TmpDebetKorSchet = this.dtData.Rows[i]["DebetKorSchet"].ToString();
                    bool TmpFlagFormReturn = bool.Parse(this.dtData.Rows[i]["FlagFormReturn"].ToString());
                    bool Unic = true;
                    foreach (Local item in LocalFarm.CurLocalPaidRashReasons)
                    {
                        if (item.LocalName == TmpColData)
                        {
                            if ((updLocal != null && (int)updLocal.Id != item.Id)
                                || updLocal == null)
                                Unic = false;
                        }
                    }
                    if (!Unic) throw new ApplicationException(string.Format("Значение {0} не является уникальным", TmpColData));

                    // Обновление
                    if (updLocal != null)
                    {
                        if (updLocal.LocalName != TmpColData
                            || ((LocalPaidRashReasons)updLocal).Osnovanie != TmpOsnovanie
                            || ((LocalPaidRashReasons)updLocal).KreditNomerSchet != TmpKreditNomerSchet
                            || ((LocalPaidRashReasons)updLocal).DebetKorSchet != TmpDebetKorSchet
                            || ((LocalPaidRashReasons)updLocal).FlagFormReturn != TmpFlagFormReturn)
                        {
                            updLocal.LocalName = TmpColData;
                            ((LocalPaidRashReasons)updLocal).Osnovanie = TmpOsnovanie;
                            ((LocalPaidRashReasons)updLocal).KreditNomerSchet = TmpKreditNomerSchet;
                            ((LocalPaidRashReasons)updLocal).DebetKorSchet = TmpDebetKorSchet;
                            ((LocalPaidRashReasons)updLocal).FlagFormReturn = TmpFlagFormReturn;
                            updLocal.Save();
                        }
                    }
                    else
                    {
                        Local newLocal = LocalFarm.CreateNewLocal("LocalPaidRashReasons");
                        newLocal.LocalName = TmpColData;
                        ((LocalPaidRashReasons)newLocal).Osnovanie = TmpOsnovanie;
                        ((LocalPaidRashReasons)newLocal).KreditNomerSchet = TmpKreditNomerSchet;
                        ((LocalPaidRashReasons)newLocal).DebetKorSchet = TmpDebetKorSchet;
                        ((LocalPaidRashReasons)newLocal).FlagFormReturn = TmpFlagFormReturn;
                        newLocal.Save();
                    }
                }

                Com.LocalFarm.UpdateLocalListFromDB();

                this.Close();
            }
            catch (Exception ex)
            {
                Com.Log.EventSave(string.Format(@"Ошибка в методе {0}:""{1}""", "btnSave_Click", ex.Message), this.GetType().FullName, EventEn.Error, true, true);
            }
        }
    }
}
