// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MaybeMonad;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace TrekkingForCharity.Api.App.Helpers
{
    public static class HttpRequestHeadersExtensions
    {
        private static OpenIdConnectConfiguration _config;
        private static IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        private static string _issuer;
        private static string _audience;

        public static async Task<Maybe<JwtSecurityToken>> GetCurrentPrinciple(
            this HttpRequestHeaders httpRequestHeaders, IConfigurationRoot config)
        {
            if (string.IsNullOrWhiteSpace(_issuer))
            {
                _issuer = config["Auth0:Issuer"];
            }

            if (string.IsNullOrWhiteSpace(_audience))
            {
                _audience = config["Auth0:Audience"];
            }

            if (!httpRequestHeaders.Contains("Authorization"))
            {
                return Maybe<JwtSecurityToken>.Nothing;
            }

            var headerValue = httpRequestHeaders.GetValues("Authorization");
            var bearerValue =
                headerValue.FirstOrDefault(v => v.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase)) ??
                string.Empty;
            if (string.IsNullOrWhiteSpace(bearerValue))
            {
                return Maybe<JwtSecurityToken>.Nothing;
            }

            var bearerToken = bearerValue.Split(' ')[1];

            return await ValidateToken(bearerToken);
        }

        private static async Task<Maybe<JwtSecurityToken>> ValidateToken(string jwtToken)
        {
            var documentRetriever = new HttpDocumentRetriever { RequireHttps = _issuer.StartsWith("https://") };

            if (_configurationManager == null)
            {
                _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{_issuer}.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever(),
                    documentRetriever);
            }

            if (_config == null)
            {
                _config = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
            }

            var validationParameter = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidAudience = _audience,
                ValidateAudience = true,
                ValidIssuer = _issuer,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = _config.SigningKeys
            };

            ClaimsPrincipal result = null;
            var tries = 0;
            SecurityToken token = null;
            while (result == null && tries <= 1)
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    result = handler.ValidateToken(jwtToken, validationParameter, out token);
                }
                catch (SecurityTokenSignatureKeyNotFoundException)
                {
                    _configurationManager.RequestRefresh();
                    tries++;
                }
                catch (SecurityTokenException)
                {
                    return Maybe<JwtSecurityToken>.Nothing;
                }
            }

            if (token is JwtSecurityToken jwtSecurityToken)
            {
                return Maybe.From(jwtSecurityToken);
            }

            return Maybe<JwtSecurityToken>.Nothing;
        }
    }
}