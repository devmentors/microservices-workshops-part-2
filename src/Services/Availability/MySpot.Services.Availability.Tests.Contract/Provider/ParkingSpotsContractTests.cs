using System;
using System.Threading.Tasks;
using Micro.Testing;
using Xunit;
using Xunit.Abstractions;

namespace MySpot.Services.Availability.Tests.Contract.Provider;

public class ParkingSpotsContractTests : IDisposable
{
    [Fact]
    public async Task should_honor_add_resource_pact_with_parking_spots_consumer()
    {
        var pactFile = _endpointContract.GetPactFile();
        await _testServer.StartAsync();
        
        _endpointContract.Verifier
            .ServiceProvider(_endpointContract.Provider, _testServer.Url)
            .WithFileSource(pactFile)
            .WithSslVerificationDisabled()
            .Verify();
    }
    
    
    // TEST PIPELINE
    // 1. Run consumer tests + publish to broker
    // 2. Get all pacts broker (being provider)
    // 3. Either skip if no provider test defined or fail

    #region ARRANGE

    private readonly EndpointContract _endpointContract;
    private readonly TestServer _testServer;
    
    public ParkingSpotsContractTests(ITestOutputHelper output)
    {
        _endpointContract = new EndpointContract("ParkingSpots", "Availability", output);
        _testServer = new TestServer("MySpot.Services.Availability.Api", output);
    }

    #endregion
    
    public void Dispose()
    {
        _endpointContract.Dispose();
        _testServer.Dispose();
    }
}