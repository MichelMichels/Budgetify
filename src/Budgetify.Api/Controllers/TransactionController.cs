using Budgetify.Core.DTOs;
using Budgetify.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Budgetify.Api.Controllers;

[ApiController]
[Route("api/v1/transaction")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ITransactionRepository _transactionRepository;

    public TransactionController(
        ITransactionService transactionService,
        ITransactionRepository transactionRepository)
    {
        _transactionService = transactionService;
        _transactionRepository = transactionRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        try
        {
            var created = await _transactionService.CreateTransactionAsync(request);
            return CreatedAtAction(nameof(GetTransactionById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ListTransactions(
        [FromQuery] DateTimeOffset? start_date,
        [FromQuery] DateTimeOffset? end_date,
        [FromQuery] Guid? account_id,
        [FromQuery] Guid? destination_account_id,
        [FromQuery] Guid? category_id,
        [FromQuery] Guid? party_id,
        [FromQuery] string? type,
        [FromQuery] string? currency,
        [FromQuery] string? payment_method,
        [FromQuery] bool? is_recurring,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        var transactions = await _transactionRepository.GetAllAsync(
            start_date, end_date, account_id, destination_account_id,
            category_id, party_id, type, currency, payment_method, is_recurring, limit, offset);

        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction == null)
            return NotFound("Transaction not found.");

        return Ok(transaction);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] UpdateTransactionRequest request)
    {
        try
        {
            var updated = await _transactionService.UpdateTransactionAsync(id, request);
            if (updated == null)
                return NotFound("Transaction not found.");

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        var deleted = await _transactionService.DeleteTransactionAsync(id);
        if (!deleted)
            return NotFound("Transaction not found.");

        return NoContent();
    }
}
