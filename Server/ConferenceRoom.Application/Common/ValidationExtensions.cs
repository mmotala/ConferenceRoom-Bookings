using FluentValidation.Results;

public static class ValidationExtensions
{
    public static Error ToValidationError(this ValidationResult validationResult)
    {
        var message = string.Join(
            "; ",
            validationResult.Errors.Select(error => error.ErrorMessage));

        return Error.Validation(message);
    }
}