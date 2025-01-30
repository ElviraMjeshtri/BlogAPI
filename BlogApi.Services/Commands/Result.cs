namespace BlogApi.Services.Commands;

using System.Net;

public class Result<T>
{
    public T? Value { get; set; }
    public bool IsSuccess { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Parameterless constructor for deserialization
    public Result() { }

    private Result(T value, bool isSuccess, HttpStatusCode statusCode, string? errorMessage = null)
    {
        Value = value;
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value)
        => new Result<T>(value, true, HttpStatusCode.OK);

    public static Result<T> Failure(HttpStatusCode statusCode, string errorMessage)
        => new Result<T>(default, false, statusCode, errorMessage);
}
