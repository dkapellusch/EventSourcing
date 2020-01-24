using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.KSQL;
using Grpc.Core;
using static EventSourcing.Contracts.NotificationSubscription;

namespace EventSourcing.NotificationRead
{
    public class NotificationReadService : Contracts.NotificationRead.NotificationReadBase
    {
        private readonly KsqlStore<Notification> _notificationStore;

        public NotificationReadService(KsqlStore<Notification> notificationStore) => _notificationStore = notificationStore;

        public override async Task Subscribe(NotificationSubscription request, IServerStreamWriter<Notification> responseStream, ServerCallContext context)
        {
            var notifications = _notificationStore.GetChanges()
                .Where(n => request.IdentifierCase switch
                    {
                        IdentifierOneofCase.Category => n.Category.Equals(request.Category, StringComparison.OrdinalIgnoreCase),
                        IdentifierOneofCase.NotificationId => n.NotificationId.Equals(request.NotificationId, StringComparison.OrdinalIgnoreCase),
                        IdentifierOneofCase.None => true,
                        _ => false
                    }
                );

            await notifications.ForEachAsync(async n => await responseStream.WriteAsync(n), context.CancellationToken);
        }

        public override async Task<Notification> GetNotification(NotificationId request, ServerCallContext context) =>
            await _notificationStore.Get(request.Id);
    }
}