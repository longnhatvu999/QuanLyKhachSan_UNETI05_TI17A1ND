// ============================================================
// Họ và tên: [Vũ Đức Huy]
// Mã sinh viên: [23203100009]
// Nội dung thực hiện: Quản lý phòng, tìm kiếm, lọc, sắp xếp và phân trang
// Module: 2
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI05_TI17A1ND.Data;
using QuanLyKhachSan_UNETI05_TI17A1ND.Models;
using QuanLyKhachSan_UNETI05_TI17A1ND.ViewModels;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Controllers
{
    public class PhongController : Controller  // Khách hàng cũng xem được → KHÔNG kế thừa BaseController
    {
        private readonly QuanLyKhachSan_UNETI05_TI17A1NDContext _context;

        public PhongController(QuanLyKhachSan_UNETI05_TI17A1NDContext context)
        {
            _context = context;
        }

        // ==================================================
        // INDEX: Tìm kiếm + Lọc + Sắp xếp + Phân trang
        // ==================================================
        public async Task<IActionResult> Index(
            string? searchString,
            int? maLoaiPhong,
            int? tang,
            string? trangThai,
            decimal? minGia,
            decimal? maxGia,
            string? sortOrder,
            int page = 1)
        {
            // ⚠️ Lưu điều kiện vào ViewBag để giữ khi chuyển trang
            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;
            ViewBag.MaLoaiPhong = maLoaiPhong;
            ViewBag.Tang = tang;
            ViewBag.TrangThai = trangThai;
            ViewBag.MinGia = minGia;
            ViewBag.MaxGia = maxGia;

            // ===== 1. BẮT ĐẦU TRUY VẤN =====
            var query = _context.Phongs
                .Include(p => p.LoaiPhong)
                .AsQueryable();

            // ===== 2. TÌM KIẾM =====
            // Nếu là Khách hàng → chỉ hiện phòng Sẵn sàng
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "Admin")
            {
                query = query.Where(p => p.TrangThai == "Sẵn sàng");
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(p =>
                    p.SoPhong.Contains(searchString) ||
                    p.LoaiPhong!.TenLoaiPhong.Contains(searchString));
            }

            // ===== 3. LỌC =====
            if (maLoaiPhong.HasValue)
                query = query.Where(p => p.MaLoaiPhong == maLoaiPhong.Value);

            if (tang.HasValue)
                query = query.Where(p => p.Tang == tang.Value);

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(p => p.TrangThai == trangThai);

            if (minGia.HasValue)
                query = query.Where(p => p.DonGiaDem >= minGia.Value);

            if (maxGia.HasValue)
                query = query.Where(p => p.DonGiaDem <= maxGia.Value);

            // ===== 4. SẮP XẾP =====
            query = sortOrder switch
            {
                "soPhong_asc" => query.OrderBy(p => p.SoPhong),
                "soPhong_desc" => query.OrderByDescending(p => p.SoPhong),
                "gia_asc" => query.OrderBy(p => p.DonGiaDem),
                "gia_desc" => query.OrderByDescending(p => p.DonGiaDem),
                "tang_asc" => query.OrderBy(p => p.Tang).ThenBy(p => p.SoPhong),
                "tang_desc" => query.OrderByDescending(p => p.Tang).ThenBy(p => p.SoPhong),
                _ => query.OrderBy(p => p.SoPhong)
            };

            // ===== 5. PHÂN TRANG =====
            const int pageSize = 6;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Đảm bảo page hợp lệ
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var danhSachPhong = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ===== 6. CHUẨN BỊ DROPDOWN =====
            ViewBag.DanhSachLoaiPhong = new SelectList(
                await _context.LoaiPhongs.ToListAsync(),
                "MaLoaiPhong", "TenLoaiPhong", maLoaiPhong);

            var dsTang = await _context.Phongs
                .Select(p => p.Tang).Distinct().OrderBy(t => t).ToListAsync();
            ViewBag.DanhSachTang = new SelectList(dsTang, tang);

            ViewBag.DanhSachTrangThai = new SelectList(new[]
            {
                new { Value = "Sẵn sàng",      Text = "Sẵn sàng" },
                new { Value = "Đang sử dụng",  Text = "Đang sử dụng" },
                new { Value = "Tạm ngừng",     Text = "Tạm ngừng" }
            }, "Value", "Text", trangThai);

            // ===== 7. TRUYỀN THÔNG TIN PHÂN TRANG =====
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(danhSachPhong);
        }

        // ==================================================
        // DETAILS: Chi tiết phòng
        // ==================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phong = await _context.Phongs
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phong == null) return NotFound();
            return View(phong);
        }

        // ==================================================
        // CREATE: Thêm phòng (chỉ Admin)
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Kiểm tra quyền Admin
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            await LoadDropdownAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Phong phong)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            // ===== VALIDATION NGHIỆP VỤ =====
            if (await _context.Phongs.AnyAsync(p => p.SoPhong == phong.SoPhong))
                ModelState.AddModelError("SoPhong", "Số phòng đã tồn tại.");

            if (!ModelState.IsValid)
            {
                await LoadDropdownAsync(phong.MaLoaiPhong);
                return View(phong);
            }

            _context.Phongs.Add(phong);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Thêm phòng {phong.SoPhong} thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // EDIT: Sửa phòng (chỉ Admin)
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            if (id == null) return NotFound();

            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null) return NotFound();

            await LoadDropdownAsync(phong.MaLoaiPhong);
            return View(phong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Phong phong)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            if (id != phong.MaPhong) return NotFound();

            // Kiểm tra số phòng không trùng (ngoại trừ chính nó)
            if (await _context.Phongs.AnyAsync(p => p.SoPhong == phong.SoPhong && p.MaPhong != id))
                ModelState.AddModelError("SoPhong", "Số phòng đã tồn tại.");

            if (!ModelState.IsValid)
            {
                await LoadDropdownAsync(phong.MaLoaiPhong);
                return View(phong);
            }

            try
            {
                _context.Update(phong);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Cập nhật phòng {phong.SoPhong} thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Phongs.Any(p => p.MaPhong == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // DELETE: Xóa phòng (chỉ Admin)
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            if (id == null) return NotFound();

            var phong = await _context.Phongs
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phong == null) return NotFound();
            return View(phong);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("TuChoi", "TaiKhoan");

            var phong = await _context.Phongs
                .Include(p => p.ChiTietDatPhongs)
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phong == null) return NotFound();

            // Không cho xóa nếu phòng đã có lượt đặt
            if (phong.ChiTietDatPhongs != null && phong.ChiTietDatPhongs.Any())
            {
                TempData["Error"] = "Không thể xóa phòng vì đã có lượt đặt phòng.";
                return RedirectToAction(nameof(Index));
            }

            _context.Phongs.Remove(phong);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Xóa phòng {phong.SoPhong} thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // HELPER: Load dropdown cho Create/Edit
        // ==================================================
        private async Task LoadDropdownAsync(int? maLoaiPhong = null, string? trangThai = null)
        {
            ViewBag.DanhSachLoaiPhong = new SelectList(
                await _context.LoaiPhongs.Where(l => l.TrangThai).ToListAsync(),
                "MaLoaiPhong", "TenLoaiPhong", maLoaiPhong);

            ViewBag.DanhSachTrangThai = new SelectList(new[]
            {
                "Sẵn sàng",
                "Đang sử dụng",
                "Tạm ngừng"
            }, trangThai);
        }
    }
}