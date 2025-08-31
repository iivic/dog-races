using DogRaces.Application.Features.Tickets.Commands.ProcessUnprocessedTickets;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DogRaces.BackgroundServices.Workers;

/// <summary>
/// Background worker that processes tickets when all their associated races are finished
/// </summary>
public class TicketProcessingWorker : BaseWorker
{
    public TicketProcessingWorker(
        ILogger<TicketProcessingWorker> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider, executeEverySeconds: 5) // Check every 5 seconds
    {
    }

    protected override async Task Execute(IServiceScope scope, CancellationToken cancellationToken)
    {
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new ProcessUnprocessedTicketsCommand();
        var result = await sender.Send(command, cancellationToken);

        if (result.ProcessedTickets > 0)
        {
            Logger.LogInformation(
                "🎫 Processed {ProcessedTickets} tickets: {WinningTickets} won, {LosingTickets} lost, total payouts: {TotalPayouts}",
                result.ProcessedTickets,
                result.WinningTickets,
                result.LosingTickets,
                result.TotalPayouts);
        }
    }
}