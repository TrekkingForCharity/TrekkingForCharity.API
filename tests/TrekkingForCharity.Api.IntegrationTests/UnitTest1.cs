using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.IdentityModel.Tokens;
using Refit;
using Xunit;

namespace TrekkingForCharity.Api.IntegrationTests
{
    public class TrekTests
    {
        [Fact]
        public async Task Should_CreateTrek_When_ModelIsValid()
        {
            
            var apiClient = RestService.For<IApiClient>(new HttpClient(new AuthenticatedHttpClientHandler(GetToken)){BaseAddress = new Uri("")});
            
            var trek = await apiClient.CreateTrek("Trek Name", "Some Description", DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                "banner image");

            Assert.NotEqual(Guid.Empty, trek.Id);
            Assert.Equal("", trek.UserId);
            Assert.Equal("Trek Name", trek.Name);
            Assert.Equal("Some Description", trek.Description);
        }

        private async Task<string> GetToken()
        {
            string key = "MIIDGTCCAgGgAwIBAgIJfucfETA7q/8WMA0GCSqGSIb3DQEBCwUAMCoxKDAmBgNVBAMTH3RyZWtraW5nZm9yY2hhcml0eS5ldS5hdXRoMC5jb20wHhcNMTcwMzMwMTI1NjUwWhcNMzAxMjA3MTI1NjUwWjAqMSgwJgYDVQQDEx90cmVra2luZ2ZvcmNoYXJpdHkuZXUuYXV0aDAuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAsDqvOUCPF2iP+49GEwb88dBaOE2afW8/7s+sx+bGuJKQ1RBjqnQIlTH/l2PG26hdrMLQPhPSjxADIjdOa2ZHRmzBUqoL2A/ndzghXb4pK+jdtlct6J0OSF2qhOnLGLILm59g/9N4fQCp/d9RzvTTJf2cF8O983Lr1SYmbwf8pf9gn0UHbmhphqXL08mckQFP76HxREItR0w26XUTWrve/2Ro8BnacrLp+VAREuZzHjKlUjmz/20/LKhRNEnEscKttLhVYTL3OFYLM6aWfY5+mt8A2yLc+32ZVO9PiJWEcO22yCk6Wa/KN9U5ywYqF5MGkCQOmbq7tI0vnr9oOgkUSQIDAQABo0IwQDAPBgNVHRMBAf8EBTADAQH/MB0GA1UdDgQWBBSfuCk+N426uHf9QI0kqfWz4rVsqDAOBgNVHQ8BAf8EBAMCAoQwDQYJKoZIhvcNAQELBQADggEBAG3OOLGYCS633N/vmNHrrEj8yQw2UaMPjhI6WRH5ISQRqn4BmYZK1VcVNcKoijJJrWfi67epN0jzKBxU78KQB9Mq6GPzScGzwaSIUm0oDipnuZeaLxs0xgeFoT6X1qdcHjB901h7lpo8vVPRJo8pEHTaySAVmtGevyfSwV2I+UrEVE5r5Jwvm64Vi6m0Xs+NAgcOn8ARHSMXeWGraQP0zhveiK1BTw7wJDqRrjeXbsrf3FJgY4JLZwQuIwaqX0G1RmjNNV3+ZvjStvG4S9YulWjJ4LqxeFCT1onUiqw3vd/6tVyBv+fGUBy5EzAIGyo3AAwD/aM69+5RGqcveKo8VY4=";

            // Create Security key  using private key above:
            // not that latest version of JWT using Microsoft namespace instead of System
            var securityKey = new Microsoft
                .IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // Also note that securityKey length should be >256b
            // so you have to make sure that your private key has a proper length
            //
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials
                (securityKey, SecurityAlgorithms.HmacSha256Signature);

            //  Finally create a Token
            var header = new JwtHeader(credentials);

            //Some PayLoad that contain information about the  customer
            var payload = new JwtPayload
            {
                { "sub ", "auth0|dsfdsfds-dsfdf "}
            };

            //
            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();

            // Token to String so you can use it in your client
            return handler.WriteToken(secToken);
        }
    }

    
}
