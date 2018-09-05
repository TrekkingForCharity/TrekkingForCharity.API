using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using TrekkingForCharity.Api.Client.Executors;
using TrekkingForCharity.Api.Client.Providers;
using TrekkingForCharity.Api.Client.ResultConstructs;
using TrekkingForCharity.Api.Client.ResultConstructs.Executors;

namespace TrekkingForCharity.Api.Client
{
    public class ApiClient : HttpClient, IApiClient
    {
        private readonly AuthorizationProvider _configuration;

        private readonly Policy _policy = Policy
            .Handle<CommunicationException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            });

        public ApiClient(AuthorizationProvider configuration)
        {
            this.BaseAddress = new Uri(configuration.ApiPath);
            this._configuration = configuration;

        }


        public async Task<CompletedExecutionWithResult<TCommandResult>> ExecuteCommandWithResult<TCommand,
            TCommandResult>(
            TCommand baseCommand)
            where TCommand : BaseCommand
            where TCommandResult : CommandResult
        {
            var token = await this._configuration.GetToken();
            this.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(baseCommand), Encoding.UTF8,
                "application/json");
            var response = await this._policy.ExecuteAsync(() =>
                this.PostAsync($"api/execute-command/{baseCommand.GetRoute()}", content));


            if (response.StatusCode == (HttpStatusCode)200)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TCommandResult>(responseContent);
                return CompletedExecutionWithResult<TCommandResult>.Ok(result);
            }

            if (response.StatusCode == (HttpStatusCode)422)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ICollection<ValidationIssue>>(responseContent);
                return CompletedExecutionWithResult<TCommandResult>.WithValidationIssues(result);
            }

            if (response.StatusCode == (HttpStatusCode)400)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<DomainError>(responseContent);
                return CompletedExecutionWithResult<TCommandResult>.WithDomainError(result);
            }

            if (response.StatusCode == (HttpStatusCode)401)
            {
                return CompletedExecutionWithResult<TCommandResult>.NotAuthorized();
            }


            throw new Exception("Unhandled HTTP status code");
        }

        public async Task<CompletedExecution> ExecuteCommand<TCommand>(TCommand baseCommand)
            where TCommand : BaseCommand
        {
            var token = await this._configuration.GetToken();
            this.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(baseCommand), Encoding.UTF8,
                "application/json");

            var response = await this._policy.ExecuteAsync(() =>
                this.PostAsync($"api/execute-command/{baseCommand.GetRoute()}", content));

            if (response.StatusCode == (HttpStatusCode)200)
            {
                return CompletedExecution.Ok();
            }

            if (response.StatusCode == (HttpStatusCode)422)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ICollection<ValidationIssue>>(responseContent);
                return CompletedExecution.WithValidationIssues(result);
            }

            if (response.StatusCode == (HttpStatusCode)400)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<DomainError>(responseContent);
                return CompletedExecution.WithDomainError(result);
            }

            if (response.StatusCode == (HttpStatusCode)401)
            {
                return CompletedExecution.NotAuthorized();
            }


            throw new Exception("Unhandled HTTP status code");
        }


        

        

        
    }
}