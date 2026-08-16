using FluentAssertions;
using GrandmasDreamNumbers.Application.Common.Exceptions;
using GrandmasDreamNumbers.Application.Dreams.Services;
using GrandmasDreamNumbers.Domain.Dreams;
using GrandmasDreamNumbers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GrandmasDreamNumbers.UnitTests.Dreams.Services;

public class DreamHistoryServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetDetailAsync_OtherUsersDream_ThrowsForbidden()
    {
        using var dbContext = CreateDbContext();
        var ownerId = Guid.NewGuid();
        var dream = new Dream { UserId = ownerId, OriginalText = "a dream" };
        dbContext.Dreams.Add(dream);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var service = new DreamHistoryService(dbContext);
        var otherUserId = Guid.NewGuid();

        var act = async () => await service.GetDetailAsync(otherUserId, dream.Id, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task DeleteAsync_OtherUsersDream_ThrowsForbidden()
    {
        using var dbContext = CreateDbContext();
        var ownerId = Guid.NewGuid();
        var dream = new Dream { UserId = ownerId, OriginalText = "a dream" };
        dbContext.Dreams.Add(dream);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var service = new DreamHistoryService(dbContext);
        var otherUserId = Guid.NewGuid();

        var act = async () => await service.DeleteAsync(otherUserId, dream.Id, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();

        (await dbContext.Dreams.FindAsync(dream.Id)).Should().NotBeNull("the dream must not be deleted when ownership fails");
    }

    [Fact]
    public async Task GetDetailAsync_UnknownDream_ThrowsNotFound()
    {
        using var dbContext = CreateDbContext();
        var service = new DreamHistoryService(dbContext);

        var act = async () => await service.GetDetailAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetHistoryAsync_OnlyReturnsCallersOwnDreams()
    {
        using var dbContext = CreateDbContext();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        dbContext.Dreams.AddRange(
            new Dream { UserId = userId, OriginalText = "mine" },
            new Dream { UserId = otherUserId, OriginalText = "not mine" });
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var service = new DreamHistoryService(dbContext);
        var history = await service.GetHistoryAsync(userId, searchQuery: null, CancellationToken.None);

        history.Should().ContainSingle(d => d.OriginalText == "mine");
    }

    [Fact]
    public async Task GetHistoryAsync_WithSearchQuery_FiltersByTextOrSummary()
    {
        using var dbContext = CreateDbContext();
        var userId = Guid.NewGuid();

        dbContext.Dreams.AddRange(
            new Dream { UserId = userId, OriginalText = "A snake chased me", Summary = "conflict dream" },
            new Dream { UserId = userId, OriginalText = "I flew over the ocean", Summary = "freedom dream" });
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var service = new DreamHistoryService(dbContext);
        var results = await service.GetHistoryAsync(userId, searchQuery: "snake", CancellationToken.None);

        results.Should().ContainSingle(d => d.OriginalText.Contains("snake"));
    }
}
