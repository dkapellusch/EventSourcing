using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.GraphqlGateway.Graphql;
using Grpc.Core;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static EventSourcing.Contracts.LocationRead;
using static EventSourcing.Contracts.VehicleRead;

namespace EventSourcing.GraphqlGateway.Services;

public class GrpcSubscriptionBridge : BackgroundService
{
    private readonly VehicleReadClient _vehicleReadClient;
    private readonly LocationReadClient _locationReadClient;
    private readonly LockRead.LockReadClient _lockReadClient;
    private readonly ITopicEventSender _eventSender;
    private readonly ILogger<GrpcSubscriptionBridge> _logger;

    public GrpcSubscriptionBridge(
        VehicleReadClient vehicleReadClient,
        LocationReadClient locationReadClient,
        LockRead.LockReadClient lockReadClient,
        ITopicEventSender eventSender,
        ILogger<GrpcSubscriptionBridge> logger)
    {
        _vehicleReadClient = vehicleReadClient;
        _locationReadClient = locationReadClient;
        _lockReadClient = lockReadClient;
        _eventSender = eventSender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var vehicleTask = SubscribeToVehicleUpdatesAsync(stoppingToken);
        var locationTask = SubscribeToLocationUpdatesAsync(stoppingToken);
        var lockTask = SubscribeToLockUpdatesAsync(stoppingToken);

        await Task.WhenAll(vehicleTask, locationTask, lockTask);
    }

    private async Task SubscribeToVehicleUpdatesAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var call = _vehicleReadClient.GetVehicleUpdates(new Empty(), cancellationToken: stoppingToken);
                await foreach (var vehicle in call.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    await _eventSender.SendAsync(nameof(Subscription.OnVehicleChangeAsync), vehicle, stoppingToken);

                    if (!string.IsNullOrEmpty(vehicle.LocationCode))
                    {
                        var locationTopic = $"{nameof(Subscription.OnVehicleAtLocationChangeAsync)}_{vehicle.LocationCode}";
                        await _eventSender.SendAsync(locationTopic, vehicle, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in vehicle subscription stream. Reconnecting...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task SubscribeToLocationUpdatesAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var call = _locationReadClient.GetLocationUpdates(new Empty(), cancellationToken: stoppingToken);
                await foreach (var location in call.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    await _eventSender.SendAsync(nameof(Subscription.OnLocationChangeAsync), location, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in location subscription stream. Reconnecting...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task SubscribeToLockUpdatesAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var call = _lockReadClient.ExpiringLocks(new Empty(), cancellationToken: stoppingToken);
                await foreach (var lockExpired in call.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    await _eventSender.SendAsync(nameof(Subscription.OnLockExpireAsync), lockExpired, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in lock subscription stream. Reconnecting...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
