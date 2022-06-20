using System.Threading;
using System.Threading.Tasks;
using Micro.Abstractions;
using Micro.Messaging.Brokers;

namespace MySpot.Services.Availability.Tests.Integration;

internal sealed class NoopMessageBroker : IMessageBroker
{
    public Task SendAsync<T>(T message, CancellationToken cancellationToken = default) where T : IMessage
        => Task.CompletedTask;
}