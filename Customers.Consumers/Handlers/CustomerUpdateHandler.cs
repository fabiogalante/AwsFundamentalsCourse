using Customers.Consumers.Messages;
using MediatR;

namespace Customers.Consumers.Handlers;

public class CustomerUpdateHandler : IRequestHandler<CustomerUpdate>
{
    private readonly ILogger<CustomerUpdateHandler> _logger;

    public CustomerUpdateHandler(ILogger<CustomerUpdateHandler> logger)
    {
        _logger = logger;
    }

    public Task<Unit> Handle(CustomerUpdate request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(request.FullName);
        return Unit.Task;
    }
}