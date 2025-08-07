using Serilog;

namespace Budget.Worker;

public static class StartupExtensions
{
    public static void AddWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((sp, lc) =>
        {
            lc.ReadFrom.Configuration(configuration)
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
        });
    }
}