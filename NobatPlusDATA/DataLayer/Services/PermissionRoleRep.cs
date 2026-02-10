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

namespace NobatPlusDATA.DataLayer.Services
{
    public class PermissionRoleRep : IPermissionRoleRep
    {
        private NobatPlusContext _context;

        public PermissionRoleRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddPermissionRolesAsync(List<MTPermissionCenter_PermissionRole> PermissionRoles)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                PermissionRoles = PermissionRoles.Where(p=>  ! _context.PermissionRoles.Any(x=> x.PermissionId == p.PermissionId && x.RoleId == p.RoleId)).ToList();
                await _context.PermissionRoles.AddRangeAsync(PermissionRoles);
                await _context.SaveChangesAsync();
                result.ID = PermissionRoles.Count> 0 ?  PermissionRoles.FirstOrDefault().ID : 0;
                foreach (var permissionRole in PermissionRoles)
                {
                    _context.Entry(permissionRole).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditPermissionRolesAsync(List<MTPermissionCenter_PermissionRole> PermissionRoles)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                PermissionRoles = PermissionRoles.Where(p => !_context.PermissionRoles.Any(x => x.PermissionId == p.PermissionId && x.RoleId == p.RoleId)).ToList();
                _context.PermissionRoles.UpdateRange(PermissionRoles);
                await _context.SaveChangesAsync();
                result.ID = PermissionRoles.Count > 0 ? PermissionRoles.FirstOrDefault().ID : 0;
                foreach (var permissionRole in PermissionRoles)
                {
                    _context.Entry(permissionRole).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistPermissionRoleAsync(long PermissionRoleId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.PermissionRoles
                    .AsNoTracking()
                    .AnyAsync(x => x.ID == PermissionRoleId);
                result.ID = PermissionRoleId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<MTPermissionCenter_PermissionRole>> GetAllPermissionRolesAsync(long RoleId = 0, long PerrmissionId = 0, string permissionType = "", int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            ListResultObject<MTPermissionCenter_PermissionRole> results = new ListResultObject<MTPermissionCenter_PermissionRole>();
            try
            {
                var query = _context.PermissionRoles.Include(x => x.Permission).AsNoTracking();
                if (PerrmissionId > 0)
                {
                    query = query.Where(x => x.PermissionId == PerrmissionId);
                }

                if (RoleId > 0)
                {
                    query = query.Where(x => x.RoleId == RoleId);

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

        public async Task<RowResultObject<MTPermissionCenter_PermissionRole>> GetPermissionRoleByIdAsync(long PermissionRoleId)
        {
            RowResultObject<MTPermissionCenter_PermissionRole> result = new RowResultObject<MTPermissionCenter_PermissionRole>();
            try
            {
                result.Result = await _context.PermissionRoles
                    .AsNoTracking()
                    .Include(x => x.Permission)
                    .SingleOrDefaultAsync(x => x.ID == PermissionRoleId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemovePermissionRolesAsync(List<MTPermissionCenter_PermissionRole> PermissionRoles)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.PermissionRoles.RemoveRange(PermissionRoles);
                await _context.SaveChangesAsync();
                result.ID = PermissionRoles.FirstOrDefault().ID;
                foreach (var permissionRole in PermissionRoles)
                {
                    _context.Entry(permissionRole).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemovePermissionRolesAsync(List<long> PermissionRoleIds)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var PermissionRolesToRemove = new List<MTPermissionCenter_PermissionRole>();

                foreach (var PermissionRoleId in PermissionRoleIds)
                {
                    var PermissionRole = await GetPermissionRoleByIdAsync(PermissionRoleId);
                    if (PermissionRole.Result != null)
                    {
                        PermissionRolesToRemove.Add(PermissionRole.Result);
                    }
                }

                if (PermissionRolesToRemove.Any())
                {
                    result = await RemovePermissionRolesAsync(PermissionRolesToRemove);
                }
                else
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching PermissionRoles found to remove.";
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