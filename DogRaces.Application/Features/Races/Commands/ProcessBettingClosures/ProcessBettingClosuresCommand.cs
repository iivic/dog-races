using MediatR;

namespace DogRaces.Application.Features.Races.Commands.ProcessBettingClosures;

public record ProcessBettingClosuresCommand : IRequest<ProcessBettingClosuresResponse>;

public record ProcessBettingClosuresResponse(
    int RacesProcessed,
    int BettingClosed
);
