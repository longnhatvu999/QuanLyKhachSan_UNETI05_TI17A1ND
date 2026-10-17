// ============================================================
// Họ và tên: Bùi HỒng Anh
// Mã sinh viên: 23203100057
// Nội dung thực hiện: ViewModel cho Dashboard
// Module: 4
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

namespace QuanLyKhachSan_UNETI05_TI17A1ND.ViewModels
{
    public class DashboardViewModel
    {
        // ===== Số liệu tổng quát =====
        public int TongLoaiPhong { get; set; }
        public int TongPhong { get; set; }
        public int TongKhachHang { get; set; }
        public int TongPhieuDat { get; set; }

        // ===== Số liệu theo trạng thái phiếu =====
        public int PhieuChoXacNhan { get; set; }
        public int PhieuDaXacNhan { get; set; }
        public int PhieuDaNhanPhong { get; set; }
        public int PhieuDaTraPhong { get; set; }
        public int PhieuDaHuy { get; set; }

        // ===== Số liệu theo trạng thái phòng =====
        public int PhongSanSang { get; set; }
        public int PhongDangSuDung { get; set; }
        public int PhongTamNgung { get; set; }

        // ===== Doanh thu =====
        public decimal TongDoanhThu { get; set; }
    }

    public class DoanhThuTheoThangViewModel
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public int SoHoaDon { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class LuotDatTheoPhongViewModel
    {
        public string SoPhong { get; set; } = string.Empty;
        public string TenLoaiPhong { get; set; } = string.Empty;
        public int SoLuotDat { get; set; }
    }

    public class TopKhachHangViewModel
    {
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public int SoLuotDat { get; set; }
        public decimal TongChiTieu { get; set; }
    }
}