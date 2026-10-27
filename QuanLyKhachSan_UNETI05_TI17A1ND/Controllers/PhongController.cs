
// ============================================================
// Họ và tên: Vũ Đức Huy
// Mã sinh viên: 23203100009
// Nội dung thực hiện: Quản lý phòng, tìm kiếm, lọc, sắp xếp và phân trang
// Module: 2
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI05_TI17A1ND.Data;
using QuanLyKhachSan_UNETI05_TI17A1ND.Models;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Controllers
{
    public class PhongController : Controller
    {
        private readonly QuanLyKhachSan_UNETI05_TI17A1NDContext _context;

        public PhongController(
            QuanLyKhachSan_UNETI05_TI17A1NDContext context)
        {
            _context = context;
        }

        // ==================================================
        // INDEX: Tìm kiếm, lọc, sắp xếp và phân trang
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
            // Lưu điều kiện tìm kiếm để hiển thị lại trên giao diện
            ViewBag.SearchString = searchString;
            ViewBag.MaLoaiPhong = maLoaiPhong;
            ViewBag.Tang = tang;
            ViewBag.TrangThai = trangThai;
            ViewBag.MinGia = minGia;
            ViewBag.MaxGia = maxGia;
            ViewBag.SortOrder = sortOrder;

            // 1. Lấy danh sách phòng
            var query = _context.Phongs
                .Include(p => p.LoaiPhong)
                .AsQueryable();

            // Khách hàng chỉ được xem phòng sẵn sàng
            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            if (vaiTro != "Admin")
            {
                query = query.Where(p => p.TrangThai == "Sẵn sàng");
            }

            // 2. Tìm kiếm theo số phòng hoặc tên loại phòng
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                query = query.Where(p =>
                    p.SoPhong.Contains(searchString) ||
                    p.LoaiPhong!.TenLoaiPhong.Contains(searchString));
            }

            // 3. Lọc theo loại phòng
            if (maLoaiPhong.HasValue)
            {
                query = query.Where(
                    p => p.MaLoaiPhong == maLoaiPhong.Value);
            }

            // Lọc theo tầng
            if (tang.HasValue)
            {
                query = query.Where(
                    p => p.Tang == tang.Value);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(
                    p => p.TrangThai == trangThai);
            }

            // Lọc theo giá thấp nhất
            if (minGia.HasValue)
            {
                query = query.Where(
                    p => p.DonGiaDem >= minGia.Value);
            }

            // Lọc theo giá cao nhất
            if (maxGia.HasValue)
            {
                query = query.Where(
                    p => p.DonGiaDem <= maxGia.Value);
            }

            // 4. Sắp xếp
            // SoPhong là chuỗi nên chuyển sang số để sắp xếp đúng.
            // Mặc định luôn ưu tiên tầng thấp trước tầng cao.
            switch (sortOrder)
            {
                case "soPhong_asc":
                    query = query
                        .OrderBy(p => p.Tang)
                        .ThenBy(p => Convert.ToInt32(p.SoPhong))
                        .ThenBy(p => p.MaPhong);
                    break;

                case "soPhong_desc":
                    query = query
                        .OrderBy(p => p.Tang)
                        .ThenByDescending(p => Convert.ToInt32(p.SoPhong))
                        .ThenBy(p => p.MaPhong);
                    break;

                case "tang_asc":
                    query = query
                        .OrderBy(p => p.Tang)
                        .ThenBy(p => Convert.ToInt32(p.SoPhong))
                        .ThenBy(p => p.MaPhong);
                    break;

                case "tang_desc":
                    query = query
                        .OrderByDescending(p => p.Tang)
                        .ThenBy(p => Convert.ToInt32(p.SoPhong))
                        .ThenBy(p => p.MaPhong);
                    break;

                case "gia_asc":
                    query = query
                        .OrderBy(p => p.DonGiaDem)
                        .ThenBy(p => p.Tang)
                        .ThenBy(p => p.MaPhong);
                    break;

                case "gia_desc":
                    query = query
                        .OrderByDescending(p => p.DonGiaDem)
                        .ThenBy(p => p.Tang)
                        .ThenBy(p => p.MaPhong);
                    break;

                default:
                    // Sắp xếp mặc định theo tầng tăng dần
                    query = query
                        .OrderBy(p => p.Tang)
                        .ThenBy(p => Convert.ToInt32(p.SoPhong))
                        .ThenBy(p => p.MaPhong);
                    break;
            }

            // 5. Phân trang
            const int pageSize = 6;

            int totalItems = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize);

            // Đảm bảo số trang hợp lệ
            if (totalPages == 0)
            {
                page = 1;
            }
            else
            {
                page = Math.Clamp(page, 1, totalPages);
            }

            // Lấy danh sách phòng của trang hiện tại
            var danhSachPhong = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 6. Chuẩn bị danh sách loại phòng
            ViewBag.DanhSachLoaiPhong = new SelectList(
                await _context.LoaiPhongs
                    .Where(l => l.TrangThai)
                    .ToListAsync(),
                "MaLoaiPhong",
                "TenLoaiPhong",
                maLoaiPhong);

            // Danh sách tầng
            var dsTang = await _context.Phongs
                .Select(p => p.Tang)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            ViewBag.DanhSachTang = new SelectList(
                dsTang,
                tang);

            // Danh sách trạng thái
            var dsTrangThai = new[]
            {
                new { Value = "Sẵn sàng", Text = "Sẵn sàng" },
                new { Value = "Đang sử dụng", Text = "Đang sử dụng" },
                new { Value = "Tạm ngừng", Text = "Tạm ngừng" }
            };

            ViewBag.DanhSachTrangThai = new SelectList(
                dsTrangThai,
                "Value",
                "Text",
                trangThai);

            // 7. Thông tin phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(danhSachPhong);
        }

        // ==================================================
        // DETAILS: Xem chi tiết phòng
        // ==================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phong = await _context.Phongs
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phong == null)
            {
                return NotFound();
            }

            return View(phong);
        }

        // ==================================================
        // CREATE: Thêm phòng - chỉ Admin
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("TuChoi", "TaiKhoan");
            }

            await LoadDropdownAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Phong phong)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("TuChoi", "TaiKhoan");
            }

            // Kiểm tra số phòng trùng
            if (await _context.Phongs.AnyAsync(
                p => p.SoPhong == phong.SoPhong))
            {
                ModelState.AddModelError(
                    "SoPhong",
                    "Số phòng đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownAsync(
                    phong.MaLoaiPhong,
                    phong.TrangThai);

                return View(phong);
            }

            _context.Phongs.Add(phong);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Thêm phòng {phong.SoPhong} thành công!";

            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // EDIT: Sửa phòng - chỉ Admin
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("TuChoi", "TaiKhoan");
            }

            if (id == null)
            {
                return NotFound();
            }

            var phong = await _context.Phongs.FindAsync(id);

            if (phong == null)
            {
                return NotFound();
            }

            await LoadDropdownAsync(
                phong.MaLoaiPhong,
                phong.TrangThai);

            return View(phong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Phong phong)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("TuChoi", "TaiKhoan");
            }

            if (id != phong.MaPhong)
            {
                return NotFound();
            }

            // Kiểm tra trùng số phòng, bỏ qua chính phòng đang sửa
            if (await _context.Phongs.AnyAsync(
                p => p.SoPhong == phong.SoPhong &&
                     p.MaPhong != id))
            {
                ModelState.AddModelError(
                    "SoPhong",
                    "Số phòng đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownAsync(
                    phong.MaLoaiPhong,
                    phong.TrangThai);

                return View(phong);
            }

            try
            {
                _context.Update(phong);
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    $"Cập nhật phòng {phong.SoPhong} thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                bool phongTonTai = await _context.Phongs
                    .AnyAsync(p => p.MaPhong == id);

                if (!phongTonTai)
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // DELETE: Xác nhận xóa phòng - chỉ Admin
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("TuChoi", "TaiKhoan");
            }

            if (id == null)
            {
                return NotFound();
            }

            var phong = await _context.Phongs
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phong == null)
            {
                return NotFound();
            }

            return View(phong);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("TuChoi", "TaiKhoan");
            }

            var phong = await _context.Phongs
                .Include(p => p.ChiTietDatPhongs)
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phong == null)
            {
                return NotFound();
            }

            // Không cho xóa phòng đã có lượt đặt
            if (phong.ChiTietDatPhongs != null &&
                phong.ChiTietDatPhongs.Any())
            {
                TempData["Error"] =
                    "Không thể xóa phòng vì đã có lượt đặt phòng.";

                return RedirectToAction(nameof(Index));
            }

            _context.Phongs.Remove(phong);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Xóa phòng {phong.SoPhong} thành công!";

            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // HELPER: Nạp dữ liệu dropdown cho Create/Edit
        // ==================================================
        private async Task LoadDropdownAsync(
            int? maLoaiPhong = null,
            string? trangThai = null)
        {
            ViewBag.DanhSachLoaiPhong = new SelectList(
                await _context.LoaiPhongs
                    .Where(l => l.TrangThai)
                    .ToListAsync(),
                "MaLoaiPhong",
                "TenLoaiPhong",
                maLoaiPhong);

            var dsTrangThai = new[]
            {
                "Sẵn sàng",
                "Đang sử dụng",
                "Tạm ngừng"
            };

            ViewBag.DanhSachTrangThai = new SelectList(
                dsTrangThai,
                trangThai);
        }
    }
}