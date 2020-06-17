using Microsoft.Azure.Search;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Index = Microsoft.Azure.Search.Models.Index;
using MakeIndex.Models;
using Microsoft.Rest.Azure;
using System.Net;

namespace MakeIndex
{
    class Program
    {
        static IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        static IConfigurationRoot configuration = builder.Build();
        static async Task Main(string[] args)
        {

            SearchServiceClient searchService = CreateSearchServiceClient(configuration);

            string indexName = "idx-" + configuration["IndexNameforHospital"];
            string indexerName = "idxr-" + configuration["IndexNameforHospital"];
            string dataSourceName = "dsn-" + configuration["IndexNameforHospital"];
            string tableName = configuration["TableNameforHospital"];

            Console.WriteLine("Deleting index...\n");
            await DeleteIndexIfExists(indexName, searchService);

            Console.WriteLine("Creating index....\n");
            await CreateIndexForHospital(indexName, searchService);

            Console.WriteLine("Indexing Azure Sql Db hospital data...\n");
            await CreateAndRunSqlDbIndexer(indexName, indexerName, dataSourceName, tableName, searchService);

            indexName = "idx-" + configuration["IndexNameforDoctor"];
            indexerName = "idxr-" + configuration["IndexNameforDoctor"];
            dataSourceName = "dsn-" + configuration["IndexNameforDoctor"];
            tableName = configuration["TableNameforDoctor"];
            

            Console.WriteLine("Deleting index...\n");
            await DeleteIndexIfExists(indexName, searchService);

            Console.WriteLine("Creating index....\n");
            await CreateIndexForDoctor(indexName, searchService);

            Console.WriteLine("Indexing Azure Sql Db hospital data...\n");
            await CreateAndRunSqlDbIndexer(indexName, indexerName, dataSourceName, tableName, searchService);

            Console.WriteLine("Complete. Press any key to end applications...\n");
            Console.ReadKey();
        }

        private static SearchServiceClient CreateSearchServiceClient(IConfigurationRoot configuration)
        {
            string searchServiceName = configuration["SearchServiceName"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];

            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));

            return serviceClient;
        }

        private static async Task DeleteIndexIfExists(string indexName, SearchServiceClient serviceClient)
        {
            if (serviceClient.Indexes.Exists(indexName))
            {
                await serviceClient.Indexes.DeleteAsync(indexName);
            }
        }

        private static async Task CreateIndexForHospital(string indexName, SearchServiceClient searchService)
        {
            var definition = new Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<Hospital>()
            };

            await searchService.Indexes.CreateAsync(definition);
        }

        private static async Task CreateIndexForDoctor(string indexName, SearchServiceClient searchService)
        {
            var definition = new Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<Doctor>()
            };

            await searchService.Indexes.CreateAsync(definition);
        }

        private static async Task CreateAndRunSqlDbIndexer(string indexName, string indexerName, string dataSourceName, string tableName, SearchServiceClient searchService)
        {
            DataSource azureSqlDataSource = DataSource.AzureSql(
                name: dataSourceName,
                sqlConnectionString: configuration["AzureSqlConnectionString"],
                tableOrViewName: tableName
                );

            //azureSqlDataSource.DataDeletionDetectionPolicy = new SoftDeleteColumnDeletionDetectionPolicy("IsDeleted", true);

            await searchService.DataSources.CreateOrUpdateAsync(azureSqlDataSource);

            Console.WriteLine("Creating AzureSQL DB indexer...\n");

            Indexer azureSqlDbIndexer = new Indexer(
                name: indexerName,
                dataSourceName: azureSqlDataSource.Name,
                targetIndexName: indexName,
                //parameters: new IndexingParameters().ParseJsonArrays(),
                schedule: new IndexingSchedule(TimeSpan.FromDays(1)));

            bool exists = await searchService.Indexers.ExistsAsync(azureSqlDbIndexer.Name);

            if (exists)
            {
                await searchService.Indexers.ResetAsync(azureSqlDbIndexer.Name);
            }

            await searchService.Indexers.CreateOrUpdateAsync(azureSqlDbIndexer);

            Console.WriteLine("Running AzureSQL DB indexer...\n");

            try
            {
                await searchService.Indexers.RunAsync(azureSqlDbIndexer.Name);
            }
            catch (CloudException e) when (e.Response.StatusCode == (HttpStatusCode)429)
            {
                Console.WriteLine("Failed to run indexer: {0}", e.Response.Content);
            }
        }


        private static async Task CreateAndRunSqlDbIndexer2(string indexName, SearchServiceClient searchService)
        {
            DataSource azureSqlDataSource2 = DataSource.AzureSql(
                name: configuration["AzureSqlDatabaseName"],
                sqlConnectionString: configuration["AzureSqlConnectionString"],
                tableOrViewName: configuration["AzureSqlTableName"]
                );

            await searchService.DataSources.CreateOrUpdateAsync(azureSqlDataSource2);

            Console.WriteLine("");


        }
    }
}
