using CleanArch.Domain.Filters;
using FluentValidation;
using MediatR;

namespace CleanArch.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key, 
                    y => y.Select(z => z.ErrorMessage).ToArray()
                );

            if (failures.Any())
            {
                throw new InternalValidationException("Validation failed", failures);
            }

            return await next();
        }
    }
}