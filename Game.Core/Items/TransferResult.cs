namespace Game.Core.Items;

/// <summary>
/// Outcome of an inventory operation (add / remove / move).
/// </summary>
public class TransferResult
{
    /// <summary>Whether the operation succeeded.</summary>
    public bool Success { get; }

    /// <summary>Human-readable reason when <see cref="Success"/> is false.</summary>
    public string? FailureReason { get; }

    private TransferResult(bool success, string? failureReason = null)
    {
        Success = success;
        FailureReason = failureReason;
    }

    /// <summary>Successful result.</summary>
    public static TransferResult Ok() => new(true);

    /// <summary>Failed result with a reason.</summary>
    public static TransferResult Fail(string reason) => new(false, reason);
}
