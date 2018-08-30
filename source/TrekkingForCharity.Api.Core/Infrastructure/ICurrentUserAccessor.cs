using System;
using System.Collections.Generic;
using System.Text;
using MaybeMonad;

namespace TrekkingForCharity.Api.Core.Infrastructure
{
    public interface ICurrentUserAccessor
    {
        Maybe<CurrentUser> GetCurrentUser();
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
