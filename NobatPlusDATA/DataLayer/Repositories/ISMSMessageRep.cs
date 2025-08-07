using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ISMSMessageRep
    {
        public Task<ListResultObject<SMSMessage>> GetAllSMSMessagesAsync(long personId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<SMSMessage>> GetSMSMessageByIdAsync(long SMSMessageId);
        public Task<BitResultObject> AddSMSMessageAsync(SMSMessage SMSMessage);
        public Task<BitResultObject> EditSMSMessageAsync(SMSMessage SMSMessage);
        public Task<BitResultObject> RemoveSMSMessageAsync(SMSMessage SMSMessage);
        public Task<BitResultObject> RemoveSMSMessageAsync(long SMSMessageId);
        public Task<BitResultObject> ExistSMSMessageAsync(long SMSMessageId);
    }
}
