
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// 1. CẤU HÌNH MVC
// =====================================================
builder.Services.AddControllersWithViews();

// =====================================================
// 2. CẤU HÌNH SQL SERVER
// =====================================================
var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Chưa cấu hình DefaultConnection " +
        "trong appsettings.json.");
}

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        options.UseSqlServer(connectionString);
    });

// =====================================================
// 3. CẤU HÌNH SESSION
// =====================================================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    options.Cookie.Name =
        ".QuanLyDeTaiDoAn.Session";

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;

    options.Cookie.SameSite =
        SameSiteMode.Lax;

    options.Cookie.SecurePolicy =
        CookieSecurePolicy.SameAsRequest;
});

// =====================================================
// 4. BUILD ỨNG DỤNG
// =====================================================
var app = builder.Build();

// =====================================================
// 5. CẤU HÌNH XỬ LÝ LỖI
// =====================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// =====================================================
// 6. CẤU HÌNH MIDDLEWARE
// =====================================================
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Session phải được kích hoạt trước khi
// các Controller và bộ lọc phân quyền sử dụng.
app.UseSession();

app.UseAuthorization();

// =====================================================
// 7. CẤU HÌNH ROUTE MẶC ĐỊNH
// =====================================================
app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=DangNhap}/{action=Index}/{id?}");

// =====================================================
// 8. KIỂM TRA DATABASE VÀ TẠO DỮ LIỆU MẪU
// =====================================================
if (app.Environment.IsDevelopment())
{
    using var scope =
        app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var logger = scope.ServiceProvider
        .GetRequiredService<ILogger<Program>>();

    try
    {
        // Kiểm tra có kết nối được SQL Server không.
        bool ketNoiThanhCong =
            await db.Database.CanConnectAsync();

        if (!ketNoiThanhCong)
        {
            throw new InvalidOperationException(
                "Không thể kết nối Database. " +
                "Hãy kiểm tra SQL Server và " +
                "DefaultConnection trong appsettings.json.");
        }

        // Kiểm tra Migration còn thiếu.
        // Không tự động sửa cấu trúc Database
        // để tránh thay đổi dữ liệu ngoài ý muốn.
        var pendingMigrations =
            (await db.Database
                .GetPendingMigrationsAsync())
            .ToList();

        if (pendingMigrations.Count > 0)
        {
            logger.LogWarning(
                "Database còn {SoLuong} Migration " +
                "chưa áp dụng: {DanhSach}",
                pendingMigrations.Count,
                string.Join(", ", pendingMigrations));

            throw new InvalidOperationException(
                "Database chưa được cập nhật đầy đủ. " +
                "Hãy chạy lệnh Update-Database " +
                "trong Package Manager Console " +
                "trước khi khởi động ứng dụng.");
        }

        // Kiểm tra bảng chính trước khi Seed.
        await db.TaiKhoans.AnyAsync();
        await db.LinhVucs.AnyAsync();
        await db.GiangViens.AnyAsync();
        await db.DeTais.AnyAsync();
        await db.SinhViens.AnyAsync();
        await db.DangKyDeTais.AnyAsync();
        await db.PhanCongHuongDans.AnyAsync();

        // Tạo tài khoản quản trị mẫu.
        await Module1SeedData.SeedAsync(db);

        // Tạo dữ liệu các Module còn lại.
        await Module2SeedData.SeedAsync(db);

        logger.LogInformation(
            "Kiểm tra Database và khởi tạo " +
            "dữ liệu mẫu hoàn tất.");
    }
    catch (Exception ex)
    {
        logger.LogError(
            ex,
            "Lỗi kiểm tra Database " +
            "hoặc khởi tạo dữ liệu mẫu.");

        throw;
    }
}

// =====================================================
// 9. CHẠY ỨNG DỤNG
// =====================================================
app.Run();
