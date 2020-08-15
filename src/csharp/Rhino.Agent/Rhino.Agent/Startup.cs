/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Gravity.Services.Comet;

using LiteDB;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Rhino.Agent.Components;
using Rhino.Agent.Domain;
using Rhino.Agent.Models;
using Rhino.Api.Extensions;
using Rhino.Api.Parser.Components;

#pragma warning disable CA1822
namespace Rhino.Agent
{
    public class Startup
    {
        // constants
        private const string CorsPolicy = "CorsPolicy";

        // statics
        public static HttpClient HttpClient => new HttpClient();
        public static LiteDatabase LiteDb => new LiteDatabase("Data.dll");
        public static JsonSerializerSettings jsonSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Creates a new instance of this Startup component
        /// </summary>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        public Startup(IConfiguration configuration) => Configuration = configuration;

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
            services.AddRazorPages();
            services.AddControllers().AddJsonOptions(i =>
            {
                i.JsonSerializerOptions.WriteIndented = true;
                i.JsonSerializerOptions.IgnoreNullValues = true;
                i.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = _ => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // This lambda determines the cross origin (CORS) behavior
            services.AddCors(o => o.AddPolicy(CorsPolicy, builder
                => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            // This lambda determines the DI container mapping            
            services.AddScoped<KnowledgeBaseManager, KnowledgeBaseManager>();
            services.AddScoped<PluginParser, PluginParser>();
            services.AddScoped<RhinoTestCaseRepository, RhinoTestCaseRepository>();
            services.AddScoped<RhinoPageModelCollection, RhinoPageModelCollection>();
            services.AddScoped<RhinoConfigurationRepository, RhinoConfigurationRepository>();
            services.AddScoped<RhinoLogsRepository, RhinoLogsRepository>();

            services.AddSingleton(typeof(JsonSerializerSettings), jsonSettings);
            services.AddSingleton(typeof(LiteDatabase), LiteDb);
            services.AddSingleton(typeof(IEnumerable<Type>), Utilities.Types);
            services.AddSingleton(typeof(HttpClient), HttpClient);
            services.AddSingleton(typeof(Orbit), new Orbit(Utilities.Types));
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
            app.UseCookiePolicy();
            app.UseCors(CorsPolicy);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapRazorPages());
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
