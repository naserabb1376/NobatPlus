using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ConstrainedExecution;
using MTPermissionCenter.EFCore.Entities;
using NobatPlusDATA.DataLayer;

namespace AITechDATA.DataLayer.Services
{
    public class UserPermissionRep : IUserPermissionRep
    {
        private NobatPlusContext _context;

        public UserPermissionRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddUserPermissionsAsync(List<MTPermissionCenter_UserPermission> UserPermissions)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.UserPermissions.AddRangeAsync(UserPermissions);
                await _context.SaveChangesAsync();
                result.ID = UserPermissions.FirstOrDefault().ID;
                foreach (var UserPermission in UserPermissions)
                {
                    _context.Entry(UserPermission).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditUserPermissionsAsync(List<MTPermissionCenter_UserPermission> UserPermissions)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.UserPermissions.UpdateRange(UserPermissions);
                await _context.SaveChangesAsync();
                result.ID = UserPermissions.FirstOrDefault().ID;
                foreach (var UserPermission in UserPermissions)
                {
                    _context.Entry(UserPermission).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistUserPermissionAsync(long UserPermissionId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.UserPermissions
                    .AsNoTracking()
                    .AnyAsync(x => x.ID == UserPermissionId);
                result.ID = UserPermissionId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<MTPermissionCenter_UserPermission>> GetAllUserPermissionsAsync(long UserId = 0, long PerrmissionId = 0, string permissionType = "", int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            ListResultObject<MTPermissionCenter_UserPermission> results = new ListResultObject<MTPermissionCenter_UserPermission>();
            try
            {
                var query = _context.UserPermissions.Include(x => x.Permission).AsNoTracking();
                if (PerrmissionId > 0)
                {
                    query = query.Where(x => x.PermissionId == PerrmissionId);
                }

                if (UserId > 0)
                {
                    query = query.Where(x => x.UserId == UserId);

                }

                if (!string.IsNullOrEmpty(permissionType))
                {
                    query = query.Where(x => x.Permission.PermissionType == permissionType);

                }

                query = query.Where(x =>
                        x.Permission.Name.ToString().Contains(searchText) ||
                        x.Permission.Routename.ToString().Contains(searchText) ||
                        x.Permission.Description.ToString().Contains(searchText) ||
                        x.Permission.Icon.ToString().Contains(searchText) ||
                        x.Permission.Key.ToString().Contains(searchText)
                    );


                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.ID)
                     .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
                    .Include(x => x.Permission)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<MTPermissionCenter_UserPermission>> GetUserPermissionByIdAsync(long UserPermissionId)
        {
            RowResultObject<MTPermissionCenter_UserPermission> result = new RowResultObject<MTPermissionCenter_UserPermission>();
            try
            {
                result.Result = await _context.UserPermissions
                    .AsNoTracking()
                    .Include(x => x.Permission)
                    .SingleOrDefaultAsync(x => x.ID == UserPermissionId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveUserPermissionsAsync(List<MTPermissionCenter_UserPermission> UserPermissions)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.UserPermissions.RemoveRange(UserPermissions);
                await _context.SaveChangesAsync();
                result.ID = UserPermissions.FirstOrDefault().ID;
                foreach (var UserPermission in UserPermissions)
                {
                    _context.Entry(UserPermission).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveUserPermissionsAsync(List<long> UserPermissionIds)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var UserPermissionsToRemove = new List<MTPermissionCenter_UserPermission>();

                foreach (var UserPermissionId in UserPermissionIds)
                {
                    var UserPermission = await GetUserPermissionByIdAsync(UserPermissionId);
                    if (UserPermission.Result != null)
                    {
                        UserPermissionsToRemove.Add(UserPermission.Result);
                    }
                }

                if (UserPermissionsToRemove.Any())
                {
                    result = await RemoveUserPermissionsAsync(UserPermissionsToRemove);
                }
                else
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching UserPermissions found to remove.";
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }
    }
}