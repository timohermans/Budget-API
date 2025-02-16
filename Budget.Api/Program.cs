using Budget.Api;
using Budget.Application;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddBudgetApi(config);
builder.Services.AddBudgetApplication(config);

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