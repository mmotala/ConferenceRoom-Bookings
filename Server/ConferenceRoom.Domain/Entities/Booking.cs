public sealed class Booking : BaseEntity
{
    public Booking(
        Guid roomId,
        Guid userId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        string purpose)
    {
        RoomId = roomId;
        UserId = userId;
        StartTimeUtc = startTimeUtc;
        EndTimeUtc = endTimeUtc;
        Purpose = purpose;
        Status = BookingStatus.Active;

        RaiseDomainEvent(new BookingCreatedDomainEvent(
            Id,
            RoomId,
            UserId,
            StartTimeUtc,
            EndTimeUtc,
            Purpose,
            DateTime.UtcNow));
    }

    public Guid RoomId { get; private set; }
    public Room Room { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public DateTime StartTimeUtc { get; private set; }
    public DateTime EndTimeUtc { get; private set; }

    public string Purpose { get; private set; } = string.Empty;

    public BookingStatus Status { get; private set; }

    public bool IsActive => Status == BookingStatus.Active;

    public void Update(
        Guid roomId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        string purpose)
    {
        RoomId = roomId;
        StartTimeUtc = startTimeUtc;
        EndTimeUtc = endTimeUtc;
        Purpose = purpose;

        RaiseDomainEvent(new BookingUpdatedDomainEvent(
            Id,
            RoomId,
            UserId,
            StartTimeUtc,
            EndTimeUtc,
            Purpose,
            DateTime.UtcNow));
    }

    public void Cancel()
    {
        Status = BookingStatus.Cancelled;

        RaiseDomainEvent(new BookingCancelledDomainEvent(
            Id,
            RoomId,
            UserId,
            StartTimeUtc,
            EndTimeUtc,
            DateTime.UtcNow));
    }
}