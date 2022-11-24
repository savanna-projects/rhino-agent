/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using LiteDB;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.IO;

namespace Rhino.Controllers.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a singleton instance of ILiteDatabase based on Rhino.Api architecture.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        /// <returns>Self reference</returns>
        public static IServiceCollection AddLiteDatabase(this IServiceCollection services, string encryptionKey)
        {
            // constants
            const string DbEncrypKeyDefault = "30908f87-8539-477a-86e7-a4c13d4583c4";

            // setup
            var path = Path.Combine(Environment.CurrentDirectory, "Data");
            var dataConnection =
                "Filename=" + Path.Combine(path, "Data.dll") + ";" +
                "Password=$(password);" +
                //"Connection=shared;" +
                "Upgrade=true";

            // build
            Directory.CreateDirectory(path);
            dataConnection = string.IsNullOrEmpty(encryptionKey)
                ? dataConnection.Replace("$(password)", DbEncrypKeyDefault)
                : dataConnection.Replace("$(password)", encryptionKey);

            // add
            var liteDb = new LiteDatabase(dataConnection);
            services.AddSingleton(typeof(ILiteDatabase), (_) => liteDb);

            // get
            return services;
        }
    }
}
