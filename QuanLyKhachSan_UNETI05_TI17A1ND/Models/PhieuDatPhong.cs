// ============================================================
// Họ và tên: Trần Tuấn Mạnh và Bùi Hồng Anh
// Mã sinh viên: 23203100017 - 23203100057
// Nội dung thực hiện:
//   - SV3: Entity + nghiệp vụ đặt phòng, chi tiết đặt phòng
//   - SV4: Nghiệp vụ duyệt, nhận/trả phòng, thống kê
// Module: 3 & 4
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Models
{
    public class PhieuDatPhong
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhieuDat { get; set; }

        [Required]
        [Display(Name = "Khách hàng")]
        public int MaKhachHang { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Ngày nhận dự kiến không được để trống")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày nhận dự kiến")]
        public DateTime NgayNhanDuKien { get; set; }

        [Required(ErrorMessage = "Ngày trả dự kiến không được để trống")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày trả dự kiến")]
        public DateTime NgayTraDuKien { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        [Display(Name = "Tổng tiền dự kiến")]
        public decimal TongTienDuKien { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Chờ xác nhận";
        // Các giá trị: "Chờ xác nhận", "Đã xác nhận", "Đã nhận phòng", "Đã trả phòng", "Đã hủy"

        // ===== Navigation Properties =====
        [ForeignKey("MaKhachHang")]
        public KhachHang? KhachHang { get; set; }

        public ICollection<ChiTietDatPhong>? ChiTietDatPhongs { get; set; }
    }
}