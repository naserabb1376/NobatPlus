using Microsoft.EntityFrameworkCore;
using MTPermissionCenter.EFCore.Entities;
using MTPermissionCenter.EFCore.Tools;
using NobatPlusDATA.DataLayer;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbTools = NobatPlusDATA.Tools.DbTools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class PermissionRep : IPermissionRep
    {
        private NobatPlusContext _context;

        public PermissionRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddPermissionAsync(MTPermissionCenter_Permission permission)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Permissions.AddAsync(permission);
                await _context.SaveChangesAsync();
                result.ID = permission.ID;
                _context.Entry(permission).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditPermissionAsync(MTPermissionCenter_Permission permission)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Permissions.Update(permission);
                await _context.SaveChangesAsync();
                result.ID = permission.ID;
                _context.Entry(permission).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistPermissionAsync(long permissionId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Permissions
                    .AsNoTracking()
                    .AnyAsync(x => x.ID == permissionId);
                result.ID = permissionId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<MTPermissionCenter_Permission>> GetAllPermissionsAsync(long roleId = 0,long userId = 0,string permissionType="", long MenuParentId = 0, string MenuIds = "", int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            ListResultObject<MTPermissionCenter_Permission> results = new ListResultObject<MTPermissionCenter_Permission>();
            try
            {
                IQueryable<MTPermissionCenter_Permission> query = _context.Permissions.AsNoTracking();

                if (roleId > 0 || userId > 0)
                {
                    query = query.Where(p =>
                        (roleId > 0 && _context.PermissionRoles.Any(pr =>
                            pr.RoleId == roleId && pr.PermissionId == p.ID))
                        ||
                        (userId > 0 && _context.UserPermissions.Any(up =>
                            up.UserId == userId && up.PermissionId == p.ID && up.IsGranted))
                    );
                }

                if (!string.IsNullOrEmpty(permissionType))
                {
                    query = query.Where(x => x.PermissionType.ToLower() == permissionType.ToLower());
                }

                if (MenuParentId > 0)
                {
                    query = query.Where(x => x.MenuParentId == MenuParentId);
                }

                if (!string.IsNullOrWhiteSpace(MenuIds))
                {
                    var inpMenuIdsArr = MenuIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToArray();

                    query = query.Where(x =>
                        x.MenuIds != null &&
                        x.MenuIds
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Any(id => inpMenuIdsArr.Contains(id))
                    );
                }


                query = query.Where(x =>
                      (!string.IsNullOrEmpty(x.Name) && x.Name.Contains(searchText))
                   || (!string.IsNullOrEmpty(x.PermissionType) && x.PermissionType.Contains(searchText))
                   || (!string.IsNullOrEmpty(x.Icon) && x.Icon.Contains(searchText))
                   || (!string.IsNullOrEmpty(x.Routename) && x.Routename.Contains(searchText))
                   || (!string.IsNullOrEmpty(x.Key) && x.Key.Contains(searchText))
                   || (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                   || (!string.IsNullOrEmpty(x.MenuIds) && x.MenuIds.Contains(searchText))
                  );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                     .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
                    //.Include(x => x.PermissionRoles)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<MTPermissionCenter_Permission>> GetPermissionByIdAsync(long permissionId)
        {
            RowResultObject<MTPermissionCenter_Permission> result = new RowResultObject<MTPermissionCenter_Permission>();
            try
            {
                result.Result = await _context.Permissions
                    .AsNoTracking()
                    .Include(x => x.PermissionRoles)
                    .SingleOrDefaultAsync(x => x.ID == permissionId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemovePermissionAsync(MTPermissionCenter_Permission permission)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();
                result.ID = permission.ID;
                _context.Entry(permission).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemovePermissionAsync(long permissionId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var permission = await GetPermissionByIdAsync(permissionId);
                result = await RemovePermissionAsync(permission.Result);
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