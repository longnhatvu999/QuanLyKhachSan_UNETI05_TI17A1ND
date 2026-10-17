// ============================================================
// Họ và tên:Trần Tuấn Mạnh
// Mã sinh viên: 23203100017
// Nội dung thực hiện: ViewModel cho chức năng đặt phòng
// Module: 3
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.ViewModels
{
    public class DatPhongViewModel
    {
        [Required]
        public int MaPhong { get; set; }

        [Display(Name = "Số phòng")]
        public string? SoPhong { get; set; }

        [Display(Name = "Loại phòng")]
        public string? TenLoaiPhong { get; set; }

        [Display(Name = "Đơn giá đêm")]
        public decimal DonGiaDem { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày nhận phòng")]
        public DateTime NgayNhanDuKien { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày trả phòng")]
        public DateTime NgayTraDuKien { get; set; } = DateTime.Today.AddDays(2);

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}