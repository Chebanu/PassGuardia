using System.ComponentModel.DataAnnotations;

namespace PassGuardia.Contracts.Http;

public class RequestPassword
{
    [Required]
    [MaxLength(100, ErrorMessage = "You password contains more than 100 characters")]
    public string Password { get; set; }
}