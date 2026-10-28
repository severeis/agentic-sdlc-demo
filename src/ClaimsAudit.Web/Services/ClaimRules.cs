using System;
using System.Collections.Generic;
using ClaimsAudit.Web.Models;

namespace ClaimsAudit.Web.Services
{
    // Audit rules ported from the old nightly job. Codes and thresholds are owned by Compliance.
    public static class ClaimRules
    {
        public const string ImpossibleUnits = "IMPOSSIBLE_UNITS";
        public const string Overbilled = "OVERBILLED";
        public const string FutureDate = "FUTURE_DATE";
        public const string UnknownCode = "UNKNOWN_CODE";

        // Medically Unlikely Edits: max units per patient per day
        private static readonly Dictionary<string, int> MaxUnitsPerDay = new Dictionary<string, int>
        {
            { "99213", 1 },
            { "99214", 1 },
            { "99215", 1 },
            { "90834", 1 },
            { "97110", 4 },
            { "36415", 3 }
        };

        // Fee schedule, USD per unit
        private static readonly Dictionary<string, decimal> FeeSchedule = new Dictionary<string, decimal>
        {
            { "99213", 95m },
            { "99214", 135m },
            { "99215", 185m },
            { "90834", 110m },
            { "97110", 32m },
            { "36415", 10m }
        };

        public static List<AuditFlag> Evaluate(Claim claim)
        {
            return Evaluate(claim, Clock.Today());
        }

        public static List<AuditFlag> Evaluate(Claim claim, DateTime today)
        {
            var flags = new List<AuditFlag>();
            if (claim == null)
                return flags;

            if (!FeeSchedule.ContainsKey(claim.CptCode))
            {
                flags.Add(new AuditFlag(UnknownCode, "CPT " + claim.CptCode + " is not in the fee schedule"));
            }
            else
            {
                var max = MaxUnitsPerDay[claim.CptCode];
                if (claim.Units > max)
                    flags.Add(new AuditFlag(ImpossibleUnits, claim.Units + " units billed, max " + max + " per day"));

                var expected = FeeSchedule[claim.CptCode] * claim.Units;
                if (claim.BilledAmount > expected * 1.5m)
                    flags.Add(new AuditFlag(Overbilled, "Billed " + claim.BilledAmount.ToString("C") + ", schedule " + expected.ToString("C")));
            }

            if (claim.ServiceDate > today)
                flags.Add(new AuditFlag(FutureDate, "Service date is in the future"));

            return flags;
        }
    }
}
