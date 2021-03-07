using Gravity.Abstraction.Logging;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

using Rhino.Controllers.Models;

using System;
using System.Net;
using System.Threading.Tasks;

namespace Rhino.Controllers.Domain.Middleware
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
            app.UseExceptionHandler(appError => AppError(appError, logger));
        }

        private static void AppError(IApplicationBuilder appError, ILogger logger)
        {
            appError.Run(async context
                => await RunContextAsync(context, logger).ConfigureAwait(false));
        }

        private static async Task RunContextAsync(HttpContext context, ILogger logger)
        {
            // setup
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

            // exit conditions
            if (contextFeature == null)
            {
                return;
            }

            // handler: logging
            logger.Fatal($"{Environment.NewLine}Something went wrong", contextFeature.Error);

            // handler: error details
            var errorDetails = new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = contextFeature.Error.Message,
                Stack = $"{contextFeature.Error.GetBaseException()}"
            };

            // write
            await context.Response.WriteAsync(errorDetails.ToString()).ConfigureAwait(false);
        }
    }
}
