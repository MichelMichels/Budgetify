using Budgetify.Core.Entities;

namespace Budgetify.Core.Interfaces;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetAllAsync(
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        Guid? accountId = null,
        Guid? destinationAccountId = null,
        Guid? categoryId = null,
        Guid? partyId = null,
        string? type = null,
        string? currency = null,
        string? paymentMethod = null,
        bool? isRecurring = null,
        int limit = 50,
        int offset = 0
    );
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<Transaction> AddAsync(Transaction transaction);
    Task<Transaction?> UpdateAsync(Transaction transaction);
    Task<bool> DeleteAsync(Guid id);
}
