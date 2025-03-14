using Budget.Api;
using Budget.Application;
using Budget.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddBudgetApi();
builder.Services.AddBudgetApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

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