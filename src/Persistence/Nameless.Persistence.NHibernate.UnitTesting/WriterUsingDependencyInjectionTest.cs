using Autofac;
using Nameless.NHibernate;
using Nameless.Persistence.NHibernate.UnitTesting.Fixtures;
using NHibernate;

namespace Nameless.Persistence.NHibernate.UnitTesting {

    public class WriterUsingDependencyInjectionTest {

        // Insert New
        // Update Existent
        // UpSert New
        // UpSert Existent

        IContainer _container;

        [OneTimeSetUp]
        public void OneTimeSetUp() {
            var builder = new ContainerBuilder();
            var options = new NHibernateOptions {
                Connection = new() {
                    ConnectionString = "Data Source=:memory:;Version=3;Page Size=4096;",
                    ReleaseMode = ReleaseMode.OnClose // Needed for SQLite in memory
                },
                EntityRootTypes = new[] {
                    typeof(EntityBase).AssemblyQualifiedName!
                },
                MappingTypes = new[] {
                    typeof(PersonClassMapping).AssemblyQualifiedName!
                }
            };

            builder
                .RegisterInstance(options);
            builder
                .RegisterModule(new NHibernateModule {
                    SchemaInfo = new() {
                        ExecuteSchema = ExecuteSchemaOptions.OnSessionResolution
                    }
                })
                .RegisterModule(new PersistenceModule());

            _container = builder.Build();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() {
            _container.Dispose();
        }

        [Test]
        public async Task Save_With_Insert_Mode_Returns_Number_Of_Records_Affected() {
            var repository = _container.Resolve<IRepository>();

            var instruction = SaveInstruction<Person>.Insert(
                entity: new Person {
                    ID = Guid.NewGuid(),
                    Name = "Test",
                    Email = "test@test.com"
                }
            );

            var result = await repository.SaveAsync(instruction, cancellationToken: default);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task Save_With_Update_Mode_Returns_Number_Of_Records_Affected() {
            var repository = _container.Resolve<IRepository>();
            var session = _container.Resolve<ISession>();
            var id = Guid.NewGuid();

            var oldName = "Test";
            var oldEmail = "test@test.com";

            var insertInstruction = SaveInstruction<Person>.Insert(
                entity: new Person {
                    ID = id,
                    Name = oldName,
                    Email = oldEmail
                }
            );
            var insertResult = await repository.SaveAsync(insertInstruction, cancellationToken: default);

            var updateInstruction = SaveInstruction<Person>.Update(
                entity: new Person {
                    ID = id,
                    Name = "Test123",
                    Email = "test123@test123.com"
                }
            );
            var updateResult = await repository.SaveAsync(updateInstruction, cancellationToken: default);

            var actual = session.Query<Person>().Single(_ => _.ID == id);

            Assert.That(insertResult, Is.EqualTo(1));
            Assert.That(updateResult, Is.EqualTo(1));
            Assert.That(id, Is.EqualTo(actual.ID));
            Assert.That(oldName, Is.Not.EqualTo(actual.Name));
            Assert.That(oldEmail, Is.Not.EqualTo(actual.Email));
        }

        [Test]
        public async Task Save_Update_Mode_Filter_And_Patch_Returns_Number_Of_Affected_Records() {
            var repository = _container.Resolve<IRepository>();
            var firstID = Guid.NewGuid();
            var secondID = Guid.NewGuid();
            var thirdID = Guid.NewGuid();

            var collection = new SaveInstructionCollection<Person> {
                SaveInstruction<Person>.Insert(
                    entity: new Person {
                        ID = firstID,
                        Name = "Test",
                        Email = "test@test.com"
                    }
                ),

                SaveInstruction<Person>.Insert(
                    entity: new Person {
                        ID = secondID,
                        Name = "TO_UPDATE",
                        Email = "test@test.com"
                    }
                ),

                SaveInstruction<Person>.Insert(
                    entity: new Person {
                        ID = thirdID,
                        Name = "TO_UPDATE",
                        Email = "test@test.com"
                    }
                )
            };

            var insertResult = await repository.SaveAsync(collection, cancellationToken: default);

            var updateInstruction = SaveInstruction<Person>.Update(
                filter: _ => _.Name == "TO_UPDATE",
                patch: _ => new Person {
                    Name = "UPDATED"
                }
            );

            var updateResult = await repository.SaveAsync(updateInstruction, cancellationToken: default);

            Assert.That(insertResult, Is.EqualTo(3));
            Assert.That(updateResult, Is.EqualTo(2));
        }

        [Test]
        public async Task Save_UpSert_Mode_Filter_And_Patch_Returns_Number_Of_Affected_Records() {
            var repository = _container.Resolve<IRepository>();
            var firstID = Guid.NewGuid();
            var secondID = Guid.NewGuid();
            var thirdID = Guid.NewGuid();

            var collection = new SaveInstructionCollection<Person> {
                SaveInstruction<Person>.Insert(
                    entity: new Person {
                        ID = firstID,
                        Name = "Test",
                        Email = "test@test.com"
                    }
                ),

                SaveInstruction<Person>.Insert(
                    entity: new Person {
                        ID = secondID,
                        Name = "TO_UPDATE",
                        Email = "test@test.com"
                    }
                ),

                SaveInstruction<Person>.Insert(
                    entity: new Person {
                        ID = thirdID,
                        Name = "TO_UPDATE",
                        Email = "test@test.com"
                    }
                )
            };

            var insertResult = await repository.SaveAsync(collection, cancellationToken: default);

            var updateInstruction = SaveInstruction<Person>.Update(
                filter: _ => _.Name == "TO_UPDATE",
                patch: _ => new Person {
                    Name = "UPDATED"
                }
            );

            var updateResult = await repository.SaveAsync(updateInstruction, cancellationToken: default);

            Assert.That(insertResult, Is.EqualTo(3));
            Assert.That(updateResult, Is.EqualTo(2));
        }

        [Test]
        public async Task Save_UpSert_Mode_With_New_Record_Returns_Number_Of_Affected_Records() {
            var repository = _container.Resolve<IRepository>();
            var id = Guid.NewGuid();

            var instruction = SaveInstruction<Person>.UpSert(
                entity: new Person {
                    ID = id,
                    Name = "Test",
                    Email = "test@test.com"
                }
            );

            var insertResult = await repository.SaveAsync(instruction, cancellationToken: default);

            Assert.That(insertResult, Is.EqualTo(1));
        }

        [Test]
        public async Task Save_UpSert_With_Attached_Object() {
            var repository = _container.Resolve<IRepository>();
            var id = Guid.NewGuid();

            var insert = SaveInstruction<Person>.Insert(
                entity: new Person {
                    ID = id,
                    Name = "Test",
                    Email = "test@test.com"
                }
            );

            var upSert = SaveInstruction<Person>.UpSert(
                entity: new Person {
                    ID = id,
                    Name = "Test",
                    Email = "test@test.com"
                }
            );

            var insertResult = await repository.SaveAsync(insert, cancellationToken: default);
            var upSertResult = await repository.SaveAsync(upSert, cancellationToken: default);

            Assert.That(insertResult, Is.EqualTo(1));
            Assert.That(upSertResult, Is.EqualTo(1));
        }
    }
}
