using Nameless.DependencyInjection;

namespace Nameless.CommandQuery {

    public sealed class Dispatcher : IDispatcher {

        #region Private Read-Only Fields

        private readonly IServiceResolver _resolver;

        #endregion

        #region Public Constructors

        public Dispatcher(IServiceResolver resolver) {
            Prevent.Null(resolver, nameof(resolver));

            _resolver = resolver;
        }

        #endregion

        #region IDispatcher Members

        public Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default) {
            Prevent.Null(command, nameof(command));

            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            dynamic handler = _resolver.Get(handlerType);

            return handler.HandleAsync((dynamic)command, cancellationToken);
        }

        public Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default) {
            Prevent.Null(query, nameof(query));

            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            dynamic handler = _resolver.Get(handlerType);

            return (Task<TResult>)handler.HandleAsync((dynamic)query, cancellationToken);
        }

        #endregion
    }
}