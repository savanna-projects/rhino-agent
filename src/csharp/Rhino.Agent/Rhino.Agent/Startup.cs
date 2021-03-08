/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Gravity.Abstraction.Logging;
using Gravity.Services.Comet;

using LiteDB;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Controllers;
using Rhino.Controllers.Domain.Automation;
using Rhino.Controllers.Domain.Data;
using Rhino.Controllers.Domain.Integration;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Domain.Middleware;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;

namespace Rhino.Agent
{
    public class Startup
    {
        // constants        
        public const string DbEncrypKey = "Rhino:StateManager:Key";
        public const string DbEncrypKeyDefault = "30908f87-8539-477a-86e7-a4c13d4583c4";
        private const string CorsPolicy = "CorsPolicy";

        // members statics
        private static readonly IEnumerable<Type> types = Utilities.GetTypes().SelectMany(i => i.Types);
        private readonly ILiteDatabase liteDatabase;

        // members: state
        private readonly ILogger logger;
        public readonly string dataConnection =
            "Filename=" + Path.Combine(Environment.CurrentDirectory, "Data", "Data.dll") + ";" +
            "Password=$(password);" +
            "Connection=shared;" +
            "Upgrade=true";

        /// <summary>
        /// Creates a new instance of Startup component
        /// </summary>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        public Startup(IConfiguration configuration)
        {
            // setup: configuration
            Configuration = configuration;

            // setup: logger
            logger = ControllerUtilities.GetLogger(configuration)?.CreateChildLogger(nameof(Startup));
            logger?.Debug("Create-Logger = Ok");

            // setup: state manager
            var encrypKey = configuration.GetValue<string>(DbEncrypKey);
            dataConnection = string.IsNullOrEmpty(encrypKey)
                ? dataConnection.Replace("$(password)", DbEncrypKeyDefault)
                : dataConnection.Replace("$(password)", encrypKey);
            liteDatabase = new LiteDatabase(dataConnection);
        }

        /// <summary>
        /// Gets this configuration as a set of key/value application configuration properties.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Components settings
            services.AddRazorPages();
            services.AddMvc().AddApplicationPart(typeof(RhinoController).Assembly).AddControllersAsServices();
            services.AddControllers().AddJsonOptions(i =>
            {
                i.JsonSerializerOptions.WriteIndented = true;
                i.JsonSerializerOptions.IgnoreNullValues = true;
                i.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v3", new OpenApiInfo { Title = "Rhino Controllers", Version = "v3" });
                c.EnableAnnotations();
            });
            services.AddApiVersioning(c =>
            {
                c.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(3, 0);
                c.AssumeDefaultVersionWhenUnspecified = true;
                c.ErrorResponses = new GenericErrorModel<IDictionary<string, object>>();
                c.ReportApiVersions = true;
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = _ => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddCors(o => o.AddPolicy(CorsPolicy, builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            // This lambda determines the DI container mapping
            services.AddTransient(typeof(ILogger), (_) => ControllerUtilities.GetLogger(Configuration));
            services.AddTransient(typeof(Orbit), (_) => new Orbit(types));

            services.AddTransient<IEnvironmentRepository, EnvironmentRepository>();
            services.AddTransient<ILogsRepository, LogsRepository>();
            services.AddTransient<IPluginsRepository, PluginsRepository>();
            services.AddTransient<IRepository<RhinoConfiguration>, ConfigurationsRepository>();
            services.AddTransient<IRepository<RhinoModelCollection>, ModelsRepository>();
            services.AddTransient<IApplicationRepository, ApplicationRepository>();
            services.AddTransient<IRhinoAsyncRepository, RhinoRepository>();
            services.AddTransient<IRhinoRepository, RhinoRepository>();
            services.AddTransient<IMetaDataRepository, MetaDataRepository>();
            services.AddTransient<ITestsRepository, TestsRepository>();

            services.AddSingleton(typeof(ILiteDatabase), (_) => liteDatabase);
            services.AddSingleton(types);

            // log
            logger?.Debug("Create-ServiceConfiguration = Ok");
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Defines a class that provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        /// <remarks>The default HSTS value is 30 days. You may want to change this for production RhinoTestCaseDocuments, see https://aka.ms/aspnetcore-hsts.</remarks>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.ConfigureExceptionHandler(logger?.CreateChildLogger("ExceptionHandler"));
            app.UseCookiePolicy();
            app.UseCors(CorsPolicy);
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v3/swagger.json", "Rhino Controllers v3"));
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapRazorPages());
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            // static files
            SetOutputsFolder(app);

            // log
            logger?.Debug("Create-ServiceApplicationSettings = Ok");
        }

        private void SetOutputsFolder(IApplicationBuilder app)
        {
            // get outputs path
            var path = ControllerUtilities.GetStaticReportsFolder(configuration: Configuration);

            // force
            Directory.CreateDirectory(path);

            // setup
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(path),
                RequestPath = "/reports",
                ServeUnknownFileTypes = true
            });
        }
    }
}