namespace AIBookStreet.API.ResponseModel
{
    public class BaseResponse
    {
        public bool IsSuccess { get; protected set; }
        public string Message { get; protected set; }

        public BaseResponse(bool isData, string message)
        {
            IsSuccess = isData;
            Message = message;
        }
    }

    public class LoginResponse<TResult> : BaseResponse where TResult : class
    {
        public TResult Result { get; }
        public string Token { get; }
        public string Expiration { get; }
        public bool PasswordChangeRequired { get; }

        public LoginResponse(string message, TResult result, string token, string expiration, bool passwordChangeRequired = false) : base(true, message)
        {
            Result = result;
            Token = token;
            Expiration = expiration;
            PasswordChangeRequired = passwordChangeRequired;
        }
    }

    public class ItemResponse<TResult> : BaseResponse where TResult : class
    {
        public TResult? Result { get; }

        public ItemResponse(string message, TResult? result = null) : base(result != null, message)
        {
            Result = result;
        }
    }

    public class ItemListResponse<TResult> : BaseResponse where TResult : class
    {
        public List<TResult>? Results { get; }

        public int TotalRecords { get; protected set; }

        public ItemListResponse(string message, List<TResult>? results = null) : base(results != null, message)
        {
            Results = results;
            TotalRecords = results != null ? results.Count : 0;
        }
    }
}
