using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using HotelManagement.Models;

namespace HotelManagement.NhanVien
{
    public partial class FormThemNhanVien : Form
    {
        public FormThemNhanVien()
        {
            InitializeComponent();
            btnThoat.DialogResult = DialogResult.Cancel;

            txtMaNV.MaxLength = 5;
            txtHoTenNV.MaxLength = 50;
            txtLuong.MaxLength = 12;
            txtSoDT.MaxLength = 10;
            txtSoCCCD.MaxLength = 12;
            txtDiaChi.MaxLength = 100;
            txtEmail.MaxLength = 100;

            cbChucVu.DropDownStyle = ComboBoxStyle.DropDownList;
            cbChucVu.SelectedIndex = 0;

            cbGioiTinh.DropDownStyle = ComboBoxStyle.DropDownList;
            cbGioiTinh.SelectedIndex = 0;

            txtLuong.KeyPress += OnlyAllowDigits;
            txtSoDT.KeyPress += OnlyAllowDigits;
            txtSoCCCD.KeyPress += OnlyAllowDigits;
        }

        private void btnThemMoi_Click(object sender, EventArgs e)
        {
            string maNV = txtMaNV.Text.Trim().ToUpper();
            string tenNV = txtHoTenNV.Text.Trim();
            string chucVu = cbChucVu.Text.Trim();
            string luong = txtLuong.Text.Trim();
            string sdt = txtSoDT.Text.Trim();
            string cccd = txtSoCCCD.Text.Trim();
            DateTime ngaySinh = dtNgaySinh.Value.Date;
            string diaChi = txtDiaChi.Text.Trim();
            string email = txtEmail.Text.Trim();
            string gioiTinh = cbGioiTinh.Text.Trim();

            if (string.IsNullOrEmpty(maNV))
            {
                MessageBox.Show("Vui lòng nhập mã nhân viên!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaNV.Focus();
                return;
            }

            if (string.IsNullOrEmpty(tenNV))
            {
                MessageBox.Show("Vui lòng nhập họ và tên!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHoTenNV.Focus();
                return;
            }

            if (string.IsNullOrEmpty(chucVu))
            {
                MessageBox.Show("Vui lòng chọn chức vụ!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbChucVu.Focus();
                return;
            }

            if (string.IsNullOrEmpty(luong))
            {
                MessageBox.Show("Vui lòng nhập lương!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLuong.Focus();
                return;
            }

            if (string.IsNullOrEmpty(sdt))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoDT.Focus();
                return;
            }

            if (string.IsNullOrEmpty(cccd))
            {
                MessageBox.Show("Vui lòng nhập số CCCD!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoCCCD.Focus();
                return;
            }

            if (string.IsNullOrEmpty(diaChi))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDiaChi.Focus();
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng nhập email!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrEmpty(gioiTinh))
            {
                MessageBox.Show("Vui lòng chọn giới tính!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbGioiTinh.Focus();
                return;
            }

            if (!decimal.TryParse(luong, NumberStyles.None, CultureInfo.InvariantCulture, out decimal luongSo) || luongSo <= 0)
            {
                MessageBox.Show("Lương phải là số lớn hơn 0.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLuong.Focus();
                return;
            }

            if (!ToanChuSo(sdt) || sdt.Length < 9)
            {
                MessageBox.Show("Số điện thoại phải gồm 9 đến 10 chữ số.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoDT.Focus();
                return;
            }

            if (!ToanChuSo(cccd) || cccd.Length != 12)
            {
                MessageBox.Show("Số CCCD phải gồm đúng 12 chữ số.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoCCCD.Focus();
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Email không đúng định dạng.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            if (ngaySinh > DateTime.Today.AddYears(-18))
            {
                MessageBox.Show("Nhân viên phải đủ 18 tuổi.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtNgaySinh.Focus();
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var nvCu = db.NhanViens.FirstOrDefault(n => n.MaNv == maNV);
                if (nvCu != null)
                {
                    if (nvCu.DaXoa == true)
                    {
                        nvCu.TenNv = tenNV;
                        nvCu.ChucVu = chucVu;
                        nvCu.Luong = luongSo;
                        nvCu.Sdt = sdt;
                        nvCu.Cccd = cccd;
                        nvCu.NgaySinh = DateOnly.FromDateTime(ngaySinh);
                        nvCu.GioiTinh = gioiTinh;
                        nvCu.DiaChi = diaChi;
                        nvCu.Email = email;
                        nvCu.DaXoa = false;
                        db.SaveChanges();

                        MessageBox.Show("Nhân viên từng bị xóa đã được khôi phục thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                        return;
                    }

                    MessageBox.Show("Mã nhân viên đã tồn tại.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMaNV.Focus();
                    return;
                }

                if (db.NhanViens.Any(n => n.DaXoa != true && n.Sdt == sdt))
                {
                    MessageBox.Show("Số điện thoại đã tồn tại.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSoDT.Focus();
                    return;
                }

                if (db.NhanViens.Any(n => n.DaXoa != true && n.Cccd == cccd))
                {
                    MessageBox.Show("Số CCCD đã tồn tại.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSoCCCD.Focus();
                    return;
                }

                if (db.NhanViens.Any(n => n.DaXoa != true && n.Email == email))
                {
                    MessageBox.Show("Email đã tồn tại.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                var nhanVien = new Models.NhanVien
                {
                    MaNv = maNV,
                    TenNv = tenNV,
                    ChucVu = chucVu,
                    Luong = luongSo,
                    Sdt = sdt,
                    Cccd = cccd,
                    NgaySinh = DateOnly.FromDateTime(ngaySinh),
                    GioiTinh = gioiTinh,
                    DiaChi = diaChi,
                    Email = email,
                    DaXoa = false
                };

                db.NhanViens.Add(nhanVien);
                int kq = db.SaveChanges();

                if (kq > 0)
                {
                    MessageBox.Show("Thêm nhân viên thành công.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Thêm nhân viên thất bại!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm nhân viên: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnlyAllowDigits(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private bool ToanChuSo(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
