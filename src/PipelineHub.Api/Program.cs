using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PipelineHub.Application;
using PipelineHub.Application.Jobs;
using PipelineHub.Domain;
using PipelineHub.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddOpenApi();
    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
    builder.Services.AddApplication();

    // Repo root: two levels up from the Api project directory (src/PipelineHub.Api -> repo root).
    // Holds assets/sample.mp4 for the public demo job. Revisit when this moves into a Docker image (Phase 9).
    var repoRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".."));
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:Default.");
    builder.Services.AddInfrastructure(repoRoot, connectionString);

    var app = builder.Build();

    // Dev convenience: apply pending migrations at startup instead of a separate `dotnet ef database update`
    // step. Revisit for a real deploy pipeline in Phase 9 — migrating on every boot doesn't belong in prod.
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PipelineHub.Infrastructure.Persistence.PipelineHubDbContext>();
        db.Database.Migrate();
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        switch (feature?.Error)
        {
            case ValidationException validationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    errors = validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                });
                break;
            case Microsoft.AspNetCore.Http.BadHttpRequestException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Malformed request body." });
                break;
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "Unexpected error." });
                break;
        }
    }));

    app.MapPost("/jobs", async (EnqueueJobRequest request, ISender sender, CancellationToken ct) =>
    {
        var id = await sender.Send(new EnqueueJobCommand(request.Type, request.Parameters ?? new Dictionary<string, string>()), ct);
        return Results.Accepted($"/jobs/{id}", new { id });
    })
    .WithName("EnqueueJob");

    app.MapGet("/jobs/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
    {
        var status = await sender.Send(new GetJobStatusQuery(id), ct);
        return status is null ? Results.NotFound() : Results.Ok(status);
    })
    .WithName("GetJobStatus");

    app.MapPost("/jobs/enqueue-and-wait", async (EnqueueJobRequest request, ISender sender, CancellationToken ct) =>
    {
        var id = await sender.Send(new EnqueueJobCommand(request.Type, request.Parameters ?? new Dictionary<string, string>()), ct);
        var status = await sender.Send(new GetJobStatusQuery(id), ct);
        return Results.Ok(status);
    })
    .WithName("EnqueueJobAndWait");

    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

record EnqueueJobRequest(JobType Type, Dictionary<string, string>? Parameters);
