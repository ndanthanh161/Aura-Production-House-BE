using System.Text.Json.Serialization;

namespace Aura.Application.Common;

/// <summary>
/// Standardized API Response wrapper (RFC 7807 inspired)
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Errors { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; set; }

    // ===== Factory Methods =====

    public static ApiResponse<T> SuccessResponse(T data, string message = "Request successful", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> CreatedResponse(T data, string message = "Resource created successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = 201,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors
        };
    }

    public static ApiResponse<T> NotFoundResponse(string message = "Resource not found")
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = 404,
            Message = message
        };
    }

    public static ApiResponse<T> UnauthorizedResponse(string message = "Unauthorized")
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = 401,
            Message = message
        };
    }

    public static ApiResponse<T> ForbiddenResponse(string message = "Forbidden")
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = 403,
            Message = message
        };
    }

    public static ApiResponse<T> InternalErrorResponse(string message = "An unexpected error occurred")
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = 500,
            Message = message
        };
    }
}
