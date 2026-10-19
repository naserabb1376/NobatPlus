using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IWorkerServiceRep
    {
        public Task<ListResultObject<WorkerServiceWithDiscountDto>> GetAllWorkerServicesAsync(long workerId = 0,long serviceId = 0, long customerId = 0,long bookingId = 0,long discountId = 0,List<long>? optionValueIds = null,bool onlyLeafServices = false,int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<WorkerServiceWithDiscountDto>> GetWorkerServiceByIdAsync(long WorkerId, long ServiceManagementId, long customerId = 0,long discountId = 0,List<long>? optionValueIds = null);
        public Task<BitResultObject> AddWorkerServicesAsync(List<WorkerService> workerServices);
        public Task<BitResultObject> EditWorkerServicesAsync(List<WorkerService> workerServices);
        public Task<BitResultObject> RemoveWorkerServicesAsync(List<WorkerService> workerServices);
        public Task<BitResultObject> RemoveWorkerServicesAsync(List<(long WorkerId, long ServiceManagementId,long? workerServicePriceVariantId)> workerServiceIds);
        public Task<BitResultObject> ExistWorkerServiceAsync(long WorkerId, long ServiceManagementId);
    }
}
