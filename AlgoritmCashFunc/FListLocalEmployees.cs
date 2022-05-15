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
    public partial class FListLocalEmployees : Form
    {
        DataTable dtData = null;
        DataView dvData = null;

        /// <summary>
        /// Конструктор
        /// </summary>
        public FListLocalEmployees()
        {
            try
            {
                InitializeComponent();

                // Наполняем таблицу данными и подключаем к гриду
                if (this.dtData == null)
                {
                    this.dtData = new DataTable();
                    this.dtData.Columns.Add(new DataColumn("CId", typeof(int)));
                    this.dtData.Columns.Add(new DataColumn("ColData", typeof(string)));

                    foreach (LocalEmployees item in Com.LocalFarm.CurLocalEmployees)
                    {
                        if (!item.IsDraft)
                        {
                            DataRow nRow = dtData.NewRow();
                            if (item.Id != null) nRow["CId"] = (int)item.Id;
                            nRow["ColData"] = item.LocalName;
                            this.dtData.Rows.Add(nRow);
                        }
                    }
                }
                this.dvData = new DataView(dtData);
                this.dgData.DataSource = this.dvData;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при загрузке формы FListLocalEmployees с ошибкой: ({0})", ex.Message));
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
                foreach (LocalEmployees item in LocalFarm.CurLocalEmployees)
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
                        foreach (Local item in LocalFarm.CurLocalEmployees)
                        {
                            if (item.Id == int.Parse(this.dtData.Rows[i]["CId"].ToString())) updLocal = item;
                        }
                    }

                    // Проверка на уникальность
                    string TmpColData = this.dtData.Rows[i]["ColData"].ToString();
                    bool Unic = true;
                    foreach (Local item in LocalFarm.CurLocalEmployees)
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
                        if (updLocal.LocalName != TmpColData)
                        {
                            updLocal.LocalName = TmpColData;
                            updLocal.Save();
                        }
                    }
                    else
                    {
                        Local newLocal = LocalFarm.CreateNewLocal("LocalEmployees");
                        newLocal.LocalName = TmpColData;
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

        // Пользователь удаляет строку
        private void tlStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            try
            {
                int index = -1;
                if (this.cntxMenuStrip.Tag != null) index = (int)this.cntxMenuStrip.Tag;
                if (index>=0) this.dgData.Rows.RemoveAt(index);
            }
            catch (Exception ex)
            {
                Com.Log.EventSave(string.Format(@"Ошибка в методе {0}:""{1}""", "tlStripMenuItemDelete_Click", ex.Message), this.GetType().FullName, EventEn.Error, true, true);
            }
        }

        // Попали в ячейку
        private void dgData_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                this.cntxMenuStrip.Tag = e.RowIndex;
            }
            catch (Exception ex)
            {
                Com.Log.EventSave(string.Format(@"Ошибка в методе {0}:""{1}""", "dgData_CellEnter", ex.Message), this.GetType().FullName, EventEn.Error, true, true);
            }
        }
    }
}
