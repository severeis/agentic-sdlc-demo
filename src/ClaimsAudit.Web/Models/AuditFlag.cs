namespace ClaimsAudit.Web.Models
{
    public class AuditFlag
    {
        public AuditFlag(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Code { get; private set; }
        public string Message { get; private set; }
    }
}
