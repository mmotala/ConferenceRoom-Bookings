public interface IAuditable
{
    DateTime CreatedAtUtc { get; set; }
    DateTime? UpdatedAtUtc { get; set; }

    Guid? CreatedByUserId { get; set; }
    Guid? UpdatedByUserId { get; set; }
}

public interface ISoftDelete
{
    bool IsDeleted { get; }
    DateTime? DeletedAtUtc { get; }
    Guid? DeletedByUserId { get; }

    void SoftDelete(Guid? deletedByUserId);
}