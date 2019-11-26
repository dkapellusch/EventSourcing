using EventSourcing.Contracts;
using FluentValidation;

namespace EventSourcing.VehicleWriteService
{
    public class VehicleValidator : AbstractValidator<Vehicle>
    {
        public VehicleValidator(LocationRead.LocationReadClient locationReadClient)
        {
            RuleFor(v => v.Vin).NotNull().NotEmpty().Length(17);
            RuleFor(v => v.LocationCode)
                .MustAsync(async (v, l, token) =>
                    string.IsNullOrEmpty(l) || await locationReadClient.GetLocationAsync(new LocationRequest {LocationCode = v.LocationCode}) != null)
                .WithMessage(v => $"No location was found for location code: {v.LocationCode}.");
        }
    }
}