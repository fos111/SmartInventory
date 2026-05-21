namespace SmartInventory.Api.Models;

public class MobileEnvelope
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public bool? NeedsVerification { get; set; }
    public string? Email { get; set; }

    public static MobileEnvelope SuccessResult(string? message = null) =>
        new() { Success = true, Message = message };

    public static MobileEnvelope FailureResult(string message) =>
        new() { Success = false, Message = message };
}

public class MobileEnvelope<T> : MobileEnvelope
{
    public T? Data { get; set; }

    public new static MobileEnvelope<T> SuccessResult(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public new static MobileEnvelope<T> FailureResult(string message) =>
        new() { Success = false, Message = message, Data = default };
}
