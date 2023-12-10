using System.ComponentModel.DataAnnotations;

public class Audit
{
    [Key]
    public int Id { get; set; }

    public string RequestPath { get; set; }

    public string RequestMethod { get; set; }

    public string Exception { get; set; }

    public int StatusCode { get; set; }

    public DateTime TimeStamp { get; set; }
}