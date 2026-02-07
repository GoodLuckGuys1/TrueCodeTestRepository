using FluentValidation;
using MediatR;

namespace TrueCodeTest.Shared.Domain.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Common.ResultBase, new()
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators == null || !_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var errors = new List<string>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors.Select(e => e.ErrorMessage));
            }
        }

        if (errors.Count > 0)
        {
            var response = new TResponse
            {
                Success = false,
                ErrorMessage = string.Join("; ", errors)
            };
            return response;
        }

        return await next();
    }
}
