// ============================================================
// Họ và tên: Vũ Đức Huy
// Mã sinh viên: 23203100009
// Nội dung thực hiện: Entity Phong - Quản lý phòng, tìm kiếm, lọc, phân trang
// Module: 2
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Models
{
    public class Phong
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhong { get; set; }

        [Required(ErrorMessage = "Số phòng không được để trống")]
        [StringLength(20)]
        [Display(Name = "Số phòng")]
        public string SoPhong { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại phòng")]
        [Display(Name = "Loại phòng")]
        public int MaLoaiPhong { get; set; }

        [Required(ErrorMessage = "Tầng không được để trống")]
        [Range(1, 50, ErrorMessage = "Tầng phải từ 1 đến 50")]
        [Display(Name = "Tầng")]
        public int Tang { get; set; }

        [Required(ErrorMessage = "Đơn giá đêm không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá đêm phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        [Display(Name = "Đơn giá đêm")]
        public decimal DonGiaDem { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [StringLength(30)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Sẵn sàng"; // Sẵn sàng / Đang sử dụng / Tạm ngừng

        // ===== Navigation Properties =====
        [ForeignKey("MaLoaiPhong")]
        public LoaiPhong? LoaiPhong { get; set; }

        public ICollection<ChiTietDatPhong>? ChiTietDatPhongs { get; set; }
    }
}