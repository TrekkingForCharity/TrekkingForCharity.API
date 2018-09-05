namespace TrekkingForCharity.Api.Client.ResultConstructs.Executors
{
    public class DomainError
    {
        public DomainError(string error, int errorCode)
        {
            this.Error = error;
            this.ErrorCode = errorCode;
        }
        public string Error { get; }
        public int ErrorCode { get; }
    }
}