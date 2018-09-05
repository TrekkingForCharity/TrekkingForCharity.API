using System.Collections.Generic;
using TrekkingForCharity.Api.Client.Executors;

namespace TrekkingForCharity.Api.Client.ResultConstructs.Executors
{
    public class CompletedExecutionWithResult<TCommandResult> where TCommandResult : CommandResult
    {
        private CompletedExecutionWithResult(TCommandResult commandResult)
        {
            this.ExecutionStatus = ExecutionStatus.Successful;
            this.CommandResult = commandResult;
        }
        private CompletedExecutionWithResult(ExecutionStatus executionStatus)
        {
            this.ExecutionStatus = executionStatus;
        }

        private CompletedExecutionWithResult(ICollection<ValidationIssue> validationIssues)
        {
            this.ExecutionStatus = ExecutionStatus.ValidationIssue;
            this.ValidationIssues = validationIssues;
        }

        private CompletedExecutionWithResult(DomainError domainError)
        {
            this.ExecutionStatus = ExecutionStatus.DomainError;
            this.DomainError = domainError;
        }

        public TCommandResult CommandResult { get; }
        public ExecutionStatus ExecutionStatus { get; }
        public ICollection<ValidationIssue> ValidationIssues { get; }
        public DomainError DomainError { get; }

        public static CompletedExecutionWithResult<TCommandResult> Ok(TCommandResult commandResult)
        {
            return new CompletedExecutionWithResult<TCommandResult>(commandResult);
        }

        public static CompletedExecutionWithResult<TCommandResult> WithValidationIssues(ICollection<ValidationIssue> validationIssues)
        {
            return new CompletedExecutionWithResult<TCommandResult>(validationIssues);
        }

        public static CompletedExecutionWithResult<TCommandResult> WithDomainError(DomainError domainError)
        {
            return new CompletedExecutionWithResult<TCommandResult>(domainError);
        }

        public static CompletedExecutionWithResult<TCommandResult> NotAuthorized()
        {
            return new CompletedExecutionWithResult<TCommandResult>(ExecutionStatus.NotAuthorized);
        }

    }
}