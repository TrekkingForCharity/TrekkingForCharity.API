// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace TrekkingForCharity.Api.Client
{
    public class ApiClientHttpClientHandler : HttpClientHandler
    {
        private readonly Func<Task<string>> _getToken;
        private readonly string _functionsKey;
        private readonly Policy _policy;

        public ApiClientHttpClientHandler(Func<Task<string>> getToken, string functionsKey)
        {
            this._getToken = getToken;
            this._functionsKey = functionsKey;
            this._policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                });
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var auth = request.Headers.Authorization;
            if (auth != null)
            {
                var token = await this._getToken();
                request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, token);
            }

            request.Headers.Add("x-functions-key", this._functionsKey);

            return await this._policy.ExecuteAsync(() => base.SendAsync(request, cancellationToken))
                .ConfigureAwait(false);
        }
    }
}