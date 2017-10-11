// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using MaybeMonad;
using Microsoft.IdentityModel.Tokens;

namespace TrekkingForCharity.Api.App.Helpers
{
    /// <summary>
    /// The http request headers extensions.
    /// </summary>
    public static class HttpRequestHeadersExtensions
    {
        public static Maybe<ClaimsPrincipal> GetCurrentPrinciple(this HttpRequestHeaders httpRequestHeaders)
        {
            if (!httpRequestHeaders.Contains("Authorization"))
            {
                return Maybe<ClaimsPrincipal>.Nothing;
            }

            var headerValue = httpRequestHeaders.GetValues("Authorization");
            var bearerValue =
                headerValue.FirstOrDefault(v => v.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase)) ??
                string.Empty;
            if (string.IsNullOrWhiteSpace(bearerValue))
            {
                return Maybe<ClaimsPrincipal>.Nothing;
            }

            var bearerToken = bearerValue.Split(' ')[1];

            return ValidateToken(bearerToken);
        }

        /// <summary>
        /// The generate certificate.
        /// </summary>
        /// <param name="cert">
        /// The cert.
        /// </param>
        /// <returns>
        /// The <see cref="X509Certificate2"/>.
        /// </returns>
        private static X509Certificate2 GenerateCertificate(string cert)
        {
            try
            {
                var rawData = Convert.FromBase64String(cert);
                return new X509Certificate2(rawData);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// The validate token.
        /// </summary>
        /// <param name="jwtToken">
        /// The jwt token.
        /// </param>
        /// <returns>
        /// The <see cref="ClaimsPrincipal"/>.
        /// </returns>
        private static Maybe<ClaimsPrincipal> ValidateToken(string jwtToken)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(jwtToken))
            {
                return null;
            }

            handler.InboundClaimTypeMap.Clear();

            var cert = new X509SecurityKey(GenerateCertificate(ConfigurationManager.AppSettings["Cert"]));

            try
            {
                return Maybe.From(handler.ValidateToken(
                    jwtToken,
                    new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = cert,
                        SignatureValidator = (t, param) => new JwtSecurityToken(t),
                        NameClaimType = "sub"
                    },
                    out _));
            }
            catch (SecurityTokenExpiredException)
            {
                return Maybe<ClaimsPrincipal>.Nothing;
            }
        }
    }
}