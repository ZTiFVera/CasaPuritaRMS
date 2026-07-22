using CasaPuritaRMS.Models;
using Microsoft.AspNetCore.Mvc;
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

        // GET: RentPayment
        public async Task<IActionResult> Index()
        {
            var payments = await _context.RentPayments
                .Include(p => p.Tenant).ThenInclude(t => t!.Unit)
                .OrderByDescending(p => p.Due_Date)
                .ToListAsync();

            return View(payments);
        }

        // POST: RentPayment/GenerateDues
        // Creates any missing monthly dues from move-in up to the current period.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateDues()
        {
            var today = DateTime.Today;
            var periodCap = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);

            var tenants = await _context.Tenants
                .Include(t => t.Unit)
                .Where(t => t.Occupancy_Status == "Active" && t.Unit_ID != null)
                .ToListAsync();

            int created = 0;
            int downPaymentsApplied = 0;

            foreach (var tenant in tenants)
            {
                var due = tenant.First_Due_Date;

                while (due <= periodCap)
                {
                    var monthStart = new DateTime(due.Year, due.Month, 1);
                    var monthEnd = monthStart.AddMonths(1);

                    bool exists = await _context.RentPayments.AnyAsync(p =>
                        p.Tenant_ID == tenant.Tenant_ID &&
                        p.Due_Date >= monthStart && p.Due_Date < monthEnd);

                    if (!exists)
                    {
                        // The down payment is only ever applied to the tenant's
                        // very first due (their move-in month). Later months
                        // always start unpaid, regardless of how many times
                        // GenerateDues runs.
                        bool isFirstDue = due == tenant.First_Due_Date;
                        decimal downPaymentApplied = isFirstDue ? tenant.Down_Payment : 0;

                        var payment = new RentPayment
                        {
                            Tenant_ID = tenant.Tenant_ID,
                            Monthly_Rent = tenant.Unit?.Monthly_Price ?? 0,
                            Amount_Paid = downPaymentApplied,
                            Payment_Date = today,
                            Due_Date = due
                        };

                        _context.RentPayments.Add(payment);
                        await _context.SaveChangesAsync();

                        // Flag the receipt so it's obvious in the list/details
                        // which charge already has the down payment credited.
                        string prefix = downPaymentApplied > 0 ? "DP-" : "RCPT-";
                        payment.Receipt_Number = prefix + due.ToString("yyMMdd") + "-" + payment.Payment_ID;
                        _context.Update(payment);
                        await _context.SaveChangesAsync();

                        created++;
                        if (downPaymentApplied > 0) downPaymentsApplied++;
                    }

                    due = due.AddMonths(1);
                }
            }

            TempData["Success"] = created > 0
                ? $"{created} due record(s) generated." +
                  (downPaymentsApplied > 0 ? $" Down payment applied for {downPaymentsApplied} tenant(s)." : "")
                : "All active tenants already have dues up to this month.";

            return RedirectToAction(nameof(Index));
        }

        // GET: RentPayment/PaymentModal/5
        public async Task<IActionResult> PaymentModal(int id)
        {
            var payment = await _context.RentPayments
                .Include(p => p.Tenant).ThenInclude(t => t!.Unit)
                .FirstOrDefaultAsync(p => p.Payment_ID == id);

            if (payment == null) return NotFound();

            // Keep rent in sync with the unit's current price.
            if (payment.Tenant?.Unit != null)
                payment.Monthly_Rent = payment.Tenant.Unit.Monthly_Price;

            return PartialView("_PaymentModal", payment);
        }

        // POST: RentPayment/RecordPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(int Payment_ID, decimal Amount_Paid, DateTime Payment_Date)
        {
            var payment = await _context.RentPayments.FindAsync(Payment_ID);
            if (payment == null) return NotFound();

            // Accumulate so partial payments add up over time.
            payment.Amount_Paid += Amount_Paid;
            payment.Payment_Date = Payment_Date;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // GET: RentPayment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.RentPayments
                .Include(p => p.Tenant).ThenInclude(t => t!.Unit)
                .FirstOrDefaultAsync(p => p.Payment_ID == id);

            if (payment == null) return NotFound();
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
    }
}