using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PassGuardia.Contracts.Models;

[Table("passwords")]
public class Password
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("encrypted_password")]
    public byte[] EncryptedPassword { get; set; }
}