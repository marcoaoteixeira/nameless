using Nameless.Backup.Internals;
using Nameless.ObjectModel;
using Nameless.Testing.Tools.IO;

namespace Nameless.Backup;

[UnitTest]
public sealed class BackupManifestTests : IDisposable
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 11, 9, 30, 0, TimeSpan.Zero);

    // sha256("hello")
    private const string HELLO_SHA256 = "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824";

    private readonly TempDirectory _temp = new();

    public void Dispose()
    {
        _temp.Dispose();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private string ManifestPath => _temp.Combine("payload", BackupManifest.FileName);

    private async Task<Result<BackupManifest>> ReadJsonAsync(string json)
    {
        _temp.CreateFile(SysPath.Combine("payload", BackupManifest.FileName), json);

        return await BackupManifestSerializer.ReadAsync(ManifestPath, Token);
    }

    private static string Json(string routines, int formatVersion = 1)
    {
        return $$"""{ "formatVersion": {{formatVersion}}, "createdAt": "2026-10-11T09:30:00+00:00", "routines": {{routines}} }""";
    }

    private static string RoutineJson(string key = "db", string files = "[]")
    {
        return $$"""{ "key": "{{key}}", "order": 0, "files": {{files}} }""";
    }

    private static string FileJson(string path = "a.txt", long length = 5, string sha256 = HELLO_SHA256)
    {
        return $$"""{ "path": "{{path}}", "length": {{length}}, "sha256": "{{sha256}}" }""";
    }

    // ---- Builder ----------------------------------------------------------

    [Fact]
    public async Task Describe_lists_files_with_relative_forward_slash_paths()
    {
        _temp.CreateFile(SysPath.Combine("payload", "db", "a.txt"), "hello");
        _temp.CreateFile(SysPath.Combine("payload", "db", "sub", "b.txt"), "hello");

        var result = await BackupManifestBuilder.DescribeAsync("db", 3, _temp.Combine("payload", "db"), Token);

        Assert.True(result.Success);
        Assert.Equal("db", result.Value.Key);
        Assert.Equal(3, result.Value.Order);
        Assert.Equal(["a.txt", "sub/b.txt"], result.Value.Files.Select(file => file.Path));
        Assert.All(result.Value.Files, file =>
        {
            Assert.Equal(5, file.Length);
            Assert.Equal(HELLO_SHA256, file.Sha256);
        });
    }

    [Fact]
    public async Task Describe_of_empty_directory_has_no_files()
    {
        var directory = _temp.CreateDirectory(SysPath.Combine("payload", "db"));

        var result = await BackupManifestBuilder.DescribeAsync("db", 0, directory, Token);

        Assert.True(result.Success);
        Assert.Empty(result.Value.Files);
    }

    [Fact]
    public async Task Describe_fails_when_directory_holds_a_link()
    {
        var target = _temp.CreateFile("target.txt", "hello");
        var directory = _temp.CreateDirectory(SysPath.Combine("payload", "db"));

        try
        {
            SysFile.CreateSymbolicLink(SysPath.Combine(directory, "link.txt"), target);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            Assert.Skip("Creating symbolic links is not allowed for this user.");
        }

        var result = await BackupManifestBuilder.DescribeAsync("db", 0, directory, Token);

        Assert.False(result.Success);
        Assert.Contains("link.txt", result.Errors[0].Message);
    }

    // ---- Serializer -------------------------------------------------------

    [Fact]
    public async Task Write_then_read_round_trips()
    {
        var manifest = new BackupManifest(
            BackupManifest.CurrentFormatVersion,
            CreatedAt,
            [new BackupManifestRoutine("db", 2, [new BackupManifestFile("sub/a.txt", 5, HELLO_SHA256)])]
        );

        SysDirectory.CreateDirectory(_temp.Combine("payload"));
        await BackupManifestSerializer.WriteAsync(manifest, ManifestPath, Token);
        var result = await BackupManifestSerializer.ReadAsync(ManifestPath, Token);

        Assert.True(result.Success);
        Assert.Equal(CreatedAt, result.Value.CreatedAt);
        var routine = Assert.Single(result.Value.Routines);
        Assert.Equal(("db", 2), (routine.Key, routine.Order));
        Assert.Equal(new BackupManifestFile("sub/a.txt", 5, HELLO_SHA256), Assert.Single(routine.Files));
    }

    [Fact]
    public async Task Written_json_uses_camel_case()
    {
        var manifest = new BackupManifest(1, CreatedAt, [new BackupManifestRoutine("db", 0, [])]);

        SysDirectory.CreateDirectory(_temp.Combine("payload"));
        await BackupManifestSerializer.WriteAsync(manifest, ManifestPath, Token);
        var json = await SysFile.ReadAllTextAsync(ManifestPath, Token);

        Assert.Contains("\"formatVersion\"", json);
        Assert.Contains("\"routines\"", json);
    }

    [Fact]
    public async Task Read_of_missing_manifest_is_a_validation_error()
    {
        var result = await BackupManifestSerializer.ReadAsync(ManifestPath, Token);

        Assert.False(result.Success);
        Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
    }

    public static TheoryData<string> InvalidManifests => new()
    {
        "not json",
        "null",
        "[]",
        Json(routines: "null"),
        Json(routines: "[]"),
        Json(routines: $"[{RoutineJson()}]", formatVersion: 2),
        Json(routines: $"[{RoutineJson()}]", formatVersion: 0),
        Json(routines: $"[{RoutineJson(key: "Bad Key")}]"),
        Json(routines: $"[{RoutineJson()}, {RoutineJson()}]"),
        Json(routines: $"[{RoutineJson(files: "null")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(path: "")}]")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(path: "/abs.txt")}]")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(path: "C:/abs.txt")}]")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(path: "../up.txt")}]")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(path: "a/../b.txt")}]")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(path: "a//b.txt")}]")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(path: "a\\\\b.txt")}]")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(path: "./a.txt")}]")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(length: -1)}]")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(sha256: "abc")}]")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(sha256: new string('Z', 64))}]")}]"),
        Json(routines: $"[{RoutineJson(files: $"[{FileJson(path: "a.txt")}, {FileJson(path: "A.TXT")}]")}]")
    };

    [Theory]
    [MemberData(nameof(InvalidManifests))]
    public async Task Read_rejects_invalid_manifest(string json)
    {
        var result = await ReadJsonAsync(json);

        Assert.False(result.Success);
        Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
    }

    // ---- Verifier ---------------------------------------------------------

    private BackupManifest ManifestWith(params BackupManifestFile[] files)
    {
        return new BackupManifest(1, CreatedAt, [new BackupManifestRoutine("db", 0, files)]);
    }

    private string Payload(params (string Path, string Content)[] files)
    {
        var payload = _temp.CreateDirectory("payload");

        _temp.CreateFile(SysPath.Combine("payload", BackupManifest.FileName), "{}");

        foreach (var (path, content) in files)
        {
            _temp.CreateFile(SysPath.Combine("payload", path), content);
        }

        return payload;
    }

    [Fact]
    public async Task Verify_accepts_matching_payload()
    {
        var payload = Payload(("db/sub/a.txt", "hello"));

        var result = await BackupManifestVerifier.VerifyAsync(ManifestWith(new BackupManifestFile("sub/a.txt", 5, HELLO_SHA256)), payload, Token);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task Verify_rejects_missing_file()
    {
        var payload = Payload();

        var result = await BackupManifestVerifier.VerifyAsync(ManifestWith(new BackupManifestFile("a.txt", 5, HELLO_SHA256)), payload, Token);

        Assert.False(result.Success);
        Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
        Assert.Contains("db/a.txt", result.Errors[0].Message);
    }

    [Fact]
    public async Task Verify_rejects_unlisted_file()
    {
        var payload = Payload(("db/a.txt", "hello"), ("db/extra.txt", "x"));

        var result = await BackupManifestVerifier.VerifyAsync(ManifestWith(new BackupManifestFile("a.txt", 5, HELLO_SHA256)), payload, Token);

        Assert.False(result.Success);
        Assert.Contains("db/extra.txt", result.Errors[0].Message);
    }

    [Fact]
    public async Task Verify_rejects_unlisted_routine_folder()
    {
        var payload = Payload(("db/a.txt", "hello"), ("other/x.txt", "x"));

        var result = await BackupManifestVerifier.VerifyAsync(ManifestWith(new BackupManifestFile("a.txt", 5, HELLO_SHA256)), payload, Token);

        Assert.False(result.Success);
        Assert.Contains("other/x.txt", result.Errors[0].Message);
    }

    [Fact]
    public async Task Verify_rejects_length_mismatch()
    {
        var payload = Payload(("db/a.txt", "hello!"));

        var result = await BackupManifestVerifier.VerifyAsync(ManifestWith(new BackupManifestFile("a.txt", 5, HELLO_SHA256)), payload, Token);

        Assert.False(result.Success);
        Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
    }

    [Fact]
    public async Task Verify_rejects_hash_mismatch()
    {
        var payload = Payload(("db/a.txt", "HELLO"));

        var result = await BackupManifestVerifier.VerifyAsync(ManifestWith(new BackupManifestFile("a.txt", 5, HELLO_SHA256)), payload, Token);

        Assert.False(result.Success);
        Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
    }
}
