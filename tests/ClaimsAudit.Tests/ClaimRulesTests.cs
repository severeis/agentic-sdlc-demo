using System;
using System.Linq;
using ClaimsAudit.Web.Models;
using ClaimsAudit.Web.Services;
using NUnit.Framework;

namespace ClaimsAudit.Tests
{
    // Characterization tests: they pin down what the rules do today, not what they should do.
    [TestFixture]
    public class ClaimRulesTests
    {
        private static readonly DateTime Today = new DateTime(2026, 10, 28);

        private static Claim MakeClaim(string cpt, int units, decimal billed, DateTime? date = null)
        {
            return new Claim
            {
                Id = 1,
                PatientId = "P-1",
                ProviderName = "Test Clinic",
                ServiceDate = date ?? new DateTime(2026, 10, 1),
                CptCode = cpt,
                Units = units,
                BilledAmount = billed,
                Status = "New"
            };
        }

        [Test]
        public void Clean_claim_has_no_flags()
        {
            var flags = ClaimRules.Evaluate(MakeClaim("99213", 1, 95m), Today);

            Assert.That(flags, Is.Empty);
        }

        [Test]
        public void Units_above_daily_maximum_are_flagged()
        {
            var flags = ClaimRules.Evaluate(MakeClaim("97110", 6, 192m), Today);

            Assert.That(flags.Select(f => f.Code), Is.EquivalentTo(new[] { ClaimRules.ImpossibleUnits }));
        }

        [Test]
        public void Units_at_daily_maximum_are_not_flagged()
        {
            var flags = ClaimRules.Evaluate(MakeClaim("97110", 4, 128m), Today);

            Assert.That(flags, Is.Empty);
        }

        [Test]
        public void Billing_more_than_150_percent_of_schedule_is_flagged()
        {
            var flags = ClaimRules.Evaluate(MakeClaim("99215", 1, 410m), Today);

            Assert.That(flags.Select(f => f.Code), Is.EquivalentTo(new[] { ClaimRules.Overbilled }));
        }

        [Test]
        public void Billing_exactly_150_percent_of_schedule_is_not_flagged()
        {
            var flags = ClaimRules.Evaluate(MakeClaim("99213", 1, 142.5m), Today);

            Assert.That(flags, Is.Empty);
        }

        [Test]
        public void Code_missing_from_fee_schedule_is_flagged()
        {
            var flags = ClaimRules.Evaluate(MakeClaim("97530", 2, 80m), Today);

            Assert.That(flags.Select(f => f.Code), Is.EquivalentTo(new[] { ClaimRules.UnknownCode }));
        }

        [Test]
        public void Service_date_after_today_is_flagged()
        {
            var flags = ClaimRules.Evaluate(MakeClaim("99214", 1, 135m, new DateTime(2026, 12, 15)), Today);

            Assert.That(flags.Select(f => f.Code), Is.EquivalentTo(new[] { ClaimRules.FutureDate }));
        }

        [Test]
        public void Multiple_problems_produce_multiple_flags()
        {
            var flags = ClaimRules.Evaluate(MakeClaim("36415", 5, 90m, new DateTime(2026, 11, 1)), Today);

            Assert.That(flags.Select(f => f.Code),
                Is.EquivalentTo(new[] { ClaimRules.ImpossibleUnits, ClaimRules.Overbilled, ClaimRules.FutureDate }));
        }

        [Test]
        public void Null_claim_returns_no_flags()
        {
            Assert.That(ClaimRules.Evaluate(null, Today), Is.Empty);
        }
    }
}
