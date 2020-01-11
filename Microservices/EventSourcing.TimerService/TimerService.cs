using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EventSourcing.TimerService
{
    public class TimerService : Timer.TimerBase
    {
        private readonly IDictionary<string, System.Threading.Timer> _runningTimers = new Dictionary<string, System.Threading.Timer>();
        private readonly KafkaProducer<string, Tick> _producer;

        public TimerService(KafkaProducer<string, Tick> producer) => _producer = producer;


        public override Task<TimerResponse> StartTimer(TimerRequest request, ServerCallContext context)
        {
            var timerResponse = new TimerResponse {TimerId = Guid.NewGuid().ToString(), CreatedTimer = request};
            _runningTimers[timerResponse.TimerId] = new System.Threading.Timer(
                async _ => await ProduceTick(timerResponse),
                timerResponse,
                0,
                request.IntervalMs
            );

            return Task.FromResult(timerResponse);
        }

        private async Task ProduceTick(TimerResponse response)
        {
            await _producer.ProduceAsync(new Tick
                {
                    Originator = response.CreatedTimer.Originator,
                    CurrentTime = DateTime.UtcNow.ToTimestamp(),
                    RoutingKey = response.CreatedTimer.RoutingKey,
                    TimerId = response.TimerId
                },
                response.TimerId);
        }
    }
}