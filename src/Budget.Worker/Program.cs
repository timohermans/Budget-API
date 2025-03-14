﻿// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Budget.Application;
using Budget.Infrastructure;
using Budget.Worker;
using MassTransit;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        services.AddWorker(hostContext.Configuration);
        services.AddBudgetApplication(hostContext.Configuration);
        services.AddInfrastructure(hostContext.Configuration, x => x.AddConsumers(entryAssembly));
    })
    .Build()
    .RunAsync();