using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using EventSourcing.RocksDb.RocksAbstractions;
using Grpc.Core;
using Timer = System.Threading.Timer;

namespace EventSourcing.NotificationWrite
{
    public class NotificationWriteService : Contracts.NotificationWrite.NotificationWriteBase
    {
        private static readonly ConcurrentDictionary<string, Timer> Timers = new ConcurrentDictionary<string, Timer>();

        private readonly KafkaProducer<string, Notification> _notificationProducer;
        private readonly RocksStore<RecurringNotificationRequest> _notifications;

        public NotificationWriteService(KafkaProducer<string, Notification> notificationProducer, RocksStore<RecurringNotificationRequest> notifications)
        {
            _notificationProducer = notificationProducer;
            _notifications = notifications;
        }

        public override Task<NotificationResponse> Notify(NotificationRequest request, ServerCallContext context)
        {
            var notificationId = Guid.NewGuid().ToString();
            var notifyTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + request.NotificationTime;
            var notification = new Notification
            {
                NotificationId = notificationId,
                Category = request.Category,
                Data = request.Data,
                NotificationTime = notifyTime
            };
            var response = new NotificationResponse
            {
                Category = request.Category,
                NotificationId = notificationId,
                NotificationTime = notifyTime
            };

            _notificationProducer.Produce(notification, notificationId);
            return Task.FromResult(response);
        }

        public override async Task<NotificationResponse> CancelRecurringNotification(NotificationId request, ServerCallContext context)
        {
            if (Timers.TryGetValue(request.Id, out var timer))
            {
                timer.Change(0, 0);
                await timer.DisposeAsync();
                await _notifications.Delete(request.Id);
                Timers.TryRemove(request.Id, out _);
            }

            return new NotificationResponse
            {
                NotificationId = request.Id
            };
        }

        public override async Task<NotificationResponse> NotifyRecurring(RecurringNotificationRequest request, ServerCallContext context)
        {
            var response = StartRecurringNotification(request);
            await _notifications.Set(request, response.NotificationId);
            return response;
        }

        public async Task StartExistingTimers()
        {
            foreach (var notification in await _notifications.Query()) StartRecurringNotification(notification);
        }

        private NotificationResponse StartRecurringNotification(RecurringNotificationRequest request)
        {
            var notificationId = Guid.NewGuid().ToString();
            var notifyTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + request.Interval;
            var response = new NotificationResponse
            {
                Category = request.Category,
                NotificationId = notificationId,
                NotificationTime = notifyTime
            };

            Timers[notificationId] = new Timer(async _ => await _notificationProducer.ProduceAsync(new Notification
                    {
                        NotificationId = notificationId,
                        Category = request.Category,
                        NotificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        Data = request.Data
                    },
                    notificationId),
                null,
                0,
                request.Interval);

            return response;
        }
    }
}