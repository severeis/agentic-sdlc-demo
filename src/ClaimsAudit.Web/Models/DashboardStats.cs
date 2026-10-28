namespace ClaimsAudit.Web.Models
{
    public class DashboardStats
    {
        public int TotalClaims { get; set; }
        public int FlaggedClaims { get; set; }
        public int FlaggedPercent { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal FlaggedBilled { get; set; }
    }
}
