using CasaPuritaRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CasaPuritaRMS.Controllers
{
    public class RentPaymentController : Controller
    {
        private readonly AppDbContext _context;

        public RentPaymentController(AppDbContext context)
        {
            _context = context;
        }

        // Helper: Calculate live payment status
        private string CalculateStatus(RentPayment payment)
        {
            bool isOverdue = DateTime.Now.Date > payment.Due_Date.Date;

            // Only Amount_Paid matters for determining if rent is paid
            // Down_Payment is just a separate transaction/deposit
            if (payment.Amount_Paid >= payment.Monthly_Rent)
                return "Paid";
            else if (isOverdue)
                return "Overdue";
            else
                return "Pending";
        }

        // Helper: Load Tenant dropdown with Full Name
        private void LoadTenantDropdown(int? selectedId = null)
        {
            var tenants = _context.Tenants
                .Select(t => new
                {
                    t.Tenant_ID,
                    FullName = t.First_Name + " " + t.Last_Name
                })
                .ToList();

            ViewData["Tenant_ID"] = new SelectList(tenants, "Tenant_ID", "FullName", selectedId);
        }

        // GET: RentPayment
        public async Task<IActionResult> Index()
        {
            var payments = await _context.RentPayments
                .Include(p => p.Tenant)
                .OrderByDescending(p => p.Payment_Date)
                .ToListAsync();

            foreach (var payment in payments)
            {
                payment.Payment_Status = CalculateStatus(payment);
            }

            return View(payments);
        }

        // GET: RentPayment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.RentPayments
                .Include(p => p.Tenant)
                .FirstOrDefaultAsync(p => p.Payment_ID == id);

            if (payment == null) return NotFound();

            payment.Payment_Status = CalculateStatus(payment);
            return View(payment);
        }

        // GET: RentPayment/Create
        public IActionResult Create()
        {
            LoadTenantDropdown();
            return View();
        }

        // POST: RentPayment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentPayment payment)
        {
            if (ModelState.IsValid)
            {
                payment.Payment_Status = CalculateStatus(payment);

                // Save first without receipt number
                _context.RentPayments.Add(payment);
                await _context.SaveChangesAsync();

                // Now generate receipt number using the auto-generated Payment_ID
                payment.Receipt_Number = "RCPT-" + DateTime.Now.ToString("yyMMddHHmmss") + "-" + payment.Payment_ID;

                // Update the record with the receipt number
                _context.Update(payment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Payment recorded successfully!";
                return RedirectToAction(nameof(Index));
            }

            LoadTenantDropdown(payment.Tenant_ID);
            return View(payment);
        }

        // GET: RentPayment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.RentPayments.FindAsync(id);
            if (payment == null) return NotFound();

            LoadTenantDropdown(payment.Tenant_ID);
            return View(payment);
        }

        // POST: RentPayment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RentPayment payment)
        {
            if (id != payment.Payment_ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    payment.Payment_Status = CalculateStatus(payment);
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Payment updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PaymentExists(payment.Payment_ID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            LoadTenantDropdown(payment.Tenant_ID);
            return View(payment);
        }

        // GET: RentPayment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.RentPayments
                .Include(p => p.Tenant)
                .FirstOrDefaultAsync(p => p.Payment_ID == id);

            if (payment == null) return NotFound();

            payment.Payment_Status = CalculateStatus(payment);
            return View(payment);
        }

        // POST: RentPayment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.RentPayments.FindAsync(id);
            if (payment != null)
            {
                _context.RentPayments.Remove(payment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Payment deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> PaymentExists(int id)
        {
            return await _context.RentPayments.AnyAsync(e => e.Payment_ID == id);
        }
    }
}