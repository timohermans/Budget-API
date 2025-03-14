using System.Reflection;
using Budget.Worker.Consumers;
using MassTransit;

namespace Budget.Worker;

public static class StartupExtensions
{
    public static void AddWorker(this IServiceCollection services, IConfiguration configuration)
    {
    }
}