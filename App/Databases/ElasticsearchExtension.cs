

using Elasticsearch.Net;
using Nest;
using PepsiCompetitive.Modules.Beats.Entities;
using PepsiCompetitive.Modules.Players.Entities;
using PepsiCompetitive.Modules.Videos.Entities;

namespace PepsiCompetitive.App.Databases
{
    public static class ElasticsearchExtension
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            string[] nodes = configuration.GetSection($"ConnectionSetting:ElasticsearchSettings:Nodes").Get<string[]>();
            ConnectionSettings connectionSettings;
            if (nodes is null || string.IsNullOrEmpty(nodes[0]))
            {
                return;
            }
            if (nodes.Length == 1)
            {
                #region Connecting to a single node
                Uri uri = new(nodes[0]);
                connectionSettings = new(uri);
                #endregion
            }
            else
            {
                #region Connecting to multiple nodes using a connection pool
                Uri[] uris = new Uri[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    uris[i] = new Uri(nodes[i]);
                }
                StaticConnectionPool staticConnectionPool = new(uris);
                connectionSettings = new(staticConnectionPool);
                #endregion
            }
            connectionSettings.ThrowExceptions(alwaysThrow: true);
            if (!string.IsNullOrEmpty(configuration["ConnectionSetting:ElasticsearchSettings:Username"]) && !string.IsNullOrEmpty(configuration["ConnectionSetting:ElasticsearchSettings:Password"]))
            {
                connectionSettings.BasicAuthentication(configuration["ConnectionSetting:ElasticsearchSettings:Username"], configuration["ConnectionSetting:ElasticsearchSettings:Password"]);
            }
            connectionSettings.RequestTimeout(TimeSpan.FromMinutes(10));

            connectionSettings
                .DefaultMappingFor<Player>(m => m.IndexName("player"))
                .DefaultMappingFor<Beat>(m => m.IndexName("beat"))
                .DefaultMappingFor<Video>(m => m.IndexName("video"));


            ElasticClient elasticClient = new(connectionSettings);

            if (!elasticClient.Indices.Exists("player").Exists)
            {
                elasticClient.Indices.Create("player", creator => creator.Map<Player>(type => type.AutoMap()));
            }
            if (!elasticClient.Indices.Exists("beat").Exists)
            {
                elasticClient.Indices.Create("beat", creator => creator.Map<Beat>(type => type.AutoMap()));
            }
            if (!elasticClient.Indices.Exists("video").Exists)
            {
                elasticClient.Indices.Create("video", creator => creator.Map<Video>(type => type.AutoMap()));
            }
            services.AddSingleton<IElasticClient>(elasticClient);
        }

        public static SortDescriptor<T> SortDescriptor<T>(this IElasticClient client, string orderQuerry) where T : class
        {
            var sortDescriptor = new SortDescriptor<T>();
            //sortDescriptor.Field("userName.keyword", Nest.SortOrder.Ascending);
            return sortDescriptor;
        }
    }
}
