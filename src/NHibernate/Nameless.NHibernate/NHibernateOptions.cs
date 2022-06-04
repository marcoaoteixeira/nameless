using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace Nameless.NHibernate {

    public abstract class NHibernateOptionsBase {

        #region Public Methods

        public IEnumerable<KeyValuePair<string, string>> GetConfigValues() {
            var properties = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(_ => !typeof(NHibernateOptionsBase).IsAssignableFrom(_.PropertyType));
            foreach (var property in properties) {
                var key = property.GetDescription() ?? property.Name;
                var obj = property.GetValue(this);
                if (obj == null) { continue; }

                var value = obj is Enum @enum ? @enum.GetDescription() : obj.ToString();
                yield return new KeyValuePair<string, string>(key, value!);
            }
        }

        #endregion
    }

    public sealed class NHibernateOptions {

        #region Public Static Read-Only Fields

        public static readonly NHibernateOptions Default = new();

        #endregion

        #region Public Properties

        public NHibernateCommonOptions Common { get; set; } = new();
        public NHibernateConnectionOptions Connection { get; set; } = new();
        public NHibernateAdoNetOptions AdoNet { get; set; } = new();
        public NHibernateCacheOptions Cache { get; set; } = new();
        public NHibernateQueryOptions Query { get; set; } = new();
        public NHibernateLinqToHqlOptions LinqToHql { get; set; } = new();
        public NHibernateHbmToDdlOptions HbmToDdl { get; set; } = new();
        public NHibernateProxyFactoryOptions ProxyFactory { get; set; } = new();
        public NHibernateCollectionTypeOptions CollectionType { get; set; } = new();
        public NHibernateTransactionOptions Transaction { get; set; } = new();
        public NHibernateSpecificOptions Specific { get; set; } = new();
        public string[] EntityRootTypes { get; set; } = Array.Empty<string>();
        public string[] MappingTypes { get; set; } = Array.Empty<string>();

        #endregion

        #region Public Methods

        public IDictionary<string, string> ToDictionary() {
            var configs = new List<KeyValuePair<string, string>>();
            
            var properties = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(_ => typeof(NHibernateOptionsBase).IsAssignableFrom(_.PropertyType));

            foreach (var property in properties) {
                var config = (NHibernateOptionsBase)property.GetValue(this)!;
                if (config == null) { continue; }
                configs.AddRange(config.GetConfigValues());
            }

            return new Dictionary<string, string>(configs.ToArray());
        }

        #endregion
    }

    public sealed class NHibernateConnectionOptions : NHibernateOptionsBase {

        #region Public Properties

        [Description("connection.provider")]
        public string? Provider { get; set; }

        [Description("connection.driver_class")]
        public string? DriverClass { get; set; } = "NHibernate.Driver.SQLite20Driver";

        [Description("connection.connection_string")]
        public string? ConnectionString { get; set; } = "Data Source=:memory:;Version=3;Page Size=4096;";

        [Description("connection.connection_string_name")]
        public string? ConnectionStringName { get; set; }

        [Description("connection.isolation")]
        public IsolationLevel? Isolation { get; set; }

        [Description("connection.release_mode")]
        public ReleaseMode? ReleaseMode { get; set; }

        #endregion
    }

    public sealed class NHibernateAdoNetOptions : NHibernateOptionsBase {

        #region Public Properties

        [Description("adonet.batch_size")]
        public int? BatchSize { get; set; }

        [Description("adonet.batch_versioned_data")]
        public bool? BatchVersionedData { get; set; }

        [Description("adonet.factory_class")]
        public string? FactoryClass { get; set; }

        [Description("adonet.wrap_result_sets")]
        public bool? WrapResultSets { get; set; }

        #endregion
    }

    public sealed class NHibernateCommonOptions : NHibernateOptionsBase {

        #region Public Properties

        [Description("dialect")]
        public string? Dialect { get; set; } = "NHibernate.Dialect.SQLiteDialect";

        [Description("default_catalog")]
        public string? DefaultCatalog { get; set; }

        [Description("default_schema")]
        public string? DefaultSchema { get; set; }

        [Description("max_fetch_depth")]
        public int? MaxFetchDepth { get; set; }

        [Description("sql_exception_converter")]
        public string? SqlExceptionConverter { get; set; }

        [Description("show_sql")]
        public bool? ShowSql { get; set; } = true;

        [Description("format_sql")]
        public bool? FormatSql { get; set; }

        [Description("use_sql_comments")]
        public bool? UseSqlComments { get; set; }

        [Description("use_proxy_validator")]
        public bool? UseProxyValidator { get; set; }

        [Description("default_flush_mode")]
        public FlushMode? DefaultFlushMode { get; set; }

        [Description("default_batch_fetch_size")]
        public int? DefaultBatchFetchSize { get; set; }

        [Description("current_session_context_class")]
        public string? CurrentSessionContextClass { get; set; }

        [Description("generate_statistics")]
        public bool? GenerateStatistics { get; set; }

        [Description("track_session_id")]
        public bool? TrackSessionID { get; set; }

        [Description("nhibernate-logger")]
        public string? NHibernateLogger { get; set; }

        [Description("prepare_sql")]
        public bool? PrepareSql { get; set; }

        [Description("command_timeout")]
        public int? CommandTimeout { get; set; }

        [Description("order_inserts")]
        public bool? OrderInserts { get; set; }

        [Description("order_updates")]
        public bool? OrderUpdates { get; set; }

        [Description("id.optimizer.pooled.prefer_lo")]
        public bool? IDOptimizerPooledPreferLo { get; set; }

        [Description("sql_types.keep_datetime")]
        public bool? SqlTypesKeepDateTime { get; set; }

        #endregion
    }

    public sealed class NHibernateCacheOptions : NHibernateOptionsBase {

        #region Public Properties

        [Description("cache.use_second_level_cache")]
        public bool? UseSecondLevelCache { get; set; }

        [Description("cache.provider_class")]
        public string? ProviderClass { get; set; }

        [Description("cache.use_minimal_puts")]
        public bool? UseMinimalPuts { get; set; }

        [Description("cache.query_cache_factory")]
        public string? QueryCacheFactory { get; set; }

        [Description("cache.region_prefix")]
        public string? RegionPrefix { get; set; }

        [Description("cache.default_expiration")]
        public int? DefaultExpiration { get; set; }

        #endregion
    }

    public sealed class NHibernateQueryOptions : NHibernateOptionsBase {

        #region Public Properties

        [Description("query.substitutions")]
        public string? Substitutions { get; set; } = "true=1;false=0";

        [Description("query.default_cast_length")]
        public int? DefaultCastLength { get; set; }

        [Description("query.default_cast_precision")]
        public int? DefaultCastPrecision { get; set; }

        [Description("query.default_cast_scale")]
        public int? DefaultCastScale { get; set; }

        [Description("query.startup_check")]
        public bool? StartupCheck { get; set; }

        [Description("query.factory_class")]
        public string? FactoryClass { get; set; }

        [Description("query.linq_provider_class")]
        public string? LinqProviderClass { get; set; }

        [Description("query.query_model_rewriter_factory")]
        public string? QueryModelRewriterFactory { get; set; }

        #endregion
    }

    public sealed class NHibernateLinqToHqlOptions : NHibernateOptionsBase {

        #region Public Properties

        [Description("linqtohql.generatorsregistry")]
        public string? GeneratorsRegistry { get; set; }

        #endregion
    }

    public sealed class NHibernateHbmToDdlOptions : NHibernateOptionsBase {

        #region Public Properties

        [Description("hbm2ddl.auto")]
        public HbmToDdlAuto? Auto { get; set; }

        [Description("hbm2ddl.keywords")]
        public HbmToDdlKeyword? Keywords { get; set; }

        #endregion
    }

    public sealed class NHibernateProxyFactoryOptions : NHibernateOptionsBase {

        #region Public Properties

        [Description("proxyfactory.factory_class")]
        public string? FactoryClass { get; set; }

        #endregion
    }

    public sealed class NHibernateCollectionTypeOptions : NHibernateOptionsBase {

        #region Public Properties

        [Description("collectiontype.factory_class")]
        public string? FactoryClass { get; set; }

        #endregion
    }

    public sealed class NHibernateTransactionOptions : NHibernateOptionsBase {

        #region Public Properties

        [Description("transaction.factory_class")]
        public string? FactoryClass { get; set; }

        [Description("transaction.use_connection_on_system_prepare")]
        public bool? UseConnectionOnSystemPrepare { get; set; }

        [Description("transaction.system_completion_lock_timeout")]
        public int? SystemCompletionLockTimeout { get; set; }

        #endregion
    }

    public sealed class NHibernateSpecificOptions : NHibernateOptionsBase {

        #region Public Properties

        [Description("firebird.disable_parameter_casting")]
        public bool? FirebirdDisableParameterCasting { get; set; }

        [Description("oracle.use_n_prefixed_types_for_unicode")]
        public bool? OracleUseNPrefixedTypesForUnicode { get; set; }

        [Description("odbc.explicit_datetime_scale")]
        public int? OdbcExplicitDateTimeScale { get; set; }

        #endregion
    }

    public enum ReleaseMode {
        [Description("auto")]
        Auto,

        [Description("on_close")]
        OnClose,

        [Description("after_transaction")]
        AfterTransaction
    }

    public enum FlushMode {
        [Description("Auto")]
        Auto,

        [Description("Manual")]
        Manual,

        [Description("Commit")]
        Commit,

        [Description("Always")]
        Always
    }

    public enum HbmToDdlAuto {
        [Description("none")]
        None,

        [Description("create")]
        Create,

        [Description("create-drop")]
        CreateDrop,

        [Description("validate")]
        Validate,

        [Description("update")]
        Update
    }

    public enum HbmToDdlKeyword {
        [Description("none")]
        None,

        [Description("keywords")]
        Keywords,

        [Description("auto-quote")]
        AutoQuote
    }
}
