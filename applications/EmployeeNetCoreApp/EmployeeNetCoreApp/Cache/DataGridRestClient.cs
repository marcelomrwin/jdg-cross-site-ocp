using System.Net;
using System.Text.Json;
using EmployeeNetCoreApp.Model;
using RestSharp;

namespace EmployeeNetCoreApp.Cache
{
    public class DataGridRestClient
    {
        private readonly CacheConfiguration cacheConfig;
        private readonly string base_url;
        private RestClient restClient;

        private readonly ILogger<DataGridRestClient> logger;

        public DataGridRestClient(CacheConfiguration pCacheConfig)
        {
            this.cacheConfig = pCacheConfig;
            this.base_url = cacheConfig.Protocol + "://" + cacheConfig.Host + ":" + cacheConfig.Port + "/rest/v2";

            var options = new RestClientOptions(this.base_url);
            options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            restClient = new RestClient(options);

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
                builder.AddEventSourceLogger();
            });
            this.logger = loggerFactory.CreateLogger<DataGridRestClient>();
            logger.LogInformation("Data Grid Client created and configured\nBase URL: [" + this.base_url + "]");
        }

        public async void AddtoCache(string uuid, string employeeJson)
        {
            var request = new RestRequest("/caches/" + cacheConfig.Cache + "/" + uuid).AddJsonBody(employeeJson);
            if (!await KeyExistsInCacheAsync(uuid))
            {
                //POST /rest/v2/caches/{cacheName}/{cacheKey}
                var response = await restClient.PostAsync(request);
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new Exception("An error occurred when trying to insert the entity into the cache. \n" + response.StatusCode + "\n" + response.Content);
                }
            }
            else
            {
                //PUT /rest/v2/caches/{cacheName}/{cacheKey}                
                var response = await restClient.PutAsync(request);
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new Exception("An error occurred when trying to insert the entity into the cache. \n" + response.StatusCode + "\n" + response.Content);
                }
            }
        }

        //HEAD /rest/v2/caches/{cacheName}/{cacheKey}
        public async Task<bool> KeyExistsInCacheAsync(string uuid)
        {
            var request = new RestRequest("/caches/" + cacheConfig.Cache + "/" + uuid);
            var response = await restClient.HeadAsync(request);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<Employee> GetEmployeeFromCache(string uuid)
        {
            //GET /rest/v2/caches/{cacheName}/{cacheKey}
            var request = new RestRequest("/caches/" + cacheConfig.Cache + "/" + uuid);
            var response = await restClient.GetAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<Employee>(response.Content, options);
            }
            return null;
        }

        public async Task<ISet<string>> GetAllKeysFromCache()
        {
            //GET /rest/v2/caches/{cacheName}?action=keys
            var request = new RestRequest("/caches/" + cacheConfig.Cache + "?action=keys");
            var response = await restClient.GetAsync(request);
            ISet<string>? list = JsonSerializer.Deserialize<ISet<string>>(response.Content);
            return list;
        }
    }
}

