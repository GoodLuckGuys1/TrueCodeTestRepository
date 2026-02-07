namespace TrueCodeTest.Shared.Domain.Common;

public abstract class ResultBase
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
