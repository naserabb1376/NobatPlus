using Microsoft.EntityFrameworkCore;

using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Domain;
using NobatPlusTokenDB.DataLayer;
using ResultObjects;
using Azure.Core;
using NobatPlusTokenDB.Tools;

namespace Services
{
    public class TokenRep : ITokenRep
    {
        private RefreshTokenDBContext _context;

        public TokenRep()
        {
            _context = TokenDbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddRefreshTokenAsync(RefreshToken RefreshToken)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.RefreshTokens.AddAsync(RefreshToken);
                await _context.SaveChangesAsync();
                result.ID = RefreshToken.ID;
                _context.Entry(RefreshToken).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditRefreshTokenAsync(RefreshToken RefreshToken)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.RefreshTokens.Update(RefreshToken);
                await _context.SaveChangesAsync();
                result.ID = RefreshToken.ID;
                _context.Entry(RefreshToken).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistRefreshTokenAsync(long RefreshTokenId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.RefreshTokens
                .AsNoTracking()
                .AnyAsync(x => x.ID == RefreshTokenId);
                result.ID = RefreshTokenId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<RowResultObject<RefreshToken>> FindTokenAsync(string Token, string type,bool status = true)
        {
            RowResultObject<RefreshToken> result = new RowResultObject<RefreshToken>();
            var nowDate = DateTime.Now.ToShamsi();


            try
            {
                var tokenrow = await _context.RefreshTokens
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Token == Token && x.Type.ToLower() == type.ToLower() && x.Status == status);

                var timeDiddrence =  tokenrow.ExpiryDate - nowDate;

                if (timeDiddrence.TotalMinutes > 0)
                {
                    result.Result = tokenrow;
                }

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<RefreshToken>> GetAllRefreshTokensAsync(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<RefreshToken> results = new ListResultObject<RefreshToken>();
            try
            {
                var query = _context.RefreshTokens
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Token.ToString()) && x.Token.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Type.ToString()) && x.Type.ToString().Contains(searchText)) ||

                    (x.ExpiryDate.ToString().Contains(searchText)) ||
                    (x.CreatedDate.ToString().Contains(searchText)) ||
                    (x.RevokedDate.HasValue && x.RevokedDate.Value.ToString().Contains(searchText))
                );

                results.TotalCount = query.Count();
                results.PageCount = TokenDbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreatedDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<RefreshToken>> GetRefreshTokenByIdAsync(long RefreshTokenId)
        {
            RowResultObject<RefreshToken> result = new RowResultObject<RefreshToken>();
            try
            {
                result.Result = await _context.RefreshTokens
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == RefreshTokenId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> MakeTokenExpireAsync(long TokenId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var RefreshToken = await GetRefreshTokenByIdAsync(TokenId);
                RefreshToken.Result.Status = false;
                RefreshToken.Result.RevokedDate = DateTime.Now.ToShamsi();
                result = await EditRefreshTokenAsync(RefreshToken.Result);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveRefreshTokenAsync(RefreshToken RefreshToken)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.RefreshTokens.Remove(RefreshToken);
                await _context.SaveChangesAsync();
                result.ID = RefreshToken.ID;
                _context.Entry(RefreshToken).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveRefreshTokenAsync(long RefreshTokenId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var RefreshToken = await GetRefreshTokenByIdAsync(RefreshTokenId);
                result = await RemoveRefreshTokenAsync(RefreshToken.Result);
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