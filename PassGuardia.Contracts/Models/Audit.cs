using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PassGuardia.Contracts.Models;

[Table("audit")]
public class Audit
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("request_path")]
    public string RequestPath { get; set; }

    [Column("request_method")]
    public string RequestMethod { get; set; }

    [Column("exception")]
    public string Exception { get; set; }

    [Column("status_code")]
    public int StatusCode { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; }
}