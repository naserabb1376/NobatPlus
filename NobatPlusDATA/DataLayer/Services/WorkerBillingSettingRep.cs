using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class WorkerBillingSettingRep : BillingReadRep<WorkerBillingSetting>, IWorkerBillingSettingRep
{
    public WorkerBillingSettingRep(NobatPlusContext context) : base(context) { }
}

