/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Services.Comet;

using LiteDB;

using Microsoft.OpenApi.Models;

using Rhino.Agent.Cli;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Converters;
using Rhino.Controllers.Controllers;
using Rhino.Controllers.Domain;
using Rhino.Controllers.Domain.Automation;
using Rhino.Controllers.Domain.Data;
using Rhino.Controllers.Domain.Integration;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Domain.Middleware;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Formatters;
using Rhino.Controllers.Models;

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v3", new OpenApiInfo { Title = "Rhino Controllers", Version = "v3" });
    c.EnableAnnotations();
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
#endregion

#region *** Dependencies  ***
// utilities
builder.Services.AddTransient(typeof(ILogger), (_) => ControllerUtilities.GetLogger(builder.Configuration));
builder.Services.AddTransient(typeof(Orbit), (_) => new Orbit(Utilities.Types));
builder.Services.AddSingleton(typeof(IEnumerable<Type>), Utilities.Types);

// data
builder.Services.AddLiteDatabase(builder.Configuration.GetValue<string>("Rhino:StateManager:Key"));

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
var logsFolder = app.Configuration.GetValue("Rhino:ReportConfiguration:LogsOut", Environment.CurrentDirectory);
var physicalPath = ControllerUtilities.GetStaticReportsFolder(configuration: app.Configuration);

// build
app.ConfigureExceptionHandler(new TraceLogger("RhinoApi", "ExceptionHandler", logsFolder));
app.UseCookiePolicy();
app.UseCors("CorsPolicy");
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v3/swagger.json", "Rhino Controllers v3"));
app.UseRouting();
app.UseBlazorFrameworkFiles();
app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
    endpoints.MapFallbackToFile("index.html");
});
app.UseEndpoints(endpoints => endpoints.MapRazorPages());
app.UseEndpoints(endpoints => endpoints.MapControllers());
app.UseStaticFiles();
app.UseStaticFiles(physicalPath, route: "/reports");
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
