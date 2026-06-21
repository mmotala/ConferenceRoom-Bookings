using ConferenceRoom.Domain.Entities;

public sealed class Booking : BaseEntity
{
    private Booking()
    {
    }

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
    }

    public void Cancel()
    {
        Status = BookingStatus.Cancelled;
    }

    public bool Overlaps(DateTime startTimeUtc, DateTime endTimeUtc)
    {
        return StartTimeUtc < endTimeUtc && startTimeUtc < EndTimeUtc;
    }
}