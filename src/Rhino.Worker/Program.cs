/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Cli;
using Gravity.Abstraction.Logging;
using Gravity.Services.Comet;
using Gravity.Services.DataContracts;

using LiteDB;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Converters;
using Rhino.Controllers.Domain;
using Rhino.Controllers.Domain.Automation;
using Rhino.Controllers.Domain.Data;
using Rhino.Controllers.Domain.Formatters;
using Rhino.Controllers.Domain.Integration;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Domain.Middleware;
using Rhino.Controllers.Domain.Orchestrator;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
// hub
builder.Services.AddSingleton(typeof(IDictionary<string, TestCaseQueueModel>), new ConcurrentDictionary<string, TestCaseQueueModel>());
builder.Services.AddSingleton(new ConcurrentQueue<TestCaseQueueModel>());
builder.Services.AddSingleton(typeof(IDictionary<string, WebAutomation>), new ConcurrentDictionary<string, WebAutomation>());
builder.Services.AddSingleton(new ConcurrentQueue<WebAutomation>());
builder.Services.AddSingleton(new ConcurrentQueue<RhinoTestRun>());
builder.Services.AddSingleton(typeof(AppSettings));
builder.Services.AddSingleton(typeof(IDictionary<string, RhinoTestRun>), new ConcurrentDictionary<string, RhinoTestRun>());
builder.Services.AddTransient<IHubRepository, HubRepository>();

// utilities
builder.Services.AddTransient(typeof(ILogger), (_) => ControllerUtilities.GetLogger(builder.Configuration));
builder.Services.AddTransient(typeof(Orbit), (_) => new Orbit(Utilities.Types));
builder.Services.AddSingleton(typeof(IEnumerable<Type>), Utilities.Types);

// data
builder.Services.AddLiteDatabase(builder.Configuration.GetValue<string>("Rhino:StateManager:DataEncryptionKey"));

// domain
builder.Services.AddTransient<IEnvironmentRepository, EnvironmentRepository>();
builder.Services.AddTransient<ILogsRepository, LogsRepository>();
builder.Services.AddTransient<IPluginsRepository, PluginsRepository>();
builder.Services.AddTransient<IRepository<RhinoConfiguration>, ConfigurationsRepository>();
builder.Services.AddTransient<IRepository<RhinoModelCollection>, ModelsRepository>();
builder.Services.AddTransient<IApplicationRepository, ApplicationRepository>();
builder.Services.AddTransient<IRhinoAsyncRepository, RhinoRepository>();
builder.Services.AddTransient<IRhinoRepository, RhinoRepository>();
builder.Services.AddTransient<IMetaDataRepository, MetaDataRepository>();
builder.Services.AddTransient<ITestsRepository, TestsRepository>();
builder.Services.AddTransient<IDomain, RhinoDomain>();
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

// log
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
    logger?.Info("Create-ServiceApplication = OK");
}

// invoke
app.Run();
