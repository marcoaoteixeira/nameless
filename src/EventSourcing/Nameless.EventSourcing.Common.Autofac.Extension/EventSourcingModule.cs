using Autofac;
using Nameless.DependencyInjection.Autofac;
using Nameless.EventSourcing.Domains;
using Nameless.EventSourcing.Events;
using Nameless.EventSourcing.Repository;
using Nameless.EventSourcing.Snapshots;

namespace Nameless.EventSourcing {

    public sealed class EventSourcingModule : ModuleBase {

        #region Public Properties

        /// <summary>
        /// Gets or sets the event source implementation type.
        /// </summary>
        public Type? EventStoreImplementationType { get; set; }

        /// <summary>
        /// Gets or sets the snapshot source implementation type.
        /// </summary>
        public Type? SnapshotStoreImplementationType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEventHandlerFactory"/>.
        /// </summary>
        /// <remarks>Default is <see cref="LifetimeScopeType.PerRequest"/>.</remarks>
        public LifetimeScopeType EventHandlerFactoryLifetimeScope { get; set; } = LifetimeScopeType.Transient;

        #endregion

        #region Protected Override Methods

        protected override void Load(ContainerBuilder builder) {
            builder
               .RegisterType<AggregateFactory>()
               .As<IAggregateFactory>()
               .SetLifetimeScope(LifetimeScopeType.Singleton);

            builder
                .RegisterType<EventPublisher>()
                .As<IEventPublisher>()
                .SetLifetimeScope(LifetimeScopeType.Singleton);

            builder
                .RegisterType<EventSerializer>()
                .As<IEventSerializer>()
                .SetLifetimeScope(LifetimeScopeType.Singleton);

            var eventStoreImplementationType = EventStoreImplementationType ?? SearchForImplementation<IEventStore>();
            if (eventStoreImplementationType == null) {
                throw new InvalidOperationException("Event store implementation not found.");
            }
            builder
                .RegisterType(eventStoreImplementationType)
                .As<IEventStore>()
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            builder
                .RegisterType<AggregateRepository>()
                .As<IAggregateRepository>()
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            builder
                .RegisterType<AggregateSession>()
                .As<IAggregateSession>()
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            var snapshotStoreImplementationType = SnapshotStoreImplementationType ?? SearchForImplementation<ISnapshotStore>();
            if (snapshotStoreImplementationType == null) {
                throw new InvalidOperationException("Snapshot store implementation not found.");
            }
            builder
                .RegisterType(snapshotStoreImplementationType)
                .As<ISnapshotStore>()
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            builder
                .RegisterType<SnapshotStrategy>()
                .As<ISnapshotStrategy>()
                .SetLifetimeScope(LifetimeScopeType.Singleton);

            base.Load(builder);
        }

        #endregion
    }
}
