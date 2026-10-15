// ============================================================
// Họ và tên: Vũ Long Nhật
// Mã sinh viên: 23203100081
// Nội dung thực hiện: Cấu hình Program.cs - DbContext, Session, Middleware
// Module: Chung
// Lớp: TI17A1ND - Nhóm 05
// ============================================================

using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI05_TI17A1ND.Data;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. Đăng ký MVC =====
builder.Services.AddControllersWithViews();

// ===== 2. Đăng ký DbContext =====
builder.Services.AddDbContext<QuanLyKhachSan_UNETI05_TI17A1NDContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== 3. Đăng ký Session =====
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ===== 4. Đăng ký HttpContextAccessor =====
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ===== 5. Seed dữ liệu mẫu =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<QuanLyKhachSan_UNETI05_TI17A1NDContext>();
    context.Database.Migrate();
    DbInitializer.Initialize(context);
}

// ===== 6. Pipeline =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠️ Session PHẢI đặt trước Authorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();