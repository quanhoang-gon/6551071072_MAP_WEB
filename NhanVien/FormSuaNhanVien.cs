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
    public partial class FormSuaNhanVien : Form
    {
        private string maNV = string.Empty;

        public FormSuaNhanVien()
        {
            InitializeComponent();
            btnThoat.DialogResult = DialogResult.Cancel;

            txtHoTenNV.MaxLength = 50;
            txtLuong.MaxLength = 12;
            txtSoDT.MaxLength = 10;
            txtSoCCCD.MaxLength = 12;
            txtDiaChi.MaxLength = 100;
            txtEmail.MaxLength = 100;

            cbChucVu.DropDownStyle = ComboBoxStyle.DropDownList;
            cbGioiTinh.DropDownStyle = ComboBoxStyle.DropDownList;

            txtLuong.KeyPress += OnlyAllowDigits;
            txtSoDT.KeyPress += OnlyAllowDigits;
            txtSoCCCD.KeyPress += OnlyAllowDigits;
        }

        public FormSuaNhanVien(string maNV) : this()
        {
            this.maNV = maNV;
        }

        private void FormSuaNhanVien_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(maNV))
            {
                MessageBox.Show("Không xác định được nhân viên cần cập nhật!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var nhanVien = db.NhanViens
                    .Where(n => n.MaNv == maNV && n.DaXoa != true)
                    .Select(n => new
                    {
                        n.MaNv,
                        n.TenNv,
                        n.ChucVu,
                        n.Luong,
                        n.Sdt,
                        n.Cccd,
                        n.NgaySinh,
                        n.GioiTinh,
                        n.DiaChi,
                        n.Email
                    })
                    .FirstOrDefault();

                if (nhanVien == null)
                {
                    MessageBox.Show("Không tìm thấy nhân viên cần cập nhật!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                txtMaNV.Text = nhanVien.MaNv;
                txtHoTenNV.Text = nhanVien.TenNv;
                cbChucVu.SelectedIndex = cbChucVu.FindStringExact(nhanVien.ChucVu);
                txtLuong.Text = nhanVien.Luong.ToString("0");
                txtSoDT.Text = nhanVien.Sdt;
                txtSoCCCD.Text = nhanVien.Cccd;
                dtNgaySinh.Value = nhanVien.NgaySinh.ToDateTime(TimeOnly.MinValue);
                cbGioiTinh.SelectedIndex = cbGioiTinh.FindStringExact(nhanVien.GioiTinh);
                txtDiaChi.Text = nhanVien.DiaChi;
                txtEmail.Text = nhanVien.Email ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load nhân viên: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            string tenNV = txtHoTenNV.Text.Trim();
            string chucVu = cbChucVu.Text.Trim();
            string luong = txtLuong.Text.Trim();
            string sdt = txtSoDT.Text.Trim();
            string cccd = txtSoCCCD.Text.Trim();
            DateTime ngaySinh = dtNgaySinh.Value.Date;
            string diaChi = txtDiaChi.Text.Trim();
            string email = txtEmail.Text.Trim();
            string gioiTinh = cbGioiTinh.Text.Trim();

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

                var nhanVien = db.NhanViens.FirstOrDefault(n => n.MaNv == maNV && n.DaXoa != true);
                if (nhanVien == null)
                {
                    MessageBox.Show("Không tìm thấy nhân viên cần cập nhật!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                if (db.NhanViens.Any(n => n.MaNv != maNV && n.Sdt == sdt && n.DaXoa != true))
                {
                    MessageBox.Show("Số điện thoại đã tồn tại.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSoDT.Focus();
                    return;
                }

                if (db.NhanViens.Any(n => n.MaNv != maNV && n.Cccd == cccd && n.DaXoa != true))
                {
                    MessageBox.Show("Số CCCD đã tồn tại.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSoCCCD.Focus();
                    return;
                }

                if (db.NhanViens.Any(n => n.MaNv != maNV && n.Email == email && n.DaXoa != true))
                {
                    MessageBox.Show("Email đã tồn tại.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                nhanVien.TenNv = tenNV;
                nhanVien.ChucVu = chucVu;
                nhanVien.Luong = luongSo;
                nhanVien.Sdt = sdt;
                nhanVien.Cccd = cccd;
                nhanVien.NgaySinh = DateOnly.FromDateTime(ngaySinh);
                nhanVien.GioiTinh = gioiTinh;
                nhanVien.DiaChi = diaChi;
                nhanVien.Email = email;

                int kq = db.SaveChanges();

                if (kq > 0)
                {
                    MessageBox.Show("Cập nhật nhân viên thành công.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Không có thay đổi nào để cập nhật!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật nhân viên: " + ex.Message, "Lỗi",
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
