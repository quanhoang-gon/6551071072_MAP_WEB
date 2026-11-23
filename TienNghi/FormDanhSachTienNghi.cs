using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ClosedXML.Excel;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.TienNghi
{
    public partial class FormDanhSachTienNghi : Form
    {
        private readonly Image _iconTienNghi = Properties.Resources.TienNghi;
        private readonly Image _iconSua = Properties.Resources.edit;
        private readonly Image _iconXoa = Properties.Resources.delete;

        public FormDanhSachTienNghi()
        {
            InitializeComponent();
        }

        private void FormDanhSachTienNghi_Load(object sender, EventArgs e)
        {
            KhoiTaoBangDuLieu();
            LoadDanhSachTienNghi();
        }

        private void KhoiTaoBangDuLieu()
        {
            if (dtgDanhSachTienNghi.Columns.Count > 0)
            {
                return;
            }

            dtgDanhSachTienNghi.ReadOnly = true;
            dtgDanhSachTienNghi.AllowUserToAddRows = false;
            dtgDanhSachTienNghi.AllowUserToDeleteRows = false;
            dtgDanhSachTienNghi.MultiSelect = false;
            dtgDanhSachTienNghi.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtgDanhSachTienNghi.RowHeadersVisible = false;
            dtgDanhSachTienNghi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dtgDanhSachTienNghi.RowTemplate.Height = 40;

            dtgDanhSachTienNghi.Columns.Add(TaoCotAnh("colIcon", "", _iconTienNghi, 50));
            dtgDanhSachTienNghi.Columns.Add("MaTN", "Mã tiện nghi");
            dtgDanhSachTienNghi.Columns.Add("TenTN", "Tên tiện nghi");
            dtgDanhSachTienNghi.Columns.Add(TaoCotAnh("Sua", "Sửa", _iconSua, 50));
            dtgDanhSachTienNghi.Columns.Add(TaoCotAnh("Xoa", "Xóa", _iconXoa, 50));
        }

        private DataGridViewImageColumn TaoCotAnh(string name, string headerText, Image image, int width)
        {
            return new DataGridViewImageColumn
            {
                Name = name,
                HeaderText = headerText,
                Image = image,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = width,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        private void LoadDanhSachTienNghi(string tuKhoa = "")
        {
            try
            {
                using var db = new HotelManagementContext();

                var query = db.TienNghis
                    .AsNoTracking()
                    .Where(t => t.DaXoa != true);

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    tuKhoa = tuKhoa.Trim();
                    query = query.Where(t => t.MaTn.Contains(tuKhoa) || t.TenTn.Contains(tuKhoa));
                }

                var danhSach = query
                    .OrderBy(t => t.MaTn)
                    .Select(t => new
                    {
                        t.MaTn,
                        t.TenTn
                    })
                    .ToList();

                dtgDanhSachTienNghi.Rows.Clear();
                foreach (var item in danhSach)
                {
                    dtgDanhSachTienNghi.Rows.Add(_iconTienNghi, item.MaTn, item.TenTn, _iconSua, _iconXoa);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách tiện nghi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            LoadDanhSachTienNghi(txtTimKiem.Text.Trim());
        }

        private void btnXuatExcel_Click(object sender, EventArgs e)
        {
            if (dtgDanhSachTienNghi.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Xuất danh sách tiện nghi";
            saveDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
            saveDialog.FileName = "DanhSachTienNghi_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (saveDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                string[] tenCot = { "MaTN", "TenTN" };

                using (XLWorkbook workbook = new XLWorkbook())
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Add("Danh sách tiện nghi");

                    for (int j = 0; j < tenCot.Length; j++)
                    {
                        worksheet.Cell(1, j + 1).Value = dtgDanhSachTienNghi.Columns[tenCot[j]].HeaderText;
                    }
                    worksheet.Range(1, 1, 1, tenCot.Length).Style.Font.Bold = true;

                    for (int i = 0; i < dtgDanhSachTienNghi.Rows.Count; i++)
                    {
                        for (int j = 0; j < tenCot.Length; j++)
                        {
                            worksheet.Cell(i + 2, j + 1).Value = dtgDanhSachTienNghi.Rows[i].Cells[tenCot[j]].Value?.ToString() ?? "";
                        }
                    }

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(saveDialog.FileName);
                }

                DialogResult moFile = MessageBox.Show(
                    "Xuất dữ liệu thành công!\nBạn có muốn mở file vừa xuất không?",
                    "Thông báo",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (moFile == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName)
                    {
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất file Excel: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThemMoi_Click(object sender, EventArgs e)
        {
            using (FormThemTienNghi f = new FormThemTienNghi())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    LoadDanhSachTienNghi(txtTimKiem.Text.Trim());
                }
            }
        }

        private void dtgDanhSachTienNghi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string tenCot = dtgDanhSachTienNghi.Columns[e.ColumnIndex].Name;
            if (tenCot != "Sua" && tenCot != "Xoa")
            {
                return;
            }

            string maTN = dtgDanhSachTienNghi.Rows[e.RowIndex].Cells["MaTN"].Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(maTN))
            {
                return;
            }

            if (tenCot == "Sua")
            {
                using (FormSuaTienNghi f = new FormSuaTienNghi(maTN))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        LoadDanhSachTienNghi(txtTimKiem.Text.Trim());
                    }
                }
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa tiện nghi này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var tienNghi = db.TienNghis.FirstOrDefault(t => t.MaTn == maTN);
                if (tienNghi == null)
                {
                    MessageBox.Show("Không tìm thấy tiện nghi cần xóa!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    tienNghi.DaXoa = true;
                    int kq = db.SaveChanges();

                    if (kq > 0)
                    {
                        MessageBox.Show("Xóa thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa tiện nghi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            LoadDanhSachTienNghi(txtTimKiem.Text.Trim());
        }
    }
}
