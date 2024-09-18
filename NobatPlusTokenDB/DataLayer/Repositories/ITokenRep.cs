using ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace Repositories
{
    public interface ITokenRep
    {
        public Task<ListResultObject<RefreshToken>> GetAllRefreshTokensAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");

        public Task<RowResultObject<RefreshToken>> GetRefreshTokenByIdAsync(long RefreshTokenId);

        public Task<BitResultObject> AddRefreshTokenAsync(RefreshToken RefreshToken);

        public Task<BitResultObject> EditRefreshTokenAsync(RefreshToken RefreshToken);

        public Task<BitResultObject> RemoveRefreshTokenAsync(RefreshToken RefreshToken);

        public Task<BitResultObject> RemoveRefreshTokenAsync(long RefreshTokenId);

        public Task<BitResultObject> ExistRefreshTokenAsync(long RefreshTokenId);
    }
}