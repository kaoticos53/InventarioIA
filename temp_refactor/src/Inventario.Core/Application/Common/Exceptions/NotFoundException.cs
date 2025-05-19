namespace Inventario.Core.Application.Common.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(message, 404) { }
        public NotFoundException(string name, object key) 
            : base($"La entidad \"{name}\" con el identificador ({key}) no fue encontrada.", 404) { }
    }
}
