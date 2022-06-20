using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Micro.Testing;
using Micro.Time;
using Microsoft.Extensions.DependencyInjection;
using MySpot.Services.Reservations.Application.Commands;
using MySpot.Services.Reservations.Application.DTO;
using MySpot.Services.Reservations.Application.Events;
using MySpot.Services.Reservations.Core.Entities;
using MySpot.Services.Reservations.Core.Repository;
using MySpot.Services.Reservations.Core.ValueObjects;
using MySpot.Services.Reservations.Infrastructure.DAL.Repositories;
using Shouldly;
using Xunit;

namespace MySpot.Services.Reservations.Tests.EndToEnd.Messaging;

[ExcludeFromCodeCoverage]
[Collection(Const.TestCollection)]
public class MakeReservationTests : IDisposable
{
    [Fact]
    public async Task make_reservation_message_should_create_reservation_and_publish_an_event()
    {
      
    }

    #region Arrange

    private readonly IClock _clock;
    private readonly TestDatabase _testDatabase;
    private readonly TestMessageBroker _testMessageBroker;
    private readonly IUserRepository _userRepository;
    private readonly TestApp<Program> _app;
    
    public MakeReservationTests()
    {
        _clock = new TestClock();
        _testDatabase = new TestDatabase();
        _userRepository = new UserRepository(_testDatabase.Context);
        _testMessageBroker = new TestMessageBroker();
        _app = new TestApp<Program>(services: services =>
        {
            services.AddSingleton(_clock);
        });
    }

    #endregion

    public void Dispose()
    {
        _testDatabase.Dispose();
        _testMessageBroker.Dispose();
    }
}