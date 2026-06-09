using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IFinancialAccountRep
    {
        Task<RowResultObject<FinancialAccountReport>> GetFinancialAccountAsync(long stylistId, int pageIndex = 1, int pageSize = 20, DateTime? fromDate = null, DateTime? toDate = null, string transactionType = "");
        Task<BitResultObject> UpdateBankInfoAsync(long stylistId, string iban, string ownerName);
        Task<BitResultObject> RequestSettlementAsync(long stylistId, decimal amount, string description = "");
        Task<ListResultObject<AdminSettlementRequestReport>> GetSettlementRequestsAsync(string status = "", int pageIndex = 1, int pageSize = 20, string searchText = "");
        Task<BitResultObject> UpdateSettlementStatusAsync(long settlementRequestId, string status, string trackingCode = "", string rejectReason = "");
    }
}
