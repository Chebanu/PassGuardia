using System.ComponentModel.DataAnnotations;

namespace PassGuardia.DTO;

public class RequestPassword
{
    [StringLength(100, MinimumLength = 1, ErrorMessage = "You are out of range")]
    public string Password { get; set; }
}