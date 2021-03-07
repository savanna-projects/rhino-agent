/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using LiteDB;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IRepository<T> : ICrudable<T>, IHasAuthentication<IRepository<T>>
    {
        string CollectionName { get; }
        ILiteDatabase LiteDb { get; }

        IRepository<T> SetCollectionName(string name);
    }
}