using System.ComponentModel.DataAnnotations;

namespace PassGuardia.DTO;

public class RequestPassword
{
	[StringLength(100, MinimumLength = 8, ErrorMessage = "You are out of range")]
	public string EncryptedPassword { get; set; }
}