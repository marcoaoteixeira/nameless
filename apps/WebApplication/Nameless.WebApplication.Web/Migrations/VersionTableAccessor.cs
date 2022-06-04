using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;

namespace Nameless.WebApplication.Web {

    internal class VersionTableAccessor : IVersionTableMetaDataAccessor {
        #region IVersionTableMetaDataAccessor Members

        IVersionTableMetaData IVersionTableMetaDataAccessor.VersionTableMetaData => new VersionTable();

        #endregion
    }

    internal class VersionTable : IVersionTableMetaData {

        #region IVersionTableMetaData Members

        object? IVersionTableMetaData.ApplicationContext { get; set; }
        string IVersionTableMetaData.AppliedOnColumnName => "applied_on";
        string IVersionTableMetaData.ColumnName => "version";
        string IVersionTableMetaData.DescriptionColumnName => "description";
        bool IVersionTableMetaData.OwnsSchema => true;
        string IVersionTableMetaData.SchemaName => "dbo";
        string IVersionTableMetaData.TableName => "__migrations__";
        string IVersionTableMetaData.UniqueIndexName => "ix_migrations_version";

        #endregion
    }

    internal static class MigrationExtension {

        #region Internal Static Methods

        internal static IMigrationRunnerBuilder WithVersionTable<T>(this IMigrationRunnerBuilder self)
            where T : class, IVersionTableMetaDataAccessor {
            Prevent.Null(self, nameof(self));

            self.Services.AddScoped<IVersionTableMetaDataAccessor, T>();

            return self;
        }

        #endregion
    }
}
