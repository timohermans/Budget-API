using Budget.Api;
using Budget.Application;
using Budget.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddBudgetApi();
builder.AddBudgetApplication();
builder.AddInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();