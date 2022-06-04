using Nameless.WebApplication.Web.Utils;

namespace Nameless.WebApplication.Web.UnitTesting {

    public class HashTest {
        [Test]
        public void Create_Hash() {
            var value = Password.Hash("TEST");

            Assert.That(value, Is.Not.Null);
        }

        [Test]
        public void Validate_Hash() {
            var value = Password.Hash("TEST");

            var result = Password.Validate("TEST", value);

            Assert.That(result, Is.True);
        }
    }
}