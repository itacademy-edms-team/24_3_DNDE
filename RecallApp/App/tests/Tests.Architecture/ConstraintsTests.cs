using Media.Domain.Objects;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Tests.Architecture;

/// <summary>
/// Якоря значений из онтологий: изменение лимита - осознанное решение
/// с правкой документации в первую очередь и кода во вторую.
/// </summary>
public sealed class ConstraintsTests
{
    /// <summary>Якорь значений из ontology/media.md: политика загрузки и ссылки на чтение.</summary>
    [Fact]
    public void MediaConstraints_Values_MatchOntology()
    {
        Assert.Equal(10L * 1024 * 1024, MediaConstraints.MaxUploadBytes);
        Assert.Equal(25_000_000, MediaConstraints.MaxPixels);
        Assert.Equal(8000, MediaConstraints.MaxSide);
        Assert.Equal(400, MediaConstraints.ThumbMaxSide);
        Assert.Equal(1600, MediaConstraints.DisplayMaxSide);
        Assert.Equal(TimeSpan.FromMinutes(10), MediaConstraints.UploadWindow);
        Assert.Equal(TimeSpan.FromMinutes(15), MediaConstraints.ReadLinkLifetime);
    }

    /// <summary>Якорь значений из ontology/sets.md (ADR 0038).</summary>
    [Fact]
    public void SetConstraints_Values_MatchOntology()
    {
        Assert.Equal(120, SetConstraints.TitleMaxLength);
        Assert.Equal(2000, SetConstraints.DescriptionMaxLength);
    }

    /// <summary>Якорь значений из ontology/sets.md §5 (ADR 0038).</summary>
    [Fact]
    public void ElementConstraints_Values_MatchOntology()
    {
        Assert.Equal(500, ElementConstraints.TermMaxLength);
        Assert.Equal(2000, ElementConstraints.DefinitionMaxLength);
    }
}
