// ============================================================
// Họ và tên: Vũ Long Nhật
// Mã sinh viên: 23203100081
// Nội dung thực hiện: Quản lý loại phòng (CRUD + Validation + Trạng thái)
// Module: 1
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI05_TI17A1ND.Data;
using QuanLyKhachSan_UNETI05_TI17A1ND.Models;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Controllers
{
    public class LoaiPhongController : BaseController  // Kế thừa BaseController → chỉ Admin
    {
        private readonly QuanLyKhachSan_UNETI05_TI17A1NDContext _context;

        public LoaiPhongController(QuanLyKhachSan_UNETI05_TI17A1NDContext context)
        {
            _context = context;
        }

        // ==================== DANH SÁCH ====================
        public async Task<IActionResult> Index()
        {
            var ds = await _context.LoaiPhongs
                .OrderBy(l => l.MaLoaiPhong)
                .ToListAsync();
            return View(ds);
        }

        // ==================== CHI TIẾT ====================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var loai = await _context.LoaiPhongs
                .Include(l => l.Phongs)
                .FirstOrDefaultAsync(l => l.MaLoaiPhong == id);

            if (loai == null) return NotFound();
            return View(loai);
        }

        // ==================== THÊM ====================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoaiPhong loai)
        {
            // Kiểm tra tên không trùng
            if (await _context.LoaiPhongs.AnyAsync(l => l.TenLoaiPhong == loai.TenLoaiPhong))
            {
                ModelState.AddModelError("TenLoaiPhong", "Tên loại phòng đã tồn tại.");
            }

            if (!ModelState.IsValid)
                return View(loai);

            _context.LoaiPhongs.Add(loai);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm loại phòng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ==================== SỬA ====================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var loai = await _context.LoaiPhongs.FindAsync(id);
            if (loai == null) return NotFound();

            return View(loai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LoaiPhong loai)
        {
            if (id != loai.MaLoaiPhong) return NotFound();

            // Kiểm tra tên không trùng (ngoại trừ chính nó)
            if (await _context.LoaiPhongs.AnyAsync(l => l.TenLoaiPhong == loai.TenLoaiPhong && l.MaLoaiPhong != id))
            {
                ModelState.AddModelError("TenLoaiPhong", "Tên loại phòng đã tồn tại.");
            }

            if (!ModelState.IsValid)
                return View(loai);

            try
            {
                _context.Update(loai);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật loại phòng thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.LoaiPhongs.Any(l => l.MaLoaiPhong == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ==================== XÓA ====================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var loai = await _context.LoaiPhongs
                .Include(l => l.Phongs)
                .FirstOrDefaultAsync(l => l.MaLoaiPhong == id);

            if (loai == null) return NotFound();
            return View(loai);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loai = await _context.LoaiPhongs
                .Include(l => l.Phongs)
                .FirstOrDefaultAsync(l => l.MaLoaiPhong == id);

            if (loai == null) return NotFound();

            // Không cho xóa nếu còn phòng liên quan
            if (loai.Phongs != null && loai.Phongs.Any())
            {
                TempData["Error"] = "Không thể xóa loại phòng vì vẫn còn phòng thuộc loại này.";
                return RedirectToAction(nameof(Index));
            }

            _context.LoaiPhongs.Remove(loai);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa loại phòng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ==================== CẬP NHẬT TRẠNG THÁI (AJAX) ====================
        [HttpPost]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var loai = await _context.LoaiPhongs.FindAsync(id);
            if (loai == null) return Json(new { success = false, message = "Không tìm thấy." });

            loai.TrangThai = !loai.TrangThai;
            await _context.SaveChangesAsync();

            return Json(new { success = true, trangThai = loai.TrangThai });
        }
    }
}