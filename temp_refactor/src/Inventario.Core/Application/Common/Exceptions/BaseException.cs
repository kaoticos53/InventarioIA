using System;
using System.Collections.Generic;

namespace Inventario.Core.Application.Common.Exceptions
{
    public abstract class BaseException : Exception
    {
        public int StatusCode { get; }
        public IDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>();

        protected BaseException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }

        protected BaseException(string message, Exception innerException, int statusCode = 500) 
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        protected BaseException(string message, IDictionary<string, string[]> errors, int statusCode = 400) 
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}
