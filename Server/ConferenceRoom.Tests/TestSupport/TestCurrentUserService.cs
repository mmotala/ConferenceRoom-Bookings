public sealed class TestCurrentUserService : ICurrentUserService
{
    public TestCurrentUserService(Guid? userId, UserRole? role)
    {
        UserId = userId;
        Role = role;
    }

    public Guid? UserId { get; private set; }
    public UserRole? Role { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;
    public bool IsAdmin => Role == UserRole.Admin;

    public void SetUser(Guid? userId, UserRole? role)
    {
        UserId = userId;
        Role = role;
    }
}
