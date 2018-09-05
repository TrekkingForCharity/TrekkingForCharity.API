using System;
using System.Collections.Generic;
using System.Text;

namespace TrekkingForCharity.Api.Client.ResultConstructs.Executors
{
    public class CompletedExecution
    {
        private CompletedExecution(ExecutionStatus executionStatus = ExecutionStatus.Successful)
        {
            this.ExecutionStatus = executionStatus;
        }

        private CompletedExecution(ICollection<ValidationIssue> validationIssues)
        {
            this.ExecutionStatus = ExecutionStatus.ValidationIssue;
            this.ValidationIssues = validationIssues;
        }

        private CompletedExecution(DomainError domainError)
        {
            this.ExecutionStatus = ExecutionStatus.DomainError;
            this.DomainError = domainError;
        }

        public ExecutionStatus ExecutionStatus { get; }
        public ICollection<ValidationIssue> ValidationIssues { get; }
        public DomainError DomainError { get; }

        public static CompletedExecution Ok()
        {
            return new CompletedExecution();
        }

        public static CompletedExecution WithValidationIssues(ICollection<ValidationIssue> validationIssues)
        {
            return new CompletedExecution(validationIssues);
        }

        public static CompletedExecution WithDomainError(DomainError domainError)
        {
            return new CompletedExecution(domainError);
        }

        public static CompletedExecution NotAuthorized()
        {
            return new CompletedExecution(ExecutionStatus.NotAuthorized);
        }
    }
}
