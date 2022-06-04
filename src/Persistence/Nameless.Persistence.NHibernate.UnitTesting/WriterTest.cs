using System.Data.SQLite;
using Nameless.NHibernate;
using Nameless.Persistence.NHibernate.UnitTesting.Fixtures;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Nameless.Persistence.NHibernate.UnitTesting {
    public class WriterTest {

        private ISessionFactory _sessionFactory;
        private Configuration _configuration;

        private static string GetConnStr(Guid id) {
            var currentID = id == Guid.Empty ? Guid.NewGuid() : id;

            return $"Data Source=.\\{currentID:N}.s3db;Version=3;Page Size=4096;";
        }

        private ISession GetSession(Guid id) {
            var currentID = id == Guid.Empty ? Guid.NewGuid() : id;

            var session = _sessionFactory
                .WithOptions()
                .Connection(new SQLiteConnection(GetConnStr(currentID)))
                .OpenSession();

            session.Connection.Open();

            return session;
        }

        private static void ExecuteSchemaExport(ISession session, Configuration configuration) {
            new SchemaExport(configuration)
                .Execute(
                    useStdOut: true,
                    execute: true,
                    justDrop: false,
                    connection: session.Connection,
                    exportOutput: TextWriter.Null
                );
        }

        [OneTimeSetUp]
        public void OneTimeSetUp() {
            var opts = new NHibernateOptions {
                Connection = new() {
                    ReleaseMode = ReleaseMode.OnClose
                },
                EntityRootTypes = new[] {
                    typeof(EntityBase).AssemblyQualifiedName!
                },
                MappingTypes = new[] {
                    typeof(PersonClassMapping).AssemblyQualifiedName!
                }
            };

            var configurationBuilder = new ConfigurationBuilder();
            _configuration = configurationBuilder.Build(opts);
            _sessionFactory = _configuration.BuildSessionFactory();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() {
            _sessionFactory.Close();
            _sessionFactory.Dispose();
        }

        [Test]
        public async Task Save_With_Insert_Returns_Number_Of_Records_Affected() {
            var sessionID = Guid.NewGuid();
            using var session = GetSession(sessionID);
            ExecuteSchemaExport(session, _configuration);
            var writer = new Writer(session);

            var instruction = SaveInstruction<Person>.Insert(
                entity: new Person {
                    ID = Guid.Parse("e0cfdf61-20de-4ae8-8832-79fac9668063"),
                    Name = "Test",
                    Email = "test@test.com"
                }
            );
            var result = await writer.SaveAsync(instruction, cancellationToken: default);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task Save_With_Update_Returns_Number_Of_Records_Affected() {
            var sessionID = Guid.NewGuid();
            var personID = Guid.NewGuid();

            // Insert
            int insertResult;
            using (var session = GetSession(sessionID)) {
                // First time, execute schema export
                ExecuteSchemaExport(session, _configuration);

                var writer = new Writer(session);
                var insertInstruction = SaveInstruction<Person>.Insert(
                    entity: new Person {
                        ID = personID,
                        Name = "Test",
                        Email = "test@test.com"
                    }
                );
                insertResult = await writer.SaveAsync(insertInstruction, cancellationToken: default);
            }

            // Update
            var updateName = "Test123";
            var updateEmail = "test123@test123.com";
            int updateResult;
            using (var session = GetSession(sessionID)) {
                var writer = new Writer(session);
                var updateInstruction = SaveInstruction<Person>.Update(
                    entity: new Person {
                        ID = personID,
                        Name = updateName,
                        Email = updateEmail
                    }
                );
                updateResult = await writer.SaveAsync(updateInstruction, cancellationToken: default);
            }

            // Assert Query
            Person person;
            using (var session = GetSession(sessionID)) {
                person = session.Query<Person>().Single(_ => _.ID == personID);
            }

            Assert.That(insertResult, Is.EqualTo(1));
            Assert.That(updateResult, Is.EqualTo(1));
            Assert.That(person.ID, Is.EqualTo(personID));
            Assert.That(person.Name, Is.EqualTo(updateName));
            Assert.That(person.Email, Is.EqualTo(updateEmail));
        }

        [Test]
        public async Task Save_With_Update_Filter_And_Path_Returns_Number_Of_Records_Affected() {
            var sessionID = Guid.NewGuid();
            var personID = Guid.NewGuid();

            // Insert
            int insertResult;
            using (var session = GetSession(sessionID)) {
                // First time, execute schema export
                ExecuteSchemaExport(session, _configuration);

                var writer = new Writer(session);
                var insertInstruction = SaveInstruction<Person>.Insert(
                    entity: new Person {
                        ID = personID,
                        Name = "Test",
                        Email = "test@test.com"
                    }
                );
                insertResult = await writer.SaveAsync(insertInstruction, cancellationToken: default);
            }

            // Update
            var updateName = "Test123";
            var updateEmail = "test123@test123.com";
            int updateResult;
            using (var session = GetSession(sessionID)) {
                var writer = new Writer(session);
                var updateInstruction = SaveInstruction<Person>.Update(
                    filter: _ => _.ID == personID,
                    patch: _ => new Person {
                        Name = updateName,
                        Email = updateEmail
                    }
                );
                updateResult = await writer.SaveAsync(updateInstruction, cancellationToken: default);
            }

            // Assert Query
            Person person;
            using (var session = GetSession(sessionID)) {
                person = session.Query<Person>().Single(_ => _.ID == personID);
            }

            Assert.That(insertResult, Is.EqualTo(1));
            Assert.That(updateResult, Is.EqualTo(1));
            Assert.That(person.ID, Is.EqualTo(personID));
            Assert.That(person.Name, Is.EqualTo(updateName));
            Assert.That(person.Email, Is.EqualTo(updateEmail));
        }

        [Test]
        public async Task Save_With_Update_All_With_Filter_And_Path_Returns_Number_Of_Records_Affected() {
            var sessionID = Guid.NewGuid();

            // Insert
            int insertResult;
            using (var session = GetSession(sessionID)) {
                // First time, execute schema export
                ExecuteSchemaExport(session, _configuration);

                var writer = new Writer(session);
                var insertCollection = new SaveInstructionCollection<Person>();
                for (var idx = 0; idx < 5; idx++) {
                    insertCollection.Add(SaveInstruction<Person>.Insert(
                        entity: new Person {
                            ID = Guid.NewGuid(),
                            Name = $"Test_{idx + 1}",
                            Email = $"test_{idx + 1}@test.com"
                        }
                    ));
                }
                insertResult = await writer.SaveAsync(insertCollection, cancellationToken: default);
            }

            // Update
            var updateName = "Test_UPDATED";
            var updateEmail = "test_Test_UPDATED@test_Test_UPDATED.com";
            int updateResult;
            using (var session = GetSession(sessionID)) {
                var writer = new Writer(session);
                var updateInstruction = SaveInstruction<Person>.Update(
                    filter: _ => _.Name == "Test_3" || _.Name == "Test_5",
                    patch: _ => new Person {
                        Name = updateName,
                        Email = updateEmail
                    }
                );
                updateResult = await writer.SaveAsync(updateInstruction, cancellationToken: default);
            }

            // Assert Query
            Person[] people;
            using (var session = GetSession(sessionID)) {
                people = session.Query<Person>().Where(_ => _.Name!.Contains("UPDATED")).ToArray();
            }

            Assert.That(insertResult, Is.EqualTo(5));
            Assert.That(updateResult, Is.EqualTo(2));
            Assert.That(people, Has.Length.EqualTo(2));
        }
    }
}