using Sets.Domain.Elements;

namespace Tests.Integration.Sets.Domain;

/// <summary>
/// Правила порядка элементов набора живут в домене: здесь проверяется, кто и куда перемещается.
/// Применение к строкам (SQL) тестируется интеграционно через эндпоинты.
/// </summary>
public sealed class SetOrderingTests
{
    [Fact]
    public void Assign_NullOrder_ReturnsOrderRequired()
    {
        var elements = MakeElements(2);

        var result = SetOrdering.Assign(elements, null);

        Assert.True(result.IsError);
        Assert.Equal("Element.OrderRequired", result.FirstError.Code);
    }

    [Fact]
    public void Assign_DuplicateId_ReturnsOrderMismatch()
    {
        var elements = MakeElements(2);
        var duplicated = new[] { elements[0].Id, elements[0].Id };

        var result = SetOrdering.Assign(elements, duplicated);

        Assert.True(result.IsError);
        Assert.Equal("Element.OrderMismatch", result.FirstError.Code);
    }

    [Fact]
    public void Assign_MissingElement_ReturnsOrderMismatch()
    {
        var elements = MakeElements(2);

        var result = SetOrdering.Assign(elements, [elements[0].Id]);

        Assert.True(result.IsError);
        Assert.Equal("Element.OrderMismatch", result.FirstError.Code);
    }

    [Fact]
    public void Assign_FullOrder_ReturnsPositionsInRequestedOrder()
    {
        var elements = MakeElements(3);

        var result = SetOrdering.Assign(elements, [elements[2].Id, elements[0].Id, elements[1].Id]);

        Assert.False(result.IsError);
        Assert.Equal(
            new[]
            {
                new ElementPosition(elements[2].Id, 0),
                new ElementPosition(elements[0].Id, 1),
                new ElementPosition(elements[1].Id, 2),
            },
            result.Value);
    }

    [Fact]
    public void ForInsert_PositionZero_ShiftsAllElements()
    {
        var elements = MakeElements(2);

        var result = SetOrdering.ForInsert(elements, 0);

        Assert.False(result.IsError);
        Assert.Equal(
            new[]
            {
                new ElementPosition(elements[0].Id, 1),
                new ElementPosition(elements[1].Id, 2),
            },
            result.Value);
    }

    [Fact]
    public void ForInsert_PositionOne_ShiftsOnlyElementsBelow()
    {
        var elements = MakeElements(3);

        var result = SetOrdering.ForInsert(elements, 1);

        Assert.False(result.IsError);
        Assert.Equal(
            new[]
            {
                new ElementPosition(elements[1].Id, 2),
                new ElementPosition(elements[2].Id, 3),
            },
            result.Value);
    }

    [Fact]
    public void ForInsert_PositionEqualsCount_ShiftsNobody()
    {
        var elements = MakeElements(2);

        var result = SetOrdering.ForInsert(elements, 2);

        Assert.False(result.IsError);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void ForInsert_PositionBeyondCount_ReturnsPositionOutOfRange()
    {
        var elements = MakeElements(2);

        var result = SetOrdering.ForInsert(elements, 3);

        Assert.True(result.IsError);
        Assert.Equal("Element.PositionOutOfRange", result.FirstError.Code);
    }

    [Fact]
    public void ForRemoval_Middle_ShiftsOnlyElementsBelow()
    {
        var elements = MakeElements(3);

        var result = SetOrdering.ForRemoval(elements, 1);

        Assert.Equal([new ElementPosition(elements[2].Id, 1)], result);
    }

    [Fact]
    public void ForRemoval_Last_ShiftsNobody()
    {
        var elements = MakeElements(2);

        Assert.Empty(SetOrdering.ForRemoval(elements, 1));
    }

    private static List<Element> MakeElements(int count)
    {
        var setId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        return Enumerable.Range(0, count)
            .Select(position =>
            {
                var created = Element.Create(
                    setId,
                    FlashcardPayload.Create($"term {position}", $"definition {position}").Value,
                    position,
                    now.AddMinutes(position));

                Assert.False(created.IsError);
                return created.Value;
            })
            .ToList();
    }
}
