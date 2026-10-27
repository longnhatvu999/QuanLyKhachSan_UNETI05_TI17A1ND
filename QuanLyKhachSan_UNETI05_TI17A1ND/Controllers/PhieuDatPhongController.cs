// ============================================================
// Họ và tên: Trần Tuấn Mạnh và Bùi Hồng Anh
// Mã sinh viên: 23203100017 - 23203100057
// Nội dung thực hiện:
//   - SV3: Đặt phòng, kiểm tra trùng lịch, tính tiền tự động, xem phiếu của tôi
//   - SV4: Duyệt/Hủy/Nhận/Trả phòng (bổ sung ở Phần 6)
// Module: 3 & 4
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI05_TI17A1ND.Data;
using QuanLyKhachSan_UNETI05_TI17A1ND.Models;
using QuanLyKhachSan_UNETI05_TI17A1ND.ViewModels;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Controllers
{
    public class PhieuDatPhongController : Controller
    {
        private readonly QuanLyKhachSan_UNETI05_TI17A1NDContext _context;

        public PhieuDatPhongController(QuanLyKhachSan_UNETI05_TI17A1NDContext context)
        {
            _context = context;
        }

        // ==================================================
        // ĐẶT PHÒNG (GET) – Hiển thị form
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> DatPhong(int maPhong)
        {
            // Yêu cầu đăng nhập
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = $"/PhieuDatPhong/DatPhong?maPhong={maPhong}" });

            var phong = await _context.Phongs
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong);

            if (phong == null)
            {
                TempData["Error"] = "Phòng không tồn tại.";
                return RedirectToAction("Index", "Phong");
            }

            if (phong.TrangThai != "Sẵn sàng")
            {
                TempData["Error"] = "Phòng hiện không sẵn sàng để đặt.";
                return RedirectToAction("Index", "Phong");
            }

            var vm = new DatPhongViewModel
            {
                MaPhong = phong.MaPhong,
                SoPhong = phong.SoPhong,
                TenLoaiPhong = phong.LoaiPhong?.TenLoaiPhong,
                DonGiaDem = phong.DonGiaDem,
                NgayNhanDuKien = DateTime.Today.AddDays(1),
                NgayTraDuKien = DateTime.Today.AddDays(2)
            };

            return View(vm);
        }

        // ==================================================
        // ĐẶT PHÒNG (POST) – Xử lý nghiệp vụ
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatPhong(DatPhongViewModel vm)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            // Lấy thông tin phòng + khách hàng
            var phong = await _context.Phongs
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == vm.MaPhong);

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);

            if (phong == null || khachHang == null)
            {
                ModelState.AddModelError("", "Không tìm thấy phòng hoặc khách hàng.");
                return View(vm);
            }

            // Gán lại thông tin hiển thị
            vm.SoPhong = phong.SoPhong;
            vm.TenLoaiPhong = phong.LoaiPhong?.TenLoaiPhong;
            vm.DonGiaDem = phong.DonGiaDem;

            // ============ VALIDATION NGHIỆP VỤ ============

            // 1. Khách hàng đang hoạt động
            if (!khachHang.TrangThai)
            {
                ModelState.AddModelError("", "Tài khoản khách hàng đã bị khóa.");
                return View(vm);
            }

            // 2. Phòng đang phục vụ
            if (phong.TrangThai != "Sẵn sàng")
            {
                ModelState.AddModelError("", "Phòng hiện không sẵn sàng để đặt.");
                return View(vm);
            }

            // 3. Ngày nhận không trước ngày hiện tại
            if (vm.NgayNhanDuKien.Date < DateTime.Today)
            {
                ModelState.AddModelError("NgayNhanDuKien", "Ngày nhận phòng không được trước ngày hiện tại.");
                return View(vm);
            }

            // 4. Ngày trả phải sau ngày nhận
            if (vm.NgayTraDuKien.Date <= vm.NgayNhanDuKien.Date)
            {
                ModelState.AddModelError("NgayTraDuKien", "Ngày trả phòng phải sau ngày nhận phòng.");
                return View(vm);
            }

            // 5. KIỂM TRA TRÙNG LỊCH (Overlap)
            // Một phòng bị coi là trùng nếu tồn tại ChiTietDatPhong mà:
            //   - Cùng MaPhong
            //   - Phiếu đặt chưa bị hủy / chưa trả phòng
            //   - Khoảng [NgayNhan, NgayTra] giao với [NgayNhanDuKien, NgayTraDuKien]
            bool isTrungLich = await _context.ChiTietDatPhongs
                .Include(ct => ct.PhieuDatPhong)
                .AnyAsync(ct =>
                    ct.MaPhong == vm.MaPhong &&
                    ct.PhieuDatPhong != null &&
                    ct.PhieuDatPhong.TrangThai != "Đã hủy" &&
                    ct.PhieuDatPhong.TrangThai != "Đã trả phòng" &&
                    vm.NgayNhanDuKien.Date < ct.PhieuDatPhong.NgayTraDuKien.Date &&
                    vm.NgayTraDuKien.Date > ct.PhieuDatPhong.NgayNhanDuKien.Date);

            if (isTrungLich)
            {
                ModelState.AddModelError("", "Phòng đã được đặt trong khoảng thời gian này. Vui lòng chọn ngày khác.");
                return View(vm);
            }

            // ============ TÍNH TOÁN (hệ thống tự tính, KHÔNG cho khách nhập) ============
            int soDem = (vm.NgayTraDuKien.Date - vm.NgayNhanDuKien.Date).Days;
            decimal thanhTien = soDem * phong.DonGiaDem;

            // ============ LƯU PHIẾU ĐẶT ============
            var phieu = new PhieuDatPhong
            {
                MaKhachHang = khachHang.MaKhachHang,
                NgayDat = DateTime.Now,
                NgayNhanDuKien = vm.NgayNhanDuKien.Date,
                NgayTraDuKien = vm.NgayTraDuKien.Date,
                TongTienDuKien = thanhTien,
                TrangThai = "Chờ xác nhận"
            };
            _context.PhieuDatPhongs.Add(phieu);
            await _context.SaveChangesAsync();

            // ============ LƯU CHI TIẾT ============
            var chiTiet = new ChiTietDatPhong
            {
                MaPhieuDat = phieu.MaPhieuDat,
                MaPhong = phong.MaPhong,
                DonGiaDem = phong.DonGiaDem,
                SoDem = soDem,
                ThanhTien = thanhTien
            };
            _context.ChiTietDatPhongs.Add(chiTiet);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đặt phòng {phong.SoPhong} thành công! Vui lòng chờ Admin xác nhận.";
            return RedirectToAction(nameof(Details), new { id = phieu.MaPhieuDat });
        }

        // ==================================================
        // KHÁCH HÀNG: Xem phiếu của tôi
        // ==================================================
        public async Task<IActionResult> CuaToi(string? trangThai)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            var kh = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);

            if (kh == null) return NotFound();

            ViewBag.TrangThai = trangThai;

            var query = _context.PhieuDatPhongs
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(ct => ct.Phong)
                        .ThenInclude(ph => ph!.LoaiPhong)
                .Where(p => p.MaKhachHang == kh.MaKhachHang)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(p => p.TrangThai == trangThai);

            var ds = await query
                .OrderByDescending(p => p.NgayDat)
                .ToListAsync();

            ViewBag.DanhSachTrangThai = new[]
            {
                "Chờ xác nhận", "Đã xác nhận", "Đã nhận phòng", "Đã trả phòng", "Đã hủy"
            };

            return View(ds);
        }

        // ==================================================
        // CHI TIẾT PHIẾU (Admin & Khách đều xem được – nhưng khách chỉ xem của mình)
        // ==================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phieu = await _context.PhieuDatPhongs
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(ct => ct.Phong)
                        .ThenInclude(ph => ph!.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhieuDat == id);

            if (phieu == null) return NotFound();

            // Nếu là Khách hàng → chỉ được xem phiếu của chính mình
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "Admin")
            {
                var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
                if (maTaiKhoan == null)
                    return RedirectToAction("DangNhap", "TaiKhoan");

                var kh = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);

                if (kh == null || phieu.MaKhachHang != kh.MaKhachHang)
                    return RedirectToAction("TuChoi", "TaiKhoan");
            }

            return View(phieu);
        }

        // ==================================================
        // KHÁCH HÀNG: Hủy phiếu (chỉ khi đang Chờ xác nhận)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyPhieu(int id)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var phieu = await _context.PhieuDatPhongs
                .Include(p => p.KhachHang)
                .FirstOrDefaultAsync(p => p.MaPhieuDat == id);

            if (phieu == null) return NotFound();

            // Kiểm tra quyền
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "Admin")
            {
                var kh = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);

                if (kh == null || phieu.MaKhachHang != kh.MaKhachHang)
                    return RedirectToAction("TuChoi", "TaiKhoan");

                // Khách chỉ hủy được khi phiếu đang Chờ xác nhận
                if (phieu.TrangThai != "Chờ xác nhận")
                {
                    TempData["Error"] = "Chỉ có thể hủy phiếu đang ở trạng thái 'Chờ xác nhận'.";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }

            // Không cho hủy khi đã nhận/trả phòng
            if (phieu.TrangThai == "Đã nhận phòng" || phieu.TrangThai == "Đã trả phòng")
            {
                TempData["Error"] = "Không thể hủy phiếu đã nhận/trả phòng.";
                return RedirectToAction(nameof(Details), new { id });
            }

            phieu.TrangThai = "Đã hủy";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hủy phiếu đặt phòng thành công.";
            return RedirectToAction(nameof(Details), new { id });
        }


        // ==================================================
        // KHÁCH HÀNG: Lịch sử lưu trú (chỉ phiếu Đã trả phòng)
        // ==================================================
        public async Task<IActionResult> LichSuLuuTru()
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            var kh = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);

            if (kh == null) return NotFound();

            var lichSu = await _context.PhieuDatPhongs
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(ct => ct.Phong)
                        .ThenInclude(ph => ph!.LoaiPhong)
                .Where(p => p.MaKhachHang == kh.MaKhachHang && p.TrangThai == "Đã trả phòng")
                .OrderByDescending(p => p.NgayTraDuKien)
                .ToListAsync();

            return View(lichSu);
        }

        //ADMIN:Quản lý và lọc phiếu
        public async Task<IActionResult> Index( string? searchString,string? trangThai,DateTime? tuNgay,DateTime? denNgay)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");

            var query = _context.PhieuDatPhongs
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(ct => ct.Phong)
                .AsQueryable();

            // Tìm theo tên khách hàng
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(p =>
                    p.KhachHang!.HoTen.Contains(searchString));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(p => p.TrangThai == trangThai);
            }

            // Lọc từ ngày nhận phòng dự kiến
            if (tuNgay.HasValue)
            {
                var ngayBatDau = tuNgay.Value.Date;
                query = query.Where(p =>
                    p.NgayNhanDuKien >= ngayBatDau);
            }

            // Lọc đến ngày nhận phòng dự kiến
            if (denNgay.HasValue)
            {
                var ngayKetThuc = denNgay.Value.Date.AddDays(1);
                query = query.Where(p =>
                    p.NgayNhanDuKien < ngayKetThuc);
            }

            ViewBag.DanhSachTrangThai = new[]
            {
        "Chờ xác nhận",
        "Đã xác nhận",
        "Đã nhận phòng",
        "Đã trả phòng",
        "Đã hủy"
    };

            var ds = await query
                .OrderByDescending(p => p.NgayDat)
                .ToListAsync();

            return View("Index_Admin", ds);
        }


        // ==================================================
        // ADMIN: Xác nhận phiếu đặt (Chờ xác nhận → Đã xác nhận)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanDatPhong(int id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            var phieu = await _context.PhieuDatPhongs.FindAsync(id);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai != "Chờ xác nhận")
            {
                TempData["Error"] = "Chỉ có thể xác nhận phiếu đang ở trạng thái 'Chờ xác nhận'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            phieu.TrangThai = "Đã xác nhận";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xác nhận phiếu #{id}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ==================================================
        // ADMIN: Xác nhận nhận phòng (Đã xác nhận → Đã nhận phòng)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanNhanPhong(int id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            var phieu = await _context.PhieuDatPhongs
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(ct => ct.Phong)
                .FirstOrDefaultAsync(p => p.MaPhieuDat == id);

            if (phieu == null) return NotFound();

            // Kiểm tra trạng thái phiếu
            if (phieu.TrangThai != "Đã xác nhận")
            {
                TempData["Error"] = "Chỉ có thể nhận phòng khi phiếu ở trạng thái 'Đã xác nhận'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Kiểm tra các phòng trong phiếu còn đủ điều kiện sử dụng
            if (phieu.ChiTietDatPhongs != null)
            {
                foreach (var ct in phieu.ChiTietDatPhongs)
                {
                    if (ct.Phong == null)
                    {
                        TempData["Error"] = "Không tìm thấy phòng trong phiếu.";
                        return RedirectToAction(nameof(Details), new { id });
                    }

                    if (ct.Phong.TrangThai != "Sẵn sàng")
                    {
                        TempData["Error"] = $"Phòng {ct.Phong.SoPhong} hiện không sẵn sàng (đang {ct.Phong.TrangThai}).";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                }
            }

            // Cập nhật trạng thái phiếu
            phieu.TrangThai = "Đã nhận phòng";

            // Cập nhật trạng thái các phòng → "Đang sử dụng"
            if (phieu.ChiTietDatPhongs != null)
            {
                foreach (var ct in phieu.ChiTietDatPhongs)
                {
                    if (ct.Phong != null)
                        ct.Phong.TrangThai = "Đang sử dụng";
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xác nhận nhận phòng cho phiếu #{id}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ==================================================
        // ADMIN: Xác nhận trả phòng (Đã nhận phòng → Đã trả phòng)
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanTraPhong(int id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            var phieu = await _context.PhieuDatPhongs
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(ct => ct.Phong)
                .FirstOrDefaultAsync(p => p.MaPhieuDat == id);

            if (phieu == null) return NotFound();

            if (phieu.TrangThai != "Đã nhận phòng")
            {
                TempData["Error"] = "Chỉ có thể trả phòng khi phiếu ở trạng thái 'Đã nhận phòng'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Cập nhật trạng thái phiếu
            phieu.TrangThai = "Đã trả phòng";

            // Cập nhật trạng thái các phòng → "Sẵn sàng"
            if (phieu.ChiTietDatPhongs != null)
            {
                foreach (var ct in phieu.ChiTietDatPhongs)
                {
                    if (ct.Phong != null)
                        ct.Phong.TrangThai = "Sẵn sàng";
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xác nhận trả phòng cho phiếu #{id}.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}