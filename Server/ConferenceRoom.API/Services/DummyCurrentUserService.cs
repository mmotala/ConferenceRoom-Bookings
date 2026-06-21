public sealed class DummyCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DummyCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return null;
            }

            var userIdHeader = httpContext.Request.Headers["X-User-Id"].FirstOrDefault();

            return Guid.TryParse(userIdHeader, out var userId)
                ? userId
                : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return null;
            }

            var roleHeader = httpContext.Request.Headers["X-User-Role"].FirstOrDefault();

            return Enum.TryParse<UserRole>(roleHeader, true, out var role)
                ? role
                : null;
        }
    }

    public bool IsAuthenticated => UserId.HasValue;

    public bool IsAdmin => Role == UserRole.Admin;
}