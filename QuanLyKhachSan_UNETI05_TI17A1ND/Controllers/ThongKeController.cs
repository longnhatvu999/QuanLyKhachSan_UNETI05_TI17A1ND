// ============================================================
// Họ và tên: Bùi Hồng Anh
// Mã sinh viên: 23203100057
// Nội dung thực hiện: Dashboard và Thống kê bằng LINQ
// Module: 4
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI05_TI17A1ND.Data;
using QuanLyKhachSan_UNETI05_TI17A1ND.ViewModels;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Controllers
{
    public class ThongKeController : BaseController  // Chỉ Admin
    {
        private readonly QuanLyKhachSan_UNETI05_TI17A1NDContext _context;

        public ThongKeController(QuanLyKhachSan_UNETI05_TI17A1NDContext context)
        {
            _context = context;
        }

        // ==================================================
        // DASHBOARD – Tổng quan hệ thống
        // ==================================================
        public async Task<IActionResult> Dashboard()
        {
            var vm = new DashboardViewModel
            {
                // ===== Số liệu tổng quát =====
                TongLoaiPhong = await _context.LoaiPhongs.CountAsync(),
                TongPhong = await _context.Phongs.CountAsync(),
                TongKhachHang = await _context.KhachHangs.CountAsync(),
                TongPhieuDat = await _context.PhieuDatPhongs.CountAsync(),

                // ===== Phiếu theo trạng thái (LINQ Count với điều kiện) =====
                PhieuChoXacNhan = await _context.PhieuDatPhongs.CountAsync(p => p.TrangThai == "Chờ xác nhận"),
                PhieuDaXacNhan = await _context.PhieuDatPhongs.CountAsync(p => p.TrangThai == "Đã xác nhận"),
                PhieuDaNhanPhong = await _context.PhieuDatPhongs.CountAsync(p => p.TrangThai == "Đã nhận phòng"),
                PhieuDaTraPhong = await _context.PhieuDatPhongs.CountAsync(p => p.TrangThai == "Đã trả phòng"),
                PhieuDaHuy = await _context.PhieuDatPhongs.CountAsync(p => p.TrangThai == "Đã hủy"),

                // ===== Phòng theo trạng thái =====
                PhongSanSang = await _context.Phongs.CountAsync(p => p.TrangThai == "Sẵn sàng"),
                PhongDangSuDung = await _context.Phongs.CountAsync(p => p.TrangThai == "Đang sử dụng"),
                PhongTamNgung = await _context.Phongs.CountAsync(p => p.TrangThai == "Tạm ngừng"),

                // ===== Doanh thu từ phiếu đã trả phòng (LINQ Sum) =====
                TongDoanhThu = await _context.PhieuDatPhongs
                    .Where(p => p.TrangThai == "Đã trả phòng")
                    .SumAsync(p => (decimal?)p.TongTienDuKien) ?? 0
            };

            return View(vm);
        }

        // ==================================================
        // THỐNG KÊ TỔNG HỢP
        // ==================================================
        public async Task<IActionResult> DoanhThu()
        {
            // ===== 1. Doanh thu theo tháng (LINQ GroupBy + Sum) =====
            var doanhThuTheoThang = await _context.PhieuDatPhongs
                .Where(p => p.TrangThai == "Đã trả phòng")
                .GroupBy(p => new { p.NgayTraDuKien.Year, p.NgayTraDuKien.Month })
                .Select(g => new DoanhThuTheoThangViewModel
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    SoHoaDon = g.Count(),
                    DoanhThu = g.Sum(p => p.TongTienDuKien)
                })
                .OrderByDescending(x => x.Nam).ThenByDescending(x => x.Thang)
                .ToListAsync();

            // ===== 2. Số lượt đặt theo từng phòng (LINQ GroupBy) =====
            var luotDatTheoPhong = await _context.ChiTietDatPhongs
                .Include(ct => ct.Phong).ThenInclude(p => p!.LoaiPhong)
                .GroupBy(ct => new { ct.MaPhong, ct.Phong!.SoPhong, TenLoai = ct.Phong.LoaiPhong!.TenLoaiPhong })
                .Select(g => new LuotDatTheoPhongViewModel
                {
                    SoPhong = g.Key.SoPhong,
                    TenLoaiPhong = g.Key.TenLoai,
                    SoLuotDat = g.Count()
                })
                .OrderByDescending(x => x.SoLuotDat)
                .ToListAsync();

            // ===== 3. Số lượt đặt theo loại phòng =====
            var luotDatTheoLoaiPhong = await _context.ChiTietDatPhongs
                .Include(ct => ct.Phong).ThenInclude(p => p!.LoaiPhong)
                .GroupBy(ct => ct.Phong!.LoaiPhong!.TenLoaiPhong)
                .Select(g => new
                {
                    TenLoaiPhong = g.Key,
                    SoLuotDat = g.Count()
                })
                .OrderByDescending(x => x.SoLuotDat)
                .ToListAsync();

            // ===== 4. Top khách hàng theo lượt đặt và tổng chi tiêu =====
            var topKhachHang = await _context.PhieuDatPhongs
                .Where(p => p.TrangThai != "Đã hủy")
                .Include(p => p.KhachHang)
                .GroupBy(p => new { p.MaKhachHang, p.KhachHang!.HoTen, p.KhachHang.SoDienThoai })
                .Select(g => new TopKhachHangViewModel
                {
                    HoTen = g.Key.HoTen,
                    SoDienThoai = g.Key.SoDienThoai,
                    SoLuotDat = g.Count(),
                    TongChiTieu = g.Sum(p => p.TongTienDuKien)
                })
                .OrderByDescending(x => x.SoLuotDat)
                .ThenByDescending(x => x.TongChiTieu)
                .Take(10)
                .ToListAsync();

            // ===== 5. Số phiếu theo trạng thái =====
            var phieuTheoTrangThai = await _context.PhieuDatPhongs
                .GroupBy(p => p.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            // ===== 6. Số phòng theo loại phòng =====
            var phongTheoLoai = await _context.Phongs
                .Include(p => p.LoaiPhong)
                .GroupBy(p => p.LoaiPhong!.TenLoaiPhong)
                .Select(g => new { TenLoaiPhong = g.Key, SoPhong = g.Count() })
                .ToListAsync();

            // Truyền qua ViewBag
            ViewBag.DoanhThuTheoThang = doanhThuTheoThang;
            ViewBag.LuotDatTheoPhong = luotDatTheoPhong;
            ViewBag.LuotDatTheoLoaiPhong = luotDatTheoLoaiPhong;
            ViewBag.TopKhachHang = topKhachHang;
            ViewBag.PhieuTheoTrangThai = phieuTheoTrangThai;
            ViewBag.PhongTheoLoai = phongTheoLoai;

            return View();
        }
    }
}