using DogRaces.Api.Endpoints;
using DogRaces.Api.Middleware;
using DogRaces.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Add Infrastructure services (Database + Application + MediatR)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map feature endpoints
app.MapWalletEndpoints();
app.MapRaceEndpoints();
app.MapConfigurationEndpoints();
app.MapTicketEndpoints();

app.Run();

// Make Program class accessible for testing
public partial class Program { }