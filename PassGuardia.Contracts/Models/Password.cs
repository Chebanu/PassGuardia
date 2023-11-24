using System.ComponentModel.DataAnnotations;

namespace PassGuardia.Contracts.Models;

public class Password
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string EncryptedPassword { get; set; }
    [Required]
    public string IV { get; set; }
}
