using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TrekkingForCharity.Api.CodeGeneration.TemplateBuilders;

namespace TrekkingForCharity.Api.CodeGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var builders = new List<ITemplateBuilder>
            {
                new ApiClientTemplateBuilder(),
                new CommandsTemplateBuilder()
            };

            Parallel.ForEach(builders, (builder) => { builder.Build(); });
        }
    }
}
