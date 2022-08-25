using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.DataContracts;

using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Rhino.Controllers.Domain.Extensions
{
    internal static class GravityExtensions
    {
        private static readonly HttpClient s_httpClient = new();
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        public static (string Name, IEnumerable<ActionAttribute> Entities) GetActions(this ExternalRepository repository)
        {
            // setup
            var endpoint = $"{repository.Url}/api/v{repository.Version}/gravity/actions";
            var request = GetRequest(repository);

            // build
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(endpoint);

            // send
            HttpResponseMessage response = null;
            try
            {
                response = s_httpClient.SendAsync(request).GetAwaiter().GetResult();
            }
            catch (Exception e) when (e != null)
            {
                Trace.TraceError("Get-Actions " +
                    $"-Type {nameof(ExternalRepository)} " +
                    $"-Url {repository.Url} " +
                    $"-Name {repository.Name} = (InternalServerError | {e.Message})");
            }
            
            if (response == null || !response.IsSuccessStatusCode)
            {
                return (null, null);
            }

            // build
            var content = response.Content.ReadAsStringAsync().Result;

            // get
            var entities = JsonSerializer.Deserialize<IEnumerable<ActionAttribute>>(content, s_jsonOptions);
            return (repository.Name, entities);
        }

        private static HttpRequestMessage GetRequest(ExternalRepository repository)
        {
            // setup
            var credentials = $"{repository.Username}:{repository.Password}";
            var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(credentials);
            var base64 = Convert.ToBase64String(bytes);
            var request = new HttpRequestMessage();

            // build
            if (!string.IsNullOrEmpty(repository.Username) || !string.IsNullOrEmpty(repository.Password))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);
            }

            // get
            return request;
        }
    }
}
