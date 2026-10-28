using System;
using System.Linq;
using ClaimsAudit.Web.Services;
using NUnit.Framework;

namespace ClaimsAudit.Tests
{
    [TestFixture]
    public class ClaimRepositoryTests
    {
        [SetUp]
        public void SetUp()
        {
            ClaimRepository.Reset();
        }

        [Test]
        public void All_returns_seeded_claims_newest_first()
        {
            var claims = ClaimRepository.All();

            Assert.That(claims.Count, Is.EqualTo(14));
            Assert.That(claims.First().Id, Is.EqualTo(1014));
        }

        [Test]
        public void Get_returns_null_for_unknown_id()
        {
            Assert.That(ClaimRepository.Get(42), Is.Null);
        }

        [Test]
        public void Search_matches_provider_name()
        {
            var ids = ClaimRepository.Search("Lakeside").Select(c => c.Id);

            Assert.That(ids, Is.EquivalentTo(new[] { 1003, 1009, 1010 }));
        }

        [Test]
        public void Search_matches_cpt_code()
        {
            var ids = ClaimRepository.Search("90834").Select(c => c.Id);

            Assert.That(ids, Is.EquivalentTo(new[] { 1006, 1007, 1013 }));
        }

        [Test]
        public void Empty_search_returns_everything()
        {
            Assert.That(ClaimRepository.Search("").Count, Is.EqualTo(14));
        }

        [Test]
        public void FilterByDate_with_only_from_returns_claims_on_or_after_it()
        {
            var ids = ClaimRepository.FilterByDate(new DateTime(2026, 10, 14), null).Select(c => c.Id);

            Assert.That(ids, Is.EquivalentTo(new[] { 1012, 1013, 1014 }));
        }

        [Test]
        public void MarkReviewed_changes_status()
        {
            Assert.That(ClaimRepository.MarkReviewed(1001), Is.True);
            Assert.That(ClaimRepository.Get(1001).Status, Is.EqualTo("Reviewed"));
        }

        [Test]
        public void MarkReviewed_returns_false_for_unknown_id()
        {
            Assert.That(ClaimRepository.MarkReviewed(42), Is.False);
        }
    }
}
