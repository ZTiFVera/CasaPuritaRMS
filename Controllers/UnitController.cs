using CasaPuritaRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasaPuritaRMS.Controllers
{
    public class UnitController : Controller
    {
        private readonly AppDbContext _context;

        public UnitController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Unit
        public async Task<IActionResult> Index()
        {
            var units = await _context.Units.ToListAsync();
            return View(units);
        }

        // GET: Unit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.Units
                .Include(u => u.Tenants)
                .FirstOrDefaultAsync(u => u.Unit_ID == id);

            if (unit == null) return NotFound();
            return View(unit);
        }

        // GET: Unit/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Unit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Unit unit)
        {
            if (ModelState.IsValid)
            {
                _context.Units.Add(unit);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Unit added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(unit);
        }

        // GET: Unit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return NotFound();

            return View(unit);
        }

        // POST: Unit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Unit unit)
        {
            if (id != unit.Unit_ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(unit);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Unit updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UnitExists(unit.Unit_ID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(unit);
        }

        // GET: Unit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Unit_ID == id);

            if (unit == null) return NotFound();
            return View(unit);
        }

        // POST: Unit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit != null)
            {
                _context.Units.Remove(unit);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Unit deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> UnitExists(int id)
        {
            return await _context.Units.AnyAsync(e => e.Unit_ID == id);
        }
    }
}
