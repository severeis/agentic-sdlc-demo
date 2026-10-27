using System.IO;
using NUnit.Framework;

namespace ClaimsAudit.Tests
{
    [TestFixture]
    public class ClaimsIndexViewTests
    {
        private static string ReadIndexView()
        {
            var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "src", "ClaimsAudit.Web", "Views", "Claims", "Index.cshtml");
                if (File.Exists(candidate))
                {
                    return File.ReadAllText(candidate);
                }
                dir = dir.Parent;
            }
            throw new FileNotFoundException("Could not locate Claims/Index.cshtml");
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
