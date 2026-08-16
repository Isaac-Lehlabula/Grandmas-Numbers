using FluentAssertions;
using GrandmasDreamNumbers.Domain.Common;

namespace GrandmasDreamNumbers.UnitTests.Domain.Common;

public class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity
    {
    }

    [Fact]
    public void NewEntity_HasNonEmptyId()
    {
        var entity = new TestEntity();

        entity.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void TwoNewEntities_HaveDifferentIds()
    {
        var first = new TestEntity();
        var second = new TestEntity();

        first.Id.Should().NotBe(second.Id);
    }
}
