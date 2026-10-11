using Budgetify.Core.DTOs;
using Budgetify.Core.Entities;
using Budgetify.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Budgetify.Api.Controllers;

[ApiController]
[Route("api/v1/account")]
public class AccountController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;

    public AccountController(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (string.IsNullOrWhiteSpace(request.AccountType))
            return BadRequest("AccountType is required.");

        var account = new Account
        {
            Name = request.Name,
            AccountType = request.AccountType.ToUpperInvariant(),
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "EUR" : request.Currency.ToUpperInvariant(),
            CurrentBalance = request.InitialBalance ?? 0,
            Iban = request.Iban,
            IsActive = request.IsActive ?? true,
            Notes = request.Notes
        };

        var created = await _accountRepository.AddAsync(account);
        return CreatedAtAction(nameof(GetAccountById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> ListAccounts(
        [FromQuery] string? account_type,
        [FromQuery] bool? is_active,
        [FromQuery] string? currency)
    {
        var accounts = await _accountRepository.GetAllAsync(account_type, is_active, currency);
        return Ok(accounts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
            return NotFound("Account not found.");

        return Ok(account);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountRequest request)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
            return NotFound("Account not found.");

        if (!string.IsNullOrWhiteSpace(request.Name))
            account.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.AccountType))
            account.AccountType = request.AccountType.ToUpperInvariant();

        if (request.Iban != null)
            account.Iban = request.Iban;

        if (request.IsActive.HasValue)
            account.IsActive = request.IsActive.Value;

        if (request.Notes != null)
            account.Notes = request.Notes;

        var updated = await _accountRepository.UpdateAsync(account);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
            return NotFound("Account not found.");

        if (await _accountRepository.HasAssociatedTransactionsAsync(id))
            return BadRequest("Cannot delete account with associated transactions.");

        await _accountRepository.DeleteAsync(id);
        return NoContent();
    }
}
