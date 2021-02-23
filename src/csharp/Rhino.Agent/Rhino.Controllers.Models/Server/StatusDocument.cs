/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
namespace Rhino.Controllers.Models
{
    /// <summary>
    /// General documentation for the different HTTP response codes.
    /// </summary>
    public static class SwaggerDocument
    {
        public static class StatusCode
        {
            public const string Status200OK = "The resource(s) found and returned as part of the response.";
            public const string Status201Created = "The resource(s) were been created and returned as part of the response.";
            public const string Status204NoContent = "The operation was successful and there is no content to return.";
            public const string Status400BadRequest = "The request is not valid, bad formatted or missing parameters.";
            public const string Status404NotFound = "The resource(s) were not found using the provided information.";
            public const string Status500InternalServerError = "The operation failed on the server side. Refer to the server logs for more information.";
        }

        public static class Parameter
        {
            public const string Parallel =
                "A flag indicates rather to run all the configurations found for this collection _**in parallel**_.\n" +
                "> Information!\n\n" +
                "> This parameter will not replace the _**maxParallelExecution**_ configuration parameter.\n" +
                "> The configurations will run in parallel in addition to any parallel tests configuration (parallel on parallel).\n\n" +
                "> The max parallel count will be the process count of the machine running the automation unless specified otherwise under 'maxParallel'." +
                "> query parameter.";
            public const string MaxParallel =
                "Specified the maximum number configurations can run in parallel.\n" +
                "> Information\n\n" +
                "> You must set 'parallel' query parameter to _**true**_ in order for this to take effect.\n" +
                "> If not specified or set to '0' or negative number, the max parallel count will be the process count of the machine running the automation.";
            public const string Congifuration = "The ID of the _**Rhino Configuration**_ to attach.";
            public const string Entity = "The request payload (as documented in the contracts section).";
            public const string Id = "The unique identifier by which to find the requested resource.";
            public const string Private = "When set to true, the entity will be created using the provided _**authentication header**_.";
        }
    }
}