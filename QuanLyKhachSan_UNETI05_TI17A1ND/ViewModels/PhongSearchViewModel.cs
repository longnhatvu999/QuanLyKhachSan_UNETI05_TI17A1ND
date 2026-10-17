// ============================================================
// Họ và tên: [Vũ Đức Huy]
// Mã sinh viên: [23203100009]
// Nội dung thực hiện: ViewModel cho tìm kiếm, lọc, sắp xếp phòng
// Module: 2
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.ViewModels
{
    public class PhongSearchViewModel
    {
        // ===== Tiêu chí tìm kiếm =====
        public string? SearchString { get; set; }       // Tìm theo Số phòng hoặc Tên loại phòng
        public int? MaLoaiPhong { get; set; }           // Lọc theo loại phòng
        public int? Tang { get; set; }                  // Lọc theo tầng
        public string? TrangThai { get; set; }          // Lọc theo trạng thái
        public decimal? MinGia { get; set; }            // Lọc giá từ
        public decimal? MaxGia { get; set; }            // Lọc giá đến
        public string? SortOrder { get; set; }          // Sắp xếp

        // ===== Phân trang =====
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 5;          // Số phòng/trang

        // ===== Kết quả =====
        public List<Models.Phong> DanhSachPhong { get; set; } = new();

        // ===== Dropdown options =====
        public SelectList? DanhSachLoaiPhong { get; set; }
        public SelectList? DanhSachTang { get; set; }
        public SelectList? DanhSachTrangThai { get; set; }
    }
}