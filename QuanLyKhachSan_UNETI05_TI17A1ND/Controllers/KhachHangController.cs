// ============================================================
// Họ và tên: Trần Tuấn Mạnh
// Mã sinh viên: 23203100017
// Nội dung thực hiện: Quản lý khách hàng + khách tự xem/sửa thông tin cá nhân
// Module: 3
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI05_TI17A1ND.Data;
using QuanLyKhachSan_UNETI05_TI17A1ND.Models;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly QuanLyKhachSan_UNETI05_TI17A1NDContext _context;

        public KhachHangController(QuanLyKhachSan_UNETI05_TI17A1NDContext context)
        {
            _context = context;
        }

        // ==================================================
        // ADMIN: Danh sách + Tìm kiếm
        // ==================================================
        public async Task<IActionResult> Index(string? searchString, bool? trangThai)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;

            var query = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .AsQueryable();

            // Tìm kiếm theo Họ tên, SĐT, Email, CCCD
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(k =>
                    k.HoTen.Contains(searchString) ||
                    k.SoDienThoai.Contains(searchString) ||
                    k.Email.Contains(searchString) ||
                    (k.CCCD != null && k.CCCD.Contains(searchString)));
            }

            // Lọc theo trạng thái
            if (trangThai.HasValue)
                query = query.Where(k => k.TrangThai == trangThai.Value);

            var ds = await query.OrderBy(k => k.MaKhachHang).ToListAsync();
            return View(ds);
        }

        // ==================================================
        // ADMIN: Chi tiết khách hàng
        // ==================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            if (id == null) return NotFound();

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .Include(k => k.PhieuDatPhongs)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null) return NotFound();
            return View(kh);
        }

        // ADMIN: Hiển thị trang sửa thông tin khách hàng
        [HttpGet]
        public async Task<IActionResult> Sua(int? id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("TuChoi", "TaiKhoan");
            }

            if (id == null)
            {
                return NotFound();
            }

            var kh = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null)
            {
                return NotFound();
            }

            return View("SuaThongTin",kh);
        }

        // ADMIN: Lưu thông tin khách hàng sau khi sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(int id, KhachHang model)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("TuChoi", "TaiKhoan");
            }

            if (id != model.MaKhachHang)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null)
            {
                return NotFound();
            }

            kh.HoTen = model.HoTen;
            kh.NgaySinh = model.NgaySinh;
            kh.GioiTinh = model.GioiTinh;
            kh.SoDienThoai = model.SoDienThoai;
            kh.Email = model.Email;
            kh.CCCD = model.CCCD;
            kh.DiaChi = model.DiaChi;

            // Đồng bộ tên và email sang tài khoản đăng nhập
            if (kh.TaiKhoan != null)
            {
                kh.TaiKhoan.HoTen = kh.HoTen;
                kh.TaiKhoan.Email = kh.Email;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thông tin khách hàng thành công!";

            return RedirectToAction(nameof(Index));
        }


        // ==================================================
        // ADMIN: Cập nhật trạng thái
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null) return NotFound();

            kh.TrangThai = !kh.TrangThai;
            if (kh.TaiKhoan != null)
                kh.TaiKhoan.TrangThai = kh.TrangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã {(kh.TrangThai ? "kích hoạt" : "khóa")} khách hàng {kh.HoTen}.";
            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // KHÁCH HÀNG: Xem thông tin cá nhân
        // ==================================================
        public async Task<IActionResult> ThongTinCaNhan()
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);

            if (kh == null) return NotFound();
            return View(kh);
        }

        // ==================================================
        // KHÁCH HÀNG: Sửa thông tin cá nhân
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> SuaThongTin()
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            var kh = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);

            if (kh == null) return NotFound();
            return View(kh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaThongTin(KhachHang model)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            // Chỉ cho phép sửa thông tin của chính mình
            var kh = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);

            if (kh == null) return NotFound();

            // Cập nhật các trường cho phép
            kh.HoTen = model.HoTen;
            kh.NgaySinh = model.NgaySinh;
            kh.GioiTinh = model.GioiTinh;
            kh.SoDienThoai = model.SoDienThoai;
            kh.Email = model.Email;
            kh.CCCD = model.CCCD;
            kh.DiaChi = model.DiaChi;

            if (!ModelState.IsValid)
                return View(kh);

            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("HoTen", kh.HoTen);
            TempData["Success"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction(nameof(ThongTinCaNhan));
        }
    }
}