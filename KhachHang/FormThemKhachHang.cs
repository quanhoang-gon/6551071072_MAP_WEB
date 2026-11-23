using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using HotelManagement.Models;

namespace HotelManagement.KhachHang
{
    public partial class FormThemKhachHang : Form
    {
        public FormThemKhachHang()
        {
            InitializeComponent();
            btnThoat.DialogResult = DialogResult.Cancel;

            txtMaKH.MaxLength = 5;
            txtHoVaTen.MaxLength = 50;
            txtCCCD.MaxLength = 12;
            txtSoDienThoai.MaxLength = 10;
            txtQuocTich.MaxLength = 30;

            cbGioiTinh.DropDownStyle = ComboBoxStyle.DropDownList;
            cbGioiTinh.SelectedIndex = 0;

            txtSoDienThoai.KeyPress += OnlyAllowDigits;
            txtCCCD.KeyPress += OnlyAllowDigits;
        }

        private void btnThemMoi_Click(object sender, EventArgs e)
        {
            string maKH = txtMaKH.Text.Trim().ToUpper();
            string tenKH = txtHoVaTen.Text.Trim();
            string sdt = txtSoDienThoai.Text.Trim();
            string cccd = txtCCCD.Text.Trim();
            string quocTich = txtQuocTich.Text.Trim();
            string gioiTinh = cbGioiTinh.Text.Trim();

            if (string.IsNullOrEmpty(maKH))
            {
                MessageBox.Show("Vui lòng nhập mã khách hàng!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaKH.Focus();
                return;
            }

            if (string.IsNullOrEmpty(tenKH))
            {
                MessageBox.Show("Vui lòng nhập họ và tên!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHoVaTen.Focus();
                return;
            }

            if (string.IsNullOrEmpty(cccd))
            {
                MessageBox.Show("Vui lòng nhập số CCCD/Passport!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCCCD.Focus();
                return;
            }

            if (string.IsNullOrEmpty(quocTich))
            {
                MessageBox.Show("Vui lòng nhập quốc tịch!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQuocTich.Focus();
                return;
            }

            if (string.IsNullOrEmpty(gioiTinh))
            {
                MessageBox.Show("Vui lòng chọn giới tính!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbGioiTinh.Focus();
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var khCu = db.KhachHangs.FirstOrDefault(k => k.MaKh == maKH);
                if (khCu != null)
                {
                    if (khCu.DaXoa == true)
                    {
                        khCu.TenKh = tenKH;
                        khCu.Sdt = string.IsNullOrEmpty(sdt) ? null : sdt;
                        khCu.CccdPassport = cccd;
                        khCu.QuocTich = quocTich;
                        khCu.GioiTinh = gioiTinh;
                        khCu.DaXoa = false;
                        db.SaveChanges();

                        MessageBox.Show("Khách hàng từng bị xóa đã được khôi phục thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                        return;
                    }

                    MessageBox.Show("Mã khách hàng đã tồn tại!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMaKH.Focus();
                    return;
                }

                if (db.KhachHangs.Any(k => k.DaXoa != true && k.CccdPassport == cccd))
                {
                    MessageBox.Show("Số CCCD/Passport đã tồn tại!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCCCD.Focus();
                    return;
                }

                var moi = new Models.KhachHang
                {
                    MaKh = maKH,
                    TenKh = tenKH,
                    Sdt = string.IsNullOrEmpty(sdt) ? null : sdt,
                    CccdPassport = cccd,
                    QuocTich = quocTich,
                    GioiTinh = gioiTinh,
                    DaXoa = false
                };

                db.KhachHangs.Add(moi);
                int kq = db.SaveChanges();

                if (kq > 0)
                {
                    MessageBox.Show("Thêm khách hàng thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Thêm khách hàng thất bại!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm khách hàng: " + ex.Message, "Lỗi",
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
    }
}
