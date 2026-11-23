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

namespace HotelManagement.DichVu
{
    public partial class FormDanhSachDichVu : Form
    {
        private readonly Image _iconDichVu = Properties.Resources.DichVu;
        private readonly Image _iconSua = Properties.Resources.edit;
        private readonly Image _iconXoa = Properties.Resources.delete;

        public FormDanhSachDichVu()
        {
            InitializeComponent();
            Load += FormDanhSachDichVu_Load;
        }

        private void FormDanhSachDichVu_Load(object? sender, EventArgs e)
        {
            CauHinhLuoi();
            LoadDanhSachDichVu();
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            LocTheoTuKhoa();
        }

        private void btnXuatExcel_Click(object sender, EventArgs e)
        {
            var cacDong = dtgDanhSachDichVu.Rows.Cast<DataGridViewRow>()
                .Where(r => r.Visible && !r.IsNewRow)
                .ToList();

            if (cacDong.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Title = "Xuất danh sách dịch vụ";
                saveDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                saveDialog.FileName = $"DanhSachDichVu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Danh sách dịch vụ");
                        string[] tenCot = { "MaDV", "TenDV", "DonGia", "SLConLai", "LoaiDV" };

                        for (int c = 0; c < tenCot.Length; c++)
                        {
                            worksheet.Cell(1, c + 1).Value = dtgDanhSachDichVu.Columns[tenCot[c]].HeaderText;
                        }
                        worksheet.Range(1, 1, 1, tenCot.Length).Style.Font.Bold = true;

                        for (int i = 0; i < cacDong.Count; i++)
                        {
                            for (int c = 0; c < tenCot.Length; c++)
                            {
                                var cellVal = cacDong[i].Cells[tenCot[c]].Value;
                                worksheet.Cell(i + 2, c + 1).Value = cellVal?.ToString() ?? "";
                            }
                        }

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(saveDialog.FileName);
                    }

                    var moFile = MessageBox.Show(
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
        }

        private void btnThemMoi_Click(object sender, EventArgs e)
        {
            using (var formThem = new FormThemDichVu())
            {
                formThem.ShowDialog();
            }

            LoadDanhSachDichVu();
        }

        private void dtgDanhSachDichVu_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string? maDV = dtgDanhSachDichVu.Rows[e.RowIndex].Cells["MaDV"].Value?.ToString();
            string? tenDV = dtgDanhSachDichVu.Rows[e.RowIndex].Cells["TenDV"].Value?.ToString();

            if (string.IsNullOrEmpty(maDV))
                return;

            string tenCot = dtgDanhSachDichVu.Columns[e.ColumnIndex].Name;

            if (tenCot == "colSua")
            {
                using (var formSua = new FormSuaDichVu())
                {
                    formSua.MaDichVu = maDV;
                    formSua.ShowDialog();
                }

                LoadDanhSachDichVu();
            }
            else if (tenCot == "colXoa")
            {
                DialogResult result = MessageBox.Show(
                    $"Bạn có chắc muốn xóa dịch vụ {tenDV} ({maDV}) không?",
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    XoaDichVu(maDV);
                    LoadDanhSachDichVu();
                }
            }
        }

        private void CauHinhLuoi()
        {
            if (dtgDanhSachDichVu.Columns.Count > 0)
                return;

            dtgDanhSachDichVu.ReadOnly = true;
            dtgDanhSachDichVu.AllowUserToAddRows = false;
            dtgDanhSachDichVu.AllowUserToDeleteRows = false;
            dtgDanhSachDichVu.AllowUserToResizeRows = false;
            dtgDanhSachDichVu.MultiSelect = false;
            dtgDanhSachDichVu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtgDanhSachDichVu.RowHeadersVisible = false;
            dtgDanhSachDichVu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dtgDanhSachDichVu.RowTemplate.Height = 40;

            dtgDanhSachDichVu.Columns.Add(TaoCotAnh("colIcon", "", _iconDichVu, 50));
            dtgDanhSachDichVu.Columns.Add("MaDV", "Mã DV");
            dtgDanhSachDichVu.Columns.Add("TenDV", "Tên dịch vụ");
            dtgDanhSachDichVu.Columns.Add("DonGia", "Đơn giá");
            dtgDanhSachDichVu.Columns.Add("SLConLai", "Số lượng còn lại");
            dtgDanhSachDichVu.Columns.Add("LoaiDV", "Loại dịch vụ");

            dtgDanhSachDichVu.Columns["DonGia"]!.DefaultCellStyle.Format = "#,##0' đ'";
            dtgDanhSachDichVu.Columns["DonGia"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dtgDanhSachDichVu.Columns["SLConLai"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dtgDanhSachDichVu.Columns.Add(TaoCotAnh("colSua", "Sửa", _iconSua, 50));
            dtgDanhSachDichVu.Columns.Add(TaoCotAnh("colXoa", "Xóa", _iconXoa, 50));
        }

        private DataGridViewImageColumn TaoCotAnh(string ten, string tieuDe, Image anh, int doRong)
        {
            return new DataGridViewImageColumn
            {
                Name = ten,
                HeaderText = tieuDe,
                Image = anh,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = doRong,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        private void LoadDanhSachDichVu()
        {
            try
            {
                using var db = new HotelManagementContext();

                var dsDichVu = db.DichVus
                    .AsNoTracking()
                    .Where(d => d.DaXoa != true)
                    .OrderBy(d => d.MaDv)
                    .Select(d => new
                    {
                        d.MaDv,
                        d.TenDv,
                        d.DonGia,
                        d.SlconLai,
                        d.LoaiDv
                    })
                    .ToList();

                dtgDanhSachDichVu.Rows.Clear();

                foreach (var d in dsDichVu)
                {
                    string soLuong = d.SlconLai == null || d.SlconLai < 0
                        ? "Không giới hạn"
                        : d.SlconLai.Value.ToString();

                    dtgDanhSachDichVu.Rows.Add(
                        _iconDichVu,
                        d.MaDv,
                        d.TenDv,
                        d.DonGia,
                        soLuong,
                        d.LoaiDv,
                        _iconSua,
                        _iconXoa
                    );
                }

                LocTheoTuKhoa();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách dịch vụ: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LocTheoTuKhoa()
        {
            string keyword = txtTimKiem.Text.Trim();

            dtgDanhSachDichVu.CurrentCell = null;

            foreach (DataGridViewRow row in dtgDanhSachDichVu.Rows)
            {
                string ma = row.Cells["MaDV"].Value?.ToString() ?? "";
                string ten = row.Cells["TenDV"].Value?.ToString() ?? "";

                row.Visible = keyword.Length == 0
                              || ma.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                              || ten.Contains(keyword, StringComparison.OrdinalIgnoreCase);
            }
        }

        private void XoaDichVu(string maDV)
        {
            try
            {
                using var db = new HotelManagementContext();

                var dichVu = db.DichVus.FirstOrDefault(d => d.MaDv == maDV && d.DaXoa != true);
                if (dichVu == null)
                {
                    MessageBox.Show("Không tìm thấy dịch vụ để xóa.", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                dichVu.DaXoa = true;
                db.SaveChanges();

                MessageBox.Show("Đã xóa dịch vụ thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa dịch vụ: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
