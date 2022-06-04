namespace Nameless.NHibernate {

    public sealed class ExecuteSchemaInfo {

        #region Public Static Read-Only Properties

        public static ExecuteSchemaInfo Default => new();

        #endregion

        #region Internal Properties

        public ExecuteSchemaOptions ExecuteSchema { get; set; } = ExecuteSchemaOptions.OnSessionFactoryResolution;
        public SchemaOutputOptions SchemaOutput { get; set; } = SchemaOutputOptions.Console;
        public string? SchemaOutputPath { get; set; }
        public bool ExecuteAgainstDatabase { get; set; } = true;
        public bool DropBeforeExecute { get; set; } = false;

        #endregion
    }
}
