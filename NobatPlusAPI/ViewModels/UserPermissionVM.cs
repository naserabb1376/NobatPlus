using Domains;

namespace NobatPlusAPI.ViewModels
{ 
    public class UserPermissionVM : BaseEntity
    {
        public long UserId { get; set; }
        public string PermissionName { get; set; }
        public string RouteName { get; set; }
        public string PermissionType { get; set; }

        public bool IsGranted { get; set; } = true; // allow/deny override
        public bool OwnerOnly { get; set; }

    }
}