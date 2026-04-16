using System.Text.Json;
using Armali.Horizon.Megnir.Model;

namespace Armali.Horizon.Megnir.Tests;

[TestClass]
public class MegnirWorkerTests
{
    [TestMethod]
    public void MegnirBackup_DeserializesValidJson()
    {
        // Arrange
        var json = """
        {
            "Jobs": [
                {
                    "Name": "db-backup",
                    "Files": ["/var/data/app.db", "/var/data/app.db-wal"],
                    "Directories": []
                },
                {
                    "Name": "logs",
                    "Files": [],
                    "Directories": ["/var/log/app"]
                }
            ]
        }
        """;

        // Act
        var backup = JsonSerializer.Deserialize<MegnirBackup>(json);

        // Assert
        backup.ShouldNotBeNull();
        backup.Jobs.Length.ShouldBe(2);
        backup.Jobs[0].Name.ShouldBe("db-backup");
        backup.Jobs[0].Files.Length.ShouldBe(2);
        backup.Jobs[0].Directories.Length.ShouldBe(0);
        backup.Jobs[1].Name.ShouldBe("logs");
        backup.Jobs[1].Files.Length.ShouldBe(0);
        backup.Jobs[1].Directories.Length.ShouldBe(1);
    }

    [TestMethod]
    public void MegnirBackup_DeserializesEmptyJobs()
    {
        var json = """{ "Jobs": [] }""";

        var backup = JsonSerializer.Deserialize<MegnirBackup>(json);

        backup.ShouldNotBeNull();
        backup.Jobs.Length.ShouldBe(0);
    }

    [TestMethod]
    public void FindFileResponse_DeserializesNotFound()
    {
        var json = """{ "Found": false, "Content": "", "ResolvedVersion": "" }""";

        var response = JsonSerializer.Deserialize<FindFileResponse>(json);

        response.ShouldNotBeNull();
        response.Found.ShouldBeFalse();
    }

    [TestMethod]
    public void FindFileResponse_DeserializesFound()
    {
        var response = new FindFileResponse
        {
            Found = true,
            Content = """{"Jobs":[]}""",
            ResolvedVersion = "1.0.0"
        };

        // Simular round-trip de serialización como haría Horizon.IO
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<FindFileResponse>(json);

        deserialized.ShouldNotBeNull();
        deserialized.Found.ShouldBeTrue();
        deserialized.ResolvedVersion.ShouldBe("1.0.0");
        
        // El contenido debería ser deserializable a MegnirBackup
        var backup = JsonSerializer.Deserialize<MegnirBackup>(deserialized.Content);
        backup.ShouldNotBeNull();
    }
}

