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

        // GET: Tenant
        public async Task<IActionResult> Index()
        {
            var tenants = await _context.Tenants
                .Include(t => t.Unit)
                .ToListAsync();
            return View(tenants);
        }

        // GET: Tenant/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                .FirstOrDefaultAsync(t => t.Tenant_ID == id);

            if (tenant == null) return NotFound();
            return View(tenant);
        }

        // GET: Tenant/Create
        public IActionResult Create()
        {
            ViewData["Unit_ID"] = new SelectList(_context.Units, "Unit_ID", "Unit_Number");
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
                TempData["Success"] = "Tenant added successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Unit_ID"] = new SelectList(_context.Units, "Unit_ID", "Unit_Number");
            return View(tenant);
        }

        // GET: Tenant/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound();

            ViewData["Unit_ID"] = new SelectList(_context.Units, "Unit_ID", "Unit_Number");
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
                try
                {
                    _context.Update(tenant);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Tenant updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TenantExists(tenant.Tenant_ID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Unit_ID"] = new SelectList(_context.Units, "Unit_ID", "Unit_Number");
            return View(tenant);
        }

        // GET: Tenant/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tenant = await _context.Tenants
                .Include(t => t.Unit)
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
                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tenant deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> TenantExists(int id)
        {
            return await _context.Tenants.AnyAsync(e => e.Tenant_ID == id);
        }
    }
}


