using CasaPuritaRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CasaPuritaRMS.Controllers
{
    public class TenantController : Controller
    {
        private readonly AppDbContext _context;

        public TenantController(AppDbContext context)
        {
            _context = context;
        }

        private void LoadUnitDropdown(int? selectedId = null)
        {
            var units = _context.Units
                .Select(u => new { u.Unit_ID, Label = u.Unit_Number + " - P" + u.Monthly_Price })
                .ToList();
            ViewData["Unit_ID"] = new SelectList(units, "Unit_ID", "Label", selectedId);
        }

        private async Task RefreshUnitStatus(int? unitId)
        {
            if (unitId == null) return;
            var unit = await _context.Units.Include(u => u.Tenants)
                .FirstOrDefaultAsync(u => u.Unit_ID == unitId);
            if (unit == null || unit.Status == "Maintenance") return;

            int active = unit.Tenants?.Count(t => t.Occupancy_Status == "Active") ?? 0;
            unit.Status = active >= unit.Capacity ? "Occupied" : "Available";
            await _context.SaveChangesAsync();
        }

        // GET: Tenant
        public async Task<IActionResult> Index()
        {
            var tenants = await _context.Tenants.Include(t => t.Unit).ToListAsync();
            return View(tenants);
        }

        // GET: Tenant/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var tenant = await _context.Tenants.Include(t => t.Unit)
                .FirstOrDefaultAsync(t => t.Tenant_ID == id);
            if (tenant == null) return NotFound();
            return View(tenant);
        }

        // GET: Tenant/Create
        public IActionResult Create()
        {
            LoadUnitDropdown();
            return View();
        }

        // POST: Tenant/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();
                await RefreshUnitStatus(tenant.Unit_ID);
                TempData["Success"] = "Tenant added successfully!";
                return RedirectToAction(nameof(Index));
            }
            LoadUnitDropdown(tenant.Unit_ID);
            return View(tenant);
        }

        // GET: Tenant/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound();
            LoadUnitDropdown(tenant.Unit_ID);
            return View(tenant);
        }

        // POST: Tenant/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tenant tenant)
        {
            if (id != tenant.Tenant_ID) return NotFound();

            if (ModelState.IsValid)
            {
                int? prevUnit = await _context.Tenants.Where(t => t.Tenant_ID == id)
                    .Select(t => t.Unit_ID).FirstOrDefaultAsync();
                try
                {
                    _context.Update(tenant);
                    await _context.SaveChangesAsync();
                    await RefreshUnitStatus(tenant.Unit_ID);
                    if (prevUnit != tenant.Unit_ID) await RefreshUnitStatus(prevUnit);
                    TempData["Success"] = "Tenant updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Tenants.AnyAsync(e => e.Tenant_ID == tenant.Tenant_ID))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            LoadUnitDropdown(tenant.Unit_ID);
            return View(tenant);
        }

        // GET: Tenant/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var tenant = await _context.Tenants.Include(t => t.Unit)
                .FirstOrDefaultAsync(t => t.Tenant_ID == id);
            if (tenant == null) return NotFound();
            return View(tenant);
        }

        // POST: Tenant/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant != null)
            {
                int? unitId = tenant.Unit_ID;
                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();
                await RefreshUnitStatus(unitId);
                TempData["Success"] = "Tenant deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}