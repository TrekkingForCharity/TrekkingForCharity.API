using System;
using System.Text;


namespace TrekkingForCharity.Api.Client.Executors
{
    public abstract class BaseCommand
    {
        private readonly string _route;

        protected BaseCommand(string route)
        {
            this._route = route;
        }

        internal string GetRoute()
        {
            return this._route;
        }
    }

    
}
