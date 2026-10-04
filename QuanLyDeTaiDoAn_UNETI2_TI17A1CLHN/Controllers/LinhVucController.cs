using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Filters;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Controllers
{
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
            var danhSach = await _context.LinhVucs
                .OrderBy(x => x.TenLinhVuc)
                .ToListAsync();

            return View(danhSach);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var linhVuc = await _context.LinhVucs
                .FirstOrDefaultAsync(x => x.MaLinhVuc == id);

            if (linhVuc == null)
            {
                return NotFound();
            }

            return View(linhVuc);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LinhVuc model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool tenBiTrung = await _context.LinhVucs
                .AnyAsync(x => x.TenLinhVuc == model.TenLinhVuc);

            if (tenBiTrung)
            {
                ModelState.AddModelError(
                    "TenLinhVuc",
                    "Tên lĩnh vực đã tồn tại."
                );

                return View(model);
            }

            _context.LinhVucs.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var linhVuc = await _context.LinhVucs.FindAsync(id);

            if (linhVuc == null)
            {
                return NotFound();
            }

            return View(linhVuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LinhVuc model)
        {
            if (id != model.MaLinhVuc)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool tenBiTrung = await _context.LinhVucs
                .AnyAsync(x =>
                    x.TenLinhVuc == model.TenLinhVuc &&
                    x.MaLinhVuc != model.MaLinhVuc);

            if (tenBiTrung)
            {
                ModelState.AddModelError(
                    "TenLinhVuc",
                    "Tên lĩnh vực đã tồn tại."
                );

                return View(model);
            }

            _context.LinhVucs.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var linhVuc = await _context.LinhVucs
                .FirstOrDefaultAsync(x => x.MaLinhVuc == id);

            if (linhVuc == null)
            {
                return NotFound();
            }

            return View(linhVuc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var linhVuc = await _context.LinhVucs.FindAsync(id);

            if (linhVuc == null)
            {
                return NotFound();
            }

            _context.LinhVucs.Remove(linhVuc);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}