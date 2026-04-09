using Armali.Horizon.Blazor;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class UtilsTests
{
    // ── Index ────────────────────────────────────────────────

    private sealed record FakeItem(int Id, string Label) : Identifiable;

    [TestMethod]
    public void Index_ReturnsElementById()
    {
        var items = new List<FakeItem>
        {
            new(1, "Alpha"),
            new(2, "Beta"),
            new(3, "Gamma")
        };

        var result = Utils.Index(items, 2);

        result.ShouldNotBeNull();
        result.Label.ShouldBe("Beta");
    }

    [TestMethod]
    public void Index_NonExistentId_ReturnsNull()
    {
        var items = new List<FakeItem> { new(1, "Alpha") };

        var result = Utils.Index(items, 99);

        result.ShouldBeNull();
    }

    [TestMethod]
    public void Index_EmptyCollection_ReturnsNull()
    {
        var result = Utils.Index(Enumerable.Empty<FakeItem>(), 1);

        result.ShouldBeNull();
    }

    // ── PrivacyIcon ──────────────────────────────────────────

    [TestMethod]
    public void PrivacyIcon_Private_ReturnsLock()
    {
        Utils.PrivacyIcon(true, "userA", "userA").ShouldBe("fa-lock");
        Utils.PrivacyIcon(true, "userA", "userB").ShouldBe("fa-lock");
    }

    [TestMethod]
    public void PrivacyIcon_PublicOwnCreator_ReturnsGlobe()
    {
        Utils.PrivacyIcon(false, "userA", "userA").ShouldBe("fa-globe");
    }

    [TestMethod]
    public void PrivacyIcon_PublicOtherCreator_ReturnsShare()
    {
        Utils.PrivacyIcon(false, "userA", "userB").ShouldBe("fa-share-nodes");
    }

    [TestMethod]
    public void PrivacyIcon_Null_ReturnsEmpty()
    {
        Utils.PrivacyIcon(null, "userA", "userA").ShouldBeEmpty();
    }

    // ── PrivacyColor ─────────────────────────────────────────

    [TestMethod]
    public void PrivacyColor_Private_ReturnsTribunal()
    {
        Utils.PrivacyColor(true, "userA", "userA").ShouldBe("text-hz-tribunal");
    }

    [TestMethod]
    public void PrivacyColor_PublicOwnCreator_ReturnsOpera()
    {
        Utils.PrivacyColor(false, "userA", "userA").ShouldBe("text-hz-opera");
    }

    [TestMethod]
    public void PrivacyColor_PublicOtherCreator_ReturnsSol()
    {
        Utils.PrivacyColor(false, "userA", "userB").ShouldBe("text-hz-sol");
    }

    [TestMethod]
    public void PrivacyColor_Null_ReturnsEmpty()
    {
        Utils.PrivacyColor(null, "userA", "userA").ShouldBeEmpty();
    }
}

