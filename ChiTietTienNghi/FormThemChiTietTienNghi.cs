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
    public partial class FormThemChiTietTienNghi : Form
    {
        private string maLPH = string.Empty;

        public FormThemChiTietTienNghi()
        {
            InitializeComponent();
        }

        public FormThemChiTietTienNghi(string maLPH) : this()
        {
            this.maLPH = maLPH;
        }

        private void FormThemChiTietTienNghi_Load(object sender, EventArgs e)
        {
            txtSoLuong.MaxLength = 4;
            txtSoLuong.KeyPress += ChiNhapSo;
            cbTenTN.DropDownStyle = ComboBoxStyle.DropDownList;

            NapDanhSachTienNghiChuaCo();
        }

        private void NapDanhSachTienNghiChuaCo()
        {
            if (string.IsNullOrEmpty(maLPH))
            {
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var maDaCo = db.Cttns
                    .Where(c => c.MaLph == maLPH && c.DaXoa != true)
                    .Select(c => c.MaTn)
                    .ToList();

                var danhSachTienNghi = db.TienNghis
                    .AsNoTracking()
                    .Where(t => t.DaXoa != true && !maDaCo.Contains(t.MaTn))
                    .OrderBy(t => t.TenTn)
                    .Select(t => new { t.MaTn, t.TenTn })
                    .ToList();

                cbTenTN.DataSource = danhSachTienNghi;
                cbTenTN.DisplayMember = "TenTn";
                cbTenTN.ValueMember = "MaTn";
                cbTenTN.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách tiện nghi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChiNhapSo(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(maLPH))
            {
                MessageBox.Show("Không xác định được loại phòng!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cbTenTN.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn tiện nghi!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbTenTN.Focus();
                return;
            }

            string maTN = cbTenTN.SelectedValue.ToString() ?? "";
            string soLuongText = txtSoLuong.Text.Trim();

            if (string.IsNullOrEmpty(soLuongText))
            {
                MessageBox.Show("Vui lòng nhập số lượng!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoLuong.Focus();
                return;
            }

            if (!int.TryParse(soLuongText, out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên lớn hơn 0!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoLuong.Focus();
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var cttnCu = db.Cttns.FirstOrDefault(c => c.MaLph == maLPH && c.MaTn == maTN);
                if (cttnCu != null)
                {
                    if (cttnCu.DaXoa == true)
                    {
                        cttnCu.Sl = soLuong;
                        cttnCu.DaXoa = false;
                        db.SaveChanges();

                        MessageBox.Show("Tiện nghi từng bị xóa đã được khôi phục thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                        return;
                    }

                    MessageBox.Show("Tiện nghi này đã có trong loại phòng!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var moi = new Models.Cttn
                {
                    MaLph = maLPH,
                    MaTn = maTN,
                    Sl = soLuong,
                    DaXoa = false
                };

                db.Cttns.Add(moi);
                int kq = db.SaveChanges();

                if (kq > 0)
                {
                    MessageBox.Show("Thêm tiện nghi cho loại phòng thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Thêm tiện nghi thất bại!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm tiện nghi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
