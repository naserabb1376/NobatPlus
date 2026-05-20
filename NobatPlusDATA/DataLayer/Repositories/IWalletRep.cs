using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IWalletRep
    {
        Task<RowResultObject<WalletReport>> GetWalletAsync(long customerId, int pageIndex = 1, int pageSize = 20);
        Task<ListResultObject<AdminWalletTransactionReport>> GetWalletTransactionsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "", string transactionType = "");
        Task<BitResultObject> ChargeWalletAsync(long customerId, decimal amount, string description = "");
        Task<BitResultObject> PayBookingAsync(long customerId, long bookingId, long discountId = 0, string description = "");
    }
}
