using System;

namespace ClaimsAudit.Web.Services
{
    // Seam added so tests can pin "today". Everything else still calls DateTime directly.
    public static class Clock
    {
        public static Func<DateTime> Today = () => DateTime.Today;
    }
}
