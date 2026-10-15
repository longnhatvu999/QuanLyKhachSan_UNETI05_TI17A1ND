// ============================================================
// Họ và tên: Vũ Long Nhật
// Mã sinh viên: 23203100081
// Nội dung thực hiện: Seed dữ liệu mẫu theo yêu cầu mục 15
// Module: Chung
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using QuanLyKhachSan_UNETI05_TI17A1ND.Models;

namespace QuanLyKhachSan_UNETI05_TI17A1ND.Data
{
    public static class DbInitializer
    {
        public static void Initialize(QuanLyKhachSan_UNETI05_TI17A1NDContext context)
        {
            context.Database.EnsureCreated();

            // Nếu đã có dữ liệu → bỏ qua
            if (context.TaiKhoans.Any()) return;

            // ============ 1. TÀI KHOẢN ADMIN ============
            var admins = new List<TaiKhoan>
            {
                new TaiKhoan { TenDangNhap = "admin",   MatKhau = "admin123", HoTen = "Quản Trị Viên 1", Email = "admin1@ks.com", VaiTro = "Admin", TrangThai = true },
                new TaiKhoan { TenDangNhap = "admin02", MatKhau = "admin123", HoTen = "Quản Trị Viên 2", Email = "admin2@ks.com", VaiTro = "Admin", TrangThai = true }
            };
            context.TaiKhoans.AddRange(admins);
            context.SaveChanges();

            // ============ 2. LOẠI PHÒNG (5 loại) ============
            var loaiPhongs = new List<LoaiPhong>
            {
                new LoaiPhong { TenLoaiPhong = "Standard", SoNguoiToiDa = 2, DonGiaDem = 500000,  MoTa = "Phòng tiêu chuẩn",        TrangThai = true },
                new LoaiPhong { TenLoaiPhong = "Superior", SoNguoiToiDa = 2, DonGiaDem = 800000,  MoTa = "Phòng cao cấp",           TrangThai = true },
                new LoaiPhong { TenLoaiPhong = "Deluxe",   SoNguoiToiDa = 3, DonGiaDem = 1200000, MoTa = "Phòng sang trọng",        TrangThai = true },
                new LoaiPhong { TenLoaiPhong = "Suite",    SoNguoiToiDa = 4, DonGiaDem = 2000000, MoTa = "Phòng Suite rộng rãi",    TrangThai = true },
                new LoaiPhong { TenLoaiPhong = "Family",   SoNguoiToiDa = 5, DonGiaDem = 1500000, MoTa = "Phòng dành cho gia đình", TrangThai = true }
            };
            context.LoaiPhongs.AddRange(loaiPhongs);
            context.SaveChanges();

            // ============ 3. PHÒNG (20 phòng) ============
            var phongs = new List<Phong>();
            int[] soPhongTheoTang = { 101, 102, 103, 104, 201, 202, 203, 204, 301, 302, 303, 304, 401, 402, 403, 404, 501, 502, 503, 504 };
            int[] loaiTheoTang = { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5 };

            for (int i = 0; i < 20; i++)
            {
                int soPhong = soPhongTheoTang[i];
                int tang = soPhong / 100;
                int maLoai = loaiTheoTang[i];
                var loai = loaiPhongs[maLoai - 1];

                phongs.Add(new Phong
                {
                    SoPhong = soPhong.ToString(),
                    MaLoaiPhong = loai.MaLoaiPhong,
                    Tang = tang,
                    DonGiaDem = loai.DonGiaDem,
                    MoTa = $"Phòng {soPhong} tầng {tang} - {loai.TenLoaiPhong}",
                    TrangThai = "Sẵn sàng"
                });
            }
            context.Phongs.AddRange(phongs);
            context.SaveChanges();

            // ============ 4. KHÁCH HÀNG (15 khách) + TÀI KHOẢN ============
            var ho = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý", "Trịnh", "Đinh" };
            var ten = new[] { "An", "Bình", "Cường", "Dũng", "Em", "Phương", "Giang", "Hùng", "Inh", "Khánh", "Linh", "Minh", "Nam", "Oanh", "Phúc" };

            var khachHangs = new List<KhachHang>();
            for (int i = 0; i < 15; i++)
            {
                string hoTen = $"{ho[i]} Văn {ten[i]}";
                string email = $"kh{i + 1}@gmail.com";
                string sdt = $"09{i + 10000000}";
                string tenDangNhap = $"kh{i + 1}";

                var tk = new TaiKhoan
                {
                    TenDangNhap = tenDangNhap,
                    MatKhau = "123456",
                    HoTen = hoTen,
                    Email = email,
                    VaiTro = "KhachHang",
                    TrangThai = true
                };
                context.TaiKhoans.Add(tk);
                context.SaveChanges();

                khachHangs.Add(new KhachHang
                {
                    MaTaiKhoan = tk.MaTaiKhoan,
                    HoTen = hoTen,
                    NgaySinh = new DateTime(1990 + i, (i % 12) + 1, (i % 28) + 1),
                    GioiTinh = (i % 2 == 0) ? "Nam" : "Nữ",
                    SoDienThoai = sdt,
                    Email = email,
                    CCCD = $"01234567{i:D3}",
                    DiaChi = $"{i + 1} Đường ABC, Hà Nội",
                    NgayDangKy = DateTime.Now.AddDays(-i * 5),
                    TrangThai = true
                });
            }
            context.KhachHangs.AddRange(khachHangs);
            context.SaveChanges();

            // ============ 5. PHIẾU ĐẶT PHÒNG (18 phiếu) + CHI TIẾT ============
            var random = new Random(42);

            for (int i = 0; i < 18; i++)
            {
                var kh = khachHangs[i % 15];
                var phong = phongs[random.Next(0, 20)];

                DateTime ngayNhan = DateTime.Today.AddDays(random.Next(1, 30));
                DateTime ngayTra = ngayNhan.AddDays(random.Next(1, 5));
                int soDem = (ngayTra - ngayNhan).Days;
                decimal thanhTien = soDem * phong.DonGiaDem;

                string trangThai;
                if (i < 5) trangThai = "Chờ xác nhận";
                else if (i < 9) trangThai = "Đã xác nhận";
                else if (i < 12) trangThai = "Đã nhận phòng";
                else if (i < 15) trangThai = "Đã trả phòng";
                else trangThai = "Đã hủy";

                var phieu = new PhieuDatPhong
                {
                    MaKhachHang = kh.MaKhachHang,
                    NgayDat = DateTime.Now.AddDays(-random.Next(1, 20)),
                    NgayNhanDuKien = ngayNhan,
                    NgayTraDuKien = ngayTra,
                    TongTienDuKien = thanhTien,
                    TrangThai = trangThai
                };
                context.PhieuDatPhongs.Add(phieu);
                context.SaveChanges();

                context.ChiTietDatPhongs.Add(new ChiTietDatPhong
                {
                    MaPhieuDat = phieu.MaPhieuDat,
                    MaPhong = phong.MaPhong,
                    DonGiaDem = phong.DonGiaDem,
                    SoDem = soDem,
                    ThanhTien = thanhTien
                });
                context.SaveChanges();
            }

            // ============ 6. CẬP NHẬT TRẠNG THÁI PHÒNG ĐANG SỬ DỤNG ============
            var phongsDangSuDung = context.ChiTietDatPhongs
                .Where(c => c.PhieuDatPhong!.TrangThai == "Đã nhận phòng")
                .Select(c => c.MaPhong)
                .Distinct()
                .ToList();

            foreach (var maPhong in phongsDangSuDung)
            {
                var p = context.Phongs.Find(maPhong);
                if (p != null) p.TrangThai = "Đang sử dụng";
            }
            context.SaveChanges();
        }
    }
}