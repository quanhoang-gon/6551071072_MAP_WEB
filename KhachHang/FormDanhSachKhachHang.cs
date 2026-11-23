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

namespace HotelManagement.KhachHang
{
    public partial class FormDanhSachKhachHang : Form
    {
        private readonly Image _iconKhachHang = Properties.Resources.KhachHang;
        private readonly Image _iconSua = Properties.Resources.edit;
        private readonly Image _iconXoa = Properties.Resources.delete;

        public FormDanhSachKhachHang()
        {
            InitializeComponent();
        }

        private void FormDanhSachKhachHang_Load(object sender, EventArgs e)
        {
            KhoiTaoBangDuLieu();
            LoadDanhSachKhachHang();
        }

        private void KhoiTaoBangDuLieu()
        {
            if (dtgDanhSachKhachHang.Columns.Count > 0)
            {
                return;
            }

            dtgDanhSachKhachHang.ReadOnly = true;
            dtgDanhSachKhachHang.AllowUserToAddRows = false;
            dtgDanhSachKhachHang.AllowUserToDeleteRows = false;
            dtgDanhSachKhachHang.MultiSelect = false;
            dtgDanhSachKhachHang.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtgDanhSachKhachHang.RowHeadersVisible = false;
            dtgDanhSachKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dtgDanhSachKhachHang.RowTemplate.Height = 40;

            dtgDanhSachKhachHang.Columns.Add(TaoCotAnh("colIcon", "", _iconKhachHang, 50));
            dtgDanhSachKhachHang.Columns.Add("MaKH", "Mã khách hàng");
            dtgDanhSachKhachHang.Columns.Add("TenKH", "Họ và tên");
            dtgDanhSachKhachHang.Columns.Add("CCCD", "CCCD/Passport");
            dtgDanhSachKhachHang.Columns.Add("SDT", "Số điện thoại");
            dtgDanhSachKhachHang.Columns.Add("QuocTich", "Quốc tịch");
            dtgDanhSachKhachHang.Columns.Add("GioiTinh", "Giới tính");
            dtgDanhSachKhachHang.Columns.Add(TaoCotAnh("Sua", "Sửa", _iconSua, 50));
            dtgDanhSachKhachHang.Columns.Add(TaoCotAnh("Xoa", "Xóa", _iconXoa, 50));
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

        private void LoadDanhSachKhachHang(string tuKhoa = "")
        {
            try
            {
                using var db = new HotelManagementContext();

                var query = db.KhachHangs
                    .AsNoTracking()
                    .Where(k => k.DaXoa != true);

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    tuKhoa = tuKhoa.Trim();
                    query = query.Where(k =>
                        k.MaKh.Contains(tuKhoa) ||
                        k.TenKh.Contains(tuKhoa) ||
                        k.CccdPassport.Contains(tuKhoa) ||
                        (k.Sdt != null && k.Sdt.Contains(tuKhoa)));
                }

                var danhSach = query
                    .OrderBy(k => k.MaKh)
                    .Select(k => new
                    {
                        k.MaKh,
                        k.TenKh,
                        k.CccdPassport,
                        k.Sdt,
                        k.QuocTich,
                        k.GioiTinh
                    })
                    .ToList();

                dtgDanhSachKhachHang.Rows.Clear();
                foreach (var item in danhSach)
                {
                    dtgDanhSachKhachHang.Rows.Add(
                        _iconKhachHang,
                        item.MaKh,
                        item.TenKh,
                        item.CccdPassport,
                        item.Sdt,
                        item.QuocTich,
                        item.GioiTinh,
                        _iconSua,
                        _iconXoa);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách khách hàng: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            LoadDanhSachKhachHang(txtTimKiem.Text.Trim());
        }

        private void btnXuatExcel_Click(object sender, EventArgs e)
        {
            if (dtgDanhSachKhachHang.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Xuất danh sách khách hàng";
            saveDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
            saveDialog.FileName = "DanhSachKhachHang_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (saveDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                string[] tenCot = { "MaKH", "TenKH", "CCCD", "SDT", "QuocTich", "GioiTinh" };

                using (XLWorkbook workbook = new XLWorkbook())
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Add("Danh sách khách hàng");

                    for (int j = 0; j < tenCot.Length; j++)
                    {
                        worksheet.Cell(1, j + 1).Value = dtgDanhSachKhachHang.Columns[tenCot[j]].HeaderText;
                    }
                    worksheet.Range(1, 1, 1, tenCot.Length).Style.Font.Bold = true;

                    for (int i = 0; i < dtgDanhSachKhachHang.Rows.Count; i++)
                    {
                        for (int j = 0; j < tenCot.Length; j++)
                        {
                            worksheet.Cell(i + 2, j + 1).Value = dtgDanhSachKhachHang.Rows[i].Cells[tenCot[j]].Value?.ToString() ?? "";
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
            using (FormThemKhachHang f = new FormThemKhachHang())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    LoadDanhSachKhachHang(txtTimKiem.Text.Trim());
                }
            }
        }

        private void dtgDanhSachKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string tenCot = dtgDanhSachKhachHang.Columns[e.ColumnIndex].Name;
            if (tenCot != "Sua" && tenCot != "Xoa")
            {
                return;
            }

            string maKH = dtgDanhSachKhachHang.Rows[e.RowIndex].Cells["MaKH"].Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(maKH))
            {
                return;
            }

            if (tenCot == "Sua")
            {
                using (FormSuaKhachHang f = new FormSuaKhachHang(maKH))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        LoadDanhSachKhachHang(txtTimKiem.Text.Trim());
                    }
                }
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa khách hàng này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var khachHang = db.KhachHangs.FirstOrDefault(k => k.MaKh == maKH);
                if (khachHang == null)
                {
                    MessageBox.Show("Không tìm thấy khách hàng cần xóa!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    khachHang.DaXoa = true;
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
                MessageBox.Show("Lỗi khi xóa khách hàng: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            LoadDanhSachKhachHang(txtTimKiem.Text.Trim());
        }
    }
}
