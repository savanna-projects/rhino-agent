using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Rhino.ControlPanel;

using System;
using System.Net.Http;

// setup
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// build
builder.RootComponents.Add<App>("#app");
builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// invoke
await builder.Build().RunAsync().ConfigureAwait(false);
