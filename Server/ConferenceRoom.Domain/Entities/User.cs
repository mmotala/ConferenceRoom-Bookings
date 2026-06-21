public class User : IAuditable, ISoftDelete
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}