using System;
using System.Collections.Generic;
using System.Linq;
using ClaimsAudit.Web.Models;

namespace ClaimsAudit.Web.Services
{
    public static class DashboardService
    {
        public static DashboardStats GetStats(IList<Claim> claims, DateTime today)
        {
            var stats = new DashboardStats();
            stats.TotalClaims = claims.Count;
            stats.TotalBilled = claims.Sum(c => c.BilledAmount);

            foreach (var claim in claims)
            {
                if (ClaimRules.Evaluate(claim, today).Count > 0)
                {
                    stats.FlaggedClaims++;
                    stats.FlaggedBilled += claim.BilledAmount;
                }
            }

            if (stats.TotalClaims > 0)
                stats.FlaggedPercent = stats.FlaggedClaims * 100 / stats.TotalClaims;

            return stats;
        }
    }
}
