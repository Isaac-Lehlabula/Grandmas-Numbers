namespace GrandmasDreamNumbers.Application.Admin.Dtos;

public sealed record AdminLuckyNumberResponse(Guid Id, int Number, string? Notes, bool IsPrimary);

public sealed record AdminDreamSymbolAliasResponse(Guid Id, string Alias);

public sealed record AdminDreamSymbolResponse(
    Guid Id,
    string Name,
    string Description,
    string TraditionalMeaning,
    bool IsActive,
    IReadOnlyCollection<AdminDreamSymbolAliasResponse> Aliases,
    IReadOnlyCollection<AdminLuckyNumberResponse> LuckyNumbers,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateDreamSymbolRequest(
    string Name,
    string Description,
    string TraditionalMeaning);

public sealed record UpdateDreamSymbolRequest(
    string Name,
    string Description,
    string TraditionalMeaning,
    bool IsActive);

public sealed record AddDreamSymbolAliasRequest(string Alias);

public sealed record AddLuckyNumberRequest(int Number, string? Notes, bool IsPrimary);
