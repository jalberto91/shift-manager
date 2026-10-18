using System.Diagnostics;
using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using Serilog;
using ShiftManager.Application.Shifts;
using ShiftManager.Infrastructure;
using ShiftManager.Infrastructure.Persistence;

const string corsPolicy = "Frontend";

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddProblemDetails(options =>
        options.CustomizeProblemDetails = context =>
            context.ProblemDetails.Extensions["traceId"] =
                Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier);

    builder.Services.AddOpenApi();

    builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
    builder.Services.AddScoped<ShiftPlacementChecker>();
    builder.Services.AddInfrastructure(
        builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:Default'."),
        builder.Configuration["NagerDate:BaseUrl"]
        ?? throw new InvalidOperationException("Missing setting 'NagerDate:BaseUrl'."));

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(options =>
        options.AddPolicy(corsPolicy, policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

    var app = builder.Build();

    await app.Services.ApplyMigrationsAsync();
    await app.Services.SeedAsync();

    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseSerilogRequestLogging();
    app.UseCors(corsPolicy);

    app.MapOpenApi();
    app.MapScalarApiReference();

    app.MapHealthChecks("/health");
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "The API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
