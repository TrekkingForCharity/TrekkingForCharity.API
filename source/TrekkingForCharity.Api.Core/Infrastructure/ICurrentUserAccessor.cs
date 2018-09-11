using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MaybeMonad;

namespace TrekkingForCharity.Api.Core.Infrastructure
{
    public interface ICurrentUserAccessor
    {
        Task<Maybe<CurrentUser>> GetCurrentUser();
    }

    public class CurrentUser
    {
        public CurrentUser(string userId)
        {
            this.UserId = userId;
        }

        public string UserId { get; }
    }
}
