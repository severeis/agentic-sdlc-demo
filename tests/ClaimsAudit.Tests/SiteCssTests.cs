using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace ClaimsAudit.Tests
{
    [TestFixture]
    public class SiteCssTests
    {
        private static string SiteCssPath()
        {
            var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "src", "ClaimsAudit.Web", "Content", "site.css");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
                dir = dir.Parent;
            }
            throw new FileNotFoundException("Could not locate Content/site.css");
        }

        [Test]
        public void Open_filter_header_has_padding_bottom()
        {
            // The "Filter" header (the <summary> of the open filter panel) should have
            // ~10px of space below it so it does not sit flush against the filter form.
            var css = File.ReadAllText(SiteCssPath());

            var match = Regex.Match(css, @"\.filter-panel\[open\]\s*>\s*summary\s*\{([^}]*)\}");
            Assert.That(match.Success, Is.True,
                "Expected a '.filter-panel[open] > summary' rule in site.css.");

            Assert.That(match.Groups[1].Value, Does.Contain("padding-bottom"),
                "The open filter header should use padding-bottom to space itself from the form.");
        }
    }
}
