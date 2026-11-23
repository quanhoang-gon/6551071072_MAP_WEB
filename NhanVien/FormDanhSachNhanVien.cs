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

namespace HotelManagement.NhanVien
{
    public partial class FormDanhSachNhanVien : Form
    {
        private readonly Image _iconNhanVien = Properties.Resources.NhanVien;
        private readonly Image _iconSua = Properties.Resources.edit;
        private readonly Image _iconXoa = Properties.Resources.delete;

        public FormDanhSachNhanVien()
        {
            InitializeComponent();
        }

        private void FormDanhSachNhanVien_Load(object sender, EventArgs e)
        {
            KhoiTaoBangDuLieu();
            LoadDanhSachNhanVien();
        }

        private void KhoiTaoBangDuLieu()
        {
            if (dtgDanhSachNhanVien.Columns.Count > 0)
            {
                return;
            }

            dtgDanhSachNhanVien.ReadOnly = true;
            dtgDanhSachNhanVien.AllowUserToAddRows = false;
            dtgDanhSachNhanVien.AllowUserToDeleteRows = false;
            dtgDanhSachNhanVien.MultiSelect = false;
            dtgDanhSachNhanVien.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtgDanhSachNhanVien.RowHeadersVisible = false;
            dtgDanhSachNhanVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dtgDanhSachNhanVien.RowTemplate.Height = 40;

            dtgDanhSachNhanVien.Columns.Add(TaoCotAnh("colIcon", "", _iconNhanVien, 50));
            dtgDanhSachNhanVien.Columns.Add("MaNV", "Mã nhân viên");
            dtgDanhSachNhanVien.Columns.Add("TenNV", "Họ và tên");
            dtgDanhSachNhanVien.Columns.Add("ChucVu", "Chức vụ");
            dtgDanhSachNhanVien.Columns.Add("Luong", "Lương");
            dtgDanhSachNhanVien.Columns.Add("SDT", "Số điện thoại");
            dtgDanhSachNhanVien.Columns.Add("Email", "Email");
            dtgDanhSachNhanVien.Columns.Add("DiaChi", "Địa chỉ");

            dtgDanhSachNhanVien.Columns["Luong"]!.DefaultCellStyle.Format = "#,##0' đ'";
            dtgDanhSachNhanVien.Columns["Luong"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dtgDanhSachNhanVien.Columns.Add(TaoCotAnh("Sua", "Sửa", _iconSua, 50));
            dtgDanhSachNhanVien.Columns.Add(TaoCotAnh("Xoa", "Xóa", _iconXoa, 50));
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

        private void LoadDanhSachNhanVien(string tuKhoa = "")
        {
            try
            {
                using var db = new HotelManagementContext();

                var query = db.NhanViens
                    .AsNoTracking()
                    .Where(n => n.DaXoa != true);

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    tuKhoa = tuKhoa.Trim();
                    query = query.Where(n =>
                        n.MaNv.Contains(tuKhoa) ||
                        n.TenNv.Contains(tuKhoa) ||
                        n.Sdt.Contains(tuKhoa) ||
                        n.Cccd.Contains(tuKhoa) ||
                        (n.Email != null && n.Email.Contains(tuKhoa)));
                }

                var danhSach = query
                    .OrderBy(n => n.MaNv)
                    .Select(n => new
                    {
                        n.MaNv,
                        n.TenNv,
                        n.ChucVu,
                        n.Luong,
                        n.Sdt,
                        n.Email,
                        n.DiaChi
                    })
                    .ToList();

                dtgDanhSachNhanVien.Rows.Clear();
                foreach (var item in danhSach)
                {
                    dtgDanhSachNhanVien.Rows.Add(
                        _iconNhanVien,
                        item.MaNv,
                        item.TenNv,
                        item.ChucVu,
                        item.Luong,
                        item.Sdt,
                        item.Email ?? "",
                        item.DiaChi,
                        _iconSua,
                        _iconXoa);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách nhân viên: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            LoadDanhSachNhanVien(txtTimKiem.Text.Trim());
        }

        private void dtgDanhSachNhanVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string tenCot = dtgDanhSachNhanVien.Columns[e.ColumnIndex].Name;
            if (tenCot != "Sua" && tenCot != "Xoa")
            {
                return;
            }

            string maNV = dtgDanhSachNhanVien.Rows[e.RowIndex].Cells["MaNV"].Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(maNV))
            {
                return;
            }

            if (tenCot == "Sua")
            {
                using (FormSuaNhanVien f = new FormSuaNhanVien(maNV))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        LoadDanhSachNhanVien(txtTimKiem.Text.Trim());
                    }
                }
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa nhân viên này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var nhanVien = db.NhanViens.FirstOrDefault(n => n.MaNv == maNV);
                if (nhanVien == null)
                {
                    MessageBox.Show("Không tìm thấy nhân viên cần xóa!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    nhanVien.DaXoa = true;
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
                MessageBox.Show("Lỗi khi xóa nhân viên: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            LoadDanhSachNhanVien(txtTimKiem.Text.Trim());
        }

        private void btnXuatExcel_Click(object sender, EventArgs e)
        {
            if (dtgDanhSachNhanVien.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Xuất danh sách nhân viên";
            saveDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
            saveDialog.FileName = "DanhSachNhanVien_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (saveDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                string[] tenCot = { "MaNV", "TenNV", "ChucVu", "Luong", "SDT", "Email", "DiaChi" };

                using (XLWorkbook workbook = new XLWorkbook())
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Add("Danh sách nhân viên");

                    for (int j = 0; j < tenCot.Length; j++)
                    {
                        worksheet.Cell(1, j + 1).Value = dtgDanhSachNhanVien.Columns[tenCot[j]].HeaderText;
                    }
                    worksheet.Range(1, 1, 1, tenCot.Length).Style.Font.Bold = true;

                    for (int i = 0; i < dtgDanhSachNhanVien.Rows.Count; i++)
                    {
                        for (int j = 0; j < tenCot.Length; j++)
                        {
                            worksheet.Cell(i + 2, j + 1).Value = dtgDanhSachNhanVien.Rows[i].Cells[tenCot[j]].Value?.ToString() ?? "";
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
            using (FormThemNhanVien f = new FormThemNhanVien())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    LoadDanhSachNhanVien(txtTimKiem.Text.Trim());
                }
            }
        }
    }
}
