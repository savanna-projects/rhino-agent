using Gravity.Services.Comet.Engine.Attributes;
using Gravity.Services.DataContracts;

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
            var response = s_httpClient.SendAsync(request).GetAwaiter().GetResult();
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
            var credentials = $"{repository.UserName}:{repository.Password}";
            var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(credentials);
            var base64 = Convert.ToBase64String(bytes);
            var request = new HttpRequestMessage();

            // build
            if (!string.IsNullOrEmpty(repository.UserName) || !string.IsNullOrEmpty(repository.Password))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);
            }

            // get
            return request;
        }
    }
}
