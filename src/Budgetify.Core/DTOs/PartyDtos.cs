namespace Budgetify.Core.DTOs;

public record CreatePartyRequest(
    string Name,
    string PartyType,
    bool? IsActive,
    string PartyCode,
    string? Iban,
    string? Notes
);

public record UpdatePartyRequest(
    string? Name,
    string? PartyType,
    bool? IsActive,
    string? PartyCode,
    string? Iban,
    string? Notes
);
