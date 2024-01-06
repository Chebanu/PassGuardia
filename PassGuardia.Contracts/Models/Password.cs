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

    [Required]
    [Column("Is_Private")]
    public bool IsPrivate { get; set; }

    [Required]
    [Column("created_by")]
    public string CreatedBy { get; set; }
}