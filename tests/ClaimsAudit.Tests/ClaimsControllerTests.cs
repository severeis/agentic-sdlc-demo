using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ClaimsAudit.Web.Controllers;
using ClaimsAudit.Web.Models;
using ClaimsAudit.Web.Services;
using NUnit.Framework;

namespace ClaimsAudit.Tests
{
    [TestFixture]
    public class ClaimsControllerTests
    {
        private Func<DateTime> _originalToday;

        [SetUp]
        public void SetUp()
        {
            ClaimRepository.Reset();
            _originalToday = Clock.Today;
            Clock.Today = () => new DateTime(2026, 10, 28);
        }

        [TearDown]
        public void TearDown()
        {
            Clock.Today = _originalToday;
        }

        [Test]
        public void Index_without_filters_lists_all_claims()
        {
            var result = (ViewResult)new ClaimsController().Index(null, null);

            Assert.That(((List<Claim>)result.Model).Count, Is.EqualTo(14));
        }

        [Test]
        public void Index_ignores_unparseable_dates()
        {
            var result = (ViewResult)new ClaimsController().Index("not-a-date", "31/12/2026");

            Assert.That(((List<Claim>)result.Model).Count, Is.EqualTo(14));
        }

        [Test]
        public void Details_for_unknown_claim_returns_404()
        {
            var result = new ClaimsController().Details(42);

            Assert.That(result, Is.InstanceOf<HttpNotFoundResult>());
        }

        [Test]
        public void Details_puts_flags_in_viewbag()
        {
            var controller = new ClaimsController();
            var result = (ViewResult)controller.Details(1003);

            var flags = (List<AuditFlag>)result.ViewBag.Flags;
            Assert.That(flags.Count, Is.EqualTo(1));
            Assert.That(flags[0].Code, Is.EqualTo(ClaimRules.ImpossibleUnits));
        }

        [Test]
        public void Search_returns_json_allowed_for_get()
        {
            var result = new ClaimsController().Search("Riverbend");

            Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.AllowGet));
        }
    }
}
