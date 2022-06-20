using System;
using System.Threading.Tasks;
using Micro.Handlers;
using Micro.Testing;
using MySpot.Services.Availability.Application.Commands;
using MySpot.Services.Availability.Application.Commands.Handlers;
using MySpot.Services.Availability.Application.Events;
using MySpot.Services.Availability.Core.Repositories;
using MySpot.Services.Availability.Core.ValueObjects;
using MySpot.Services.Availability.Infrastructure.DAL.Repositories;
using Shouldly;
using Xunit;

namespace MySpot.Services.Availability.Tests.Integration.Commands;

public class AddResourceHandlerTests : IDisposable
{
    Task Act(AddResource command) => _handler.HandleAsync(command);

    [Fact]
    public async Task given_valid_command_adding_resource_should_persist_data_into_db_and_publish_message()
    {
        // ARRANGE
        await _testsDatabase.InitAsync();
        var resourceAddedSubscription = _testMessageBroker.SubscribeAsync<ResourceAdded>();
        
        var command = new AddResource(Guid.NewGuid(), 2, new[] {"test"});

        await Act(command);

        var resource = await _resourcesRepository.GetAsync(command.ResourceId);
        resource.ShouldNotBeNull();
        var resourceAdded = await resourceAddedSubscription;
        resourceAdded.ShouldNotBeNull();
        resourceAdded.ResourceId.ShouldBe(command.ResourceId);
    }

    #region ARRANGE

    private readonly TestsDatabase _testsDatabase;
    private readonly TestMessageBroker _testMessageBroker;
    private readonly IResourcesRepository _resourcesRepository;
    private readonly ICommandHandler<AddResource> _handler;

    public AddResourceHandlerTests()
    {
        _testsDatabase = new TestsDatabase();
        _testMessageBroker = new TestMessageBroker();
        _resourcesRepository = new ResourcesRepository(_testsDatabase.Context);
        _handler = new AddResourceHandler(_resourcesRepository, _testMessageBroker.MessageBroker);
    }

    #endregion
    
    public void Dispose()
    {
        _testsDatabase.Dispose();
        _testMessageBroker.Dispose();
    }
}