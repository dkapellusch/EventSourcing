using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using Grpc.Core;

namespace EventSourcing.LocationWriteService
{
    public class LocationWriteService : LocationWrite.LocationWriteBase
    {
        private readonly KafkaProducer<string, Location> _kafkaProducer;

        public LocationWriteService(KafkaProducer<string, Location> kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
        }

        public override async Task<Location> AddLocation(Location request, ServerCallContext context)
        {
            await _kafkaProducer.ProduceAsync(request, request.LocationCode);
            return request;
        }
    }
}