using EventSourcing.Contracts;
using HotChocolate;
using HotChocolate.Types;
using Location = EventSourcing.Contracts.Location;

namespace EventSourcing.GraphqlGateway.Graphql;

[SubscriptionType]
public class Subscription
{
    [Subscribe]
    [Topic(nameof(OnVehicleChangeAsync))]
    public Vehicle OnVehicleChangeAsync([EventMessage] Vehicle vehicle) => vehicle;

    [Subscribe]
    [Topic(nameof(OnLocationChangeAsync))]
    public Location OnLocationChangeAsync([EventMessage] Location location) => location;

    [Subscribe]
    [Topic(nameof(OnLockExpireAsync))]
    public Contracts.Lock OnLockExpireAsync([EventMessage] Contracts.Lock lockExpired) => lockExpired;
}
