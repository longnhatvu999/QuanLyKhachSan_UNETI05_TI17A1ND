// ============================================================
// Họ và tên: Trần Tuấn Mạnh
// Mã sinh viên: 23203100017
// Nội dung thực hiện: Entity ChiTietDatPhong - Chi tiết đặt phòng
// Module: 3
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Models
{
    public class ChiTietDatPhong
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaChiTiet { get; set; }

        [Required]
        public int MaPhieuDat { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng")]
        [Display(Name = "Phòng")]
        public int MaPhong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        [Display(Name = "Đơn giá đêm")]
        public decimal DonGiaDem { get; set; }

        [Display(Name = "Số đêm")]
        public int SoDem { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        [Display(Name = "Thành tiền")]
        public decimal ThanhTien { get; set; }

        // ===== Navigation Properties =====
        [ForeignKey("MaPhieuDat")]
        public PhieuDatPhong? PhieuDatPhong { get; set; }

        [ForeignKey("MaPhong")]
        public Phong? Phong { get; set; }
    }
}