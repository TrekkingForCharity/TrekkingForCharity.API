using Algolia.Search;
using Microsoft.Extensions.Configuration;

namespace TrekkingForCharity.Api.App.Infrastructure
{
    public static class AlgoliaClientAccessor
    {
        private static AlgoliaClient algoliaClient;


        public static AlgoliaClient GetAlgoliaClient(IConfigurationRoot config) 
        {
            var applicationId = config["Algolia:ApplicationId"];
            var apiKey = config["Algolia:ApiKey"];

            if (algoliaClient != null)
            {
                return algoliaClient;
            }
                
            algoliaClient = new AlgoliaClient(applicationId, apiKey);

            return algoliaClient;            
        }
    }
}