using Serilog;
using SistemaGVP.API.Extensions;
using SistemaGVP.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    });

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseApiMiddleware();
    app.MapApiEndpoints();

    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

    await DependencyInjection.InitializeDatabaseAsync(app.Services);

    Log.Information("SistemaGVP API iniciada en {Urls}", string.Join(", ", app.Urls));
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Termino inesperado de la aplicacion");
}
finally
{
    Log.CloseAndFlush();
}
