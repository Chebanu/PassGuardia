using System.ComponentModel.DataAnnotations;

namespace PassGuardia.Contracts.Models;

public class Password
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public byte[] EncryptedPassword { get; set; }
}