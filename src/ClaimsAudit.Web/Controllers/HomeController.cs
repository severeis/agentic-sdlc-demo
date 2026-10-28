using System.Web.Mvc;
using ClaimsAudit.Web.Services;

namespace ClaimsAudit.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var stats = DashboardService.GetStats(ClaimRepository.All(), Clock.Today());
            return View(stats);
        }
    }
}
