/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using LiteDB;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Converters;
using Rhino.Controllers.Domain;
using Rhino.Controllers.Domain.Extensions;
using Rhino.Controllers.Domain.Formatters;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Domain.Middleware;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;
using Rhino.Settings;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using ILogger = Gravity.Abstraction.Logging.ILogger;

// Setup
ControllerUtilities.RenderWorkerLogo();
var builder = WebApplication.CreateBuilder(args);

#region *** Url & Kestrel ***
builder.WebHost.UseUrls();
#endregion

#region *** Service       ***
// application
builder.Services.AddRouting(i => i.LowercaseUrls = true);
builder.Services.AddRazorPages();

// formats & serialization
builder.Services
    .AddControllers(i => i.InputFormatters.Add(new TextPlainInputFormatter()))
    .AddJsonOptions(i =>
    {
        i.JsonSerializerOptions.WriteIndented = true;
        i.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        i.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        i.JsonSerializerOptions.Converters.Add(new TypeConverter());
        i.JsonSerializerOptions.Converters.Add(new ExceptionConverter());
    });

// open api
builder.Services.AddSwaggerGen(i =>
{
    i.SwaggerDoc("v3", new OpenApiInfo { Title = "Rhino Controllers", Version = "v3" });
    i.OrderActionsBy(a => a.HttpMethod);
    i.EnableAnnotations();
});

// versions manager
builder.Services.AddApiVersioning(c =>
{
    c.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(3, 0);
    c.AssumeDefaultVersionWhenUnspecified = true;
    c.ErrorResponses = new GenericErrorModel<object>();
    c.ReportApiVersions = true;
});

// cookies & CORS
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder
    .Services
    .AddCors(o => o.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// signalR
builder.Services.AddSignalR((o) =>
{
    o.EnableDetailedErrors = true;
    o.MaximumReceiveMessageSize = long.MaxValue;
});
#endregion

#region *** Dependencies  ***
RhinoDomain.CreateDependencies(builder);
#endregion

#region *** Configuration ***
// build
var app = builder.Build();

// development settings
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// setup
var logsPath = app.Configuration.GetValue("Rhino:ReportConfiguration:LogsOut", Environment.CurrentDirectory);

// build
app.ConfigureExceptionHandler(new TraceLogger("RhinoApi", "ExceptionHandler", logsPath));

app.UseCookiePolicy();
app.UseCors("CorsPolicy");
app.UseSwagger();
app.UseSwaggerUI(i =>
{
    i.SwaggerEndpoint("/swagger/v3/swagger.json", "Rhino Controllers v3");
    i.DisplayRequestDuration();
    i.EnableFilter();
    i.EnableTryItOutByDefault();
});
app.UseRouting();

app.MapDefaultControllerRoute();
app.MapControllers();
#endregion

// sync from hub
using (var scope = app.Services.CreateScope())
{
    // services
    var environment = scope.ServiceProvider.GetRequiredService<IEnvironmentRepository>();
    var models = scope.ServiceProvider.GetRequiredService<IRepository<RhinoModelCollection>>();
    var resources = scope.ServiceProvider.GetRequiredService<IResourcesRepository>();
    var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
    var repairs = scope.ServiceProvider.GetRequiredService<ConcurrentBag<(RhinoTestCase TestCase, IDictionary<string, object>)>>();

    // invoke
    new StartWorkerMiddleware(appSettings, environment, models, resources, repairs).Start(args);
    logger?.Info($"Sync-Worker -MaxParallel {appSettings?.Worker?.MaxParallel} = OK");
}

#region *** Cache         ***
DomainUtilities.SyncCache();
#endregion

// invoke
app.Run();
