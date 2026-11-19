using Sets.Domain.Elements;

namespace Tests.Integration.Sets.Domain;

/// <summary>
/// Правила порядка набора: элемент встаёт между соседями через генератор рангов,
/// а некорректные соседи отклоняются до записи.
/// </summary>
public sealed class SetOrderingTests
{
    private static readonly Guid SetId = Guid.NewGuid();

    [Fact]
    public void PositionFor_BothNeighbours_PassesTheirRanksToGenerator()
    {
        var generator = new RecordingGenerator();
        var elements = MakeElements("0|000001:", "0|000002:", "0|000003:");

        var result = SetOrdering.PositionFor(
            generator, elements, elements[2].Id, elements[1].Id, null);

        Assert.False(result.IsError);
        Assert.Equal("0|000002:", generator.Previous?.Value);
        Assert.Null(generator.Next);
    }

    [Fact]
    public void PositionFor_FirstElement_UsesNullPrevious()
    {
        var generator = new RecordingGenerator();
        var elements = MakeElements("0|000001:", "0|000002:");

        var result = SetOrdering.PositionFor(
            generator, elements, elements[1].Id, null, elements[0].Id);

        Assert.False(result.IsError);
        Assert.Null(generator.Previous);
        Assert.Equal("0|000001:", generator.Next?.Value);
    }

    [Fact]
    public void PositionFor_NoNeighbours_ReturnsNeighboursRequired()
    {
        var elements = MakeElements("0|000001:");

        var result = SetOrdering.PositionFor(
            new RecordingGenerator(), elements, elements[0].Id, null, null);

        Assert.Equal("Element.OrderNeighboursRequired", result.FirstError.Code);
    }

    [Fact]
    public void PositionFor_SameNeighbourTwice_ReturnsOrderInvalid()
    {
        var elements = MakeElements("0|000001:");

        var result = SetOrdering.PositionFor(
            new RecordingGenerator(), elements, Guid.NewGuid(), elements[0].Id, elements[0].Id);

        Assert.Equal("Element.OrderInvalid", result.FirstError.Code);
    }

    [Fact]
    public void PositionFor_ElementItselfAsNeighbour_ReturnsOrderInvalid()
    {
        var elements = MakeElements("0|000001:", "0|000002:");

        var result = SetOrdering.PositionFor(
            new RecordingGenerator(), elements, elements[0].Id, elements[0].Id, elements[1].Id);

        Assert.Equal("Element.OrderInvalid", result.FirstError.Code);
    }

    [Fact]
    public void PositionFor_UnknownNeighbour_ReturnsNeighbourNotFound()
    {
        var elements = MakeElements("0|000001:");
        var strangerId = Guid.NewGuid();

        var result = SetOrdering.PositionFor(
            new RecordingGenerator(), elements, elements[0].Id, strangerId, null);

        Assert.Equal("Element.OrderNeighbourNotFound", result.FirstError.Code);
    }

    [Fact]
    public void PositionFor_PreviousAfterNext_ReturnsOrderInvalid()
    {
        var elements = MakeElements("0|000001:", "0|000002:");

        var result = SetOrdering.PositionFor(
            new RecordingGenerator(), elements, Guid.NewGuid(), elements[1].Id, elements[0].Id);

        Assert.Equal("Element.OrderInvalid", result.FirstError.Code);
    }

    [Fact]
    public void PositionAtEnd_EmptySet_PassesNullPrevious()
    {
        var generator = new RecordingGenerator();

        SetOrdering.PositionAtEnd(generator, null);

        Assert.Null(generator.Previous);
        Assert.Null(generator.Next);
    }

    [Fact]
    public void PositionAtEnd_NonEmptySet_UsesLastRank()
    {
        var generator = new RecordingGenerator();
        var elements = MakeElements("0|000001:", "0|000002:");

        SetOrdering.PositionAtEnd(generator, elements[^1]);

        Assert.Equal("0|000002:", generator.Previous?.Value);
        Assert.Null(generator.Next);
    }

    private static List<Element> MakeElements(params string[] positions)
    {
        var now = DateTime.UtcNow;

        return positions
            .Select((position, index) =>
                Element.Create(
                    SetId,
                    FlashcardPayload.Create($"term {index}", $"definition {index}").Value,
                    Position.FromStorage(position),
                    now.AddMinutes(index)).Value)
            .ToList();
    }

    /// <summary>Запоминает переданные соседей и возвращает предсказуемый ранг.</summary>
    private sealed class RecordingGenerator : IPositionGenerator
    {
        public Position? Previous { get; private set; }

        public Position? Next { get; private set; }

        public Position Between(Position? previous, Position? next)
        {
            Previous = previous;
            Next = next;
            return Position.FromStorage("0|000009:");
        }
    }
}
