using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Transactions;

namespace LockedIn.BusinessObject.Interfaces;

public interface ITransactionService
{
    Task<ApiResponse<PagedResult<AdminTransactionResponse>>> GetAdminTransactionsAsync(TransactionSearchRequest request);
    Task<ApiResponse<PagedResult<UserTransactionResponse>>> GetMyTransactionsAsync(TransactionSearchRequest request);
}
