using GrandmasDreamNumbers.Application.Admin.Dtos;
using GrandmasDreamNumbers.Application.Common.Exceptions;
using GrandmasDreamNumbers.Application.Common.Interfaces;
using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.EntityFrameworkCore;

namespace GrandmasDreamNumbers.Application.Admin.Services;

/// <summary>
/// Full CRUD over the cheat sheet, for the Admin-only endpoints. Unlike
/// the public search service, this is allowed to return everything,
/// including lucky numbers - access control is enforced at the controller
/// via role-based authorization.
/// </summary>
public class AdminDreamSymbolService(IApplicationDbContext dbContext)
{
    public async Task<IReadOnlyCollection<AdminDreamSymbolResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var symbols = await dbContext.DreamSymbols
            .Include(s => s.Aliases)
            .Include(s => s.LuckyNumbers)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return symbols.Select(ToResponse).ToList();
    }

    public async Task<AdminDreamSymbolResponse> CreateAsync(
        CreateDreamSymbolRequest request, CancellationToken cancellationToken)
    {
        var symbol = new DreamSymbol
        {
            Name = request.Name,
            Description = request.Description,
            TraditionalMeaning = request.TraditionalMeaning,
        };

        dbContext.DreamSymbols.Add(symbol);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(symbol);
    }

    public async Task<AdminDreamSymbolResponse> UpdateAsync(
        Guid id, UpdateDreamSymbolRequest request, CancellationToken cancellationToken)
    {
        var symbol = await GetSymbolOrThrowAsync(id, cancellationToken);

        symbol.Name = request.Name;
        symbol.Description = request.Description;
        symbol.TraditionalMeaning = request.TraditionalMeaning;
        symbol.IsActive = request.IsActive;
        symbol.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(symbol);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var symbol = await GetSymbolOrThrowAsync(id, cancellationToken);

        dbContext.DreamSymbols.Remove(symbol);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AdminDreamSymbolResponse> AddAliasAsync(
        Guid id, AddDreamSymbolAliasRequest request, CancellationToken cancellationToken)
    {
        var symbol = await GetSymbolOrThrowAsync(id, cancellationToken);

        symbol.Aliases.Add(new DreamSymbolAlias { DreamSymbolId = symbol.Id, Alias = request.Alias });
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(symbol);
    }

    public async Task<AdminDreamSymbolResponse> AddLuckyNumberAsync(
        Guid id, AddLuckyNumberRequest request, CancellationToken cancellationToken)
    {
        var symbol = await GetSymbolOrThrowAsync(id, cancellationToken);

        symbol.LuckyNumbers.Add(new LuckyNumber
        {
            DreamSymbolId = symbol.Id,
            Number = request.Number,
            Notes = request.Notes,
            IsPrimary = request.IsPrimary,
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(symbol);
    }

    private async Task<DreamSymbol> GetSymbolOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.DreamSymbols
            .Include(s => s.Aliases)
            .Include(s => s.LuckyNumbers)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Dream symbol '{id}' was not found.");
    }

    private static AdminDreamSymbolResponse ToResponse(DreamSymbol symbol) => new(
        symbol.Id,
        symbol.Name,
        symbol.Description,
        symbol.TraditionalMeaning,
        symbol.IsActive,
        symbol.Aliases.Select(a => new AdminDreamSymbolAliasResponse(a.Id, a.Alias)).ToList(),
        symbol.LuckyNumbers.Select(n => new AdminLuckyNumberResponse(n.Id, n.Number, n.Notes, n.IsPrimary)).ToList(),
        symbol.CreatedAt,
        symbol.UpdatedAt);
}
