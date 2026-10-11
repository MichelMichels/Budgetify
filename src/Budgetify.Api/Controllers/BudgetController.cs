using Budgetify.Core.DTOs;
using Budgetify.Core.Entities;
using Budgetify.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Budgetify.Api.Controllers;

[ApiController]
[Route("api/v1/budget")]
public class BudgetController : ControllerBase
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;

    public BudgetController(
        IBudgetRepository budgetRepository,
        ICategoryRepository categoryRepository)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Budget amount must be greater than zero.");

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            return BadRequest($"Category with ID {request.CategoryId} was not found.");

        var existing = await _budgetRepository.GetForCategoryAndPeriodAsync(request.CategoryId, request.Period);
        if (existing != null)
            return Conflict("A budget for this category and period already exists.");

        var budget = new Budget
        {
            CategoryId = request.CategoryId,
            Period = string.IsNullOrWhiteSpace(request.Period) ? null : request.Period.Trim(),
            Amount = request.Amount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "EUR" : request.Currency.ToUpperInvariant(),
            Notes = request.Notes
        };

        var created = await _budgetRepository.AddAsync(budget);
        return CreatedAtAction(nameof(GetBudgetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> ListBudgets(
        [FromQuery] Guid? category_id,
        [FromQuery] string? period,
        [FromQuery] bool? is_recurring,
        [FromQuery] string? currency)
    {
        var budgets = await _budgetRepository.GetAllAsync(category_id, period, is_recurring, currency);
        return Ok(budgets);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBudgetById(Guid id)
    {
        var budget = await _budgetRepository.GetByIdAsync(id);
        if (budget == null)
            return NotFound("Budget not found.");

        return Ok(budget);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBudget(Guid id, [FromBody] UpdateBudgetRequest request)
    {
        var budget = await _budgetRepository.GetByIdAsync(id);
        if (budget == null)
            return NotFound("Budget not found.");

        if (request.Amount.HasValue)
        {
            if (request.Amount.Value <= 0)
                return BadRequest("Budget amount must be greater than zero.");
            budget.Amount = request.Amount.Value;
        }

        if (request.Notes != null)
            budget.Notes = request.Notes;

        var updated = await _budgetRepository.UpdateAsync(budget);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBudget(Guid id)
    {
        var deleted = await _budgetRepository.DeleteAsync(id);
        if (!deleted)
            return NotFound("Budget not found.");

        return NoContent();
    }
}
