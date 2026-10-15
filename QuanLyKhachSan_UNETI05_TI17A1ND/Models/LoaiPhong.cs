// ============================================================
// Họ và tên: Vũ Long Nhật
// Mã sinh viên: 23203100081
// Nội dung thực hiện: Entity LoaiPhong - Quản lý loại phòng
// Module: 1
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Models
{
    public class LoaiPhong
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLoaiPhong { get; set; }

        [Required(ErrorMessage = "Tên loại phòng không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên loại phòng")]
        public string TenLoaiPhong { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số người tối đa không được để trống")]
        [Range(1, 20, ErrorMessage = "Số người tối đa phải từ 1 đến 20")]
        [Display(Name = "Số người tối đa")]
        public int SoNguoiToiDa { get; set; }

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

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // ===== Navigation Properties =====
        public ICollection<Phong>? Phongs { get; set; }
    }
}