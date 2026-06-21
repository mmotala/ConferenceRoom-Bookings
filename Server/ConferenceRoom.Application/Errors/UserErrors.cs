public static class UserErrors
{
    public static readonly Error NotFound = new(
        "User.NotFound",
        "The requested user was not found.",
        ErrorType.NotFound);

    public static readonly Error Unauthorized = new(
        "User.Unauthorized",
        "You must be logged in to perform this action.",
        ErrorType.Unauthorized);

    public static readonly Error Forbidden = new(
        "User.Forbidden",
        "You do not have permission to perform this action.",
        ErrorType.Forbidden);
}