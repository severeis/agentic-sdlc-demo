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
        // GET /Claims?from=2026-09-01&to=2026-09-30&sort=billed&dir=asc
        public ActionResult Index(string from, string to, string sort, string dir)
        {
            DateTime? fromDate = ParseDate(from);
            DateTime? toDate = ParseDate(to);

            var claims = (fromDate.HasValue || toDate.HasValue)
                ? ClaimRepository.FilterByDate(fromDate, toDate)
                : ClaimRepository.All();

            claims = SortClaims(claims, sort, dir);

            var today = Clock.Today();
            var flags = new Dictionary<int, List<AuditFlag>>();
            foreach (var c in claims)
                flags[c.Id] = ClaimRules.Evaluate(c, today);

            ViewBag.Flags = flags;
            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.Sort = sort;
            ViewBag.Dir = dir;
            return View(claims);
        }

        // Sorts the grid by the requested column. An unknown or empty sort key
        // leaves the repository's default order untouched.
        private static List<Claim> SortClaims(List<Claim> claims, string sort, string dir)
        {
            if (string.IsNullOrEmpty(sort))
                return claims;

            bool desc = dir == "desc";
            IEnumerable<Claim> sorted;
            switch (sort)
            {
                case "id": sorted = desc ? claims.OrderByDescending(c => c.Id) : claims.OrderBy(c => c.Id); break;
                case "date": sorted = desc ? claims.OrderByDescending(c => c.ServiceDate) : claims.OrderBy(c => c.ServiceDate); break;
                case "patient": sorted = desc ? claims.OrderByDescending(c => c.PatientId) : claims.OrderBy(c => c.PatientId); break;
                case "provider": sorted = desc ? claims.OrderByDescending(c => c.ProviderName) : claims.OrderBy(c => c.ProviderName); break;
                case "cpt": sorted = desc ? claims.OrderByDescending(c => c.CptCode) : claims.OrderBy(c => c.CptCode); break;
                case "units": sorted = desc ? claims.OrderByDescending(c => c.Units) : claims.OrderBy(c => c.Units); break;
                case "billed": sorted = desc ? claims.OrderByDescending(c => c.BilledAmount) : claims.OrderBy(c => c.BilledAmount); break;
                case "status": sorted = desc ? claims.OrderByDescending(c => c.Status) : claims.OrderBy(c => c.Status); break;
                default: return claims;
            }
            return sorted.ToList();
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
