using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using RuleSimulation.Api.Data;
using RuleSimulation.Api.Rules;
using RuleSimulation.Api.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RuleSimulation",
        Version = "v1"
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRuleEvaluationEngine, RuleEvaluationEngine>();
builder.Services.AddScoped<RuleSetService>();
builder.Services.AddScoped<SimulationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "RuleSimulation V1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program { }
