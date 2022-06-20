using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EasyNetQ;
using EasyNetQ.Topology;
using Humanizer;
using Micro.Attributes;
using Micro.Contexts.Accessors;
using Micro.Contexts.Providers;
using Micro.Messaging.Brokers;
using Micro.Messaging.RabbitMQ;
using Micro.Messaging.RabbitMQ.Internals;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using IMessage = Micro.Abstractions.IMessage;

namespace Micro.Testing;

[ExcludeFromCodeCoverage]
public sealed class TestMessageBroker : IDisposable
{
    private readonly HashSet<string> _queues = new();
    private readonly IBus _bus;
    private readonly MessageTypeRegistry _messageTypeRegistry;
    
    public IMessageBroker MessageBroker { get; }

    public async Task<T> SubscribeAsync<T>(Func<T, CancellationToken, Task>? handler = null, TimeSpan? timeout = null)
        where T : class, IMessage
    {
        _messageTypeRegistry.Register<T>();
        var messageAttribute = typeof(T).GetCustomAttribute<MessageAttribute>() ?? new MessageAttribute();
        var tcs = new TaskCompletionSource<T>();
        var cancelTask = Task.Delay(timeout ?? TimeSpan.FromSeconds(10));

        var queue = $"test.{typeof(T).Name.Underscore()}.{Guid.NewGuid():N}";
        _queues.Add(queue);
        
        _ = _bus.PubSub.SubscribeAsync<T>(string.Empty,
            async (message, cancellationToken) =>
            {
                if (handler is not null)
                {
                    await handler(message, cancellationToken);
                }

                tcs.SetResult(message);
            },
            configuration =>
            {
                configuration.WithQueueName(queue);
                if (!string.IsNullOrWhiteSpace(messageAttribute.Topic))
                {
                    configuration.WithTopic(messageAttribute.Topic);
                }
            });

        await Task.WhenAny(tcs.Task, cancelTask);
        if (tcs.Task.IsCompleted)
        {
            return tcs.Task.Result;
        }

        throw new TimeoutException("Subscriber has timed out.");
    }

    public TestMessageBroker(string connectionString = "host=localhost;port=5672;virtualHost=/;username=guest;password=guest")
    {
        var contextAccessor = new ContextAccessor();
        var messageContextAccessor = new MessageContextAccessor();
        _messageTypeRegistry = new MessageTypeRegistry();

        _bus = RabbitHutch.CreateBus(connectionString,
            register =>
            {
                register.Register<IConventions, CustomConventions>();
                register.Register<IMessageSerializationStrategy, CustomMessageSerializationStrategy>();
                register.Register<IMessageTypeRegistry>(_messageTypeRegistry);
                register.Register<IContextAccessor>(contextAccessor);
                register.Register<IMessageContextAccessor>(messageContextAccessor);
            });
        
        var client = new RabbitMqBrokerClient(_bus, messageContextAccessor, new NullLogger<RabbitMqBrokerClient>());
        var contextProvider = new ContextProvider(new HttpContextAccessor(), contextAccessor);
        MessageBroker = new MessageBroker(client, contextProvider);
    }

    public void Dispose()
    {
        foreach (var queue in _queues)
        {
            _bus.Advanced.QueueDelete(new Queue(queue));
        }
    }
}