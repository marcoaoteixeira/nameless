using Moq;

namespace Nameless.NHibernate.UnitTesting {

    public class SessionProviderTest {

        [Test]
        public void GetSession_Should_Return() {
            var configurationBuilder = new ConfigurationBuilder();
            var opts = new NHibernateOptions();

            ISessionProvider sessionProvider = new SessionProvider(configurationBuilder, opts);

            var currentSession = sessionProvider.GetSession();

            Assert.That(currentSession, Is.Not.Null);
        }
    }
}