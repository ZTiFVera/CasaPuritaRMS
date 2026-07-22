using CasaPuritaRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CasaPuritaRMS.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MaintenanceController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Loads the tenant dropdown with full names.
        private void LoadTenantDropdown(int? selectedId = null)
        {
            var tenants = _context.Tenants
                .Select(t => new { t.Tenant_ID, Name = t.First_Name + " " + t.Last_Name })
                .ToList();

            ViewData["Tenant_ID"] = new SelectList(tenants, "Tenant_ID", "Name", selectedId);
        }

        // Saves an uploaded file and returns its relative path.
        private async Task<string?> SaveAttachment(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);

            var name = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
            var full = Path.Combine(uploads, name);

            using var stream = new FileStream(full, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/" + name;
        }

        // GET: Maintenance
        public async Task<IActionResult> Index()
        {
            var requests = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .OrderByDescending(m => m.Date_Submitted)
                .ToListAsync();

            return View(requests);
        }

        // GET: Maintenance/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .FirstOrDefaultAsync(m => m.Request_ID == id);

            if (request == null) return NotFound();
            return View(request);
        }

        // GET: Maintenance/Create
        public IActionResult Create()
        {
            LoadTenantDropdown();
            return View();
        }

        // POST: Maintenance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceRequest request, IFormFile? attachment)
        {
            if (ModelState.IsValid)
            {
                request.Attachment_Path = await SaveAttachment(attachment);
                request.Date_Submitted = DateTime.Now;
                request.Status = "Open";

                _context.MaintenanceRequests.Add(request);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Maintenance request submitted successfully!";
                return RedirectToAction(nameof(Index));
            }

            LoadTenantDropdown(request.Tenant_ID);
            return View(request);
        }

        // GET: Maintenance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null) return NotFound();

            LoadTenantDropdown(request.Tenant_ID);
            return View(request);
        }

        // POST: Maintenance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceRequest request, IFormFile? attachment)
        {
            if (id != request.Request_ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Keep the existing attachment unless a new file is uploaded.
                    var newPath = await SaveAttachment(attachment);
                    if (newPath != null) request.Attachment_Path = newPath;

                    _context.Update(request);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Maintenance request updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await RequestExists(request.Request_ID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            LoadTenantDropdown(request.Tenant_ID);
            return View(request);
        }

        // GET: Maintenance/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .FirstOrDefaultAsync(m => m.Request_ID == id);

            if (request == null) return NotFound();
            return View(request);
        }

        // POST: Maintenance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request != null)
            {
                _context.MaintenanceRequests.Remove(request);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Maintenance request deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> RequestExists(int id)
        {
            return await _context.MaintenanceRequests.AnyAsync(e => e.Request_ID == id);
        }
    }
}