using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inventario.Core.Application.Common.Exceptions
{
    public class ValidationException : BaseException
    {
        public ValidationException() 
            : base("Se han producido uno o más errores de validación.", 400)
        {
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            var failureGroups = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage);

            foreach (var failureGroup in failureGroups)
            {
                var propertyName = failureGroup.Key;
                var propertyFailures = failureGroup.ToArray();

                Errors.Add(propertyName, propertyFailures);
            }
        }
    }
}
