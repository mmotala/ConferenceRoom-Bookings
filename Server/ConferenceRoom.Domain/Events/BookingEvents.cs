public sealed record BookingCreatedDomainEvent(
    Guid BookingId,
    Guid RoomId,
    Guid UserId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    string Purpose,
    DateTime OccurredAtUtc) : IDomainEvent;


public sealed record BookingUpdatedDomainEvent(
    Guid BookingId,
    Guid RoomId,
    Guid UserId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    string Purpose,
    DateTime OccurredAtUtc) : IDomainEvent;

public sealed record BookingCancelledDomainEvent(
    Guid BookingId,
    Guid RoomId,
    Guid UserId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    DateTime OccurredAtUtc) : IDomainEvent;