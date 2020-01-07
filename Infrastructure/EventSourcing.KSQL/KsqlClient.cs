using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EventSourcing.KSQL
{
    public sealed class KsqlClient
    {
        private const string KsqlMediaType = "application/vnd.ksql.v1+json";
        private readonly HttpClient _client;

        public KsqlClient(HttpClient client) => _client = client;

        public async Task<Stream> ExecuteQueryAsync(KsqlQuery query, CancellationToken token = default)
        {
            var request = JsonConvert.SerializeObject(query);
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "")
                {
                    Content = new StringContent(request, Encoding.UTF8, KsqlMediaType),
                    Headers = {{"accept", MediaTypeWithQualityHeaderValue.Parse(KsqlMediaType).ToString()}}
                },
                HttpCompletionOption.ResponseHeadersRead,
                token);

            return await response.Content.ReadAsStreamAsync();
        }
    }
}