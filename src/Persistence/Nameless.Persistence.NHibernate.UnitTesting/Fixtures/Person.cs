using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.Persistence.NHibernate.UnitTesting.Fixtures {

    public class Person : EntityBase {

        public virtual string? Name { get; set; }
        public virtual string? Email { get; set; }
    }

    public class PersonClassMapping : ClassMapping<Person> {

        public PersonClassMapping() {

            Id(_ => _.ID, mapping => {
                mapping.Type(NHibernateUtil.Guid);
            });

            Property(_ => _.Name);
            Property(_ => _.Email);
        }
    }
}
