using System.Threading.Tasks;
using TrekkingForCharity.Api.Client.Providers;

namespace TrekkingForCharity.Api.Client
{
    public abstract class AuthorizationProvider
    {
        private readonly IApiConfig _apiConfig;


        protected AuthorizationProvider(string apiPath)
        {
            this._apiConfig = new ApiConfig { ApiPath = apiPath };
        }

        public abstract Task<string> GetToken();

        public string ApiPath => this._apiConfig.ApiPath;
    }
}
