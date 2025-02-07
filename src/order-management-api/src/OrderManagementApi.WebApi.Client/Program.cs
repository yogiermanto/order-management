using OrderManagementApi.Infrastructure;
using OrderManagementApi.Shared.Infrastructure.Cache;
using OrderManagementApi.Shared.Infrastructure.Contexts;
using OrderManagementApi.Shared.Infrastructure.Logging;
using OrderManagementApi.Shared.Infrastructure.Serialization.SystemTextJson;
using OrderManagementApi.Shared.Infrastructure.Api;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderManagementApi.WebApi.Client.Common;
using OrderManagementApi.WebApi.Client.HealthChecks;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLogging();
builder.Host.UseLogging();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IAuthManager, AuthManager>();
builder.Services.AddSwaggerGen2();
builder.Services.AddAuth();

builder.Services.AddRedisCache(builder.Configuration);

builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = false; });
builder.Services.AddControllers(options =>
    {
        options.Conventions.Add(
            new CustomRouteToken(
                "namespace",
                c => c.ControllerType.Namespace?.Split('.').Last()
            ));
        options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
    })
    .AddJsonOptions(options =>
    {
        //remove based from discussion
        //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks()
    .AddCheck<ApplicationHealthCheck>("application")
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

if (!app.Environment.IsProduction())
    app.UseSwaggerGenAndReDoc();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});
app.UseCors("cors");
app.UseCustomExceptionHandler();
app.UseAuth();
app.UseContext();
app.UseLogging();
app.UseRouting();
app.MapHealthChecks("/health-check", new HealthCheckOptions
{
    Predicate = _ => true,
    AllowCachingResponses = true,
    ResponseWriter = HealthCheckResponseWriter.WriteResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});
app.UseAuthorization();
app.MapControllers();
app.Run();