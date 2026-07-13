namespace AselDevBlazorArchitecture.Application.Common
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }

        public ServiceResponse() { }

        public ServiceResponse(string message = "")
        {
            Success = false;
            Data = default;
            Message = message;
            StatusCode = 200;
        }

        public ServiceResponse(T data, string message = "", int statusCode = 200)
        {
            Success = true;
            Data = data;
            Message = message;
            StatusCode = statusCode;
        }

        public ServiceResponse(string message, int statusCode)
        {
            Success = false;
            Data = default;
            Message = message;
            StatusCode = statusCode;
        }
    }

    public class ServiceResponse : ServiceResponse<object>
    {
        public static ServiceResponse Ok(string message = "Success")
            => new() { Success = true, Message = message, StatusCode = 200 };

        public static ServiceResponse NotFound(string message = "Record not found")
            => new() { Success = false, Message = message, StatusCode = 404 };

        public static ServiceResponse ServerError(string message = "An unexpected error occurred")
            => new() { Success = false, Message = message, StatusCode = 500 };

        public static ServiceResponse Error(string message = "Something went wrong", int statusCode = 400)
            => new() { Success = false, Message = message, StatusCode = statusCode };
    }
}
