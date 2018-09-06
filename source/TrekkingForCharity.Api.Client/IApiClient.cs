using System.Threading.Tasks;
using TrekkingForCharity.Api.Client.Executors;
using TrekkingForCharity.Api.Client.ResultConstructs.Executors;

namespace TrekkingForCharity.Api.Client
{
    public interface IApiClient
    {
        Task<CompletedExecution> ExecuteCommand<TCommand>(TCommand baseCommand) where TCommand : BaseCommand;

        Task<CompletedExecutionWithResult<TCommandResult>> ExecuteCommandWithResult<TCommand, TCommandResult>(
            TCommand baseCommand)
            where TCommand : BaseCommand
            where TCommandResult : CommandResult;

       
    }
}