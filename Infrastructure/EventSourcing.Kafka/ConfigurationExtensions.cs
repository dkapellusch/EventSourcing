using Confluent.Kafka;
using EventSourcing.Contracts.Serialization;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Kafka
{
    public static class Configuration
    {
        public static IServiceCollection AddKafkaConsumer<TPayload>(this IServiceCollection services, ConsumerConfig config, string topic = null) where TPayload : IMessage<TPayload>, new() =>
            services
                .AddSingleton(typeof(IMessageSerializer<>), typeof(JsonMessageSerializer<>))
                .AddTransient(p => new Consumer<TPayload>(config, p.GetService<IMessageSerializer<TPayload>>(), topic));

        public static IServiceCollection AddKafkaProducer<TKey, TPayload>(this IServiceCollection services, ProducerConfig config, string topic = null) where TPayload : IMessage<TPayload>, new() =>
            services
                .AddSingleton(typeof(IMessageSerializer<>), typeof(JsonMessageSerializer<>))
                .AddSingleton(p => new KafkaProducer<TKey, TPayload>(config, p.GetService<IMessageSerializer<TPayload>>(), topic));
    }
}