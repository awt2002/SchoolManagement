using SMS.Domain.Enums;

namespace SMS.Application.Interfaces
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        string Username { get; }
        UserRole Role { get; }
        bool IsAuthenticated { get; }
    }
}
