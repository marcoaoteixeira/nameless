using Nameless.NHibernate;
using NHibernate;
using NHibernate.Type;

namespace Nameless.Persistence.NHibernate {

    public sealed class SQLDirective : DirectiveBase<IList<dynamic>?> {

        #region Public Constructors

        public SQLDirective(ISession session) : base(session) { }

        #endregion

        #region Public Override Methods

        public override Task<IList<dynamic>?> ExecuteAsync(ParameterSet parameters, CancellationToken cancellationToken = default) {
            Prevent.Null(parameters, nameof(parameters));

            var paramSql = parameters["sql"];
            if (paramSql == null || !paramSql.TryGetValue(out string? sql)) {
                throw new ArgumentException("Missing SQL command.");
            }
            var query = Session.CreateSQLQuery(sql);

            var paramSqlParameters = parameters["sqlParameters"];
            if (paramSqlParameters != null && paramSqlParameters.TryGetValue(out IDictionary<string, object>? sqlParameters)) {
                sqlParameters!.Each(kvp => {
                    query.SetParameter(kvp.Key, kvp.Value);
                });
            }

            var paramScalars = parameters["scalars"];
            if (paramScalars != null && paramScalars.TryGetValue(out IDictionary<string, IType>? scalars)) {
                scalars!.Each(kvp => {
                    query.AddScalar(kvp.Key, kvp.Value);
                });
            }

            var paramEntities = parameters["entities"];
            if (paramEntities != null && paramEntities.TryGetValue(out IList<Type>? entities)) {
                entities!.Each(item => {
                    query.AddEntity(item);
                });
            }

            var paramUseDynamicResult = parameters["useDynamicResult"];
            var result = (paramUseDynamicResult != null && paramUseDynamicResult.TryGetValue(out bool useDynamicResult) && useDynamicResult)
                ? query.AsDynamicList()
                : query.List<object>();

            return (Task<IList<dynamic>?>)result;
        }

        #endregion
    }
}
