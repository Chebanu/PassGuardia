namespace PassGuardia.Contracts.Http;

public class AuditRequest
{
    public int PageSize { get; set; } = 1;
    public int PageNumber { get; set; } = 100;
}