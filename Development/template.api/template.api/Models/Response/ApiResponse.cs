namespace template.api.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }

        public ApiResponse(bool success, string message, T result, int validationStatus)
        {
            Success = success;
            Message = message;
            Result = result;
        }

        public ApiResponse()
        {
            Success = true;
            Message = string.Empty;
            Result = default(T);
        }
    }
}
