namespace Nameless.NHibernate.UnitTesting {

    public class ConfigurationBuilderTest {

        [Test]
        public void Build_Should_Return_Configuration() {

            IConfigurationBuilder builder = new ConfigurationBuilder();

            var config = builder.Build();

            Assert.NotNull(config);
        }

        [Test]
        public void Build_With_Options_Should_Return_Custom_Configuration() {
            IConfigurationBuilder builder = new ConfigurationBuilder();

            var expected = "This is a test";

            var opts = new NHibernateOptions();
            opts.Common.Dialect = expected;

            var config = builder.Build(opts);

            Assert.NotNull(config);
            Assert.That(config.Properties["dialect"], Is.EqualTo(expected));
        }
    }
}
