namespace Inventario.Core.Application.Common.Exceptions
{
    public class ForbiddenAccessException : BaseException
    {
        public ForbiddenAccessException() : base("Acceso denegado. No tiene permisos para realizar esta acción.", 403) { }
        public ForbiddenAccessException(string message) : base(message, 403) { }
    }
}
