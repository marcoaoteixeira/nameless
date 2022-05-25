namespace Nameless.FileStorage.System.UnitTesting {

    public class FileStorageTest {

        [Test]
        public async Task GetFileAsync_Retrieves_File() {
            var fileStorage = new SystemFileStorage(null);

            var file = await fileStorage.GetFileAsync("Temp.txt");

            Assert.NotNull(file);
        }

        [Test]
        public async Task GetFileAsync_Retrieves_Existent_File() {
            var fileStorage = new SystemFileStorage(null);

            var fileName = Path.GetFileName(typeof(FileStorageTest).Assembly.Location);
            var file = await fileStorage.GetFileAsync(fileName);

            Assert.NotNull(file);
            Assert.True(file.Exists);
        }
    }
}
