
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    public class DotDangKyForm
    {
        public string NamHoc { get; set; } = "";
        public int HocKy { get; set; }
        public bool DangMo { get; set; }
    }

    [PhanQuyen("Admin")]
    public class DotDangKyController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DotDangKyController> _logger;

        private static readonly SemaphoreSlim KhoaGhi =
            new SemaphoreSlim(1, 1);

        public DotDangKyController(
            ApplicationDbContext db,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<DotDangKyController> logger)
        {
            _db = db;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        private DotDangKyForm LayCauHinh()
        {
            return new DotDangKyForm
            {
                NamHoc = _configuration[
                    "DotDangKy:NamHoc"] ?? "",

                HocKy = _configuration.GetValue<int>(
                    "DotDangKy:HocKy"),

                DangMo = _configuration.GetValue<bool>(
                    "DotDangKy:DangMo")
            };
        }

        private async Task NapDanhSachNamHoc()
        {
            ViewBag.DanhSachNamHoc = await _db.DeTais
                .AsNoTracking()
                .Select(x => x.NamHoc)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();
        }

        // HIỂN THỊ CẤU HÌNH
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await NapDanhSachNamHoc();

            var model = LayCauHinh();

            if (TempData["Success"] != null)
                ViewBag.ThongBao = TempData["Success"];

            return View(model);
        }

        // LƯU CẤU HÌNH
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            DotDangKyForm model)
        {
            model.NamHoc = model.NamHoc?.Trim() ?? "";

            if (!Regex.IsMatch(
                model.NamHoc,
                @"^\d{4}-\d{4}$"))
            {
                ModelState.AddModelError(
                    nameof(model.NamHoc),
                    "Năm học phải có dạng 2026-2027.");
            }
            else
            {
                var nam = model.NamHoc.Split('-');

                int namBatDau = int.Parse(nam[0]);
                int namKetThuc = int.Parse(nam[1]);

                if (namKetThuc != namBatDau + 1)
                {
                    ModelState.AddModelError(
                        nameof(model.NamHoc),
                        "Năm kết thúc phải lớn hơn " +
                        "năm bắt đầu đúng một năm.");
                }
            }

            if (model.HocKy < 1 || model.HocKy > 3)
            {
                ModelState.AddModelError(
                    nameof(model.HocKy),
                    "Học kỳ phải từ 1 đến 3.");
            }

            // Không cho mở kỳ chưa có đề tài
            if (ModelState.IsValid && model.DangMo)
            {
                bool coDeTai = await _db.DeTais
                    .AnyAsync(x =>
                        x.NamHoc == model.NamHoc &&
                        x.HocKy == model.HocKy &&
                        x.TrangThai == "Mở đăng ký");

                if (!coDeTai)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể mở đợt đăng ký vì " +
                        "năm học/học kỳ này chưa có " +
                        "đề tài ở trạng thái Mở đăng ký.");
                }
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachNamHoc();
                return View(model);
            }

            await KhoaGhi.WaitAsync();

            try
            {
                string duongDan = Path.Combine(
                    _environment.ContentRootPath,
                    "appsettings.json");

                if (!System.IO.File.Exists(duongDan))
                {
                    ModelState.AddModelError(
                        "",
                        "Không tìm thấy file appsettings.json.");

                    await NapDanhSachNamHoc();
                    return View(model);
                }

                string noiDung =
                    await System.IO.File.ReadAllTextAsync(
                        duongDan);

                var root = JsonNode.Parse(noiDung)
                    as JsonObject;

                if (root == null)
                {
                    throw new InvalidOperationException(
                        "Cấu trúc appsettings.json không hợp lệ.");
                }

                // Chỉ cập nhật DotDangKy.
                // Không xóa ConnectionStrings hoặc Logging.
                root["DotDangKy"] = new JsonObject
                {
                    ["NamHoc"] = model.NamHoc,
                    ["HocKy"] = model.HocKy,
                    ["DangMo"] = model.DangMo
                };

                string json = root.ToJsonString(
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                // Ghi ra file tạm trước, tránh ghi
                // trực tiếp file cấu hình đang sử dụng.
                string fileTam = duongDan + ".tmp";

                try
                {
                    await System.IO.File.WriteAllTextAsync(
                        fileTam,
                        json);

                    System.IO.File.Move(
                        fileTam,
                        duongDan,
                        overwrite: true);
                }
                finally
                {
                    if (System.IO.File.Exists(fileTam))
                        System.IO.File.Delete(fileTam);
                }

                // Nạp lại cấu hình để Controller đăng ký
                // có thể đọc giá trị mới ngay.
                if (_configuration is IConfigurationRoot rootConfig)
                {
                    rootConfig.Reload();
                }

                TempData["Success"] = model.DangMo
                    ? "Đã mở đợt đăng ký thành công."
                    : "Đã đóng đợt đăng ký thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Không thể lưu cấu hình đợt đăng ký.");

                ModelState.AddModelError(
                    "",
                    "Không thể lưu cấu hình. " +
                    "Hãy kiểm tra quyền ghi file " +
                    "appsettings.json và thử lại.");

                await NapDanhSachNamHoc();
                return View(model);
            }
            finally
            {
                KhoaGhi.Release();
            }
        }
    }
}
