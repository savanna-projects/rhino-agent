﻿/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Rhino.Controllers.Models.Server
{
    /// <summary>
    /// Contract for api/:version/:meta operator(s).
    /// </summary>
    [DataContract]
    public class RhinoVerbModel : BaseModel<object>
    { }
}