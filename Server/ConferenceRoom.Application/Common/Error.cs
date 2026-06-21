public sealed record Error(
    string Code,
    string Message,
    ErrorType Type)
{
    public static readonly Error None = new(
        string.Empty,
        string.Empty,
        ErrorType.None);

    public static Error Validation(string message)
    {
        return new Error(
            "Validation.Failed",
            message,
            ErrorType.Validation);
    }
}