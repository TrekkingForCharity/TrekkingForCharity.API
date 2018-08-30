using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace TrekkingForCharity.Api.IntegrationTests
{
    public interface IApiClient
    {
        [Post("api/treks")]
        Task<Trek> CreateTrek(string name, string description, long whenToStart, string bannerImage);
    }

    public class Trek
    {
        public Trek(string name, string description, string bannerImage, int status, long whenToStart, long? whenStarted, Guid id, string userId, string slug)
        {
            this.Name = name;
            this.Description = description;
            this.BannerImage = bannerImage;
            this.Status = status;
            this.WhenToStart = whenToStart;
            this.WhenStarted = whenStarted;
            this.Id = id;
            this.UserId = userId;
            this.Slug = slug;
        }
        public string Name { get; }

        public string Description { get; }

        public string BannerImage { get; }

        public int Status { get; }

        public long WhenToStart { get; }

        public long? WhenStarted { get; }

        public Guid Id { get; }
        public string UserId { get; }

        public string Slug { get; }
    }

    class AuthenticatedHttpClientHandler : HttpClientHandler
    {
        private readonly Func<Task<string>> getToken;

        public AuthenticatedHttpClientHandler(Func<Task<string>> getToken)
        {
            if (getToken == null) throw new ArgumentNullException("getToken");
            this.getToken = getToken;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // See if the request has an authorize header
            var auth = request.Headers.Authorization;
            if (auth != null)
            {
                var token = await getToken().ConfigureAwait(false);
                request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, token);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}