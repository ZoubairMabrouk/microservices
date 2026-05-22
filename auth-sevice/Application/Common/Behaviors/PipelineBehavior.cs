using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AUTH_Sevice.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
        {
            if (!validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var failures = new List<ValidationFailure>();

            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(context, ct);

                if (result.Errors.Any())
                    failures.AddRange(result.Errors);
            }

            if (failures.Count == 0)
                return await next();

            logger.LogWarning(
                "Validation failed for {Request}: {Errors}",
                typeof(TRequest).Name,
                string.Join(", ", failures.Select(f => f.ErrorMessage)));

            throw new ValidationException(failures);
        }
    }

    public class LoggingBehavior<TRequest, TResponse>(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
        {
            var requestName = typeof(TRequest).Name;

            logger.LogInformation("Handling {RequestName}", requestName);

            var response = await next();

            logger.LogInformation("Handled {RequestName}", requestName);

            return response;
        }
    }
}