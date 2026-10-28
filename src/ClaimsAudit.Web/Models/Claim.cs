using System;

namespace ClaimsAudit.Web.Models
{
    public class Claim
    {
        public int Id { get; set; }
        public string PatientId { get; set; }
        public string ProviderName { get; set; }
        public DateTime ServiceDate { get; set; }
        public string CptCode { get; set; }
        public int Units { get; set; }
        public decimal BilledAmount { get; set; }

        // "New", "Reviewed"
        public string Status { get; set; }
    }
}
