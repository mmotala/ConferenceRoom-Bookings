public sealed class RecurringBookingSeries : BaseEntity
{
    private RecurringBookingSeries()
    {
    }

    public RecurringBookingSeries(
        Guid userId,
        Guid roomId,
        DateTime seriesStartUtc,
        DateTime seriesEndUtc,
        RecurrenceType recurrenceType,
        DateTime recurrenceUntilUtc,
        string purpose)
    {
        UserId = userId;
        RoomId = roomId;
        SeriesStartUtc = seriesStartUtc;
        SeriesEndUtc = seriesEndUtc;
        RecurrenceType = recurrenceType;
        RecurrenceUntilUtc = recurrenceUntilUtc;
        Purpose = purpose;
    }

    public Guid UserId { get; private set; }
    public Guid RoomId { get; private set; }

    public DateTime SeriesStartUtc { get; private set; }
    public DateTime SeriesEndUtc { get; private set; }

    public RecurrenceType RecurrenceType { get; private set; }
    public DateTime RecurrenceUntilUtc { get; private set; }

    public string Purpose { get; private set; } = string.Empty;
}