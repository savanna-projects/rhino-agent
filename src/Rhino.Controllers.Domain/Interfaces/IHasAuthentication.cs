/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IHasAuthentication<T>
    {
        Authentication Authentication { get; }

        T SetAuthentication(Authentication authentication);
    }
}