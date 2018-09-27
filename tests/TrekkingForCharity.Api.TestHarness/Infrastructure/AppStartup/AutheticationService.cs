// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TrekkingForCharity.Api.TestHarness.Infrastructure.AppStartup
{
    public static class AutheticationService
    {
        
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddCookie(options =>
                {
                    options.Events = new CookieAuthenticationEvents
                    {
                       OnValidatePrincipal = ValidateExpirationAndTryRefresh,
                        
                    };
                })
                .AddOpenIdConnect("Auth0", options =>
                {
                    // Set the authority to your Auth0 domain
                    options.Authority = $"https://{configuration["Auth0:Domain"]}";

                    // Configure the Auth0 Client ID and Client Secret
                    options.ClientId = configuration["Auth0:ClientId"];
                    options.ClientSecret = configuration["Auth0:ClientSecret"];

                    // Set response type to code
                    options.ResponseType = "code";

                    // Configure the scope
                    options.Scope.Clear();
                    options.Scope.Add("openid");

                    // Set the callback path, so Auth0 will call back to http://localhost:5000/signin-auth0
                    // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard
                    options.CallbackPath = new PathString("/signin-auth0");

                    options.SaveTokens = true;

                    // Configure the Claims Issuer to be Auth0
                    options.ClaimsIssuer = "Auth0";

                    options.Events = new OpenIdConnectEvents
                    {
                        // handle the logout redirection
                        OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut,
                        OnRedirectToIdentityProvider = OnRedirectToIdentityProvider,
                        OnTicketReceived = OnTicketReceived
                    };
                });

            return services;
        }

        private static Task OnTicketReceived(TicketReceivedContext context)
        {
            if (!(context.Principal.Identity is ClaimsIdentity identity))
            {
                return Task.FromResult(0);
            }

            if (!context.Properties.Items.ContainsKey(".TokenNames"))
            {
                return Task.FromResult(0);
            }

            var tokenNames = context.Properties.Items[".TokenNames"].Split(';');

            foreach (var tokenName in tokenNames)
            {
                var tokenValue = context.Properties.Items[$".Token.{tokenName}"];
                identity.AddClaim(new Claim(tokenName, tokenValue));
            }

            return Task.FromResult(0);
        }

        private static Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            var auth0Settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<Auth0Settings>>();
            var logoutUri =
                $"https://{auth0Settings.Value.Domain}/v2/logout?client_id={auth0Settings.Value.ClientId}";

            var postLogoutUri = context.Properties.RedirectUri;
            if (!string.IsNullOrEmpty(postLogoutUri))
            {
                if (postLogoutUri.StartsWith("/"))
                {
                    // transform to absolute
                    var request = context.Request;
                    postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase +
                                    postLogoutUri;
                }

                logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
            }

            context.Response.Redirect(logoutUri);
            context.HandleResponse();

            return Task.CompletedTask;
        }

        private static Task OnRedirectToIdentityProvider(RedirectContext context)
        {
            context.ProtocolMessage.SetParameter("audience", "https://api.trekkingforcharity.org");

            return Task.FromResult(0);
        }


        public static async Task ValidateExpirationAndTryRefresh(CookieValidatePrincipalContext context)
        {
            var auth0Settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<Auth0Settings>>();
            var shouldReject = true;
            
            var expClaim = context.Principal.FindFirst(c => c.Type == "expires_at");

            // Unix timestamp is seconds past epoch
            var validTo = DateTimeOffset.Parse(expClaim.Value);

            if (validTo > DateTimeOffset.UtcNow)
            {
                shouldReject = false;
            }
            else
            {
                var refreshToken = context.Principal.FindFirst("refresh_token")?.Value;
                if (refreshToken != null)
                {
                    // Try to get a new id_token from auth0 using refresh token
                    var authClient = new AuthenticationApiClient(new Uri($"https://{auth0Settings.Value.Domain}"));
                    var newIdToken =
                        await
                        authClient.GetTokenAsync(
                            new RefreshTokenRequest
                            {
                                ClientId = auth0Settings.Value.ClientId,
                                ClientSecret = auth0Settings.Value.ClientSecret,
                                RefreshToken = refreshToken
                            });

                    if (!string.IsNullOrWhiteSpace(newIdToken.IdToken))
                    {
                        var newPrincipal = ValidateJwt(newIdToken.IdToken, auth0Settings);
                        var identity = expClaim.Subject;
                        identity.RemoveClaim(expClaim);
                        identity.AddClaim(newPrincipal.FindFirst("exp"));

                        // Remove existing id_token claim
                        var tokenClaim = identity.FindFirst("id_token");
                        if (tokenClaim != null)
                        {
                            identity.RemoveClaim(tokenClaim);
                        }

                        // Add the new token claim
                        identity.AddClaim(new Claim("id_token", newIdToken.IdToken));

                        // now issue a new cookie
                        context.ShouldRenew = true;
                        shouldReject = false;
                    }
                }
            }

            if (shouldReject)
            {
                context.RejectPrincipal();

                // optionally clear cookie
                await context.HttpContext.Authentication.SignOutAsync("Auth0");
            }
        }

        private static ClaimsPrincipal ValidateJwt(string encodedJwt, IOptions<Auth0Settings> auth0Settings)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                //IssuerSigningKey = new SymmetricSecurityKey(Base64UrlEncoder.DecodeBytes(auth0Settings.Value.ClientSecret)),
                ValidIssuer = $"https://{auth0Settings.Value.Domain}/",
                ValidAudience = auth0Settings.Value.ClientId,
                // no real signature validation, we are trusting the delegation endpoint here.
                // Is that correct?
                SignatureValidator =
                    (token, parameters) =>
                        new JwtSecurityTokenHandler().ReadJwtToken(token)
            };

            SecurityToken securityToken;

            var newPrincipal = new JwtSecurityTokenHandler().ValidateToken(
                encodedJwt,
                tokenValidationParameters,
                out securityToken);

            return newPrincipal;
        }
    }
}

