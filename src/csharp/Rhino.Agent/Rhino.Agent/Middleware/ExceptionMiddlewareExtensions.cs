using Gravity.Abstraction.Logging;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

using Rhino.Agent.Models;

using System;
using System.Net;

namespace Rhino.Agent.Middleware
{
    /// <summary>
    /// Extensions package for error handling middleware.
    /// </summary>
    public static class ExceptionMiddlewareExtensions
    {
        /// <summary>
        /// Configure global exceptions handler
        /// </summary>
        /// <param name="app">Defines a class that provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="logger">Logger implementation to use with this middleware</param>
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        logger.Fatal($"{Environment.NewLine}Something went wrong", contextFeature.Error);
                        await context.Response.WriteAsync(new ErrorDetails()
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = contextFeature.Error.Message,
                            Stack = $"{contextFeature.Error}"
                        }
                        .ToString())
                        .ConfigureAwait(false);
                    }
                });
            });
        }
    }
}