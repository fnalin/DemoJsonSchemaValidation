namespace Api.Models;

public class ValidateSchemaVM
{
    public int SchemaId { get; set; }
    public dynamic Data { get; set; } = null!;
}