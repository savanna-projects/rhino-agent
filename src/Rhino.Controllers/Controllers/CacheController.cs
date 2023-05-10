/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Converters;
using Rhino.Controllers.Domain.Cache;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;
using Rhino.Settings;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text.Json;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion($"{AppSettings.ApiVersion}.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        // POST api/v3/cache/sync
        [HttpGet, Route("sync")]
        [SwaggerOperation(
            Summary = "Sync-Cache",
            Description = "Synchronizing the application cache, reloading and parsing all entities. Please note, this process can take a while.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(object))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult SyncCache()
        {
            try
            {
                MetaDataCache.SyncCache();
                return Ok(new
                {
                    TotalPlugins = MetaDataCache.Plugins.Count
                });
            }
            catch (Exception e) when (e!=null)
            {
                // setup
                var errorResponse = new GenericErrorModel<string>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Request = e.StackTrace,
                    RouteData = Request.RouteValues
                };
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                options.Converters.Add(new ExceptionConverter());

                // get
                return new ContentResult
                {
                    Content = JsonSerializer.Serialize(errorResponse, options),
                    ContentType = MediaTypeNames.Application.Json,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        // POST api/v3/cache/plugins/sync
        [HttpPost, Route("plugins/sync")]
        [SwaggerOperation(
            Summary = "Sync-Plugins",
            Description = "Synchronizing the `Rhino Plugins` cache, reloading and parsing all changed entities.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(object))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult SyncPlugins(IEnumerable<PluginCacheSyncModel> models)
        {
            try
            {
                var m = new PluginCacheSyncModel();
                m.Specification = System.IO.File.ReadAllText(@"C:\Users\s_roe\Desktop\finastra\source-code\rhino-agent\src\Rhino.Agent\Plugins\Rhino\ValidatePaymentData\PluginSpec.rhino");

                var j = JsonSerializer.Serialize(m);

                MetaDataCache.SyncPlugins(new[]{ m});
                return Ok(new
                {
                    TotalPlugins = MetaDataCache.Plugins.Count
                });
            }
            catch (Exception e) when (e != null)
            {
                // setup
                var errorResponse = new GenericErrorModel<string>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Request = e.StackTrace,
                    RouteData = Request.RouteValues
                };
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                options.Converters.Add(new ExceptionConverter());

                // get
                return new ContentResult
                {
                    Content = JsonSerializer.Serialize(errorResponse, options),
                    ContentType = MediaTypeNames.Application.Json,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
