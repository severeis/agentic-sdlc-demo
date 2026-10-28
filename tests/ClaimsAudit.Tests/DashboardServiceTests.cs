using System;
using ClaimsAudit.Web.Services;
using NUnit.Framework;

namespace ClaimsAudit.Tests
{
    [TestFixture]
    public class DashboardServiceTests
    {
        private static readonly DateTime Today = new DateTime(2026, 10, 28);

        [SetUp]
        public void SetUp()
        {
            ClaimRepository.Reset();
        }

        [Test]
        public void Counts_claims_and_flags_for_seed_data()
        {
            var stats = DashboardService.GetStats(ClaimRepository.All(), Today);

            Assert.That(stats.TotalClaims, Is.EqualTo(14));
            Assert.That(stats.FlaggedClaims, Is.EqualTo(6));
        }

        [Test]
        public void Sums_billed_amounts()
        {
            var stats = DashboardService.GetStats(ClaimRepository.All(), Today);

            Assert.That(stats.TotalBilled, Is.EqualTo(1878m));
            Assert.That(stats.FlaggedBilled, Is.EqualTo(1087m));
        }

        [Test]
        public void Empty_list_gives_zero_stats()
        {
            var stats = DashboardService.GetStats(new System.Collections.Generic.List<ClaimsAudit.Web.Models.Claim>(), Today);

            Assert.That(stats.TotalClaims, Is.EqualTo(0));
            Assert.That(stats.FlaggedPercent, Is.EqualTo(0));
        }
    }
}
