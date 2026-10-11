using Budgetify.Core.DTOs;
using Budgetify.Core.Entities;
using Budgetify.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Budgetify.Api.Controllers;

[ApiController]
[Route("api/v1/party")]
public class PartyController : ControllerBase
{
    private readonly IPartyRepository _partyRepository;

    public PartyController(IPartyRepository partyRepository)
    {
        _partyRepository = partyRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateParty([FromBody] CreatePartyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (string.IsNullOrWhiteSpace(request.PartyType))
            return BadRequest("PartyType is required.");

        if (string.IsNullOrWhiteSpace(request.PartyCode))
            return BadRequest("PartyCode is required.");

        if (await _partyRepository.ExistsByCodeAsync(request.PartyCode))
            return BadRequest($"Party code '{request.PartyCode}' already exists.");

        var party = new Party
        {
            Name = request.Name,
            PartyType = request.PartyType.ToUpperInvariant(),
            IsActive = request.IsActive ?? true,
            PartyCode = request.PartyCode.ToUpperInvariant(),
            Iban = request.Iban,
            Notes = request.Notes
        };

        var created = await _partyRepository.AddAsync(party);
        return CreatedAtAction(nameof(GetPartyById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> ListParties(
        [FromQuery] string? party_type,
        [FromQuery] bool? is_active,
        [FromQuery] string? search)
    {
        var parties = await _partyRepository.GetAllAsync(party_type, is_active, search);
        return Ok(parties);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPartyById(Guid id)
    {
        var party = await _partyRepository.GetByIdAsync(id);
        if (party == null)
            return NotFound("Party not found.");

        return Ok(party);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateParty(Guid id, [FromBody] UpdatePartyRequest request)
    {
        var party = await _partyRepository.GetByIdAsync(id);
        if (party == null)
            return NotFound("Party not found.");

        if (!string.IsNullOrWhiteSpace(request.Name))
            party.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.PartyType))
            party.PartyType = request.PartyType.ToUpperInvariant();

        if (request.IsActive.HasValue)
            party.IsActive = request.IsActive.Value;

        if (!string.IsNullOrWhiteSpace(request.PartyCode))
        {
            if (await _partyRepository.ExistsByCodeAsync(request.PartyCode, excludeId: id))
                return BadRequest($"Party code '{request.PartyCode}' already exists.");
            party.PartyCode = request.PartyCode.ToUpperInvariant();
        }

        if (request.Iban != null)
            party.Iban = request.Iban;

        if (request.Notes != null)
            party.Notes = request.Notes;

        var updated = await _partyRepository.UpdateAsync(party);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteParty(Guid id)
    {
        var party = await _partyRepository.GetByIdAsync(id);
        if (party == null)
            return NotFound("Party not found.");

        if (await _partyRepository.HasAssociatedTransactionsAsync(id))
            return BadRequest("Cannot delete party with associated transactions. Use soft deactivation instead.");

        await _partyRepository.DeleteAsync(id);
        return NoContent();
    }
}
