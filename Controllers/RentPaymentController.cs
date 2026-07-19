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

        // Helper: Calculate live payment status.
        // Counts Down_Payment + Amount_Paid together (Total_Paid), and checks
        // today's date against Due_Date so a payment isn't wrongly marked
        // "Overdue" before it's actually due.
        //
        //   Paid     -> Total_Paid covers the Monthly_Rent
        //   Overdue  -> due date has passed and it's not fully paid
        //   Partial  -> something has been paid, due date hasn't passed yet
        //   Pending  -> nothing paid yet, due date hasn't passed yet
        private string CalculateStatus(RentPayment payment)
        {
            decimal totalPaid = payment.Down_Payment + payment.Amount_Paid;

            if (totalPaid >= payment.Monthly_Rent)
                return "Paid";

            if (DateTime.Today > payment.Due_Date.Date)
                return "Overdue";

            return totalPaid > 0 ? "Partial" : "Pending";
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

        // GET: RentPayment/GetLastPayment?tenantId=5
        // Used by Create.cshtml (AJAX) to auto-fill Monthly_Rent and suggest
        // the next Due_Date from this tenant's most recent payment record,
        // so the admin isn't retyping the same rent amount every month.
        // The admin can still edit the value if the rent actually changed.
        [HttpGet]
        public async Task<JsonResult> GetLastPayment(int tenantId)
        {
            var last = await _context.RentPayments
                .Where(p => p.Tenant_ID == tenantId)
                .OrderByDescending(p => p.Payment_Date)
                .FirstOrDefaultAsync();

            if (last == null)
                return Json(new { found = false });

            return Json(new
            {
                found = true,
                monthlyRent = last.Monthly_Rent,
                nextDueDate = last.Due_Date.AddMonths(1).ToString("yyyy-MM-dd")
            });
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

                // generate receipt number using the auto-generated Payment_ID
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