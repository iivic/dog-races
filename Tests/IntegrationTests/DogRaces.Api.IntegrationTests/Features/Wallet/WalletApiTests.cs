using DogRaces.Api.IntegrationTests.Infrastructure;
using DogRaces.Application.Features.Wallet.Commands.ReleaseFunds;
using DogRaces.Application.Features.Wallet.Commands.ReserveFunds;
using DogRaces.Application.Features.Wallet.Commands.ResetWallet;
using DogRaces.Application.Features.Wallet.Queries.GetWalletStatus;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DogRaces.Api.IntegrationTests.Features.Wallet;

public class WalletApiTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _httpClient;

    public WalletApiTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task GetWalletBalance_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/wallet/balance");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWalletBalance_ShouldReturnInitialBalance()
    {
        // Arrange - Reset wallet to ensure clean state
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));

        // Act
        var response = await _httpClient.GetFromJsonAsync<GetWalletStatusResponse>("/api/wallet/balance");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(100m, response.AvailableBalance);
        Assert.Equal(0m, response.ReservedAmount);
        Assert.Equal(100m, response.TotalFunds);
    }

    [Fact]
    public async Task ReserveFunds_WithValidAmount_ShouldReturnSuccess()
    {
        // Arrange
        var reserveRequest = new ReserveFundsCommand(50.0m);

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/wallet/reserve", reserveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ReserveFundsResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotEqual(Guid.Empty, result.TicketId);
    }

    [Fact]
    public async Task ReleaseFunds_WithValidTicket_ShouldReturnSuccess()
    {
        // Arrange - First reserve funds
        var reserveRequest = new ReserveFundsCommand(50.0m);
        var reserveResponse = await _httpClient.PostAsJsonAsync("/api/wallet/reserve", reserveRequest);
        var reserveResult = await reserveResponse.Content.ReadFromJsonAsync<ReserveFundsResponse>();

        var releaseRequest = new ReleaseFundsCommand(reserveResult!.Amount, reserveResult.TicketId);

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/wallet/release", releaseRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ReleaseFundsResponse>();
        Assert.NotNull(result);
        Assert.Contains($"Released {reserveResult.Amount} for ticket {reserveResult.TicketId}", result.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task ReserveFunds_WithInvalidAmount_ShouldReturnOkButFail(decimal amount)
    {
        // Arrange
        var reserveRequest = new ReserveFundsCommand(amount);

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/wallet/reserve", reserveRequest);

        // Assert - The wallet service returns OK but with success = false for invalid amounts
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ReserveFundsResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success); // Should fail for invalid amounts
    }
}