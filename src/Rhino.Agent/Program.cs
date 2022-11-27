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

using Rhino.Agent.Cli;
using Rhino.Api.Converters;
using Rhino.Controllers.Controllers;
using Rhino.Controllers.Domain;
using Rhino.Controllers.Domain.Formatters;
using Rhino.Controllers.Domain.Middleware;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Hubs;
using Rhino.Controllers.Models;

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using ILogger = Gravity.Abstraction.Logging.ILogger;

// Setup
ControllerUtilities.RenderLogo();
var builder = WebApplication.CreateBuilder(args);

#region *** Url & Kestrel ***
builder.WebHost.UseUrls();
#endregion

#region *** Service       ***
// application
builder.Services.AddRouting(i => i.LowercaseUrls = true);
builder.Services.AddRazorPages();
builder.Services.AddMvc().AddApplicationPart(typeof(RhinoController).Assembly).AddControllersAsServices();

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
builder.Services.AddApiVersioning(i =>
{
    i.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(3, 0);
    i.AssumeDefaultVersionWhenUnspecified = true;
    i.ErrorResponses = new GenericErrorModel<object>();
    i.ReportApiVersions = true;
});

// cookies & CORS
builder.Services.Configure<CookiePolicyOptions>(i =>
{
    i.CheckConsentNeeded = _ => true;
    i.MinimumSameSitePolicy = SameSiteMode.None;
});
builder
    .Services
    .AddCors(i => i.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// signalR
builder.Services.AddSignalR((i) =>
{
    i.EnableDetailedErrors = true;
    i.MaximumReceiveMessageSize = long.MaxValue;
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
var reportsPath = ControllerUtilities.GetStaticReportsFolder(configuration: app.Configuration);
var statusPath = Path.Combine(Environment.CurrentDirectory, "Pages", "Status");

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
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseStaticFiles(reportsPath, route: "/reports");
app.UseStaticFiles(statusPath, route: "/status");

app.MapDefaultControllerRoute();
app.MapFallbackToFile("index.html");
app.MapRazorPages();
app.MapControllers();
app.MapHub<RhinoHub>($"/api/v{AppSettings.ApiVersion}/rhino/orchestrator");
#endregion

#region *** Program       ***
new CommandInvoker(Utilities.Types, args).Invoke();
#endregion

// log
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
    logger?.Info("Create-ServiceApplication = OK");
}

// invoke
app.Run();
