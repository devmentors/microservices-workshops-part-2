using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Micro.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySpot.Services.Availability.Application.Commands;
using MySpot.Services.Availability.Application.Events;
using MySpot.Services.Availability.Core.Entities;
using Shouldly;
using Xunit;
using TestServer = Microsoft.AspNetCore.TestHost.TestServer;

namespace MySpot.Services.Availability.Tests.EndToEnd.Endpoints;

public class ResourceEndpointsTests : IDisposable
{

    [Fact]
    public async Task post_add_resource_should_return_201_created_status_code()
    {
        var command = new AddResource(Guid.NewGuid(), 2, new[] {"test"});
        var resourceAddedSubscription = _testMessageBroker.SubscribeAsync<ResourceAdded>();

        var response = await _app.Client.PostAsJsonAsync("resources", command);

        //HTTP
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        
        //DB
        var dbResult = await _testsDatabase.Context.Resources
            .SingleOrDefaultAsync(x => x.Id == new AggregateId(command.ResourceId));
        dbResult.ShouldNotBeNull();
        
        //Message Broker
        var resourceAdded = await resourceAddedSubscription;
        resourceAdded.ShouldNotBeNull();
        resourceAdded.ResourceId.ShouldBe(command.ResourceId);
    }

    #region ARRANGE

    private readonly TestApp<Program> _app;
    private readonly TestsDatabase _testsDatabase;
    private readonly TestMessageBroker _testMessageBroker;

    public ResourceEndpointsTests()
    {
        _app = new TestApp<Program>();
        _testsDatabase = new TestsDatabase();
        _testMessageBroker = new TestMessageBroker();
    }
    
    #endregion
    
    public void Dispose()
    {
        _testsDatabase.Dispose();
    }
}