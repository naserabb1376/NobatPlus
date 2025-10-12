using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusAPI.DataLayer.Services
{
    public class RoleRep : IRoleRep
    {
        private NobatPlusContext _context;

        public RoleRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddRoleAsync(Role role)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Roles.AddAsync(role);
                await _context.SaveChangesAsync();
                result.ID = role.ID;
                _context.Entry(role).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditRoleAsync(Role role)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();
                result.ID = role.ID;
                _context.Entry(role).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistRoleAsync(long roleId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Roles
                    .AsNoTracking()
                    .AnyAsync(x => x.ID == roleId);
                result.ID = roleId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<Role>> GetAllRolesAsync(long permissionId=0,int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<Role> results = new ListResultObject<Role>();
            try
            {
                IQueryable<Role> query;
               // if (permissionId > 0)
               // {
               //     query = _context.PermissionRoles.Where(x => x.PerrmissionId == permissionId).Select(x => x.Role)
               //    .AsNoTracking()
               //    .Where(x =>
               //         (!string.IsNullOrEmpty(x.Name) && x.Name.Contains(searchText)) ||
               //        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
               //    );
               // }
               //else
               // {
                    query = _context.Roles
                   .AsNoTracking()
                   .Where(x =>
                       (!string.IsNullOrEmpty(x.Name) && x.Name.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                   );
                //}

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                     .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
                   // .Include(x => x.Persons)
                   // .Include(x => x.PermissionRoles)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<Role>> GetRoleByIdAsync(long roleId)
        {
            RowResultObject<Role> result = new RowResultObject<Role>();
            try
            {
                result.Result = await _context.Roles
                    .AsNoTracking()
                 //   .Include(x => x.Persons)
                   // .Include(x => x.PermissionRoles)
                    .SingleOrDefaultAsync(x => x.ID == roleId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveRoleAsync(Role role)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                result.ID = role.ID;
                _context.Entry(role).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveRoleAsync(long roleId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var role = await GetRoleByIdAsync(roleId);
                result = await RemoveRoleAsync(role.Result);
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