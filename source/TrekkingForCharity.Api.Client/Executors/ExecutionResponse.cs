using System.Collections.Generic;
using TrekkingForCharity.Api.Client.ResultConstructs;

namespace TrekkingForCharity.Api.Client.Executors
{
    public class ExecutionResponse
    {
        public bool IsSuccess { get; set; }

        public string IsFailure { get; set; }
        public ICollection<ValidationIssue> ValidationIssues { get; set; }

    }

    public class ExecutionResponse<T> : ExecutionResponse
        where T : CommandResult
    {
        public T Result { get; set; }

    }
}