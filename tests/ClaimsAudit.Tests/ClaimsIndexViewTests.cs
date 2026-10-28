using System.IO;
using NUnit.Framework;

namespace ClaimsAudit.Tests
{
    [TestFixture]
    public class ClaimsIndexViewTests
    {
        private static string IndexViewPath()
        {
            var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "src", "ClaimsAudit.Web", "Views", "Claims", "Index.cshtml");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
                dir = dir.Parent;
            }
            throw new FileNotFoundException("Could not locate Claims/Index.cshtml");
        }

        private static string ReadIndexView()
        {
            return File.ReadAllText(IndexViewPath());
        }

        [Test]
        public void Column_headers_are_sortable()
        {
            var view = ReadIndexView();

            Assert.That(view, Does.Not.Contain("<th>Id</th>"),
                "Column headers should be sortable links, not plain text.");
            Assert.That(view, Does.Contain("SortHeader"),
                "Index view should render sortable column headers.");
        }

        [Test]
        public void Sort_indicators_are_pure_ascii_so_they_survive_the_server_code_page()
        {
            // The view is a Razor file with no BOM. The runtime reads such files using the
            // server's default ANSI code page (CP1251 in production), not UTF-8. Any non-ASCII
            // glyph (↑ ↓ ▲ ▼) is therefore mis-decoded into Cyrillic mojibake on the server even
            // though it looks correct on the CP1252 build machine. Keep the arrow markup ASCII-only
            // (HTML numeric character references) so it renders identically regardless of code page.
            var bytes = File.ReadAllBytes(IndexViewPath());
            foreach (var b in bytes)
            {
                Assert.That(b, Is.LessThan(0x80),
                    "Index.cshtml must be pure ASCII: a byte >= 0x80 depends on the file encoding and " +
                    "is mis-decoded under the server's CP1251 code page.");
            }

            var view = ReadIndexView();
            Assert.That(view, Does.Contain("&#9650;"),
                "Ascending sort should be the up triangle as an ASCII numeric entity (&#9650; = U+25B2).");
            Assert.That(view, Does.Contain("&#9660;"),
                "Descending sort should be the down triangle as an ASCII numeric entity (&#9660; = U+25BC).");
        }

        [Test]
        public void Filter_is_collapsible()
        {
            var view = ReadIndexView();

            Assert.That(view, Does.Contain("<details"),
                "The filter should be wrapped in a <details> element so it can be collapsed and expanded.");
            Assert.That(view, Does.Contain("<summary"),
                "A <summary> gives the clickable control that collapses and expands the filter.");
            Assert.That(view, Does.Contain("class=\"filters\""),
                "The From/To filter form should still be present inside the collapsible panel.");
        }

        [Test]
        public void Reset_control_is_a_button_not_a_link()
        {
            var view = ReadIndexView();

            Assert.That(view, Does.Not.Contain(">Reset</a>"),
                "Reset should be a button, not an anchor link.");
            Assert.That(view, Does.Contain(">Reset</button>"),
                "Reset should be rendered as a button element.");
        }
    }
}
