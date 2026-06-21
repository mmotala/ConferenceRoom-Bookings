using ConferenceRoom.Domain.Entities;

public sealed class User : BaseEntity
{
    private User()
    {
    }

    public User(string name, string email, UserRole role)
    {
        Name = name;
        Email = email;
        Role = role;
    }

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }

    public bool IsAdmin => Role == UserRole.Admin;
}