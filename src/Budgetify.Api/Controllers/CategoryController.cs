using Budgetify.Core.DTOs;
using Budgetify.Core.Entities;
using Budgetify.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Budgetify.Api.Controllers;

[ApiController]
[Route("api/v1/category")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest("Description is required.");

        if (string.IsNullOrWhiteSpace(request.CategoryCode))
            return BadRequest("CategoryCode is required.");

        if (await _categoryRepository.ExistsByCodeAsync(request.CategoryCode))
            return BadRequest($"Category code '{request.CategoryCode}' already exists.");

        var category = new Category
        {
            Description = request.Description,
            FlowType = string.IsNullOrWhiteSpace(request.FlowType) ? "EXPENSE" : request.FlowType.ToUpperInvariant(),
            IsActive = request.IsActive ?? true,
            CategoryCode = request.CategoryCode.ToUpperInvariant()
        };

        var created = await _categoryRepository.AddAsync(category);
        return CreatedAtAction(nameof(GetCategoryById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> ListCategories([FromQuery] string? flow_type, [FromQuery] bool? is_active)
    {
        var categories = await _categoryRepository.GetAllAsync(flow_type, is_active);
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound("Category not found.");

        return Ok(category);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound("Category not found.");

        if (!string.IsNullOrWhiteSpace(request.Description))
            category.Description = request.Description;

        if (!string.IsNullOrWhiteSpace(request.FlowType))
            category.FlowType = request.FlowType.ToUpperInvariant();

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        if (!string.IsNullOrWhiteSpace(request.CategoryCode))
        {
            if (await _categoryRepository.ExistsByCodeAsync(request.CategoryCode, excludeId: id))
                return BadRequest($"Category code '{request.CategoryCode}' already exists.");
            category.CategoryCode = request.CategoryCode.ToUpperInvariant();
        }

        var updated = await _categoryRepository.UpdateAsync(category);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound("Category not found.");

        if (await _categoryRepository.HasAssociatedTransactionsAsync(id))
            return BadRequest("Cannot delete category with associated transactions. Use soft deactivation instead.");

        await _categoryRepository.DeleteAsync(id);
        return NoContent();
    }
}
