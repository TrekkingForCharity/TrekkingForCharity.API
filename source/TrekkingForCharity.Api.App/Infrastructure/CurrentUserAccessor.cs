﻿// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MaybeMonad;
using Microsoft.Extensions.Configuration;
using TrekkingForCharity.Api.App.Helpers;
using TrekkingForCharity.Api.Core.Infrastructure;

namespace TrekkingForCharity.Api.App.Infrastructure
{
    public class CurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly IConfigurationRoot _config;
        private readonly HttpRequestMessage _requestMessage;

        public CurrentUserAccessor(IConfigurationRoot config, HttpRequestMessage requestMessage)
        {
            this._config = config;
            this._requestMessage = requestMessage;
        }

        public async Task<Maybe<CurrentUser>> GetCurrentUser()
        {
            var principleMaybe = await this._requestMessage.Headers.GetCurrentPrinciple(this._config);
            if (principleMaybe.HasNoValue)
            {
                return Maybe<CurrentUser>.Nothing;
            }

            var principle = principleMaybe.Value;
            return Maybe.From(new CurrentUser(principle.Claims.First(x => x.Type == "sub").Value));
        }
    }
}