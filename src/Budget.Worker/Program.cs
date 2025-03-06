// See https://aka.ms/new-console-template for more information

using Budget.Worker;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddWorker(hostContext.Configuration);
    })
    .Build()
    .RunAsync();