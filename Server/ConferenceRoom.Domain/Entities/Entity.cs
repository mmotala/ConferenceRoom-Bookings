namespace ConferenceRoom.Domain.Entities
{
    public abstract class BaseEntity : IAuditable, ISoftDelete
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }
        public Guid? DeletedByUserId { get; private set; }

        public void SoftDelete(Guid? deletedByUserId)
        {
            if (IsDeleted)
            {
                return;
            }

            IsDeleted = true;
            DeletedAtUtc = DateTime.UtcNow;
            DeletedByUserId = deletedByUserId;
        }
    }
}
