using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.Extensions;
using EventSourcing.Kafka;
using FluentValidation;
using Grpc.Core;

namespace EventSourcing.VehicleWriteService
{
    public class VehicleWriteService : VehicleWrite.VehicleWriteBase
    {
        private readonly IValidator<Vehicle> _validator;
        private readonly KafkaProducer<string, Vehicle> _vehicleProducer;

        public VehicleWriteService(KafkaProducer<string, Vehicle> vehicleProducer, IValidator<Vehicle> validator)
        {
            _vehicleProducer = vehicleProducer;
            _validator = validator;
        }

        public override async Task<Vehicle> AddVehicle(Vehicle request, ServerCallContext context)
        {
            await _validator.ValidateOrThrowAsync(request);
            await _vehicleProducer.ProduceAsync(request, request.Vin);
            return request;
        }
    }
}