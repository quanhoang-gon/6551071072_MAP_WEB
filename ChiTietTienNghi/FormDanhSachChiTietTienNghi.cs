using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.ChiTietTienNghi
{
    public partial class FormDanhSachChiTietTienNghi : Form
    {
        private readonly Image _iconTienNghi = Properties.Resources.TienNghi;
        private readonly Image _iconSua = Properties.Resources.edit;
        private readonly Image _iconXoa = Properties.Resources.delete;

        private string maLPH = string.Empty;
        private string tenLPH = string.Empty;

        public FormDanhSachChiTietTienNghi()
        {
            InitializeComponent();
        }

        public FormDanhSachChiTietTienNghi(string maLPH, string tenLPH) : this()
        {
            this.maLPH = maLPH;
            this.tenLPH = tenLPH;
        }

        private void FormDanhSachChiTietTienNghi_Load(object sender, EventArgs e)
        {
            lblTenLoaiPhong.Text = tenLPH;
            KhoiTaoBangDuLieu();
            LoadChiTietTienNghi();
        }

        private void KhoiTaoBangDuLieu()
        {
            if (dtgDanhSachTienNghiTheoLoaiPhong.Columns.Count > 0)
            {
                return;
            }

            dtgDanhSachTienNghiTheoLoaiPhong.ReadOnly = true;
            dtgDanhSachTienNghiTheoLoaiPhong.AllowUserToAddRows = false;
            dtgDanhSachTienNghiTheoLoaiPhong.AllowUserToDeleteRows = false;
            dtgDanhSachTienNghiTheoLoaiPhong.MultiSelect = false;
            dtgDanhSachTienNghiTheoLoaiPhong.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtgDanhSachTienNghiTheoLoaiPhong.RowHeadersVisible = false;
            dtgDanhSachTienNghiTheoLoaiPhong.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dtgDanhSachTienNghiTheoLoaiPhong.RowTemplate.Height = 40;

            dtgDanhSachTienNghiTheoLoaiPhong.Columns.Add(TaoCotAnh("colIcon", "", _iconTienNghi, 50));
            dtgDanhSachTienNghiTheoLoaiPhong.Columns.Add("MaTN", "Mã tiện nghi");
            dtgDanhSachTienNghiTheoLoaiPhong.Columns.Add("TenTN", "Tên tiện nghi");
            dtgDanhSachTienNghiTheoLoaiPhong.Columns.Add("SL", "Số lượng");
            dtgDanhSachTienNghiTheoLoaiPhong.Columns["SL"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dtgDanhSachTienNghiTheoLoaiPhong.Columns.Add(TaoCotAnh("Sua", "Sửa", _iconSua, 50));
            dtgDanhSachTienNghiTheoLoaiPhong.Columns.Add(TaoCotAnh("Xoa", "Xóa", _iconXoa, 50));
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

        private void LoadChiTietTienNghi()
        {
            if (string.IsNullOrEmpty(maLPH))
            {
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var danhSach = db.Cttns
                    .AsNoTracking()
                    .Where(c => c.MaLph == maLPH && c.DaXoa != true && c.MaTnNavigation.DaXoa != true)
                    .OrderBy(c => c.MaTn)
                    .Select(c => new
                    {
                        c.MaTn,
                        c.MaTnNavigation.TenTn,
                        c.Sl
                    })
                    .ToList();

                dtgDanhSachTienNghiTheoLoaiPhong.Rows.Clear();
                foreach (var item in danhSach)
                {
                    dtgDanhSachTienNghiTheoLoaiPhong.Rows.Add(
                        _iconTienNghi,
                        item.MaTn,
                        item.TenTn,
                        item.Sl,
                        _iconSua,
                        _iconXoa
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải chi tiết tiện nghi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dtgDanhSachTienNghiTheoLoaiPhong_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string tenCot = dtgDanhSachTienNghiTheoLoaiPhong.Columns[e.ColumnIndex].Name;
            if (tenCot != "Sua" && tenCot != "Xoa")
            {
                return;
            }

            string maTN = dtgDanhSachTienNghiTheoLoaiPhong.Rows[e.RowIndex].Cells["MaTN"].Value?.ToString() ?? "";
            string tenTN = dtgDanhSachTienNghiTheoLoaiPhong.Rows[e.RowIndex].Cells["TenTN"].Value?.ToString() ?? "";
            int sl = Convert.ToInt32(dtgDanhSachTienNghiTheoLoaiPhong.Rows[e.RowIndex].Cells["SL"].Value ?? 0);

            if (string.IsNullOrEmpty(maTN))
            {
                return;
            }

            if (tenCot == "Sua")
            {
                using (FormSuaChiTietTienNghi f = new FormSuaChiTietTienNghi(maLPH, maTN, tenTN, sl))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        LoadChiTietTienNghi();
                    }
                }
                return;
            }

            if (MessageBox.Show($"Bạn có chắc muốn xóa tiện nghi '{tenTN}'?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var chiTiet = db.Cttns.FirstOrDefault(c => c.MaLph == maLPH && c.MaTn == maTN);
                if (chiTiet == null)
                {
                    MessageBox.Show("Không tìm thấy tiện nghi cần xóa!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    db.Cttns.Remove(chiTiet);
                    int kq = db.SaveChanges();

                    if (kq > 0)
                    {
                        MessageBox.Show("Xóa thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại!", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa tiện nghi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            LoadChiTietTienNghi();
        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            using (FormThemChiTietTienNghi f = new FormThemChiTietTienNghi(maLPH))
            {
                f.ShowDialog();
            }

            LoadChiTietTienNghi();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
