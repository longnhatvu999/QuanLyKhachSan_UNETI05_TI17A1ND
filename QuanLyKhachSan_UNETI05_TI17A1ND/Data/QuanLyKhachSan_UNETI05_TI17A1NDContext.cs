// ============================================================
// Họ và tên: Vũ Long Nhật
// Mã sinh viên: 23203100081
// Nội dung thực hiện: DbContext - Cấu hình toàn bộ Entity, quan hệ, ràng buộc
// Module: Chung
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI05_TI17A1ND.Models;
using System.Reflection.Emit;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Data
{
    public class QuanLyKhachSan_UNETI05_TI17A1NDContext : DbContext
    {
        public QuanLyKhachSan_UNETI05_TI17A1NDContext(DbContextOptions<QuanLyKhachSan_UNETI05_TI17A1NDContext> options)
            : base(options)
        {
        }

        // ===== DbSet =====
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<LoaiPhong> LoaiPhongs { get; set; }
        public DbSet<Phong> Phongs { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<PhieuDatPhong> PhieuDatPhongs { get; set; }
        public DbSet<ChiTietDatPhong> ChiTietDatPhongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== 1. TaiKhoan: TenDangNhap UNIQUE =====
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();

            // ===== 2. TaiKhoan (1) – (1) KhachHang =====
            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.TaiKhoan)
                .WithOne(t => t.KhachHang)
                .HasForeignKey<KhachHang>(k => k.MaTaiKhoan)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== 3. LoaiPhong (1) – (n) Phong =====
            modelBuilder.Entity<Phong>()
                .HasOne(p => p.LoaiPhong)
                .WithMany(l => l.Phongs)
                .HasForeignKey(p => p.MaLoaiPhong)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa LoaiPhong nếu còn Phong

            // ===== 4. Phong: SoPhong UNIQUE =====
            modelBuilder.Entity<Phong>()
                .HasIndex(p => p.SoPhong)
                .IsUnique();

            // ===== 5. LoaiPhong: TenLoaiPhong UNIQUE =====
            modelBuilder.Entity<LoaiPhong>()
                .HasIndex(l => l.TenLoaiPhong)
                .IsUnique();

            // ===== 6. KhachHang (1) – (n) PhieuDatPhong =====
            modelBuilder.Entity<PhieuDatPhong>()
                .HasOne(p => p.KhachHang)
                .WithMany(k => k.PhieuDatPhongs)
                .HasForeignKey(p => p.MaKhachHang)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== 7. PhieuDatPhong (1) – (n) ChiTietDatPhong =====
            modelBuilder.Entity<ChiTietDatPhong>()
                .HasOne(c => c.PhieuDatPhong)
                .WithMany(p => p.ChiTietDatPhongs)
                .HasForeignKey(c => c.MaPhieuDat)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== 8. Phong (1) – (n) ChiTietDatPhong =====
            modelBuilder.Entity<ChiTietDatPhong>()
                .HasOne(c => c.Phong)
                .WithMany(p => p.ChiTietDatPhongs)
                .HasForeignKey(c => c.MaPhong)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== 9. Index tối ưu truy vấn =====
            modelBuilder.Entity<PhieuDatPhong>()
                .HasIndex(p => p.TrangThai);

            modelBuilder.Entity<ChiTietDatPhong>()
                .HasIndex(c => new { c.MaPhong, c.MaPhieuDat });
        }
    }
}