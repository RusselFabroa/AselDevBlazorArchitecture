namespace AselDevBlazorArchitecture.Domain.Entities.Responses
{
    public class APIResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }

        public APIResponse() { }

        public APIResponse(T? data, string message = "", int statusCode = 200)
        {
            Success = true;
            Data = data;
            Message = message;
            StatusCode = statusCode;
        }

        public APIResponse(string message, int statusCode)
        {
            Success = false;
            Data = default;
            Message = message;
            StatusCode = statusCode;
        }
    }
}
