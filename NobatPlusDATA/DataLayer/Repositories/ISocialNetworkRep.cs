using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ISocialNetworkRep
    {
        public Task<ListResultObject<SocialNetwork>> GetAllSocialNetworksAsync(long stylistId=0,int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<SocialNetwork>> GetSocialNetworkByIdAsync(long SocialNetworkId);
        public Task<BitResultObject> AddSocialNetworkAsync(SocialNetwork SocialNetwork);
        public Task<BitResultObject> EditSocialNetworkAsync(SocialNetwork SocialNetwork);
        public Task<BitResultObject> RemoveSocialNetworkAsync(SocialNetwork SocialNetwork);
        public Task<BitResultObject> RemoveSocialNetworkAsync(long SocialNetworkId);
        public Task<BitResultObject> ExistSocialNetworkAsync(long SocialNetworkId);
        public Task<BitResultObject> ExistSocialNetworkAccountAsync(string socialnetworkName,long stylistId=0,string phoneNumber ="",string accountLink="");
    }
}
