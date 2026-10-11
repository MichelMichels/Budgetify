using Budgetify.Core.DTOs;
using Budgetify.Core.Entities;

namespace Budgetify.Core.Interfaces;

public interface ITransactionService
{
    Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request);
    Task<Transaction?> UpdateTransactionAsync(Guid id, UpdateTransactionRequest request);
    Task<bool> DeleteTransactionAsync(Guid id);
}
