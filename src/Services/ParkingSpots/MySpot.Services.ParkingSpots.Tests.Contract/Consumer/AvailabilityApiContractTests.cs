using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Micro.HTTP;
using Micro.Testing;
using Microsoft.Extensions.Options;
using MySpot.Services.ParkingSpots.Core.Clients;
using MySpot.Services.ParkingSpots.Core.Clients.DTO;
using PactNet.Matchers;
using Xunit;
using Xunit.Abstractions;

namespace MySpot.Services.ParkingSpots.Tests.Contract.Consumer;

public class AvailabilityApiContractTests : IDisposable
{
    [Fact]
    public async Task given_valid_post_request_resource_should_be_created()
    {
        var dto = new AddResourceDto(Guid.NewGuid(), 2, new[] {"test"});

        _endpointContract.Pact
            .UponReceiving("A valid request to add a resource")
            .WithRequest(HttpMethod.Post, "/resources")
            .WithJsonBody(dto)
            .WillRespond()
            .WithStatus(HttpStatusCode.Created);

        await _endpointContract.Pact.VerifyAsync(_ =>
            _apiClient.AddResourceAsync(dto.ResourceId, dto.Capacity, dto.Tags));
    }
    
    #region ARRANGE

    private readonly EndpointContract _endpointContract;
    private IAvailabilityApiClient _apiClient;
    
    public AvailabilityApiContractTests(ITestOutputHelper output)
    {
        _endpointContract = new EndpointContract("ParkingSpots", "Availability", output);
        _apiClient = new AvailabilityApiClient(new TestHttpClientFactory(),
            new OptionsWrapper<HttpClientOptions>(new HttpClientOptions
            {
                Services = new Dictionary<string, string>
                {
                    ["availability"] = $"http://localhost:{_endpointContract.Port}"
                }
            }));
    }
    

    #endregion
    
    public void Dispose()
    {
    }
}