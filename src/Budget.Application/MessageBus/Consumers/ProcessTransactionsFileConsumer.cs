using Budget.Domain.Commands;
using MassTransit;

namespace Budget.Application.MessageBus.Consumers;

public class ProcessTransactionsFileConsumer : IConsumer<ProcessTransactionsFile>
{
    public Task Consume(ConsumeContext<ProcessTransactionsFile> context)
    {
        throw new NotImplementedException();
    }
}