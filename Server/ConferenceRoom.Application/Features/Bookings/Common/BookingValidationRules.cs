using FluentValidation;

public static class BookingValidationRules
{
    public static IRuleBuilderOptions<T, DateTime> MustStartInFuture<T>(
        this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
            .WithMessage("Booking start time must be in the future.");
    }

    public static IRuleBuilderOptions<T, T> MustEndAfterStart<T>(
        this IRuleBuilder<T, T> ruleBuilder,
        Func<T, DateTime> startTimeSelector,
        Func<T, DateTime> endTimeSelector)
    {
        return ruleBuilder
            .Must(command => endTimeSelector(command) > startTimeSelector(command))
            .WithMessage("Booking end time must be after start time.");
    }

    public static IRuleBuilderOptions<T, T> MustNotExceedMaximumDuration<T>(
        this IRuleBuilder<T, T> ruleBuilder,
        Func<T, DateTime> startTimeSelector,
        Func<T, DateTime> endTimeSelector)
    {
        return ruleBuilder
            .Must(command => endTimeSelector(command) <= startTimeSelector(command).AddHours(8))
            .WithMessage("A booking cannot be longer than 8 hours.");
    }
}
