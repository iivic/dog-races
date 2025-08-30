using DogRaces.BackgroundServices.Workers;
using DogRaces.Infrastructure.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Add Infrastructure services (Database + Application + MediatR)
builder.Services.AddInfrastructure(builder.Configuration);

// Add the race scheduling background worker
builder.Services.AddHostedService<RaceSchedulingWorker>();

var host = builder.Build();

Console.WriteLine("🏁 Dog Race Background Service starting...");
Console.WriteLine("⚡ Managing race lifecycle every 2 seconds");
Console.WriteLine("📊 Maintaining minimum of 7 concurrent races");
Console.WriteLine("🔄 Press Ctrl+C to stop");
Console.WriteLine();

host.Run();