using System.ComponentModel.DataAnnotations;

namespace PassGuardia.Contracts.Http;

public class RequestPassword
{
    [StringLength(100, MinimumLength = 1, ErrorMessage = "You are out of range")]
    public string Password { get; set; }
}