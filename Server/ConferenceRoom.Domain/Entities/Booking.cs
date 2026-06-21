public class Booking : IAuditable, ISoftDelete
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int AttendeeCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}