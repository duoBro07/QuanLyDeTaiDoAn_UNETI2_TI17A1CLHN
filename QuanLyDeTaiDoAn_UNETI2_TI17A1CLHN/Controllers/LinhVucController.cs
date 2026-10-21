using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;
using System.Data;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
    // Người thực hiện: Phạm Tùng Dương - MSSV: [điền MSSV]
    // Module 1: quản lý lĩnh vực đề tài.
    [SessionAuthorize("Admin")]
    public class LinhVucController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LinhVucController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.LinhVucs
                .AsNoTracking()
                .OrderBy(x => x.TenLinhVuc)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var linhVuc = await _context.LinhVucs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLinhVuc == id);

            if (linhVuc == null) return NotFound();
            return View(linhVuc);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new LinhVuc { TrangThai = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LinhVuc model)
        {
            model.TenLinhVuc = model.TenLinhVuc?.Trim() ?? string.Empty;
            model.MoTa = string.IsNullOrWhiteSpace(model.MoTa) ? null : model.MoTa.Trim();

            if (await _context.LinhVucs.AnyAsync(x => x.TenLinhVuc == model.TenLinhVuc))
            {
                ModelState.AddModelError(nameof(model.TenLinhVuc), "Tên lĩnh vực đã tồn tại.");
            }

            if (!ModelState.IsValid) return View(model);

            _context.LinhVucs.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm lĩnh vực thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var linhVuc = await _context.LinhVucs.FindAsync(id);
            if (linhVuc == null) return NotFound();

            return View(linhVuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LinhVuc model)
        {
            if (id != model.MaLinhVuc) return NotFound();

            model.TenLinhVuc = model.TenLinhVuc?.Trim() ?? string.Empty;
            model.MoTa = string.IsNullOrWhiteSpace(model.MoTa) ? null : model.MoTa.Trim();

            if (await _context.LinhVucs.AnyAsync(x =>
                    x.TenLinhVuc == model.TenLinhVuc && x.MaLinhVuc != model.MaLinhVuc))
            {
                ModelState.AddModelError(nameof(model.TenLinhVuc), "Tên lĩnh vực đã tồn tại.");
            }

            if (!ModelState.IsValid) return View(model);

            var entity = await _context.LinhVucs.FindAsync(id);
            if (entity == null) return NotFound();

            entity.TenLinhVuc = model.TenLinhVuc;
            entity.MoTa = model.MoTa;
            entity.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật lĩnh vực thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var linhVuc = await _context.LinhVucs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLinhVuc == id);

            if (linhVuc == null) return NotFound();

            ViewBag.IntegrationNote = "Hiện Module 2 chưa ghép. Khi có bảng GiangViens/DeTais, hệ thống sẽ tự kiểm tra dữ liệu liên quan trước khi xóa.";
            return View(linhVuc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var linhVuc = await _context.LinhVucs.FindAsync(id);
            if (linhVuc == null) return NotFound();

            if (await CoDuLieuModule2LienQuanAsync(id))
            {
                TempData["Error"] = "Không thể xóa lĩnh vực vì đang có giảng viên hoặc đề tài liên quan.";
                return RedirectToAction(nameof(Index));
            }

            _context.LinhVucs.Remove(linhVuc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa lĩnh vực thành công.";
            return RedirectToAction(nameof(Index));
        }

        // Demo tích hợp Module 2 không cần tạo Entity giả.
        // Khi Module 2 được merge đúng tên bảng/cột chuẩn, hàm này tự kiểm tra quan hệ.
        private async Task<bool> CoDuLieuModule2LienQuanAsync(int maLinhVuc)
        {
            var connection = _context.Database.GetDbConnection();
            var needClose = connection.State != ConnectionState.Open;

            if (needClose) await connection.OpenAsync();

            try
            {
                foreach (var table in new[] { "GiangViens", "GiangVien", "DeTais", "DeTai" })
                {
                    await using var existsCommand = connection.CreateCommand();
                    existsCommand.CommandText =
                        "SELECT CASE WHEN OBJECT_ID(@TableName, N'U') IS NULL THEN 0 ELSE 1 END";

                    var tableParameter = existsCommand.CreateParameter();
                    tableParameter.ParameterName = "@TableName";
                    tableParameter.Value = $"dbo.{table}";
                    existsCommand.Parameters.Add(tableParameter);

                    var tableExists = Convert.ToInt32(await existsCommand.ExecuteScalarAsync()) == 1;
                    if (!tableExists) continue;

                    await using var relationCommand = connection.CreateCommand();
                    relationCommand.CommandText =
                        $"SELECT CASE WHEN EXISTS (SELECT 1 FROM [dbo].[{table}] WHERE [MaLinhVuc] = @MaLinhVuc) THEN 1 ELSE 0 END";

                    var idParameter = relationCommand.CreateParameter();
                    idParameter.ParameterName = "@MaLinhVuc";
                    idParameter.Value = maLinhVuc;
                    relationCommand.Parameters.Add(idParameter);

                    var hasRelatedData = Convert.ToInt32(await relationCommand.ExecuteScalarAsync()) == 1;
                    if (hasRelatedData) return true;
                }

                return false;
            }
            finally
            {
                if (needClose) await connection.CloseAsync();
            }
        }
    }
}
