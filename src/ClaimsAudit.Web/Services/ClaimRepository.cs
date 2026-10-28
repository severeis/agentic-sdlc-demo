using System;
using System.Collections.Generic;
using System.Linq;
using ClaimsAudit.Web.Models;

namespace ClaimsAudit.Web.Services
{
    // In-memory store. The original system used a stored-procedure layer; this keeps the same shape.
    public static class ClaimRepository
    {
        private static readonly object _lock = new object();
        private static List<Claim> _claims = Seed();

        public static List<Claim> All()
        {
            lock (_lock)
            {
                return _claims.OrderByDescending(c => c.ServiceDate).ThenBy(c => c.Id).ToList();
            }
        }

        public static Claim Get(int id)
        {
            lock (_lock)
            {
                return _claims.FirstOrDefault(c => c.Id == id);
            }
        }

        public static List<Claim> Search(string q)
        {
            if (string.IsNullOrEmpty(q))
                return All();

            lock (_lock)
            {
                return _claims
                    .Where(c => c.ProviderName.Contains(q) || c.PatientId.Contains(q) || c.CptCode.Contains(q))
                    .OrderByDescending(c => c.ServiceDate)
                    .ToList();
            }
        }

        // from/to come from the filter form on Claims/Index
        public static List<Claim> FilterByDate(DateTime? from, DateTime? to)
        {
            lock (_lock)
            {
                IEnumerable<Claim> q = _claims;
                if (from.HasValue)
                    q = q.Where(c => c.ServiceDate >= from.Value);
                if (to.HasValue)
                    q = q.Where(c => c.ServiceDate < to.Value);
                return q.OrderByDescending(c => c.ServiceDate).ToList();
            }
        }

        public static bool MarkReviewed(int id)
        {
            lock (_lock)
            {
                var claim = _claims.FirstOrDefault(c => c.Id == id);
                if (claim == null)
                    return false;
                claim.Status = "Reviewed";
                return true;
            }
        }

        public static void Reset()
        {
            lock (_lock)
            {
                _claims = Seed();
            }
        }

        private static List<Claim> Seed()
        {
            return new List<Claim>
            {
                new Claim { Id = 1001, PatientId = "P-20417", ProviderName = "Mercy Family Clinic", ServiceDate = new DateTime(2026, 9, 2), CptCode = "99213", Units = 1, BilledAmount = 95m, Status = "New" },
                new Claim { Id = 1002, PatientId = "P-20417", ProviderName = "Mercy Family Clinic", ServiceDate = new DateTime(2026, 9, 2), CptCode = "36415", Units = 1, BilledAmount = 10m, Status = "New" },
                new Claim { Id = 1003, PatientId = "P-31877", ProviderName = "Lakeside Physical Therapy", ServiceDate = new DateTime(2026, 9, 5), CptCode = "97110", Units = 6, BilledAmount = 192m, Status = "New" },
                new Claim { Id = 1004, PatientId = "P-11902", ProviderName = "Northgate Internal Medicine", ServiceDate = new DateTime(2026, 9, 9), CptCode = "99215", Units = 1, BilledAmount = 410m, Status = "New" },
                new Claim { Id = 1005, PatientId = "P-11902", ProviderName = "Northgate Internal Medicine", ServiceDate = new DateTime(2026, 9, 12), CptCode = "99214", Units = 1, BilledAmount = 135m, Status = "Reviewed" },
                new Claim { Id = 1006, PatientId = "P-48810", ProviderName = "Riverbend Behavioral Health", ServiceDate = new DateTime(2026, 9, 15), CptCode = "90834", Units = 2, BilledAmount = 220m, Status = "New" },
                new Claim { Id = 1007, PatientId = "P-48810", ProviderName = "Riverbend Behavioral Health", ServiceDate = new DateTime(2026, 9, 22), CptCode = "90834", Units = 1, BilledAmount = 110m, Status = "New" },
                new Claim { Id = 1008, PatientId = "P-55021", ProviderName = "Mercy Family Clinic", ServiceDate = new DateTime(2026, 9, 30), CptCode = "99214", Units = 1, BilledAmount = 140m, Status = "New" },
                new Claim { Id = 1009, PatientId = "P-60233", ProviderName = "Lakeside Physical Therapy", ServiceDate = new DateTime(2026, 10, 1), CptCode = "97110", Units = 3, BilledAmount = 96m, Status = "New" },
                new Claim { Id = 1010, PatientId = "P-60233", ProviderName = "Lakeside Physical Therapy", ServiceDate = new DateTime(2026, 10, 1), CptCode = "97530", Units = 2, BilledAmount = 80m, Status = "New" },
                new Claim { Id = 1011, PatientId = "P-71540", ProviderName = "Northgate Internal Medicine", ServiceDate = new DateTime(2026, 10, 6), CptCode = "99213", Units = 1, BilledAmount = 95m, Status = "Reviewed" },
                new Claim { Id = 1012, PatientId = "P-82214", ProviderName = "Mercy Family Clinic", ServiceDate = new DateTime(2026, 10, 14), CptCode = "36415", Units = 5, BilledAmount = 50m, Status = "New" },
                new Claim { Id = 1013, PatientId = "P-90011", ProviderName = "Riverbend Behavioral Health", ServiceDate = new DateTime(2026, 10, 20), CptCode = "90834", Units = 1, BilledAmount = 110m, Status = "New" },
                new Claim { Id = 1014, PatientId = "P-90011", ProviderName = "Northgate Internal Medicine", ServiceDate = new DateTime(2026, 12, 15), CptCode = "99214", Units = 1, BilledAmount = 135m, Status = "New" }
            };
        }
    }
}
