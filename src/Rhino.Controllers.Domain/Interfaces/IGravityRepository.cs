/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IGravityRepository
    {
        /// <summary>
        /// Converts RhinoStep (specifications) to an ActionRule object.
        /// </summary>
        /// <param name="specifications">The specifications to convert.</param>
        /// <returns>An ActionRule object converted from the specifications.</returns>
        (int StatusCode, ActionRule ActionRule) Convert(string specifications);

        /// <summary>
        /// Converts RhinoStep (specifications) to an ActionRule object.
        /// </summary>
        /// <param name="specifications">The specifications to convert.</param>
        /// <param name="repositories">A collection of ExternalRepository to retrieve action from.</param>
        /// <returns>An ActionRule object converted from the specifications.</returns>
        (int StatusCode, ActionRule ActionRule) Convert(string specifications, IEnumerable<ExternalRepository> repositories);

        /// <summary>
        /// Invokes a WebAutomation object.
        /// </summary>
        /// <param name="automation">The  WebAutomation to invoke.</param>
        (int StatusCode, OrbitResponse Response) Invoke(WebAutomation automation);

        /// <summary>
        /// Invokes a WebAutomation object.
        /// </summary>
        /// <param name="automation">The  WebAutomation to invoke.</param>
        Task<OrbitResponse> InvokeAsync(WebAutomation automation);
    }
}
