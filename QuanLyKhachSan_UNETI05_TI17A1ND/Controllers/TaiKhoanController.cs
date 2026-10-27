// ============================================================
// Họ và tên: Vũ Long Nhật
// Mã sinh viên: 23203100081
// Nội dung thực hiện: Quản lý tài khoản, đăng nhập, đăng xuất, phân quyền
// Module: 1
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI05_TI17A1ND.Data;
using QuanLyKhachSan_UNETI05_TI17A1ND.ViewModels;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly QuanLyKhachSan_UNETI05_TI17A1NDContext _context;

        public TaiKhoanController(QuanLyKhachSan_UNETI05_TI17A1NDContext context)
        {
            _context = context;
        }

        // ==================== ĐĂNG NHẬP ====================
        [HttpGet]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            // Nếu đã đăng nhập → chuyển về trang chủ
            if (HttpContext.Session.GetInt32("MaTaiKhoan") != null)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // Dùng LINQ kiểm tra tài khoản
            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap
                                       && t.MatKhau == model.MatKhau);

            if (taiKhoan == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (!taiKhoan.TrangThai)
            {
                ModelState.AddModelError("", "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.");
                return View(model);
            }

            // ===== Lấy tên khách hàng mới nhất =====
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == taiKhoan.MaTaiKhoan);

            string hoTenHienThi = !string.IsNullOrWhiteSpace(khachHang?.HoTen)
                ? khachHang.HoTen
                : taiKhoan.HoTen;

            // ===== Lưu Session =====
            HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString("HoTen", hoTenHienThi);
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);
            HttpContext.Session.SetString("TenDangNhap", taiKhoan.TenDangNhap);

            // ===== Điều hướng theo vai trò =====
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (taiKhoan.VaiTro == "Admin")
                return RedirectToAction("Dashboard", "ThongKe");

            return RedirectToAction("Index", "Home");
        }

        // ==================== ĐĂNG XUẤT ====================
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        // ==================== TRANG TỪ CHỐI TRUY CẬP ====================
        public IActionResult TuChoi()
        {
            return View();
        }
        // ==================================================
        // ĐĂNG KÝ TÀI KHOẢN (GET)
        // ==================================================
        [HttpGet]
        public IActionResult DangKy()
        {
            // Nếu đã đăng nhập → chuyển về trang chủ
            if (HttpContext.Session.GetInt32("MaTaiKhoan") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // ==================================================
        // ĐĂNG KÝ TÀI KHOẢN (POST)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ===== VALIDATION NGHIỆP VỤ =====

            // 1. Kiểm tra tên đăng nhập đã tồn tại chưa
            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã được sử dụng. Vui lòng chọn tên khác.");
                return View(model);
            }

            // 2. Kiểm tra email đã tồn tại chưa (trong bảng TaiKhoan hoặc KhachHang)
            if (await _context.TaiKhoans.AnyAsync(t => t.Email == model.Email)
                || await _context.KhachHangs.AnyAsync(k => k.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng. Vui lòng chọn email khác.");
                return View(model);
            }

            // 3. Kiểm tra CCCD đã tồn tại chưa (nếu có nhập)
            if (!string.IsNullOrWhiteSpace(model.CCCD))
            {
                if (await _context.KhachHangs.AnyAsync(k => k.CCCD == model.CCCD))
                {
                    ModelState.AddModelError("CCCD", "CCCD đã được đăng ký. Vui lòng kiểm tra lại.");
                    return View(model);
                }
            }

            // ===== TẠO TÀI KHOẢN =====
            var taiKhoan = new Models.TaiKhoan
            {
                TenDangNhap = model.TenDangNhap,
                MatKhau = model.MatKhau, // ⚠️ Trong thực tế nên hash mật khẩu
                HoTen = model.HoTen,
                Email = model.Email,
                VaiTro = "KhachHang",
                TrangThai = true
            };
            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            // ===== TẠO KHÁCH HÀNG LIÊN KẾT =====
            var khachHang = new Models.KhachHang
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                HoTen = model.HoTen,
                NgaySinh = model.NgaySinh,
                GioiTinh = model.GioiTinh,
                SoDienThoai = model.SoDienThoai,
                Email = model.Email,
                CCCD = model.CCCD,
                DiaChi = model.DiaChi,
                NgayDangKy = DateTime.Now,
                TrangThai = true
            };
            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đăng ký thành công! Chào mừng {model.HoTen}. Vui lòng đăng nhập.";
            return RedirectToAction(nameof(DangNhap));
        }
        // ==================================================
        // ĐỔI MẬT KHẨU (GET)
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> DoiMatKhau()
        {
            // Yêu cầu đăng nhập
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
                return RedirectToAction("DangNhap", "TaiKhoan",
                    new { returnUrl = "/TaiKhoan/DoiMatKhau" });

            // Load thông tin để hiển thị ở View
            var taiKhoan = await _context.TaiKhoans.FindAsync(maTaiKhoan.Value);
            if (taiKhoan == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            ViewBag.HoTen = taiKhoan.HoTen;
            ViewBag.TenDangNhap = taiKhoan.TenDangNhap;
            ViewBag.VaiTro = taiKhoan.VaiTro;

            return View();
        }

        // ==================================================
        // ĐỔI MẬT KHẨU (POST)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            var taiKhoan = await _context.TaiKhoans.FindAsync(maTaiKhoan.Value);
            if (taiKhoan == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Load lại thông tin cho ViewBag (phòng khi return View khi có lỗi)
            ViewBag.HoTen = taiKhoan.HoTen;
            ViewBag.TenDangNhap = taiKhoan.TenDangNhap;
            ViewBag.VaiTro = taiKhoan.VaiTro;

            if (!ModelState.IsValid)
                return View(model);

            // ===== VALIDATION NGHIỆP VỤ =====

            // 1. Mật khẩu hiện tại phải đúng
            if (taiKhoan.MatKhau != model.MatKhauHienTai)
            {
                ModelState.AddModelError("MatKhauHienTai", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            // 2. Mật khẩu mới không được trùng mật khẩu cũ
            if (model.MatKhauMoi == model.MatKhauHienTai)
            {
                ModelState.AddModelError("MatKhauMoi", "Mật khẩu mới phải khác mật khẩu hiện tại.");
                return View(model);
            }

            // ===== CẬP NHẬT MẬT KHẨU =====
            taiKhoan.MatKhau = model.MatKhauMoi;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(DangNhap));
        }
        // ==================================================
        // ADMIN: Danh sách tài khoản
        // ==================================================
        public async Task<IActionResult> DanhSachTaiKhoan()
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            var dsTaiKhoan = await _context.TaiKhoans.ToListAsync();
            return View(dsTaiKhoan);
        }

        // ==================================================
        // ADMIN: Sửa tài khoản (GET) - Bao gồm cả đổi vai trò & trạng thái
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> SuaTaiKhoan(int id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null) return NotFound();

            return View(tk);
        }

        // ==================================================
        // ADMIN: Sửa tài khoản (POST)
        // ==================================================
        // ==================================================
        // ADMIN: Sửa tài khoản (POST)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaTaiKhoan(int id, string hoTen, string email, string vaiTroMoi, bool trangThai)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            // Lấy Tài khoản kèm theo thông tin Khách hàng (nếu có)
            var tk = await _context.TaiKhoans
                .Include(t => t.KhachHang)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (tk == null) return NotFound();

            // Không cho phép Admin tự tước quyền hoặc tự khóa chính mình
            int? maTaiKhoanHienTai = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (tk.MaTaiKhoan == maTaiKhoanHienTai && (vaiTroMoi != "Admin" || trangThai == false))
            {
                TempData["Error"] = "Bạn không thể tự hạ quyền hoặc tự khóa tài khoản của chính mình!";
                return RedirectToAction(nameof(DanhSachTaiKhoan));
            }

            // 1. Cập nhật bảng Tài khoản
            tk.HoTen = hoTen;
            tk.Email = email;
            tk.VaiTro = vaiTroMoi;
            tk.TrangThai = trangThai;
            _context.Update(tk);

            // 2. Đồng bộ sang bảng Khách hàng (nếu tài khoản này có hồ sơ khách hàng)
            if (tk.KhachHang != null)
            {
                tk.KhachHang.HoTen = hoTen;
                tk.KhachHang.Email = email;
                tk.KhachHang.TrangThai = trangThai; // Đồng bộ luôn trạng thái
                _context.Update(tk.KhachHang);
            }

            await _context.SaveChangesAsync();

            // 3. Cập nhật lại Session nếu Admin tự sửa tên của chính mình
            if (tk.MaTaiKhoan == maTaiKhoanHienTai)
            {
                HttpContext.Session.SetString("HoTen", tk.HoTen);
            }

            TempData["Success"] = $"Đã cập nhật tài khoản {tk.TenDangNhap} thành công.";
            return RedirectToAction(nameof(DanhSachTaiKhoan));
        }

        // ==================================================
        // ADMIN: Xóa tài khoản (Có kiểm tra ràng buộc dữ liệu)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTaiKhoan(int id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            var tk = await _context.TaiKhoans
                .Include(t => t.KhachHang)
                .ThenInclude(k => k.PhieuDatPhongs)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (tk == null) return NotFound();

            // 1. Không cho Admin tự xóa mình
            if (tk.MaTaiKhoan == HttpContext.Session.GetInt32("MaTaiKhoan"))
            {
                TempData["Error"] = "Bạn không thể tự xóa tài khoản của chính mình!";
                return RedirectToAction(nameof(DanhSachTaiKhoan));
            }

            // ========================================================
            // 2. THÊM DÒNG NÀY: Chặn không cho xóa bất kỳ tài khoản Admin nào
            // ========================================================
            if (tk.VaiTro == "Admin")
            {
                TempData["Error"] = "Không thể xóa tài khoản Admin! Nếu muốn vô hiệu hóa, vui lòng dùng chức năng 'Sửa' để Khóa tài khoản.";
                return RedirectToAction(nameof(DanhSachTaiKhoan));
            }

            // 3. Kiểm tra xem khách hàng này đã có lịch sử đặt phòng chưa
            if (tk.KhachHang != null && tk.KhachHang.PhieuDatPhongs != null && tk.KhachHang.PhieuDatPhongs.Any())
            {
                TempData["Error"] = "Không thể xóa! Tài khoản này đã có lịch sử đặt phòng. Hãy dùng chức năng 'Sửa' để Khóa tài khoản.";
                return RedirectToAction(nameof(DanhSachTaiKhoan));
            }

            // 4. Tiến hành xóa (Phải xóa Khách Hàng trước để tránh lỗi Khóa Ngoại)
            if (tk.KhachHang != null)
            {
                _context.KhachHangs.Remove(tk.KhachHang);
            }
            _context.TaiKhoans.Remove(tk);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa tài khoản {tk.TenDangNhap} thành công.";
            return RedirectToAction(nameof(DanhSachTaiKhoan));
        }
    }


}