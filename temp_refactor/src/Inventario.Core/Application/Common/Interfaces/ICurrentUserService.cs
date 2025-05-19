namespace Inventario.Core.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        IList<string> Roles { get; }
    }
}
