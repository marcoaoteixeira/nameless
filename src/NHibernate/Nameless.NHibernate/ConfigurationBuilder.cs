using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.NHibernate {

    public sealed class ConfigurationBuilder : IConfigurationBuilder {

        #region Private Static Methods

        private static bool IsMappingType(Type? type) {
            if (type == null || type.IsAbstract || type.IsInterface) {
                return false;
            }

            return type.IsAssignableToGenericType(typeof(ClassMapping<>))
                || type.IsAssignableToGenericType(typeof(JoinedSubclassMapping<>))
                || type.IsAssignableToGenericType(typeof(SubclassMapping<>))
                || type.IsAssignableToGenericType(typeof(UnionSubclassMapping<>));
        }

        #endregion

        #region IConfigurationBuilder Members

        public Configuration Build(NHibernateOptions options) {
            options ??= new();

            var configuration = new Configuration();
            configuration.SetProperties(options.ToDictionary());

            var entityBaseTypes = options.EntityBaseTypes.Select(Type.GetType).ToArray();
            var modelInspector = new ModelInspector(entityBaseTypes!);
            var modelMapper = new ModelMapper(modelInspector);

            var mappingTypes = options.MappingTypes
                .Select(Type.GetType)
                .Where(IsMappingType)
                .ToArray();
            modelMapper.AddMappings(mappingTypes);

            configuration.AddDeserializedMapping(modelMapper.CompileMappingForAllExplicitlyAddedEntities(), null);

            return configuration;
        }

        #endregion
    }
}
