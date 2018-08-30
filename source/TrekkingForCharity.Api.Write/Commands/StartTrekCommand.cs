using System;
using System.Collections.Generic;
using System.Text;
using TrekkingForCharity.Api.Core.Commands;

namespace TrekkingForCharity.Api.Write.Commands
{
    public class StartTrekCommand : ICommand
    {
        public Guid TrekId { get; set; }
    }
}
