using Armali.Horizon.Core.Linq;

namespace Armali.Horizon.Core.Tests;

[TestClass]
public class HorizonExtensionsTests
{
    // ── ForEach ──────────────────────────────────────────────

    [TestMethod]
    public void ForEach_ExecutesActionForEachElement()
    {
        var source = new[] { 1, 2, 3 };
        var result = new List<int>();

        source.ForEach(x => result.Add(x * 2));

        result.ShouldBe([2, 4, 6]);
    }

    [TestMethod]
    public void ForEach_EmptySequence_DoesNotExecuteAction()
    {
        var executed = false;

        Array.Empty<int>().ForEach(_ => executed = true);

        executed.ShouldBeFalse();
    }

    // ── Peek ─────────────────────────────────────────────────

    [TestMethod]
    public void Peek_ExecutesActionWithoutConsumingElements()
    {
        var source = new[] { "a", "b", "c" };
        var sideEffects = new List<string>();

        var result = source.Peek(x => sideEffects.Add(x)).ToList();

        result.ShouldBe(["a", "b", "c"]);
        sideEffects.ShouldBe(["a", "b", "c"]);
    }

    [TestMethod]
    public void Peek_EmptySequence_ReturnsEmpty()
    {
        var result = Array.Empty<int>().Peek(_ => { }).ToList();

        result.ShouldBeEmpty();
    }

    // ── Random ───────────────────────────────────────────────

    [TestMethod]
    public void Random_ReturnsElementFromSequence()
    {
        var source = new[] { 10, 20, 30, 40, 50 };

        var result = source.Random();

        // El resultado debe ser uno de los elementos de la secuencia
        source.ShouldContain(result);
    }

    [TestMethod]
    public void Random_EmptySequence_ReturnsDefault()
    {
        var result = Array.Empty<int>().Random();

        result.ShouldBe(default);
    }

    [TestMethod]
    public void Random_SingleElement_ReturnsThatElement()
    {
        var result = new[] { 42 }.Random();

        result.ShouldBe(42);
    }

    // ── Stringify ─────────────────────────────────────────────

    [TestMethod]
    public void Stringify_NoFunctionNoSeparator_UsesToString()
    {
        var result = new[] { 1, 2, 3 }.Stringify();

        result.ShouldBe("123");
    }

    [TestMethod]
    public void Stringify_WithSeparator_InsertsSeparatorBetweenElements()
    {
        var result = new[] { "a", "b", "c" }.Stringify(separator: ", ");

        result.ShouldBe("a, b, c");
    }

    [TestMethod]
    public void Stringify_WithFunctionAndSeparator()
    {
        var result = new[] { 1, 2, 3 }.Stringify(x => $"[{x}]", "-");

        result.ShouldBe("[1]-[2]-[3]");
    }

    [TestMethod]
    public void Stringify_SingleElement_NoExtraSeparator()
    {
        var result = new[] { "solo" }.Stringify(separator: ", ");

        result.ShouldBe("solo");
    }

    // ── Generate ─────────────────────────────────────────────

    [TestMethod]
    public void Generate_ProducesInfiniteSequenceFromSeed()
    {
        var result = 1.Generate(x => x * 2).Take(5).ToList();

        result.ShouldBe([1, 2, 4, 8, 16]);
    }

    [TestMethod]
    public void Generate_ZeroSeed_ProducesConstantSequence()
    {
        var result = 0.Generate(x => x).Take(3).ToList();

        result.ShouldBe([0, 0, 0]);
    }

    // ── Enumerate ────────────────────────────────────────────

    [TestMethod]
    public void Enumerate_ConvertsElementToSingleElementSequence()
    {
        var result = 42.Enumerate().ToList();

        result.ShouldBe([42]);
    }

    [TestMethod]
    public void Enumerate_WithNull_ReturnsSequenceWithNull()
    {
        string? value = null;
        var result = value.Enumerate().ToList();

        result.Count.ShouldBe(1);
        result[0].ShouldBeNull();
    }
}


