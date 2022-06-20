using System.Collections.Concurrent;
using Humanizer;
using Micro.Abstractions;
using Microsoft.Extensions.Logging;

namespace Micro.Messaging.RabbitMQ.Internals;

internal sealed class MessageHandler : IMessageHandler
{
    private readonly ConcurrentDictionary<Type, string> _names = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(IServiceProvider serviceProvider, ILogger<MessageHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task HandleAsync<T>(Func<IServiceProvider, T, CancellationToken, Task> handler, T message,
        CancellationToken cancellationToken = default) where T : IMessage
    {
        try
        {
            var name = _names.GetOrAdd(typeof(T), typeof(T).Name.Underscore());
            _logger.LogInformation("Received a message: {MessageName}", name);
            await handler(_serviceProvider, message, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);
            throw;
        }
    }
}