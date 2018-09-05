using System;
using TrekkingForCharity.Api.CodeGeneration.TemplateBuilders;

namespace TrekkingForCharity.Api.CodeGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var b = new CommandsTemplateBuilder();
            b.Build();
        }
    }
}
