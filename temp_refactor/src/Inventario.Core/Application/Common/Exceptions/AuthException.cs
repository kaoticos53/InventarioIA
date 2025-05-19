namespace Inventario.Core.Application.Common.Exceptions
{
    public class AuthException : BaseException
    {
        public AuthException(string message) : base(message, 401) { }
    }
}
