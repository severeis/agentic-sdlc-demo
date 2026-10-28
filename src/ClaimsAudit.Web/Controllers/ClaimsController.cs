using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ClaimsAudit.Web.Models;
using ClaimsAudit.Web.Services;

namespace ClaimsAudit.Web.Controllers
{
    public class ClaimsController : Controller
    {
        // GET /Claims?from=2026-09-01&to=2026-09-30
        public ActionResult Index(string from, string to)
        {
            DateTime? fromDate = ParseDate(from);
            DateTime? toDate = ParseDate(to);

            var claims = (fromDate.HasValue || toDate.HasValue)
                ? ClaimRepository.FilterByDate(fromDate, toDate)
                : ClaimRepository.All();

            var today = Clock.Today();
            var flags = new Dictionary<int, List<AuditFlag>>();
            foreach (var c in claims)
                flags[c.Id] = ClaimRules.Evaluate(c, today);

            ViewBag.Flags = flags;
            ViewBag.From = from;
            ViewBag.To = to;
            return View(claims);
        }

        public ActionResult Details(int id)
        {
            var claim = ClaimRepository.Get(id);
            if (claim == null)
                return HttpNotFound();

            ViewBag.Flags = ClaimRules.Evaluate(claim, Clock.Today());
            return View(claim);
        }

        // Used by Scripts/claims.js for the search box
        public JsonResult Search(string q)
        {
            var today = Clock.Today();
            var result = ClaimRepository.Search(q).Select(c => new
            {
                c.Id,
                c.PatientId,
                c.ProviderName,
                ServiceDate = c.ServiceDate.ToString("yyyy-MM-dd"),
                c.CptCode,
                c.Units,
                BilledAmount = c.BilledAmount.ToString("0.00", CultureInfo.InvariantCulture),
                c.Status,
                Flags = ClaimRules.Evaluate(c, today).Select(f => f.Code).ToArray()
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkReviewed(int id)
        {
            if (!ClaimRepository.MarkReviewed(id))
                return HttpNotFound();
            return RedirectToAction("Details", new { id = id });
        }

        private static DateTime? ParseDate(string value)
        {
            DateTime d;
            if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                return d;
            return null;
        }
    }
}
