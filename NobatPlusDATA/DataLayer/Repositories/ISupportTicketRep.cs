using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.ViewModels;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ISupportTicketRep
    {
        Task<ListResultObject<SupportTicketVM>> GetAllSupportTicketsAsync(
            long personId = 0,
            string status = "",
            string priority = "",
            string category = "",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageIndex = 1,
            int pageSize = 20,
            string searchText = "",
            string sortQuery = "");

        Task<RowResultObject<SupportTicketVM>> GetSupportTicketByIdAsync(long id);
        Task<BitResultObject> AddSupportTicketAsync(SupportTicket ticket, string firstMessage);
        Task<BitResultObject> AddSupportTicketMessageAsync(long ticketId, long senderPersonId, bool isAdminReply, string message, string nextStatus = "");
        Task<BitResultObject> UpdateSupportTicketStatusAsync(long ticketId, string status, long? assignedAdminPersonId = null);
        Task<BitResultObject> RemoveSupportTicketAsync(long id);
    }
}
