namespace SmartEmployeePortal.API.Common;

/// <summary>
/// Standard API response envelope — wraps ALL responses in a consistent shape.
/// 
/// Every API response looks like:
/// {
///   "success": true,
///   "message": "Employee created successfully",
///   "data": { ... },
///   "errors": null
/// }
///
/// Why? The Angular frontend can always expect the same structure regardless
/// of whether it's a single employee, a list, or an error message.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Fail(string message, IDictionary<string, string[]>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };
}
