using Api.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(config =>
{
    var connString = builder.Configuration.GetConnectionString("Default");
    config.UseMySql(connString, ServerVersion.AutoDetect(connString));
});

var app = builder.Build();



app.MapGet("/", (AppDbContext ctx) => {
    var configs = ctx.Configs.ToList().Select(x=> new {
        Id = x.Id,
        Settings = System.Text.Json.JsonSerializer.Deserialize<dynamic>(x.Schema)
    }).ToList();
    return Results.Ok(configs);
});

app.MapPost("/validateschema", async (AppDbContext ctx, ValidateSchemaVM vm) =>
{
    var configs = await ctx.Configs.FindAsync(vm.SchemaId);
    
    if(configs == null)
    {
        return Results.NotFound();
    }
    
    string jsonData = System.Text.Json.JsonSerializer.Serialize(vm.Data);
    string schemaData = configs.Schema;

    JSchema schema = JSchema.Parse(schemaData);
    JObject json = JObject.Parse(jsonData);
    
    if (json.IsValid(schema, out IList<string> errors))
    {
        return Results.Ok("JSON is valid");
    }
    else
    {
        var msg = "JSON is invalid. Errors:";
        foreach (var error in errors)
        {
            msg+=$"- {error}";
        }
        
        return Results.BadRequest(msg);

    }
    
    return Results.Ok(configs);
});

app.MapPost("/", async (AppDbContext ctx, SchemaVM vm) =>
{
    var config = new Config
    {
        Schema = System.Text.Json.JsonSerializer.Serialize(vm.Data)
    };
    ctx.Configs.Add(config);
    await ctx.SaveChangesAsync();
    return Results.Ok(config);
});

await app.RunAsync();