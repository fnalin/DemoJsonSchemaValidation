using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models;

public class Config
{
    public int Id { get; set; }
    
    [Column(TypeName = "json")]
    public string Schema { get; set; } = null!;
}